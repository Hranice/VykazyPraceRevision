using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;

namespace VykazyPrace.Core.Services
{
    /// <summary>
    /// Parametry mapování Outlook meeting => TimeEntry.
    /// </summary>
    public sealed class MeetingImportOptions
    {
        /// <summary>Projekt, do kterého se zapisují meetingy.</summary>
        // TODO: Checknout ID podle finální db.
        public int ProjectIdForMeetings { get; init; } = 25;
        /// <summary>Typ zápisu pro meetingy.</summary>
        // TODO: Checknout ID podle finální db.
        public int EntryTypeIdForMeetings { get; init; } = 25;
        /// <summary>Zaokrouhlení délky z meetingu (v min).</summary>
        public int RoundStepMinutes { get; init; } = 30;
        /// <summary>Minimální délka záznamu (v min).</summary>
        public int MinMinutes { get; init; } = 30;
        /// <summary>Délka pro celodenní/bez času (v min).</summary>
        public int AllDayDefaultMinutes { get; init; } = 450;
    }

    public enum ImportStatus
    {
        Added,              // úspěšně přidáno (vznikl TimeEntry)
        SkippedConflict,    // kolize s jiným záznamem
        SkippedDuplicate,   // těsně duplicitní meeting – nehodnotíme jako chybu, ale nebudeme přidávat
        InvalidInput,       // špatné datum/čas z UI
        Error               // jiná chyba
    }

    public sealed class ImportResult
    {
        public ImportStatus Status { get; init; }
        public string Message { get; init; } = "";
        public int? TimeEntryId { get; init; }
        public int ItemId { get; init; }
    }

    /// <summary>
    /// Middleware pro import meetingů do TimeEntries:
    ///  - parsuje UI (datum/čas),
    ///  - počítá délky s rozumnými defaulty,
    ///  - brání konfliktům a duplicitám,
    ///  - vytváří záznamy a nastavuje UserItemState.Written.
    /// </summary>
    public class OutlookMeetingImportService
    {
        private readonly CalendarRepository _calRepo;
        private readonly TimeEntryRepository _timeRepo;
        private readonly MeetingImportOptions _opt;

        public OutlookMeetingImportService(MeetingImportOptions? options = null)
        {
            _calRepo = new CalendarRepository();
            _timeRepo = new TimeEntryRepository();
            _opt = options ?? new MeetingImportOptions();
        }

        /// <summary>
        /// Přidá meeting z UI labelů (datum/čas/subject) pro daný CalendarItem.
        /// Po úspěchu označí položku jako Written.
        /// </summary>
        public async Task<ImportResult> AddSingleFromUiAsync(
            int userId,
            int itemId,
            string dateText,   // "dd.MM.yyyy"
            string timeText,   // "HH:mm - HH:mm" | "HH:mm" | "Celý den"
            string? subject)
        {
            // validace/parsování
            if (!TryParseUiDate(dateText, out var dateLocal))
                return Fail(itemId, ImportStatus.InvalidInput, "Neplatné datum.");

            TryParseUiTime(dateLocal, timeText, out var startLocal, out var endLocal);
            var (start, end, minutes) = ComputeInterval(startLocal, endLocal);

            // kolize/duplicitní kontrola
            var dayEntries = await _timeRepo.GetTimeEntriesByUserAndDateAsync(new User { Id = userId }, start.Date);

            if (HasOverlap(dayEntries, start, end))
                return Fail(itemId, ImportStatus.SkippedConflict, "Konflikt s existujícím záznamem.");

            if (LooksLikeSameMeeting(dayEntries, start, _opt.ProjectIdForMeetings, _opt.EntryTypeIdForMeetings, subject))
            {
                await _calRepo.SetUserStateAsync(userId, itemId, UserItemStateEnum.Written);
                return Ok(itemId, ImportStatus.SkippedDuplicate, "Duplicitní meeting – označeno jako zapsané.");
            }

            // vytvoření time entry
            var te = await _timeRepo.CreateTimeEntryAsync(new TimeEntry
            {
                UserId = userId,
                ProjectId = _opt.ProjectIdForMeetings,
                EntryTypeId = _opt.EntryTypeIdForMeetings,
                Timestamp = start,
                Description = "Outlook událost",
                EntryMinutes = minutes,
                AfterCare = 0,
                Note = string.IsNullOrWhiteSpace(subject) ? "(bez názvu)" : subject!.Trim(),
                IsLocked = 0,
                IsValid = 1
            });

            await _calRepo.SetUserStateAsync(userId, itemId, UserItemStateEnum.Written);
            return new ImportResult { ItemId = itemId, Status = ImportStatus.Added, Message = "Přidáno.", TimeEntryId = te.Id };
        }

        /// <summary>
        /// Přidá hromadně "viditelné" (ne-Written/IgnoreTombstone) události pro uživatele.
        /// Zamezí konfliktům s DB i napříč právě přidávanými položkami. Po úspěchu označí Written.
        /// </summary>
        public async Task<(int added, int conflicts, int duplicates)> AddAllVisibleAsync(
            int userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            var items = await _calRepo.GetVisibleItemsForUserByAttendanceAsync(userId, fromUtc, toUtc);

            if (items.Count == 0) return (0, 0, 0);

            var candidates = items.Select(x => x.Item)
                                  .Where(i => i != null)
                                  .Select(MakeCandidateFromItem)
                                  .Where(c => c != null)
                                  .Cast<Candidate>()
                                  .OrderBy(c => c.StartLocal)
                                  .ToList();

            int added = 0, conflicts = 0, duplicates = 0;

            var existingByDay = new Dictionary<DateTime, List<TimeEntry>>(); // lokální den -> záznamy z DB
            var stagedByDay = new Dictionary<DateTime, List<(DateTime s, DateTime e)>>(); // co jsme právě přidali

            foreach (var c in candidates)
            {
                if (!existingByDay.TryGetValue(c.Day, out var dayEntries))
                {
                    dayEntries = await _timeRepo.GetTimeEntriesByUserAndDateAsync(new User { Id = userId }, c.Day);
                    existingByDay[c.Day] = dayEntries;
                }
                if (!stagedByDay.ContainsKey(c.Day))
                    stagedByDay[c.Day] = new List<(DateTime s, DateTime e)>();

                bool conflictExisting = HasOverlap(dayEntries, c.StartLocal, c.EndLocal);
                bool conflictStaged = stagedByDay[c.Day].Any(x => Overlaps(c.StartLocal, c.EndLocal, x.s, x.e));

                if (conflictExisting || conflictStaged)
                {
                    conflicts++;
                    continue;
                }

                if (LooksLikeSameMeeting(dayEntries, c.StartLocal, _opt.ProjectIdForMeetings, _opt.EntryTypeIdForMeetings, c.Subject))
                {
                    duplicates++;
                    await _calRepo.SetUserStateAsync(userId, c.ItemId, UserItemStateEnum.Written);
                    continue;
                }

                await _timeRepo.CreateTimeEntryAsync(new TimeEntry
                {
                    UserId = userId,
                    ProjectId = _opt.ProjectIdForMeetings,
                    EntryTypeId = _opt.EntryTypeIdForMeetings,
                    Timestamp = c.StartLocal,
                    Description = "Outlook událost",
                    EntryMinutes = c.Minutes,
                    AfterCare = 0,
                    Note = string.IsNullOrWhiteSpace(c.Subject) ? "(bez názvu)" : c.Subject!.Trim(),
                    IsLocked = 0,
                    IsValid = 1
                });
                await _calRepo.SetUserStateAsync(userId, c.ItemId, UserItemStateEnum.Written);

                stagedByDay[c.Day].Add((c.StartLocal, c.EndLocal));
                added++;
            }

            return (added, conflicts, duplicates);
        }

        private sealed class Candidate
        {
            public int ItemId { get; init; }
            public string Subject { get; init; } = "";
            public DateTime Day { get; init; }
            public DateTime StartLocal { get; init; }
            public DateTime EndLocal { get; init; }
            public int Minutes { get; init; }
        }

        private Candidate? MakeCandidateFromItem(CalendarItem item)
        {
            if (item.StartUtc.HasValue && item.EndUtc.HasValue && item.EndUtc > item.StartUtc)
            {
                var s = item.StartUtc.Value.ToLocalTime();
                var e = item.EndUtc.Value.ToLocalTime();
                var raw = (int)Math.Round((e - s).TotalMinutes);
                var minutes = RoundToStep(raw, _opt.RoundStepMinutes, _opt.MinMinutes);
                return new Candidate
                {
                    ItemId = item.Id,
                    Subject = string.IsNullOrWhiteSpace(item.Subject) ? "(bez názvu)" : item.Subject.Trim(),
                    Day = s.Date,
                    StartLocal = s,
                    EndLocal = s.AddMinutes(minutes),
                    Minutes = minutes
                };
            }
            else
            {
                // Celodenní / bez času => default 7,5h od půlnoci
                var day = DateTime.Now.Date;
                return new Candidate
                {
                    ItemId = item.Id,
                    Subject = string.IsNullOrWhiteSpace(item.Subject) ? "(bez názvu)" : item.Subject.Trim(),
                    Day = day,
                    StartLocal = day,
                    EndLocal = day.AddMinutes(_opt.AllDayDefaultMinutes),
                    Minutes = _opt.AllDayDefaultMinutes
                };
            }
        }

        private static bool TryParseUiDate(string dateText, out DateTime dateLocal)
        {
            return DateTime.TryParseExact(
                dateText, "dd.MM.yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"),
                System.Globalization.DateTimeStyles.None,
                out dateLocal);
        }

        private static void TryParseUiTime(DateTime dateLocal, string? timeText, out DateTime? startLocal, out DateTime? endLocal)
        {
            startLocal = null;
            endLocal = null;
            var txt = timeText?.Trim();
            if (string.IsNullOrEmpty(txt) || txt.Equals("Celý den", StringComparison.OrdinalIgnoreCase))
                return;

            var parts = txt.Split('-', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 2)
            {
                if (TimeSpan.TryParse(parts[0], out var tsFrom)) startLocal = dateLocal.Date.Add(tsFrom);
                if (TimeSpan.TryParse(parts[1], out var tsTo)) endLocal = dateLocal.Date.Add(tsTo);
            }
            else if (parts.Length == 1 && TimeSpan.TryParse(parts[0], out var tsOnly))
            {
                startLocal = dateLocal.Date.Add(tsOnly);
            }
        }

        private (DateTime start, DateTime end, int minutes) ComputeInterval(DateTime? startLocal, DateTime? endLocal)
        {
            if (startLocal.HasValue && endLocal.HasValue && endLocal > startLocal)
            {
                var raw = (int)Math.Round((endLocal.Value - startLocal.Value).TotalMinutes);
                var minutes = RoundToStep(raw, _opt.RoundStepMinutes, _opt.MinMinutes);
                var start = startLocal.Value;
                return (start, start.AddMinutes(minutes), minutes);
            }
            else
            {
                var start = (startLocal ?? DateTime.Now.Date);
                return (start, start.AddMinutes(_opt.AllDayDefaultMinutes), _opt.AllDayDefaultMinutes);
            }
        }

        private static int RoundToStep(int minutes, int step, int min)
        {
            if (step <= 0) return Math.Max(minutes, min);
            var rounded = (int)Math.Round(minutes / (double)step) * step;
            return Math.Max(rounded, min);
        }

        private static bool Overlaps(DateTime a1, DateTime a2, DateTime b1, DateTime b2)
            => a1 < b2 && a2 > b1;

        private static bool HasOverlap(IEnumerable<TimeEntry> dayEntries, DateTime start, DateTime end)
        {
            return dayEntries
                .Where(e => e.Timestamp.HasValue && e.EntryMinutes > 0)
                .Any(e =>
                {
                    var es = e.Timestamp!.Value;
                    var ee = es.AddMinutes(e.EntryMinutes);
                    return Overlaps(start, end, es, ee);
                });
        }

        private static bool LooksLikeSameMeeting(IEnumerable<TimeEntry> dayEntries, DateTime start,
                                                 int projectId, int entryTypeId, string? subject)
        {
            var subj = (subject ?? "").Trim();
            return dayEntries.Any(e =>
                e.ProjectId == projectId &&
                e.EntryTypeId == entryTypeId &&
                string.Equals((e.Description ?? "").Trim(), subj, StringComparison.Ordinal) &&
                e.Timestamp.HasValue &&
                Math.Abs((e.Timestamp.Value - start).TotalMinutes) <= 5);
        }

        private static ImportResult Ok(int itemId, ImportStatus status, string msg) =>
            new ImportResult { ItemId = itemId, Status = status, Message = msg };

        private static ImportResult Fail(int itemId, ImportStatus status, string msg) =>
            new ImportResult { ItemId = itemId, Status = status, Message = msg };
    }
}

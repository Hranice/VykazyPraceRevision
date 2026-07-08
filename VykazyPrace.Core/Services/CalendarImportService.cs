using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Core.Services
{
    /// <summary>
    /// Parametry mapování Outlook meeting => TimeEntry.
    /// </summary>
    public sealed class MeetingImportOptions
    {
        public int ProjectIdForMeetings { get; init; } = WorkLogIds.Projects.Other;
        public int EntryTypeIdForMeetings { get; init; } = WorkLogIds.EntryTypes.OutlookEvent;
        public int RoundStepMinutes { get; init; } = 30;
        public int MinMinutes { get; init; } = 30;
        public int AllDayDefaultMinutes { get; init; } = 450;
    }

    public enum ImportStatus
    {
        Added,
        SkippedConflict,
        SkippedDuplicate,
        InvalidInput,
        Error
    }

    public sealed class ImportResult
    {
        public ImportStatus Status { get; init; }
        public string Message { get; init; } = "";
        public int? TimeEntryId { get; init; }
        public int ItemId { get; init; }
    }

    public class OutlookMeetingImportService
    {
        private readonly CalendarRepository _calRepo;
        private readonly TimeEntryRepository _timeRepo;
        private readonly MeetingImportOptions _opt;

        public OutlookMeetingImportService(
            CalendarRepository calRepo,
            TimeEntryRepository timeRepo,
            MeetingImportOptions? options = null)
        {
            _calRepo = calRepo;
            _timeRepo = timeRepo;
            _opt = options ?? new MeetingImportOptions();
        }

        public async Task<ImportResult> AddSingleFromUiAsync(
            int userId,
            int itemId,
            string dateText,
            string timeText,
            string? subject)
        {
            if (!TryParseUiDate(dateText, out var dateLocal))
                return Fail(itemId, ImportStatus.InvalidInput, "Neplatné datum.");

            TryParseUiTime(dateLocal, timeText, out var startLocal, out var endLocal);

            var (start, end, minutes) = ComputeInterval(startLocal, endLocal);

            var dayEntries = await _timeRepo.GetTimeEntriesByUserAndDateAsync(
                new User { Id = userId },
                start.Date);

            if (HasOverlap(dayEntries, start, end))
                return Fail(itemId, ImportStatus.SkippedConflict, "Konflikt s existujícím záznamem.");

            if (LooksLikeSameMeeting(
                    dayEntries,
                    start,
                    _opt.ProjectIdForMeetings,
                    _opt.EntryTypeIdForMeetings,
                    subject))
            {
                await _calRepo.SetUserStateAsync(userId, itemId, UserItemStateEnum.Written);

                return Ok(
                    itemId,
                    ImportStatus.SkippedDuplicate,
                    "Duplicitní meeting – označeno jako zapsané.");
            }

            var timeEntry = await _timeRepo.CreateTimeEntryAsync(new Database.Models.TimeEntry
            {
                UserId = userId,
                ProjectId = _opt.ProjectIdForMeetings,
                EntryTypeId = _opt.EntryTypeIdForMeetings,
                Timestamp = start,
                Description = "Outlook událost",
                EntryMinutes = minutes,
                AfterCare = 0,
                Note = string.IsNullOrWhiteSpace(subject) ? "(bez názvu)" : subject.Trim(),
                IsLocked = 0,
                IsValid = 1
            });

            await _calRepo.SetUserStateAsync(userId, itemId, UserItemStateEnum.Written);

            return new ImportResult
            {
                ItemId = itemId,
                Status = ImportStatus.Added,
                Message = "Přidáno.",
                TimeEntryId = timeEntry.Id
            };
        }

        public async Task<(int added, int conflicts, int duplicates)> AddAllVisibleAsync(
            int userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            var items = await _calRepo.GetVisibleItemsForUserByAttendanceAsync(
                userId,
                fromUtc,
                toUtc);

            if (items.Count == 0)
                return (0, 0, 0);

            var candidates = items
                .Select(x => x.Item)
                .Where(i => i != null)
                .Select(MakeCandidateFromItem)
                .Where(c => c != null)
                .Cast<Candidate>()
                .OrderBy(c => c.StartLocal)
                .ToList();

            int added = 0;
            int conflicts = 0;
            int duplicates = 0;

            var existingByDay = new Dictionary<DateTime, List<Database.Models.TimeEntry>>();
            var stagedByDay = new Dictionary<DateTime, List<(DateTime Start, DateTime End)>>();

            foreach (var candidate in candidates)
            {
                if (!existingByDay.TryGetValue(candidate.Day, out var dayEntries))
                {
                    dayEntries = await _timeRepo.GetTimeEntriesByUserAndDateAsync(
                        new User { Id = userId },
                        candidate.Day);

                    existingByDay[candidate.Day] = dayEntries;
                }

                if (!stagedByDay.ContainsKey(candidate.Day))
                {
                    stagedByDay[candidate.Day] = new List<(DateTime Start, DateTime End)>();
                }

                bool conflictExisting = HasOverlap(
                    dayEntries,
                    candidate.StartLocal,
                    candidate.EndLocal);

                bool conflictStaged = stagedByDay[candidate.Day]
                    .Any(x => Overlaps(
                        candidate.StartLocal,
                        candidate.EndLocal,
                        x.Start,
                        x.End));

                if (conflictExisting || conflictStaged)
                {
                    conflicts++;
                    continue;
                }

                if (LooksLikeSameMeeting(
                        dayEntries,
                        candidate.StartLocal,
                        _opt.ProjectIdForMeetings,
                        _opt.EntryTypeIdForMeetings,
                        candidate.Subject))
                {
                    duplicates++;

                    await _calRepo.SetUserStateAsync(
                        userId,
                        candidate.ItemId,
                        UserItemStateEnum.Written);

                    continue;
                }

                await _timeRepo.CreateTimeEntryAsync(new Database.Models.TimeEntry
                {
                    UserId = userId,
                    ProjectId = _opt.ProjectIdForMeetings,
                    EntryTypeId = _opt.EntryTypeIdForMeetings,
                    Timestamp = candidate.StartLocal,
                    Description = "Outlook událost",
                    EntryMinutes = candidate.Minutes,
                    AfterCare = 0,
                    Note = string.IsNullOrWhiteSpace(candidate.Subject)
                        ? "(bez názvu)"
                        : candidate.Subject.Trim(),
                    IsLocked = 0,
                    IsValid = 1
                });

                await _calRepo.SetUserStateAsync(
                    userId,
                    candidate.ItemId,
                    UserItemStateEnum.Written);

                stagedByDay[candidate.Day].Add((
                    candidate.StartLocal,
                    candidate.EndLocal));

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
            if (item.StartUtc.HasValue &&
                item.EndUtc.HasValue &&
                item.EndUtc > item.StartUtc)
            {
                var start = item.StartUtc.Value.ToLocalTime();
                var end = item.EndUtc.Value.ToLocalTime();

                var rawMinutes = (int)Math.Round((end - start).TotalMinutes);
                var minutes = RoundToStep(rawMinutes, _opt.RoundStepMinutes, _opt.MinMinutes);

                return new Candidate
                {
                    ItemId = item.Id,
                    Subject = string.IsNullOrWhiteSpace(item.Subject)
                        ? "(bez názvu)"
                        : item.Subject.Trim(),
                    Day = start.Date,
                    StartLocal = start,
                    EndLocal = start.AddMinutes(minutes),
                    Minutes = minutes
                };
            }

            var day = DateTime.Now.Date;

            return new Candidate
            {
                ItemId = item.Id,
                Subject = string.IsNullOrWhiteSpace(item.Subject)
                    ? "(bez názvu)"
                    : item.Subject.Trim(),
                Day = day,
                StartLocal = day,
                EndLocal = day.AddMinutes(_opt.AllDayDefaultMinutes),
                Minutes = _opt.AllDayDefaultMinutes
            };
        }

        private static bool TryParseUiDate(string dateText, out DateTime dateLocal)
        {
            return DateTime.TryParseExact(
                dateText,
                "dd.MM.yyyy",
                System.Globalization.CultureInfo.GetCultureInfo("cs-CZ"),
                System.Globalization.DateTimeStyles.None,
                out dateLocal);
        }

        private static void TryParseUiTime(
            DateTime dateLocal,
            string? timeText,
            out DateTime? startLocal,
            out DateTime? endLocal)
        {
            startLocal = null;
            endLocal = null;

            var text = timeText?.Trim();

            if (string.IsNullOrEmpty(text) ||
                text.Equals("Celý den", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            var parts = text.Split(
                '-',
                StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 2)
            {
                if (TimeSpan.TryParse(parts[0], out var from))
                    startLocal = dateLocal.Date.Add(from);

                if (TimeSpan.TryParse(parts[1], out var to))
                    endLocal = dateLocal.Date.Add(to);
            }
            else if (parts.Length == 1 && TimeSpan.TryParse(parts[0], out var only))
            {
                startLocal = dateLocal.Date.Add(only);
            }
        }

        private (DateTime start, DateTime end, int minutes) ComputeInterval(
            DateTime? startLocal,
            DateTime? endLocal)
        {
            if (startLocal.HasValue &&
                endLocal.HasValue &&
                endLocal > startLocal)
            {
                var rawMinutes = (int)Math.Round(
                    (endLocal.Value - startLocal.Value).TotalMinutes);

                var minutes = RoundToStep(
                    rawMinutes,
                    _opt.RoundStepMinutes,
                    _opt.MinMinutes);

                var start = startLocal.Value;

                return (start, start.AddMinutes(minutes), minutes);
            }

            var fallbackStart = startLocal ?? DateTime.Now.Date;

            return (
                fallbackStart,
                fallbackStart.AddMinutes(_opt.AllDayDefaultMinutes),
                _opt.AllDayDefaultMinutes);
        }

        private static int RoundToStep(int minutes, int step, int min)
        {
            if (step <= 0)
                return Math.Max(minutes, min);

            var rounded = (int)Math.Round(minutes / (double)step) * step;

            return Math.Max(rounded, min);
        }

        private static bool Overlaps(DateTime a1, DateTime a2, DateTime b1, DateTime b2)
            => a1 < b2 && a2 > b1;

        private static bool HasOverlap(
            IEnumerable<Database.Models.TimeEntry> dayEntries,
            DateTime start,
            DateTime end)
        {
            return dayEntries
                .Where(e => e.Timestamp.HasValue && e.EntryMinutes > 0)
                .Any(e =>
                {
                    var entryStart = e.Timestamp!.Value;
                    var entryEnd = entryStart.AddMinutes(e.EntryMinutes);

                    return Overlaps(start, end, entryStart, entryEnd);
                });
        }

        private static bool LooksLikeSameMeeting(
            IEnumerable<Database.Models.TimeEntry> dayEntries,
            DateTime start,
            int projectId,
            int entryTypeId,
            string? subject)
        {
            var normalizedSubject = (subject ?? string.Empty).Trim();

            return dayEntries.Any(e =>
                e.ProjectId == projectId &&
                e.EntryTypeId == entryTypeId &&
                string.Equals(
                    (e.Note ?? string.Empty).Trim(),
                    normalizedSubject,
                    StringComparison.Ordinal) &&
                e.Timestamp.HasValue &&
                Math.Abs((e.Timestamp.Value - start).TotalMinutes) <= 5);
        }

        private static ImportResult Ok(
            int itemId,
            ImportStatus status,
            string message)
        {
            return new ImportResult
            {
                ItemId = itemId,
                Status = status,
                Message = message
            };
        }

        private static ImportResult Fail(
            int itemId,
            ImportStatus status,
            string message)
        {
            return new ImportResult
            {
                ItemId = itemId,
                Status = status,
                Message = message
            };
        }
    }
}

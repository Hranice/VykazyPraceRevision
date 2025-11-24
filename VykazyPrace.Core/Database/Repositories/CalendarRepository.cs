using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models.OutlookEvents;
using VykazyPrace.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repozitář pro práci s kalendářovými položkami, stavy uživatelů, účastníky a logy.
    /// </summary>
    public class CalendarRepository
    {
        private readonly VykazyPraceContext _context;

        public CalendarRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Vloží/aktualizuje CalendarItem dle unikátního klíče.
        /// </summary>
        public async Task<CalendarItem> UpsertCalendarItemAsync(CalendarItem input)
        {
            // Najdeme existující položku podle unikátní trojice
            var existing = await _context.CalendarItems
                .FirstOrDefaultAsync(ci =>
                    ci.StoreId == input.StoreId &&
                    ci.EntryId == input.EntryId &&
                    ci.OccurrenceStartUtc == input.OccurrenceStartUtc);

            if (existing == null)
            {
                _context.CalendarItems.Add(input);
            }
            else
            {
                // Přepíše "otisk" a metadata – ponecháme Id
                existing.LastSeenAtUtc = input.LastSeenAtUtc;
                existing.LastModifiedUtc = input.LastModifiedUtc;
                existing.LastFolderEntryId = input.LastFolderEntryId;
                existing.LastHash = input.LastHash;

                existing.GlobalAppointmentId = input.GlobalAppointmentId ?? existing.GlobalAppointmentId;
                existing.ICalUid = input.ICalUid ?? existing.ICalUid;

                existing.Subject = input.Subject;
                existing.Location = input.Location;
                existing.Organizer = input.Organizer;
                existing.StartUtc = input.StartUtc;
                existing.EndUtc = input.EndUtc;
                existing.IsAllDay = input.IsAllDay;
                existing.IsRecurringSeries = input.IsRecurringSeries;
                existing.IsException = input.IsException;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return existing ?? input;
        }

        /// <summary>
        /// Nahraď účastníky položky (smaž a vlož).
        /// </summary>
        public async Task UpsertAttendeesAsync(int itemId, IEnumerable<ItemAttendee> attendees)
        {
            var olds = _context.ItemAttendees.Where(a => a.ItemId == itemId);
            _context.ItemAttendees.RemoveRange(olds);

            foreach (var a in attendees)
            {
                a.ItemId = itemId; // jistota
                _context.ItemAttendees.Add(a);
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Nastaví stav položky z pohledu uživatele (UPSERT).
        /// Např. State = IgnoreTombstone = 3.
        /// </summary>
        public async Task<UserItemState> SetUserStateAsync(int userId, int itemId, UserItemStateEnum state, string? note = null)
        {
            var existing = await _context.UserItemStates
                .FirstOrDefaultAsync(s => s.UserId == userId && s.ItemId == itemId);

            if (existing == null)
            {
                existing = new UserItemState
                {
                    UserId = userId,
                    ItemId = itemId,
                    State = state,
                    Note = note,
                    UpdatedAtUtc = DateTime.UtcNow
                };
                _context.UserItemStates.Add(existing);
            }
            else
            {
                existing.State = state;
                existing.Note = note;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return existing;
        }

        /// <summary>
        /// Vrátí položky pro uživatele. 
        /// Ve výchozím stavu skrývá IGNORE_TOMBSTONE (State=3).
        /// </summary>
        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetItemsForUserAsync(
      int userId,
      bool includeIgnored = false,
      DateTime? fromUtc = null,
      DateTime? toUtc = null)
        {
            var q = from ci in _context.CalendarItems
                    join uis in _context.UserItemStates.Where(x => x.UserId == userId)
                        on ci.Id equals uis.ItemId into gj
                    from uis in gj.DefaultIfEmpty()
                    select new
                    {
                        ci,
                        state = (UserItemStateEnum?)uis.State
                    };

            if (!includeIgnored)
            {
                q = q.Where(x => x.state == null || x.state != UserItemStateEnum.IgnoreTombstone);
            }

            // test překryvu intervalů [StartUtc, EndUtc] a [fromUtc, toUtc]
            if (fromUtc.HasValue || toUtc.HasValue)
            {
                var f = fromUtc ?? DateTime.MinValue;
                var t = toUtc ?? DateTime.MaxValue;

                q = q.Where(x =>
                    (x.ci.StartUtc == null || x.ci.EndUtc == null) ||
                    (x.ci.StartUtc <= t && x.ci.EndUtc >= f)
                );
            }

            var data = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            return data
                .Select(x => (x.ci, x.state))
                .ToList();
        }



        public async Task LogChangeAsync(int itemId, string action, int? userId = null, string? detailsJson = null)
        {
            _context.ItemChangeLogs.Add(new ItemChangeLog
            {
                ItemId = itemId,
                UserId = userId,
                Action = action,
                DetailsJson = detailsJson,
                WhenUtc = DateTime.UtcNow
            });

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Vrátí seznam položek, které nebyly viděny od zadaného času (pro případný housekeeping).
        /// </summary>
        public async Task<List<CalendarItem>> GetStaleItemsAsync(DateTime notSeenSinceUtc)
        {
            return await _context.CalendarItems
                .Where(ci => ci.LastSeenAtUtc < notSeenSinceUtc)
                .ToListAsync();
        }

        public async Task<CalendarItem?> GetByKeyAsync(string storeId, string entryId, DateTime? occurrenceStartUtc)
        {
            return await _context.CalendarItems
                .FirstOrDefaultAsync(ci =>
                    ci.StoreId == storeId &&
                    ci.EntryId == entryId &&
                    ci.OccurrenceStartUtc == occurrenceStartUtc);
        }

        public async Task<CalendarItem?> GetByIdAsync(int id)
        {
            return await _context.CalendarItems.FindAsync(id);
        }

        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetVisibleItemsForUserAsync(
    int userId,
    DateTime? fromUtc = null,
    DateTime? toUtc = null)
        {
            // Stav, které NEMÁME zobrazovat
            var hiddenStates = new[] { UserItemStateEnum.IgnoreTombstone, UserItemStateEnum.Written };

            var q = from ci in _context.CalendarItems
                    join uis in _context.UserItemStates.Where(x => x.UserId == userId)
                        on ci.Id equals uis.ItemId into gj
                    from uis in gj.DefaultIfEmpty()
                    where uis == null || !hiddenStates.Contains(uis.State)
                    select new { ci, state = (UserItemStateEnum?)uis.State };

            if (fromUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc <= toUtc.Value);

            var data = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            return data.Select(x => (x.ci, x.state)).ToList();
        }

        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetVisibleItemsForUserByAttendanceAsync(
     int userId,
     DateTime? fromUtc = null,
     DateTime? toUtc = null)
        {
            var hiddenStates = new[] { UserItemStateEnum.IgnoreTombstone, UserItemStateEnum.Written };

            var q =
                from ci in _context.CalendarItems

                    // stav pro daného usera (LEFT JOIN)
                join uisTmp in _context.UserItemStates.Where(x => x.UserId == userId)
                    on ci.Id equals uisTmp.ItemId into gj
                from uis in gj.DefaultIfEmpty()

                    // attendees (INNER JOIN) - jen eventy kde je tenhle user účastník
                join ia in _context.ItemAttendees
                    on ci.Id equals ia.ItemId

                where ia.UserId == userId
                   && (uis == null || !hiddenStates.Contains(uis.State))

                select new
                {
                    ci,
                    state = (UserItemStateEnum?)uis.State
                };

            if (fromUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc <= toUtc.Value);

            var rawData = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            // DEDUP
            var grouped = rawData
                .GroupBy(x =>
                {
                    var ci = x.ci;

                    string startKey = ci.StartUtc?.ToString("yyyy-MM-ddTHH:mm") ?? "nostart";
                    string endKey = ci.EndUtc?.ToString("yyyy-MM-ddTHH:mm") ?? "noend";

                    if (!string.IsNullOrEmpty(ci.GlobalAppointmentId))
                    {
                        return "GA:" + ci.GlobalAppointmentId + "|" + (ci.OccurrenceStartUtc?.ToString("o") ?? "noocc");
                    }
                    else
                    {
                        // fallback klíč
                        var subj = (ci.Subject ?? "").Trim();
                        return "FB:" + subj + "|" + startKey + "|" + endKey;
                    }
                })
                .Select(g =>
                {
                    // logika: vezmi ten, který má nejnovější LastModifiedUtc (pokud je null, ber min)
                    var best = g
                        .OrderByDescending(x => x.ci.LastModifiedUtc ?? DateTime.MinValue)
                        .First();

                    return (best.ci, best.state);
                })
                .OrderBy(x => x.ci.StartUtc ?? DateTime.MaxValue)
                .ToList();

            return grouped;
        }

        public sealed class CalendarItemKeyInfo
        {
            public int Id { get; set; }
            public string LastHash { get; set; }
        }

        public async Task<CalendarItemKeyInfo?> TryGetItemKeyInfoAsync(string storeId, string entryId, DateTime? occurrenceStartUtc)
        {
            return await _context.CalendarItems
                .Where(ci => ci.StoreId == storeId
                          && ci.EntryId == entryId
                          && ((ci.OccurrenceStartUtc == null && occurrenceStartUtc == null)
                               || (ci.OccurrenceStartUtc != null && occurrenceStartUtc != null
                                   && ci.OccurrenceStartUtc == occurrenceStartUtc)))
                .Select(ci => new CalendarItemKeyInfo
                {
                    Id = ci.Id,
                    LastHash = ci.LastHash
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}

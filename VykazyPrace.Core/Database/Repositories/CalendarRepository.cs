using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Models.OutlookEvents;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repozitář pro práci s kalendářovými položkami, stavy uživatelů, účastníky a logy.
    /// </summary>
    public class CalendarRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public CalendarRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Vloží/aktualizuje CalendarItem dle unikátního klíče.
        /// </summary>
        public async Task<CalendarItem> UpsertCalendarItemAsync(CalendarItem input)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.CalendarItems
                .FirstOrDefaultAsync(ci =>
                    ci.StoreId == input.StoreId &&
                    ci.EntryId == input.EntryId &&
                    ci.OccurrenceStartUtc == input.OccurrenceStartUtc);

            if (existing == null)
            {
                context.CalendarItems.Add(input);
                await VykazyPraceContextExtensions.SafeSaveAsync(context);
                return input;
            }

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

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return existing;
        }

        /// <summary>
        /// Nahraď účastníky položky.
        /// </summary>
        public async Task UpsertAttendeesAsync(int itemId, IEnumerable<ItemAttendee> attendees)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var oldAttendees = await context.ItemAttendees
                .Where(a => a.ItemId == itemId)
                .ToListAsync();

            context.ItemAttendees.RemoveRange(oldAttendees);

            foreach (var attendee in attendees)
            {
                attendee.ItemId = itemId;
                context.ItemAttendees.Add(attendee);
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(context);
        }

        /// <summary>
        /// Nastaví stav položky z pohledu uživatele.
        /// </summary>
        public async Task<UserItemState> SetUserStateAsync(
            int userId,
            int itemId,
            UserItemStateEnum state,
            string? note = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.UserItemStates
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

                context.UserItemStates.Add(existing);
            }
            else
            {
                existing.State = state;
                existing.Note = note;
                existing.UpdatedAtUtc = DateTime.UtcNow;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return existing;
        }

        /// <summary>
        /// Vrátí položky pro uživatele. Ve výchozím stavu skrývá IgnoreTombstone.
        /// </summary>
        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetItemsForUserAsync(
            int userId,
            bool includeIgnored = false,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var q =
                from ci in context.CalendarItems.AsNoTracking()
                join uis in context.UserItemStates
                        .AsNoTracking()
                        .Where(x => x.UserId == userId)
                    on ci.Id equals uis.ItemId into gj
                from uis in gj.DefaultIfEmpty()
                select new
                {
                    ci,
                    state = (UserItemStateEnum?)uis.State
                };

            if (!includeIgnored)
            {
                q = q.Where(x =>
                    x.state == null ||
                    x.state != UserItemStateEnum.IgnoreTombstone);
            }

            if (fromUtc.HasValue || toUtc.HasValue)
            {
                var from = fromUtc ?? DateTime.MinValue;
                var to = toUtc ?? DateTime.MaxValue;

                q = q.Where(x =>
                    x.ci.StartUtc == null ||
                    x.ci.EndUtc == null ||
                    (x.ci.StartUtc <= to && x.ci.EndUtc >= from));
            }

            var data = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            return data
                .Select(x => (x.ci, x.state))
                .ToList();
        }

        public async Task LogChangeAsync(
            int itemId,
            string action,
            int? userId = null,
            string? detailsJson = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.ItemChangeLogs.Add(new ItemChangeLog
            {
                ItemId = itemId,
                UserId = userId,
                Action = action,
                DetailsJson = detailsJson,
                WhenUtc = DateTime.UtcNow
            });

            await VykazyPraceContextExtensions.SafeSaveAsync(context);
        }

        /// <summary>
        /// Vrátí seznam položek, které nebyly viděny od zadaného času.
        /// </summary>
        public async Task<List<CalendarItem>> GetStaleItemsAsync(DateTime notSeenSinceUtc)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.CalendarItems
                .AsNoTracking()
                .Where(ci => ci.LastSeenAtUtc < notSeenSinceUtc)
                .ToListAsync();
        }

        public async Task<CalendarItem?> GetByKeyAsync(
            string storeId,
            string entryId,
            DateTime? occurrenceStartUtc)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.CalendarItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci =>
                    ci.StoreId == storeId &&
                    ci.EntryId == entryId &&
                    ci.OccurrenceStartUtc == occurrenceStartUtc);
        }

        public async Task<CalendarItem?> GetByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.CalendarItems
                .AsNoTracking()
                .FirstOrDefaultAsync(ci => ci.Id == id);
        }

        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetVisibleItemsForUserAsync(
            int userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var hiddenStates = new[]
            {
                UserItemStateEnum.IgnoreTombstone,
                UserItemStateEnum.Written
            };

            var q =
                from ci in context.CalendarItems.AsNoTracking()
                join uis in context.UserItemStates
                        .AsNoTracking()
                        .Where(x => x.UserId == userId)
                    on ci.Id equals uis.ItemId into gj
                from uis in gj.DefaultIfEmpty()
                where uis == null || !hiddenStates.Contains(uis.State)
                select new
                {
                    ci,
                    state = (UserItemStateEnum?)uis.State
                };

            if (fromUtc.HasValue)
            {
                q = q.Where(x =>
                    x.ci.StartUtc == null ||
                    x.ci.StartUtc >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                q = q.Where(x =>
                    x.ci.StartUtc == null ||
                    x.ci.StartUtc <= toUtc.Value);
            }

            var data = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            return data
                .Select(x => (x.ci, x.state))
                .ToList();
        }

        public async Task<List<(CalendarItem Item, UserItemStateEnum? State)>> GetVisibleItemsForUserByAttendanceAsync(
            int userId,
            DateTime? fromUtc = null,
            DateTime? toUtc = null)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var hiddenStates = new[]
            {
                UserItemStateEnum.IgnoreTombstone,
                UserItemStateEnum.Written
            };

            var q =
                from ci in context.CalendarItems.AsNoTracking()

                join uisTmp in context.UserItemStates
                        .AsNoTracking()
                        .Where(x => x.UserId == userId)
                    on ci.Id equals uisTmp.ItemId into gj
                from uis in gj.DefaultIfEmpty()

                join ia in context.ItemAttendees.AsNoTracking()
                    on ci.Id equals ia.ItemId

                where ia.UserId == userId &&
                      (uis == null || !hiddenStates.Contains(uis.State))

                select new
                {
                    ci,
                    state = (UserItemStateEnum?)uis.State
                };

            if (fromUtc.HasValue)
            {
                q = q.Where(x =>
                    x.ci.StartUtc == null ||
                    x.ci.StartUtc >= fromUtc.Value);
            }

            if (toUtc.HasValue)
            {
                q = q.Where(x =>
                    x.ci.StartUtc == null ||
                    x.ci.StartUtc <= toUtc.Value);
            }

            var rawData = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            var grouped = rawData
                .GroupBy(x =>
                {
                    var ci = x.ci;

                    var startKey = ci.StartUtc?.ToString("yyyy-MM-ddTHH:mm") ?? "nostart";
                    var endKey = ci.EndUtc?.ToString("yyyy-MM-ddTHH:mm") ?? "noend";

                    if (!string.IsNullOrEmpty(ci.GlobalAppointmentId))
                    {
                        return "GA:" + ci.GlobalAppointmentId + "|" +
                               (ci.OccurrenceStartUtc?.ToString("o") ?? "noocc");
                    }

                    var subject = (ci.Subject ?? string.Empty).Trim();
                    return "FB:" + subject + "|" + startKey + "|" + endKey;
                })
                .Select(g =>
                {
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
            public string? LastHash { get; set; }
        }

        public async Task<CalendarItemKeyInfo?> TryGetItemKeyInfoAsync(
            string storeId,
            string entryId,
            DateTime? occurrenceStartUtc)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.CalendarItems
                .AsNoTracking()
                .Where(ci =>
                    ci.StoreId == storeId &&
                    ci.EntryId == entryId &&
                    (
                        (ci.OccurrenceStartUtc == null && occurrenceStartUtc == null) ||
                        (ci.OccurrenceStartUtc != null &&
                         occurrenceStartUtc != null &&
                         ci.OccurrenceStartUtc == occurrenceStartUtc)
                    ))
                .Select(ci => new CalendarItemKeyInfo
                {
                    Id = ci.Id,
                    LastHash = ci.LastHash
                })
                .FirstOrDefaultAsync();
        }
    }
}
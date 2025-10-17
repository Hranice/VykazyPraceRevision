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

        // ------------------------------------------------------------
        // UPSERT položky podle (StoreId, EntryId, OccurrenceStartUtc)
        // ------------------------------------------------------------
        /// <summary>
        /// Vloží/aktualizuje CalendarItem dle unikátního klíče.
        /// Používej při každém syncu ze zdroje (Outlook Interop).
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
                // Přepíšeme „otisk“ a metadata – ponecháme Id
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

        // ------------------------------------------------------------
        // Účastníci – jednoduchá strategie „replace“
        // ------------------------------------------------------------
        /// <summary>
        /// Nahraď účastníky položky (smaž a vlož).
        /// Volat po každém updatu položky, ať je seznam konzistentní.
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

        // ------------------------------------------------------------
        // Stav uživatele (tombstone, skrytí, zapsáno…)
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // Výpis pro uživatele (s ignorací tombstonů)
        // ------------------------------------------------------------
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
                    select new { ci, state = (UserItemStateEnum?)uis.State };

            if (!includeIgnored)
            {
                q = q.Where(x => x.state == null || x.state != UserItemStateEnum.IgnoreTombstone);
            }

            if (fromUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc >= fromUtc.Value);

            if (toUtc.HasValue)
                q = q.Where(x => x.ci.StartUtc == null || x.ci.StartUtc <= toUtc.Value);

            var data = await q
                .OrderBy(x => x.ci.StartUtc)
                .ToListAsync();

            return data.Select(x => (x.ci, x.state)).ToList();
        }

        // ------------------------------------------------------------
        // Logování změn
        // ------------------------------------------------------------
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

        // ------------------------------------------------------------
        // „Stárnutí“ – označení/čištění záznamů, které jsme dlouho neviděli
        // ------------------------------------------------------------
        /// <summary>
        /// Vrátí seznam položek, které nebyly „viděny“ od zadaného času (pro případný housekeeping).
        /// Nic nemaže – mazání nech na UI/politice.
        /// </summary>
        public async Task<List<CalendarItem>> GetStaleItemsAsync(DateTime notSeenSinceUtc)
        {
            return await _context.CalendarItems
                .Where(ci => ci.LastSeenAtUtc < notSeenSinceUtc)
                .ToListAsync();
        }

        // ------------------------------------------------------------
        // Vyhledání konkrétní položky dle Outlook klíče
        // ------------------------------------------------------------
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

    }
}

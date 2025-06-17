using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging.VykazyPrace.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntryRepository
    {
        private readonly VykazyPraceContext _context;

        public TimeEntryRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Přidání nového časového záznamu.
        /// </summary>
        public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry timeEntry)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_PŘIDÁNÍ]: '{FormatHelper.FormatTimeEntryToString(timeEntry)}'.");
            _context.TimeEntries.Add(timeEntry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_PŘIDÁNÍ]: 'HOTOVO'.");
            return timeEntry;
        }

        /// <summary>
        /// Získání všech časových záznamů.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesAsync()
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY'.");
            var result = await _context.TimeEntries
                .Include(t => t.User)
                .ThenInclude(t => t.UserGroup)
                .Include(t => t.EntryType)
                .Include(t => t.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }


        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, bool includeSnacks = false)
        {
            List<TimeEntry> result = new List<TimeEntry>();

            if (includeSnacks)
            {
                AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY VČETNĚ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'.");
                result = await _context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.UserId == user.Id)
                    .Include(te => te.Project)
                    .ToListAsync();
            }

            else
            {
                AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY BEZ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'.");
                result = await _context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.UserId == user.Id &&
                                !(te.ProjectId == 132 && te.EntryTypeId == 24))
                    .Include(te => te.Project)
                    .ToListAsync();
            }

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele podle zakázky/projektu.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A TYPU PROJEKTU '{projectType}'.");
            var result = await _context.TimeEntries
                .Where(te => te.UserId == user.Id && te.Project != null && te.Project.ProjectType == projectType)
                .Include(te => te.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele, typ projektu a konkrétní den.
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByProjectTypeAndDateAsync(User user, int projectType, DateTime date)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A TYPU PROJEKTU '{projectType}' A KONKRÉTNÍ DEN '{date}'.");
            var result = await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Project.ProjectType == projectType
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele a konkrétní den.
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A KONKRÉTNÍ DEN '{date}'.");
            var result = await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }

        /// <summary>
        /// Získání všech časových záznamů mezi dvěma daty (včetně hran).
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesBetweenDatesAsync(DateTime fromDate, DateTime toDate)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY' PRO ČASOVÉ ROZMEZÍ '{fromDate}-{toDate}'.");
            var result = await _context.TimeEntries
                .Where(te => te.Timestamp.HasValue &&
                             te.Timestamp.Value.Date >= fromDate.Date &&
                             te.Timestamp.Value.Date <= toDate.Date)
                .Include(te => te.User)
                    .ThenInclude(u => u.UserGroup)
                .Include(te => te.EntryType)
                .Include(te => te.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }


        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele a týden zadaného data (po-ne).
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByUserAndCurrentWeekAsync(User user, DateTime date)
        {
            var startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek + (date.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var endOfWeek = startOfWeek.AddDays(6);
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A ČASOVÉ ROZMEZÍ '{startOfWeek}-{endOfWeek}'.");

            var result = await _context.TimeEntries
                .AsNoTracking()
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date >= startOfWeek
                             && te.Timestamp.Value.Date <= endOfWeek)
                .Include(te => te.Project)
                .ToListAsync();

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO {result.Count} ZÁZNAMŮ'.");
            return result;
        }


        /// <summary>
        /// Získání součtu odpracovaných minut pro konkrétního uživatele a zvolený den.
        /// </summary>
        public async Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
        {
            DateTime today = DateTime.Today;
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A KONKRÉTNÍ DEN '{date}'.");

            var result = await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .SumAsync(te => te.EntryMinutes);

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'ODPRACOVÁNO: {result} MINUT'.");
            return result;
        }

        /// <summary>
        /// Získání součtu odpracovaných minut pro konkrétního uživatele, zvolený den a projekt.
        /// </summary>
        public async Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date, int projectType)
        {
            DateTime today = DateTime.Today;
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A KONKRÉTNÍ DEN '{date}' A TYP PROJEKTU '{projectType}'.");

            var result = await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Project.ProjectType == projectType
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .SumAsync(te => te.EntryMinutes);

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'ODPRACOVÁNO: {result} MINUT'.");
            return result;
        }


        /// <summary>
        /// Získání časového záznamu podle ID, nezávisle na datu.
        /// </summary>
        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'KONKRÉTNÍ' PODLE ID '{id}'.");
            var result = await _context.TimeEntries
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'VRÁCENO: {FormatHelper.FormatTimeEntryToString(result)}'.");
            return result;
        }


        /// <summary>
        /// Aktualizace časového záznamu.
        /// </summary>
        public async Task<bool> UpdateTimeEntryAsync(TimeEntry? timeEntry)
        {
            var existingEntry = await _context.TimeEntries.FindAsync(timeEntry.Id);
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: '{FormatHelper.FormatTimeEntryToString(existingEntry)}' NA '{FormatHelper.FormatTimeEntryToString(timeEntry)}'.");

            if (existingEntry == null)
                return false;

            existingEntry.EntryTypeId = timeEntry.EntryTypeId;
            existingEntry.Description = timeEntry.Description;
            existingEntry.EntryMinutes = timeEntry.EntryMinutes;
            existingEntry.Timestamp = timeEntry.Timestamp;
            existingEntry.UserId = timeEntry.UserId;
            existingEntry.ProjectId = timeEntry.ProjectId;
            existingEntry.AfterCare = timeEntry.AfterCare;
            existingEntry.IsValid = timeEntry.IsValid;
            existingEntry.Note = timeEntry.Note;

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'HOTOVO'.");
            return true;
        }

        public async Task<int> UpdateUnlockedDescriptionsForUserAsync(int userId, string oldDescription, string newDescription)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'VŠECHNY ODEMČENÉ' PRO UŽIVATELE S ID '{userId}'.");
            var entriesToUpdate = await _context.TimeEntries
                .Where(e => e.UserId == userId &&
                            e.IsLocked == 0 &&
                            e.Description == oldDescription)
                .ToListAsync();

            foreach (var entry in entriesToUpdate)
            {
                AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'POPIS' Z '{entry.Description}' NA '{newDescription}'.");
                entry.Description = newDescription;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'HOTOVO' 'UPRAVENO: {entriesToUpdate.Count} ZÁZNAMŮ'.");

            return entriesToUpdate.Count;
        }


        public async Task<bool> ExistsEntryAsync(int userId, DateTime day, int projectId, int entryTypeId)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'EXISTUJE?' PRO UŽIVATELE S ID '{userId}' A KONKRÉTNÍ DEN '{day}' A ID PROJEKTU '{projectId}' A ID TYPU ZÁZNAMU '{entryTypeId}'.");
            bool result = await _context.TimeEntries.AnyAsync(e =>
                e.UserId == userId &&
                e.Timestamp.HasValue &&
                e.Timestamp.Value.Date == day.Date &&
                e.ProjectId == projectId &&
                e.EntryTypeId == entryTypeId);

            AppLogger.Debug($"[ČAS.ZÁZNAM_ZÍSKÁNÍ]: 'HOTOVO' 'EXISTUJE: {result}'.");
            return result;
        }



        /// <summary>
        /// Smazání časového záznamu podle ID.
        /// </summary>
        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_SMAZÁNÍ]: 'SMAZAT' PRO ID '{id}'.");
            var entry = await _context.TimeEntries.FindAsync(id);
            if (entry == null)
            {
                AppLogger.Debug($"[ČAS.ZÁZNAM_SMAZÁNÍ]: 'HOTOVO' 'ÚSPĚCH: NE'.");
                return false;
            }

            _context.TimeEntries.Remove(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_SMAZÁNÍ]: 'HOTOVO' 'ÚSPĚCH: ANO'.");
            return true;
        }

        /// <summary>
        /// Získání součtu odpracovaných hodin pro každého uživatele a projekt v daném časovém rozsahu.
        /// </summary>
        public async Task<List<TimeEntrySummary>> GetTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.TimeEntries
                .Where(te => te.Timestamp.HasValue
                             && te.Timestamp.Value.Date >= fromDate.Date
                             && te.Timestamp.Value.Date <= toDate.Date
                             && !(te.ProjectId == 132 && te.EntryTypeId == 24))
                .GroupBy(te => new { te.UserId, te.ProjectId })
                .Select(g => new TimeEntrySummary
                {
                    UserId = g.Key.UserId,
                    ProjectId = g.Key.ProjectId,
                    TotalHours = g.Sum(te => te.EntryMinutes) / 60.0
                })
                .ToListAsync();
        }


        public async Task LockAllEntriesInMonth(string month)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'ZAMKNOUT' PRO MĚSÍC '{month}'.");
            var monthNumber = month switch
            {
                "Leden" => 1,
                "Únor" => 2,
                "Březen" => 3,
                "Duben" => 4,
                "Květen" => 5,
                "Červen" => 6,
                "Červenec" => 7,
                "Srpen" => 8,
                "Září" => 9,
                "Říjen" => 10,
                "Listopad" => 11,
                "Prosinec" => 12,
                _ => throw new ArgumentException("Neplatný měsíc: " + month)
            };

            var entries = await _context.TimeEntries
                .Where(e => e.Timestamp.HasValue &&
                            e.Timestamp.Value.Month == monthNumber)
                .ToListAsync();

            foreach (var entry in entries)
            {
                entry.IsLocked = 1;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'HOTOVO'.");
        }

        /// <summary>
        /// Nahradí všechny záznamy s daným původním ID projektu novým ID projektu.
        /// </summary>
        /// <param name="oldProjectId">Původní ID projektu.</param>
        /// <param name="newProjectId">Nové ID projektu.</param>
        /// <returns>Počet upravených záznamů.</returns>
        public async Task<int> UpdateProjectIdForEntriesAsync(int oldProjectId, int newProjectId)
        {
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'AKTUALIZACE ID PROJEKTU' PRO ID PROJEKTU '{oldProjectId}' NA '{newProjectId}'.");
            var entriesToUpdate = await _context.TimeEntries
                .Where(e => e.ProjectId == oldProjectId)
                .ToListAsync();

            foreach (var entry in entriesToUpdate)
            {
                entry.ProjectId = newProjectId;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            AppLogger.Debug($"[ČAS.ZÁZNAM_AKTUALIZACE]: 'HOTOVO' 'UPRAVENO {entriesToUpdate.Count} ZÁZNAMŮ'.");
            return entriesToUpdate.Count;
        }


        /// <summary>
        /// Pomocná třída pro souhrnné záznamy.
        /// </summary>
        public class TimeEntrySummary
        {
            public int? UserId { get; set; }
            public int? ProjectId { get; set; }
            public double TotalHours { get; set; }
        }
    }
}

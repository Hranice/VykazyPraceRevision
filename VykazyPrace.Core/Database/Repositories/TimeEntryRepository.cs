using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using VykazyPrace.Core.Database.Models;

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
            _context.TimeEntries.Add(timeEntry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return timeEntry;
        }

        /// <summary>
        /// Získání všech časových záznamů.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesAsync()
        {
            return await _context.TimeEntries
                .Include(t => t.User)
                .ThenInclude(t => t.UserGroup)
                .Include(t => t.EntryType)
                .Include(t => t.Project)
                .ToListAsync();
        }


        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, bool includeSnacks = false)
        {
            if (includeSnacks)
            {
                return await _context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.UserId == user.Id)
                    .Include(te => te.Project)
                    .ToListAsync();
            }

            else
            {
                return await _context.TimeEntries
                    .AsNoTracking()
                    .Where(te => te.UserId == user.Id &&
                                !(te.ProjectId == 132 && te.EntryTypeId == 24))
                    .Include(te => te.Project)
                    .ToListAsync();

            }
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele podle zakázky/projektu.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
        {
            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id && te.Project != null && te.Project.ProjectType == projectType)
                .Include(te => te.Project)
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele, typ projektu a konkrétní den.
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByProjectTypeAndDateAsync(User user, int projectType, DateTime date)
        {
            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Project.ProjectType == projectType
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele a konkrétní den.
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
        {
            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele a týden zadaného data (po-ne).
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByUserAndCurrentWeekAsync(User user, DateTime date)
        {
            var startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek + (date.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var endOfWeek = startOfWeek.AddDays(6);

            return await _context.TimeEntries
                .AsNoTracking()
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date >= startOfWeek
                             && te.Timestamp.Value.Date <= endOfWeek)
                .Include(te => te.Project)
                .ToListAsync();
        }


        /// <summary>
        /// Získání součtu odpracovaných minut pro konkrétního uživatele a zvolený den.
        /// </summary>
        public async Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
        {
            DateTime today = DateTime.Today;

            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .SumAsync(te => te.EntryMinutes);
        }

        /// <summary>
        /// Získání součtu odpracovaných minut pro konkrétního uživatele, zvolený den a projekt.
        /// </summary>
        public async Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date, int projectType)
        {
            DateTime today = DateTime.Today;

            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Project.ProjectType == projectType
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .SumAsync(te => te.EntryMinutes);
        }


        /// <summary>
        /// Získání časového záznamu podle ID, nezávisle na datu.
        /// </summary>
        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            return await _context.TimeEntries
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
        }


        /// <summary>
        /// Aktualizace časového záznamu.
        /// </summary>
        public async Task<bool> UpdateTimeEntryAsync(TimeEntry? timeEntry)
        {
            var existingEntry = await _context.TimeEntries.FindAsync(timeEntry.Id);
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
            return true;
        }

        /// <summary>
        /// Smazání časového záznamu podle ID.
        /// </summary>
        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            var entry = await _context.TimeEntries.FindAsync(id);
            if (entry == null)
                return false;

            _context.TimeEntries.Remove(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
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

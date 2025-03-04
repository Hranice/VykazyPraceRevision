using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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
            await _context.SaveChangesAsync();
            return timeEntry;
        }

        /// <summary>
        /// Získání všech časových záznamů.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesAsync()
        {
            return await _context.TimeEntries.Include(t => t.User).Include(t => t.Project).ToListAsync();
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele.
        /// </summary>
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user)
        {
            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id)
                .Include(te => te.Project)
                .ToListAsync();
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
        /// Získání časového záznamu podle ID.
        /// </summary>
        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            return await _context.TimeEntries
                .Include(t => t.User)
                .Include(t => t.Project)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        /// <summary>
        /// Aktualizace časového záznamu.
        /// </summary>
        public async Task<bool> UpdateTimeEntryAsync(TimeEntry timeEntry)
        {
            var existingEntry = await _context.TimeEntries.FindAsync(timeEntry.Id);
            if (existingEntry == null)
                return false;

            existingEntry.Description = timeEntry.Description;
            existingEntry.EntryMinutes = timeEntry.EntryMinutes;
            existingEntry.Timestamp = timeEntry.Timestamp;
            existingEntry.UserId = timeEntry.UserId;
            existingEntry.ProjectId = timeEntry.ProjectId;

            await _context.SaveChangesAsync();
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
            await _context.SaveChangesAsync();
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
                             && te.Timestamp.Value.Date <= toDate.Date)
                .GroupBy(te => new { te.UserId, te.ProjectId })
                .Select(g => new TimeEntrySummary
                {
                    UserId = g.Key.UserId,
                    ProjectId = g.Key.ProjectId,
                    TotalHours = g.Sum(te => te.EntryMinutes) / 60.0
                })
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech typů časových záznamů.
        /// </summary>
        public async Task<List<TimeEntryType>> GetAllTimeEntryTypesAsync()
        {
            return await _context.TimeEntryTypes.ToListAsync();
        }
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

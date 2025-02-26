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
    }
}

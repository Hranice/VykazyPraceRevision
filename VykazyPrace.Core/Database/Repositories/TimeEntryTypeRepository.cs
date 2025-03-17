using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntryTypeRepository
    {
        private readonly VykazyPraceContext _context;

        public TimeEntryTypeRepository()
        {
            _context = new VykazyPraceContext();
        }

/// <summary>
        /// Vytvoří nový typ časového záznamu, pokud ještě neexistuje.
        /// </summary>
        public async Task<TimeEntryType?> CreateTimeEntryTypeAsync(TimeEntryType timeEntryType)
        {
            var existingEntry = await _context.TimeEntryTypes.FirstOrDefaultAsync(t => t.Title == timeEntryType.Title);

            if (existingEntry != null)
            {
                return existingEntry;
            }

            _context.TimeEntryTypes.Add(timeEntryType);
            await _context.SaveChangesAsync();
            return timeEntryType;
        }

        public async Task<List<TimeEntryType>> GetAllTimeEntryTypesAsync()
        {
            return await _context.TimeEntryTypes.ToListAsync();
        }

        public async Task<TimeEntryType?> GetTimeEntryTypeByIdAsync(int id)
        {
            return await _context.TimeEntryTypes.FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> UpdateTimeEntryTypeAsync(TimeEntryType type)
        {
            var existingType = await _context.TimeEntryTypes.FindAsync(type.Id);
            if (existingType == null) return false;

            existingType.Title = type.Title;
            existingType.Color = type.Color;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTimeEntryTypeAsync(int id)
        {
            var type = await _context.TimeEntryTypes.FindAsync(id);
            if (type == null) return false;

            _context.TimeEntryTypes.Remove(type);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

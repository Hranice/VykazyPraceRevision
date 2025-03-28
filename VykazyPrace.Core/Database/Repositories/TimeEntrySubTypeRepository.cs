using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntrySubTypeRepository
    {
        private readonly VykazyPraceContext _context;

        public TimeEntrySubTypeRepository()
        {
            _context = new VykazyPraceContext();
        }

        public async Task<TimeEntrySubType> CreateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            var existingEntry = await _context.TimeEntrySubTypes.FirstOrDefaultAsync(t => t.Title == subType.Title && t.UserId == subType.UserId);

            if (existingEntry != null)
            {
                return existingEntry;
            }

            _context.TimeEntrySubTypes.Add(subType);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return subType;
        }

        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesAsync()
        {
            return await _context.TimeEntrySubTypes.Include(t => t.User).ToListAsync();
        }

        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesByUserIdAsync(int userId)
        {
            return await _context.TimeEntrySubTypes
                .Where(t => t.UserId == userId)
                .Include(t => t.User)
                .ToListAsync();
        }


        public async Task<TimeEntrySubType?> GetTimeEntrySubTypeByIdAsync(int id)
        {
            return await _context.TimeEntrySubTypes.Include(t => t.User).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> UpdateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            var existingSubType = await _context.TimeEntrySubTypes.FindAsync(subType.Id);
            if (existingSubType == null) return false;

            existingSubType.Title = subType.Title;
            existingSubType.UserId = subType.UserId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTimeEntrySubTypeAsync(int id)
        {
            var subType = await _context.TimeEntrySubTypes.FindAsync(id);
            if (subType == null) return false;

            _context.TimeEntrySubTypes.Remove(subType);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }
    }
}

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
            _context.TimeEntrySubTypes.Add(subType);
            await _context.SaveChangesAsync();
            return subType;
        }

        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesAsync()
        {
            return await _context.TimeEntrySubTypes.Include(t => t.Group).ToListAsync();
        }

        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesByGroupIdAsync(int groupId)
        {
            return await _context.TimeEntrySubTypes
                .Where(t => t.GroupId == groupId)
                .Include(t => t.Group)
                .ToListAsync();
        }


        public async Task<TimeEntrySubType?> GetTimeEntrySubTypeByIdAsync(int id)
        {
            return await _context.TimeEntrySubTypes.Include(t => t.Group).FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<bool> UpdateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            var existingSubType = await _context.TimeEntrySubTypes.FindAsync(subType.Id);
            if (existingSubType == null) return false;

            existingSubType.Title = subType.Title;
            existingSubType.GroupId = subType.GroupId;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTimeEntrySubTypeAsync(int id)
        {
            var subType = await _context.TimeEntrySubTypes.FindAsync(id);
            if (subType == null) return false;

            _context.TimeEntrySubTypes.Remove(subType);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

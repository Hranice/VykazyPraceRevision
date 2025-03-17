using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class UserGroupRepository
    {
        private readonly VykazyPraceContext _context;

        public UserGroupRepository()
        {
            _context = new VykazyPraceContext();
        }

        public async Task<UserGroup> CreateUserGroupAsync(UserGroup userGroup)
        {
            _context.UserGroups.Add(userGroup);
            await _context.SaveChangesAsync();
            return userGroup;
        }

        public async Task<List<UserGroup>> GetAllUserGroupsAsync()
        {
            return await _context.UserGroups.Include(ug => ug.TimeEntrySubTypes).ToListAsync();
        }

        public async Task<UserGroup?> GetUserGroupByIdAsync(int id)
        {
            return await _context.UserGroups
                .Include(ug => ug.TimeEntrySubTypes)
                .FirstOrDefaultAsync(ug => ug.Id == id);
        }

        public async Task<bool> UpdateUserGroupAsync(UserGroup userGroup)
        {
            var existingGroup = await _context.UserGroups.FindAsync(userGroup.Id);
            if (existingGroup == null) return false;

            existingGroup.Title = userGroup.Title;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteUserGroupAsync(int id)
        {
            var userGroup = await _context.UserGroups.FindAsync(id);
            if (userGroup == null) return false;

            _context.UserGroups.Remove(userGroup);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}

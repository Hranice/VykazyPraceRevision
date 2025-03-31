using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class UserRepository
    {
        private readonly VykazyPraceContext _context;

        public UserRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Přidání nového uživatele.
        /// </summary>
        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return user;
        }

        /// <summary>
        /// Získání všech uživatelů.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .OrderBy(u => u.UserGroupId)
                .ToListAsync();
        }


        /// <summary>
        /// Získání uživatele podle ID.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Získání uživatele podle přihlašovacího jména do Windows.
        /// </summary>
        public async Task<User?> GetUserByWindowsUsernameAsync(string windowsUsername)
        {
            return await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.WindowsUsername == windowsUsername);
        }

        /// <summary>
        /// Aktualizace uživatele.
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return false;

            existingUser.FirstName = user.FirstName;
            existingUser.Surname = user.Surname;
            existingUser.PersonalNumber = user.PersonalNumber;
            existingUser.WindowsUsername = user.WindowsUsername;
            existingUser.LevelOfAccess = user.LevelOfAccess;

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }

        /// <summary>
        /// Smazání uživatele podle ID.
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }
    }
}

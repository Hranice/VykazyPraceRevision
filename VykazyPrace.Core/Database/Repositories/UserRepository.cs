using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
        public async Task<SaveResult> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Získání všech uživatelů.
        /// </summary>
        public async Task<(bool Success, List<User>? Users, string? Error)> GetAllUsersAsync()
        {
            try
            {
                var users = await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .OrderBy(u => u.UserGroupId)
                .ToListAsync();

                return (true, users, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }


        /// <summary>
        /// Získání uživatele podle ID.
        /// </summary>
        public async Task<(bool Success, User? User, string? Error)> GetUserByIdAsync(int id)
        {
            try
            {
                var user = await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.Id == id);

                return (true, user, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Získání uživatele podle přihlašovacího jména do Windows.
        /// </summary>
        public async Task<(bool Success, User? User, string? Error)> GetUserByWindowsUsernameAsync(string windowsUsername)
        {
            try
            {
                var user = await _context.Users
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.WindowsUsername == windowsUsername);

                return (true, user, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Aktualizace uživatele.
        /// </summary>
        public async Task<SaveResult> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
                return new SaveResult(false, "Uživatel neexistuje.", 0);

            existingUser.FirstName = user.FirstName;
            existingUser.Surname = user.Surname;
            existingUser.PersonalNumber = user.PersonalNumber;
            existingUser.WindowsUsername = user.WindowsUsername;
            existingUser.LevelOfAccess = user.LevelOfAccess;

            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Smazání uživatele podle ID.
        /// </summary>
        public async Task<SaveResult> DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
                return new SaveResult(false, "Uživatel neexistuje.", 0);

            _context.Users.Remove(user);
            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }
    }
}

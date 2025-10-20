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

        /// <summary>
        /// Najde uživatele podle e-mailu
        /// </summary>
        public async Task<User?> ResolveByEmailOrWindowsAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            var e = email.Trim();
            var eLower = e.ToLowerInvariant();

            var byEmail = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Email != null && u.Email.ToLower() == eLower);
            if (byEmail != null) return byEmail;

            // rozpad emailu
            var atIdx = e.IndexOf('@');
            var local = atIdx > 0 ? e.Substring(0, atIdx) : e;
            var domain = atIdx > 0 ? e.Substring(atIdx + 1) : null;
            var domainUpper = domain?.ToUpperInvariant();
            var domainLower = domain?.ToLowerInvariant();

            var byUpn = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.WindowsUsername.ToLower() == eLower);
            if (byUpn != null) return byUpn;

            var byLocal = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.WindowsUsername.ToLower() == local.ToLowerInvariant());
            if (byLocal != null) return byLocal;

            if (!string.IsNullOrEmpty(local) && !string.IsNullOrEmpty(domain))
            {
                var domLocalUpper = $"{domainUpper}\\{local}";
                var byDomLocalUpper = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.WindowsUsername.ToUpper() == domLocalUpper);
                if (byDomLocalUpper != null) return byDomLocalUpper;

                var domLocalLower = $"{domainLower}\\{local}";
                var byDomLocalLower = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.WindowsUsername.ToLower() == domLocalLower);
                if (byDomLocalLower != null) return byDomLocalLower;
            }

            return null;
        }
    }
}

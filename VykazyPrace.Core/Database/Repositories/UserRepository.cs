using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class UserRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public UserRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// P�id�n� nov�ho u�ivatele.
        /// </summary>
        public async Task<User> CreateUserAsync(User user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.Users.Add(user);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return user;
        }

        /// <summary>
        /// Z�sk�n� v�ech u�ivatel�.
        /// </summary>
        public async Task<List<User>> GetAllUsersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .AsNoTracking()
                .Include(u => u.UserGroup)
                .OrderBy(u => u.UserGroupId)
                .ToListAsync();
        }
        /// <summary>
        /// Z�sk�n� aktivn�ch u�ivatel� pro b�n� v�b�ry.
        /// </summary>
        public async Task<List<User>> GetActiveUsersAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .AsNoTracking()
                .Include(u => u.UserGroup)
                .Where(u => !u.IsArchived)
                .OrderBy(u => u.UserGroupId)
                .ToListAsync();
        }

        /// <summary>
        /// Z�sk�n� u�ivatel�, kte�� maj� �asov� z�znamy v dan�m obdob�.
        /// </summary>
        public async Task<List<User>> GetUsersWithTimeEntriesAsync(DateTime fromDate, DateTime toDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var fromInclusive = fromDate.Date;
            var toInclusive = toDate.Date;

            if (toInclusive < fromInclusive)
                throw new ArgumentException("Datum do nesm� b�t men�� ne� datum od.");

            var toExclusive = toInclusive.AddDays(1);

            return await context.Users
                .AsNoTracking()
                .Include(u => u.UserGroup)
                .Where(u => context.TimeEntries.Any(te =>
                    te.UserId == u.Id &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= fromInclusive &&
                    te.Timestamp.Value < toExclusive))
                .OrderBy(u => u.UserGroupId)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToListAsync();
        }


        /// <summary>
        /// Z�sk�n� u�ivatele podle ID.
        /// </summary>
        public async Task<User?> GetUserByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .AsNoTracking()
                .Include(u => u.Projects)
                .Include(u => u.TimeEntries)
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        /// <summary>
        /// Z�sk�n� u�ivatele podle p�ihla�ovac�ho jm�na do Windows.
        /// </summary>
        public async Task<User?> GetUserByWindowsUsernameAsync(string windowsUsername)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Users
                .AsNoTracking()
                .Include(u => u.UserGroup)
                .FirstOrDefaultAsync(u => u.WindowsUsername == windowsUsername);
        }

        /// <summary>
        /// Aktualizace u�ivatele.
        /// </summary>
        public async Task<bool> UpdateUserAsync(User user)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingUser = await context.Users.FindAsync(user.Id);

            if (existingUser == null)
                return false;

            existingUser.FirstName = user.FirstName;
            existingUser.Surname = user.Surname;
            existingUser.PersonalNumber = user.PersonalNumber;
            existingUser.WindowsUsername = user.WindowsUsername;
            existingUser.LevelOfAccess = user.LevelOfAccess;
            existingUser.UserGroupId = user.UserGroupId;
            existingUser.Email = user.Email;
            existingUser.MasterUserId = user.MasterUserId;
            existingUser.IsArchived = user.IsArchived;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Smaz�n� u�ivatele podle ID.
        /// </summary>
        public async Task<bool> DeleteUserAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(id);

            if (user == null)
                return false;

            context.Users.Remove(user);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Najde u�ivatele podle e-mailu nebo Windows username.
        /// </summary>
        public async Task<User?> ResolveByEmailOrWindowsAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return null;

            await using var context = await _contextFactory.CreateDbContextAsync();

            var normalizedEmail = email.Trim();
            var normalizedEmailLower = normalizedEmail.ToLowerInvariant();

            var byEmail = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.Email != null &&
                    u.Email.ToLower() == normalizedEmailLower);

            if (byEmail != null)
                return byEmail;

            var atIndex = normalizedEmail.IndexOf('@');

            var local = atIndex > 0
                ? normalizedEmail[..atIndex]
                : normalizedEmail;

            var domain = atIndex > 0
                ? normalizedEmail[(atIndex + 1)..]
                : null;

            var localLower = local.ToLowerInvariant();

            var byUpn = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.WindowsUsername != null &&
                    u.WindowsUsername.ToLower() == normalizedEmailLower);

            if (byUpn != null)
                return byUpn;

            var byLocal = await context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u =>
                    u.WindowsUsername != null &&
                    u.WindowsUsername.ToLower() == localLower);

            if (byLocal != null)
                return byLocal;

            if (!string.IsNullOrWhiteSpace(local) && !string.IsNullOrWhiteSpace(domain))
            {
                var domainUpper = domain.ToUpperInvariant();
                var domainLower = domain.ToLowerInvariant();

                var domainLocalUpper = $"{domainUpper}\\{local}";
                var domainLocalLower = $"{domainLower}\\{localLower}";

                var byDomainLocalUpper = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.WindowsUsername != null &&
                        u.WindowsUsername.ToUpper() == domainLocalUpper);

                if (byDomainLocalUpper != null)
                    return byDomainLocalUpper;

                var byDomainLocalLower = await context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u =>
                        u.WindowsUsername != null &&
                        u.WindowsUsername.ToLower() == domainLocalLower);

                if (byDomainLocalLower != null)
                    return byDomainLocalLower;
            }

            return null;
        }
    }
}
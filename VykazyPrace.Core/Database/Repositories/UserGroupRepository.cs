using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class UserGroupRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public UserGroupRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<UserGroup> CreateUserGroupAsync(UserGroup userGroup)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.UserGroups.Add(userGroup);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return userGroup;
        }

        public async Task<List<UserGroup>> GetAllUserGroupsAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.UserGroups
                .AsNoTracking()
                .Include(g => g.Users)
                .OrderBy(g => g.Title)
                .ToListAsync();
        }

        public async Task<UserGroup?> GetUserGroupByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.UserGroups
                .AsNoTracking()
                .FirstOrDefaultAsync(ug => ug.Id == id);
        }

        public async Task<bool> UpdateUserGroupAsync(UserGroup userGroup)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingGroup = await context.UserGroups.FindAsync(userGroup.Id);

            if (existingGroup == null)
                return false;

            existingGroup.Title = userGroup.Title;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        public async Task<bool> DeleteUserGroupAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var userGroup = await context.UserGroups.FindAsync(id);

            if (userGroup == null)
                return false;

            context.UserGroups.Remove(userGroup);

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }
    }
}
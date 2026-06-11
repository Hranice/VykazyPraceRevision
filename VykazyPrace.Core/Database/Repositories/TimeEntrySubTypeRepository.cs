using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="TimeEntrySubType"/> entities.
    /// Provides methods for CRUD operations and retrieval filtered by user.
    /// </summary>
    public class TimeEntrySubTypeRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public TimeEntrySubTypeRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Helpers

        private void Log(string action, string message)
            => AppLogger.Debug($"[PODTYPZÁZNAMU_{action}]: {message}");

        private static IQueryable<TimeEntrySubType> BaseQuery(
            VykazyPraceContext context,
            bool noTracking = false)
        {
            var query = context.TimeEntrySubTypes.AsQueryable();

            if (noTracking)
                query = query.AsNoTracking();

            return query.Include(st => st.User);
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new <see cref="TimeEntrySubType"/>, or returns existing non-archived entry if duplicate.
        /// </summary>
        public async Task<TimeEntrySubType> CreateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("PŘIDÁNÍ", $"'{subType.Title}' for user {subType.UserId}.");

            var existing = await BaseQuery(context, noTracking: true)
                .FirstOrDefaultAsync(t =>
                    t.Title == subType.Title &&
                    t.UserId == subType.UserId);

            if (existing != null && existing.IsArchived == 0)
            {
                Log("PŘIDÁNÍ", $"Exists non-archived with ID {existing.Id}.");
                return existing;
            }

            context.TimeEntrySubTypes.Add(subType);

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("PŘIDÁNÍ", $"Created with ID {subType.Id}.");

            return subType;
        }

        /// <summary>
        /// Retrieves all <see cref="TimeEntrySubType"/> entries.
        /// </summary>
        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", "All subtypes.");

            var list = await BaseQuery(context, noTracking: true)
                .ToListAsync();

            Log("ZÍSKÁNÍ", $"Returned {list.Count} items.");

            return list;
        }

        /// <summary>
        /// Retrieves unique non-archived <see cref="TimeEntrySubType"/> titles for a user, ordered by custom order.
        /// </summary>
        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesByUserIdAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", $"User {userId} non-archived unique subtypes.");

            var uniqueIds = await BaseQuery(context, noTracking: true)
                .Where(t =>
                    t.UserId == userId &&
                    t.IsArchived == 0)
                .GroupBy(t => t.Title)
                .Select(g => g.Min(t => t.Id))
                .ToListAsync();

            var list = await BaseQuery(context, noTracking: true)
                .Where(t => uniqueIds.Contains(t.Id))
                .OrderBy(t => t.Order == null)
                .ThenBy(t => t.Order)
                .ToListAsync();

            Log("ZÍSKÁNÍ", $"Returned {list.Count} items.");

            return list;
        }

        /// <summary>
        /// Retrieves a <see cref="TimeEntrySubType"/> by its ID.
        /// </summary>
        public async Task<TimeEntrySubType?> GetTimeEntrySubTypeByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", $"ID {id}.");

            var item = await BaseQuery(context, noTracking: true)
                .FirstOrDefaultAsync(t => t.Id == id);

            Log("ZÍSKÁNÍ", item != null ? $"Found {item.Title}." : "Not found.");

            return item;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntrySubType"/>.
        /// </summary>
        public async Task<bool> UpdateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.TimeEntrySubTypes.FindAsync(subType.Id);

            if (existing == null)
            {
                Log("AKTUALIZACE", $"ID {subType.Id} not found.");
                return false;
            }

            Log("AKTUALIZACE", $"Updating ID {subType.Id}.");

            existing.Title = subType.Title;
            existing.UserId = subType.UserId;
            existing.Order = subType.Order;
            existing.IsArchived = subType.IsArchived;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("AKTUALIZACE", "Completed.");

            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntrySubType"/> by ID.
        /// </summary>
        public async Task<bool> DeleteTimeEntrySubTypeAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("SMAZÁNÍ", $"ID {id}.");

            var existing = await context.TimeEntrySubTypes.FindAsync(id);

            if (existing == null)
            {
                Log("SMAZÁNÍ", "Not found.");
                return false;
            }

            context.TimeEntrySubTypes.Remove(existing);

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("SMAZÁNÍ", "Completed.");

            return true;
        }

        #endregion
    }
}
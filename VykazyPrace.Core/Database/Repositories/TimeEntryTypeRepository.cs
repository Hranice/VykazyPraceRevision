using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="TimeEntryType"/> entities.
    /// Provides CRUD operations and retrieval by project type.
    /// </summary>
    public class TimeEntryTypeRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public TimeEntryTypeRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Helpers

        private void Log(string action, string message)
            => AppLogger.Debug($"[TYPZÁZNAMU_{action}]: {message}");

        private static IQueryable<TimeEntryType> BaseQuery(
            VykazyPraceContext context,
            bool noTracking = false)
        {
            var query = context.TimeEntryTypes.AsQueryable();

            return noTracking
                ? query.AsNoTracking()
                : query;
        }

        private async Task<List<TimeEntryType>> FetchAsync(
            string descriptor,
            Func<IQueryable<TimeEntryType>, IQueryable<TimeEntryType>> applyFilter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", descriptor);

            var list = await applyFilter(BaseQuery(context, noTracking: true))
                .SafeToListAsync();

            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {list.Count} TYPŮ");

            return list;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new <see cref="TimeEntryType"/>, or returns existing if duplicate.
        /// </summary>
        public async Task<TimeEntryType?> CreateTimeEntryTypeAsync(TimeEntryType timeEntryType)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("PŘIDÁNÍ", $"'{timeEntryType.Title}' for project type {timeEntryType.ForProjectType}");

            var existing = await BaseQuery(context, noTracking: true)
                .SafeFirstOrDefaultAsync(t =>
                    t.Title == timeEntryType.Title &&
                    t.ForProjectType == timeEntryType.ForProjectType);

            if (existing != null)
            {
                Log("PŘIDÁNÍ", $"Exists with ID {existing.Id}");
                return existing;
            }

            context.TimeEntryTypes.Add(timeEntryType);

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("PŘIDÁNÍ", $"Created with ID {timeEntryType.Id}");

            return timeEntryType;
        }

        /// <summary>
        /// Retrieves all <see cref="TimeEntryType"/> entries.
        /// </summary>
        public Task<List<TimeEntryType>> GetAllTimeEntryTypesAsync()
            => FetchAsync("VŠECHNY", q => q);

        /// <summary>
        /// Retrieves all <see cref="TimeEntryType"/> entries filtered by project type.
        /// </summary>
        public Task<List<TimeEntryType>> GetAllTimeEntryTypesByProjectTypeAsync(int projectType)
            => FetchAsync(
                $"PRO TYP PROJEKTU {projectType}",
                q => q.Where(t => t.ForProjectType == projectType));

        /// <summary>
        /// Retrieves a <see cref="TimeEntryType"/> by its identifier.
        /// </summary>
        public async Task<TimeEntryType?> GetTimeEntryTypeByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", $"ID {id}");

            var item = await BaseQuery(context, noTracking: true)
                .SafeFirstOrDefaultAsync(t => t.Id == id);

            Log("ZÍSKÁNÍ", item != null ? $"Found '{item.Title}'" : "Not found");

            return item;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntryType"/>.
        /// </summary>
        public async Task<bool> UpdateTimeEntryTypeAsync(TimeEntryType type)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.TimeEntryTypes.FindAsync(type.Id);

            if (existing == null)
            {
                Log("AKTUALIZACE", $"ID {type.Id} not found");
                return false;
            }

            Log("AKTUALIZACE", $"Updating ID {type.Id}");

            existing.Title = type.Title;
            existing.Color = type.Color;
            existing.ForProjectType = type.ForProjectType;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("AKTUALIZACE", "HOTOVO");

            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntryType"/> by its identifier.
        /// </summary>
        public async Task<bool> DeleteTimeEntryTypeAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("SMAZÁNÍ", $"ID {id}");

            var existing = await context.TimeEntryTypes.FindAsync(id);

            if (existing == null)
            {
                Log("SMAZÁNÍ", "Not found");
                return false;
            }

            context.TimeEntryTypes.Remove(existing);

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            Log("SMAZÁNÍ", "HOTOVO");

            return true;
        }

        #endregion
    }
}
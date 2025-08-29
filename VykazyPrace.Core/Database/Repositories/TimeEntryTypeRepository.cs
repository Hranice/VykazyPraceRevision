using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="TimeEntryType"/> entities.
    /// Provides CRUD operations and retrieval by project type.
    /// </summary>
    public class TimeEntryTypeRepository
    {
        private readonly VykazyPraceContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="TimeEntryTypeRepository"/>.
        /// </summary>
        public TimeEntryTypeRepository() => _context = new VykazyPraceContext();

        #region Helpers

        /// <summary>
        /// Logs debug messages with standardized format.
        /// </summary>
        /// <param name="action">Action name (e.g., "PŘIDÁNÍ").</param>
        /// <param name="message">Detail message.</param>
        private void Log(string action, string message)
            => AppLogger.Debug($"[TYPZÁZNAMU_{action}]: {message}");

        /// <summary>
        /// Builds base query for <see cref="TimeEntryType"/> entities.
        /// </summary>
        /// <param name="noTracking">Set to true to disable change tracking.</param>
        /// <returns>IQueryable of <see cref="TimeEntryType"/>.</returns>
        private IQueryable<TimeEntryType> BaseQuery(bool noTracking = false)
        {
            var query = _context.TimeEntryTypes.AsQueryable();
            return noTracking ? query.AsNoTracking() : query;
        }

        /// <summary>
        /// Executes a filtered query and returns list of <see cref="TimeEntryType"/>.
        /// </summary>
        /// <param name="descriptor">Description for logging.</param>
        /// <param name="applyFilter">Function to apply filters on the base query.</param>
        /// <returns>List of <see cref="TimeEntryType"/> matching the filter.</returns>
        private async Task<List<TimeEntryType>> FetchAsync(
            string descriptor,
            Func<IQueryable<TimeEntryType>, IQueryable<TimeEntryType>> applyFilter)
        {
            Log("ZÍSKÁNÍ", descriptor);
            var list = await applyFilter(BaseQuery(noTracking: true)).SafeToListAsync();
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {list.Count} TYPŮ");
            return list;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new <see cref="TimeEntryType"/>, or returns existing if duplicate.
        /// </summary>
        /// <param name="timeEntryType">Type to create.</param>
        /// <returns>Created or existing <see cref="TimeEntryType"/>, or null if input is invalid.</returns>
        public async Task<TimeEntryType?> CreateTimeEntryTypeAsync(TimeEntryType timeEntryType)
        {
            Log("PŘIDÁNÍ", $"'{timeEntryType.Title}' for project type {timeEntryType.ForProjectType}");
            var existing = await BaseQuery(noTracking: true)
                .SafeFirstOrDefaultAsync(t => t.Title == timeEntryType.Title
                                          && t.ForProjectType == timeEntryType.ForProjectType);
            if (existing != null)
            {
                Log("PŘIDÁNÍ", $"Exists with ID {existing.Id}");
                return existing;
            }

            _context.TimeEntryTypes.Add(timeEntryType);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("PŘIDÁNÍ", $"Created with ID {timeEntryType.Id}");
            return timeEntryType;
        }

        /// <summary>
        /// Retrieves all <see cref="TimeEntryType"/> entries.
        /// </summary>
        /// <returns>List of all <see cref="TimeEntryType"/>.</returns>
        public Task<List<TimeEntryType>> GetAllTimeEntryTypesAsync()
            => FetchAsync("VŠECHNY", q => q);

        /// <summary>
        /// Retrieves all <see cref="TimeEntryType"/> entries filtered by project type.
        /// </summary>
        /// <param name="projectType">Project type to filter.</param>
        /// <returns>List of matching <see cref="TimeEntryType"/>.</returns>
        public Task<List<TimeEntryType>> GetAllTimeEntryTypesByProjectTypeAsync(int projectType)
            => FetchAsync(
                $"PRO TYP PROJEKTU {projectType}",
                q => q.Where(t => t.ForProjectType == projectType));

        /// <summary>
        /// Retrieves a <see cref="TimeEntryType"/> by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the type.</param>
        /// <returns>Matching <see cref="TimeEntryType"/>, or null if not found.</returns>
        public async Task<TimeEntryType?> GetTimeEntryTypeByIdAsync(int id)
        {
            Log("ZÍSKÁNÍ", $"ID {id}");
            var item = await BaseQuery(noTracking: true).SafeFirstOrDefaultAsync(t => t.Id == id);
            Log("ZÍSKÁNÍ", item != null ? $"Found '{item.Title}'" : "Not found");
            return item;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntryType"/>.
        /// </summary>
        /// <param name="type">Object containing updated values.</param>
        /// <returns>True if update succeeded; false if not found.</returns>
        public async Task<bool> UpdateTimeEntryTypeAsync(TimeEntryType type)
        {
            var existing = await _context.TimeEntryTypes.FindAsync(type.Id);
            if (existing == null)
            {
                Log("AKTUALIZACE", $"ID {type.Id} not found");
                return false;
            }

            Log("AKTUALIZACE", $"Updating ID {type.Id}");
            existing.Title = type.Title;
            existing.Color = type.Color;
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", "HOTOVO");
            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntryType"/> by its identifier.
        /// </summary>
        /// <param name="id">Identifier of the type to delete.</param>
        /// <returns>True if deletion succeeded; false otherwise.</returns>
        public async Task<bool> DeleteTimeEntryTypeAsync(int id)
        {
            Log("SMAZÁNÍ", $"ID {id}");
            var existing = await _context.TimeEntryTypes.FindAsync(id);
            if (existing == null)
            {
                Log("SMAZÁNÍ", "Not found");
                return false;
            }

            _context.TimeEntryTypes.Remove(existing);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("SMAZÁNÍ", "HOTOVO");
            return true;
        }

        #endregion
    }
}
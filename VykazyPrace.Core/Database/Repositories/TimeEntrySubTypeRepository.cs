using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
        private readonly VykazyPraceContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="TimeEntrySubTypeRepository"/>.
        /// </summary>
        public TimeEntrySubTypeRepository() => _context = new VykazyPraceContext();

        #region Helpers

        /// <summary>
        /// Logs debug messages with standardized format.
        /// </summary>
        /// <param name="action">Action name (e.g., "PŘIDÁNÍ").</param>
        /// <param name="message">Detail message.</param>
        private void Log(string action, string message)
            => AppLogger.Debug($"[PODTYPZÁZNAMU_{action}]: {message}");

        /// <summary>
        /// Builds base query for <see cref="TimeEntrySubType"/>, including related user.
        /// </summary>
        /// <param name="noTracking">Disable change tracking if true.</param>
        /// <returns>IQueryable of <see cref="TimeEntrySubType"/>.</returns>
        private IQueryable<TimeEntrySubType> BaseQuery(bool noTracking = false)
        {
            var query = _context.TimeEntrySubTypes.AsQueryable();
            if (noTracking) query = query.AsNoTracking();
            return query.Include(st => st.User);
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Creates a new <see cref="TimeEntrySubType"/>, or returns existing non-archived entry if duplicate.
        /// </summary>
        /// <param name="subType">The subtype to add.</param>
        /// <returns>The created or existing <see cref="TimeEntrySubType"/> instance.</returns>
        public async Task<TimeEntrySubType> CreateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            Log("PŘIDÁNÍ", $"'{subType.Title}' for user {subType.UserId}.");
            var existing = await BaseQuery(noTracking: true)
                .FirstOrDefaultAsync(t => t.Title == subType.Title && t.UserId == subType.UserId);
            if (existing != null && existing.IsArchived == 0)
            {
                Log("PŘIDÁNÍ", $"Exists non-archived with ID {existing.Id}.");
                return existing;
            }

            _context.TimeEntrySubTypes.Add(subType);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("PŘIDÁNÍ", $"Created with ID {subType.Id}.");
            return subType;
        }

        /// <summary>
        /// Retrieves all <see cref="TimeEntrySubType"/> entries.
        /// </summary>
        /// <returns>List of all <see cref="TimeEntrySubType"/>.</returns>
        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesAsync()
        {
            Log("ZÍSKÁNÍ", "All subtypes.");
            var list = await BaseQuery(noTracking: true).ToListAsync();
            Log("ZÍSKÁNÍ", $"Returned {list.Count} items.");
            return list;
        }

        /// <summary>
        /// Retrieves unique non-archived <see cref="TimeEntrySubType"/> titles for a user, ordered by custom order.
        /// </summary>
        /// <param name="userId">User ID filter.</param>
        /// <returns>Ordered list of unique <see cref="TimeEntrySubType"/>.</returns>
        public async Task<List<TimeEntrySubType>> GetAllTimeEntrySubTypesByUserIdAsync(int userId)
        {
            Log("ZÍSKÁNÍ", $"User {userId} non-archived unique subtypes.");
            var uniqueIds = await BaseQuery(noTracking: true)
                .Where(t => t.UserId == userId && t.IsArchived == 0)
                .GroupBy(t => t.Title)
                .Select(g => g.Min(t => t.Id))
                .ToListAsync();

            var list = await BaseQuery(noTracking: true)
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
        /// <param name="id">Subtype identifier.</param>
        /// <returns>Matching <see cref="TimeEntrySubType"/>, or null if not found.</returns>
        public async Task<TimeEntrySubType?> GetTimeEntrySubTypeByIdAsync(int id)
        {
            Log("ZÍSKÁNÍ", $"ID {id}.");
            var item = await BaseQuery(noTracking: true).FirstOrDefaultAsync(t => t.Id == id);
            Log("ZÍSKÁNÍ", item != null ? $"Found {item.Title}." : "Not found.");
            return item;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntrySubType"/>.
        /// </summary>
        /// <param name="subType">Object with updated values.</param>
        /// <returns>True if update succeeded; false if not found.</returns>
        public async Task<bool> UpdateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            var existing = await _context.TimeEntrySubTypes.FindAsync(subType.Id);
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

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", "Completed.");
            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntrySubType"/> by ID.
        /// </summary>
        /// <param name="id">Subtype identifier.</param>
        /// <returns>True if deletion succeeded; false otherwise.</returns>
        public async Task<bool> DeleteTimeEntrySubTypeAsync(int id)
        {
            Log("SMAZÁNÍ", $"ID {id}.");
            var existing = await _context.TimeEntrySubTypes.FindAsync(id);
            if (existing == null)
            {
                Log("SMAZÁNÍ", "Not found.");
                return false;
            }

            _context.TimeEntrySubTypes.Remove(existing);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("SMAZÁNÍ", "Completed.");
            return true;
        }

        #endregion
    }
}

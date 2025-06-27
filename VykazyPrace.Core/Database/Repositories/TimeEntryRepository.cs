using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging.VykazyPrace.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    /// <summary>
    /// Repository for managing <see cref="TimeEntry"/> entities.
    /// Provides methods for CRUD operations and summaries.
    /// </summary>
    public class TimeEntryRepository
    {
        private readonly VykazyPraceContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="TimeEntryRepository"/>.
        /// </summary>
        public TimeEntryRepository() => _context = new VykazyPraceContext();

        #region Helpers

        /// <summary>
        /// Logs debug messages with standardized format.
        /// </summary>
        /// <param name="action">The action being logged (e.g., "PŘIDÁNÍ").</param>
        /// <param name="message">The detail message.</param>
        private void Log(string action, string message)
            => AppLogger.Debug($"[ČAS.ZÁZNAM_{action}]: {message}");

        /// <summary>
        /// Builds base query for <see cref="TimeEntry"/>, including related entities.
        /// </summary>
        /// <param name="noTracking">Whether to disable change tracking.</param>
        /// <returns>IQueryable of <see cref="TimeEntry"/>.</returns>
        private IQueryable<TimeEntry> BaseQuery(bool noTracking = false)
        {
            var query = _context.TimeEntries.AsQueryable();
            if (noTracking) query = query.AsNoTracking();

            return query
                .Include(te => te.User)
                    .ThenInclude(u => u.UserGroup)
                .Include(te => te.EntryType)
                .Include(te => te.Project);
        }

        /// <summary>
        /// Executes a filtered query and returns a list of <see cref="TimeEntry"/>.
        /// </summary>
        /// <param name="descriptor">Description for logging.</param>
        /// <param name="applyFilter">Function to apply filters on the base query.</param>
        /// <returns>List of <see cref="TimeEntry"/> matching the filter.</returns>
        private async Task<List<TimeEntry>> FetchAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            Log("ZÍSKÁNÍ", descriptor);
            var list = await applyFilter(BaseQuery(noTracking: true)).ToListAsync();
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {list.Count} ZÁZNAMŮ");
            return list;
        }

        /// <summary>
        /// Executes a filtered query and returns sum of minutes.
        /// </summary>
        /// <param name="descriptor">Description for logging.</param>
        /// <param name="applyFilter">Function to apply filters on the base query.</param>
        /// <returns>Total minutes summed from matching entries.</returns>
        private async Task<int> SumAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            Log("ZÍSKÁNÍ", descriptor);
            var total = await applyFilter(BaseQuery()).SumAsync(te => te.EntryMinutes);
            Log("ZÍSKÁNÍ", $"HOTOVO ODPRACOVÁNO: {total} MINUT");
            return total;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Adds a new <see cref="TimeEntry"/>.
        /// </summary>
        /// <param name="timeEntry">The TimeEntry to create.</param>
        /// <returns>The created <see cref="TimeEntry"/> with updated ID.</returns>
        public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry timeEntry)
        {
            Log("PŘIDÁNÍ", $"'{FormatHelper.FormatTimeEntryToString(timeEntry)}'.");
            _context.TimeEntries.Add(timeEntry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("PŘIDÁNÍ", "HOTOVO.");
            return timeEntry;
        }

        /// <summary>
        /// Retrieves all time entries.
        /// </summary>
        /// <returns>List of all <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetAllTimeEntriesAsync()
            => FetchAsync("'VŠECHNY'", q => q);

        /// <summary>
        /// Retrieves all time entries for a specific user.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="includeSnacks">Include snack entries when true.</param>
        /// <returns>Filtered list of <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, bool includeSnacks = false)
            => FetchAsync(
                includeSnacks
                    ? $"'VŠECHNY VČETNĚ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'"
                    : $"'VŠECHNY BEZ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'",
                q =>
                {
                    var f = q.Where(te => te.UserId == user.Id);
                    if (!includeSnacks)
                        f = f.Where(te => !(te.ProjectId == 132 && te.EntryTypeId == 24));
                    return f;
                });

        /// <summary>
        /// Retrieves time entries for a user by project type.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="projectType">Project type filter.</param>
        /// <returns>Filtered list of <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A TYPU PROJEKTU '{projectType}'",
                q => q.Where(te => te.UserId == user.Id && te.Project.ProjectType == projectType));

        /// <summary>
        /// Retrieves time entries for a user by project type and date.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="projectType">Project type filter.</param>
        /// <param name="date">Specific date filter.</param>
        /// <returns>Filtered list of <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetTimeEntriesByProjectTypeAndDateAsync(User user, int projectType, DateTime date)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', PROJEKT '{projectType}', DEN '{date:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Project.ProjectType == projectType
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Retrieves time entries for a user by date.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="date">Specific date filter.</param>
        /// <returns>Filtered list of <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{date:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Retrieves time entries between two dates.
        /// </summary>
        /// <param name="fromDate">Start date inclusive.</param>
        /// <param name="toDate">End date inclusive.</param>
        /// <returns>List of <see cref="TimeEntry"/> within range.</returns>
        public Task<List<TimeEntry>> GetAllTimeEntriesBetweenDatesAsync(DateTime fromDate, DateTime toDate)
            => FetchAsync(
                $"'VŠECHNY' OBDOBÍ '{fromDate:yyyy-MM-dd} - {toDate:yyyy-MM-dd}'",
                q => q.Where(te => te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date >= fromDate.Date
                                   && te.Timestamp.Value.Date <= toDate.Date));

        /// <summary>
        /// Retrieves time entries for a user in the week of a given date (Mon-Sun).
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="date">Any date within the target week.</param>
        /// <returns>Weekly list of <see cref="TimeEntry"/>.</returns>
        public Task<List<TimeEntry>> GetTimeEntriesByUserAndCurrentWeekAsync(User user, DateTime date)
        {
            var start = date.Date.AddDays(-(int)date.DayOfWeek + (date.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var end = start.AddDays(6);
            return FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', TÝDEN '{start:yyyy-MM-dd} - {end:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date >= start
                                   && te.Timestamp.Value.Date <= end));
        }

        /// <summary>
        /// Retrieves a <see cref="TimeEntry"/> by its ID.
        /// </summary>
        /// <param name="id">Entry identifier.</param>
        /// <returns>The matching <see cref="TimeEntry"/>, or null if not found.</returns>
        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            Log("ZÍSKÁNÍ", $"'KONKRÉTNÍ' PODLE ID '{id}'");
            var entry = await BaseQuery(noTracking: true)
                .FirstOrDefaultAsync(te => te.Id == id);
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO: '{FormatHelper.FormatTimeEntryToString(entry)}'");
            return entry;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntry"/>.
        /// </summary>
        /// <param name="timeEntry">Entry with updated values.</param>
        /// <returns>True if update succeeded; false if entry not found.</returns>
        public async Task<bool> UpdateTimeEntryAsync(TimeEntry timeEntry)
        {
            var existing = await _context.TimeEntries.FindAsync(timeEntry.Id);
            if (existing == null) return false;
            Log("AKTUALIZACE", $"'{FormatHelper.FormatTimeEntryToString(existing)}' NA '{FormatHelper.FormatTimeEntryToString(timeEntry)}'");
            _context.Entry(existing).CurrentValues.SetValues(timeEntry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", "HOTOVO");
            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntry"/> by ID.
        /// </summary>
        /// <param name="id">Entry identifier.</param>
        /// <returns>True if deletion succeeded; false otherwise.</returns>
        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            Log("SMAZÁNÍ", $"PRO ID '{id}'");
            var entry = await _context.TimeEntries.FindAsync(id);
            if (entry == null)
            {
                Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: NE");
                return false;
            }
            _context.TimeEntries.Remove(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: ANO");
            return true;
        }

        #endregion

        #region Summaries and Utilities

        /// <summary>
        /// Gets total minutes for a user on a specific day.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="date">Date filter.</param>
        /// <returns>Total minutes worked.</returns>
        public Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
            => SumAsync(
                $"'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{date:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Gets total minutes for a user on a specific day and project type.
        /// </summary>
        /// <param name="user">User filter.</param>
        /// <param name="date">Date filter.</param>
        /// <param name="projectType">Project type filter.</param>
        /// <returns>Total minutes worked.</returns>
        /// <see cref="GetTotalMinutesForUserByDayAsync(User, DateTime)"/>
        public Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date, int projectType)
            => SumAsync(
                $"'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{date:yyyy-MM-dd}', TYP PROJEKTU '{projectType}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Project.ProjectType == projectType
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Retrieves summary of hours per user and project in a date range.
        /// </summary>
        /// <param name="fromDate">Start date inclusive.</param>
        /// <param name="toDate">End date inclusive.</param>
        /// <returns>List of <see cref="TimeEntrySummary"/>.</returns>
        public async Task<List<TimeEntrySummary>> GetTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            Log("ZÍSKÁNÍ", $"'SUMMARY' OBDOBÍ '{fromDate:yyyy-MM-dd} - {toDate:yyyy-MM-dd}'");
            var summary = await _context.TimeEntries
                .Where(te => te.Timestamp.HasValue
                             && te.Timestamp.Value.Date >= fromDate.Date
                             && te.Timestamp.Value.Date <= toDate.Date
                             && !(te.ProjectId == 132 && te.EntryTypeId == 24))
                .GroupBy(te => new { te.UserId, te.ProjectId })
                .Select(g => new TimeEntrySummary
                {
                    UserId = g.Key.UserId,
                    ProjectId = g.Key.ProjectId,
                    TotalHours = g.Sum(te => te.EntryMinutes) / 60.0
                })
                .ToListAsync();
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {summary.Count} ZÁZNAMŮ");
            return summary;
        }

        /// <summary>
        /// Locks all entries in the specified month.
        /// </summary>
        /// <param name="month">
        /// Month name in Czech (e.g., "Leden").
        /// </param>
        /// <exception cref="ArgumentException">Thrown for invalid month name.</exception>
        public async Task LockAllEntriesInMonth(string month)
        {
            Log("AKTUALIZACE", $"ZAMKNOUT '{month}'");
            var monthNum = month switch
            {
                "Leden" => 1,
                "Únor" => 2,
                "Březen" => 3,
                "Duben" => 4,
                "Květen" => 5,
                "Červen" => 6,
                "Červenec" => 7,
                "Srpen" => 8,
                "Září" => 9,
                "Říjen" => 10,
                "Listopad" => 11,
                "Prosinec" => 12,
                _ => throw new ArgumentException("Neplatný měsíc: " + month)
            };
            var entries = await _context.TimeEntries
                .Where(e => e.Timestamp.HasValue && e.Timestamp.Value.Month == monthNum)
                .ToListAsync();
            entries.ForEach(e => e.IsLocked = 1);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", "HOTOVO");
        }

        /// <summary>
        /// Updates project ID for all entries from old to new project ID.
        /// </summary>
        /// <param name="oldProjectId">Original project ID.</param>
        /// <param name="newProjectId">New project ID.</param>
        /// <returns>Count of updated entries.</returns>
        public async Task<int> UpdateProjectIdForEntriesAsync(int oldProjectId, int newProjectId)
        {
            Log("AKTUALIZACE", $"ID PROJEKTU '{oldProjectId}' NA '{newProjectId}'");
            var entries = await _context.TimeEntries
                .Where(e => e.ProjectId == oldProjectId)
                .ToListAsync();
            entries.ForEach(e => e.ProjectId = newProjectId);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");
            return entries.Count;
        }

        /// <summary>
        /// Bulk change descriptions of all not-locked records for
        /// the specified user.
        /// </summary>
        /// <param name="userId">ID of the user.</param>
        /// <param name="oldDescription">Original description used that will be updated.</param>
        /// <param name="newDescription">New description to be set.</param>
        /// <returns>Count of updated entries.</returns>
        public async Task<int> UpdateUnlockedDescriptionsForUserAsync(int userId, string oldDescription, string newDescription)
        {
            Log("AKTUALIZACE", $"VŠECHNY ODEMČENÉ PRO UŽIVATELE '{userId}' S POPISEM '{oldDescription}' ⇒ '{newDescription}'");
            // Načteme jen nezamčené záznamy s odpovídajícím popisem
            var entries = await BaseQuery(noTracking: false)
                .Where(te => te.UserId == userId
                          && te.IsLocked == 0
                          && te.Description == oldDescription)
                .ToListAsync();

            foreach (var e in entries)
                e.Description = newDescription;

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");
            return entries.Count;
        }

        /// <summary>
        /// Determines whether an entry exists for a given user, date, project, and entry type.
        /// </summary>
        /// <param name="userId">ID of the user.</param>
        /// <param name="day">Day to check in.</param>
        /// <param name="projectId">ID of the project.</param>
        /// <param name="entryTypeId">ID of the entry type.</param>
        /// <returns>True if any entry was found.</returns>
        public async Task<bool> ExistsEntryAsync(int userId, DateTime day, int projectId, int entryTypeId)
        {
            Log("ZÍSKÁNÍ", $"EXISTUJE? UŽIVATEL '{userId}', DEN '{day:yyyy-MM-dd}', PROJEKT '{projectId}', TYP '{entryTypeId}'");
            bool exists = await BaseQuery(noTracking: true)
                .AnyAsync(te =>
                    te.UserId == userId &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value.Date == day.Date &&
                    te.ProjectId == projectId &&
                    te.EntryTypeId == entryTypeId);
            Log("ZÍSKÁNÍ", $"HOTOVO EXISTUJE: {exists}");
            return exists;
        }


        #endregion

        /// <summary>
        /// Summary DTO for grouping time entries.
        /// </summary>
        public class TimeEntrySummary
        {
            /// <summary>User identifier.</summary>
            public int? UserId { get; set; }

            /// <summary>Project identifier.</summary>
            public int? ProjectId { get; set; }

            /// <summary>Total hours worked.</summary>
            public double TotalHours { get; set; }
        }
    }
}

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
        /// Executes a filtered query and returns a list of <see cref="TimeEntry"/> safely.
        /// </summary>
        private async Task<List<TimeEntry>> FetchAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            Log("ZÍSKÁNÍ", descriptor);
            var list = await applyFilter(BaseQuery(noTracking: true)).SafeToListAsync();
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {list.Count} ZÁZNAMŮ");
            return list;
        }

        /// <summary>
        /// Executes a filtered query and returns sum of minutes safely.
        /// </summary>
        private async Task<int> SumAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            Log("ZÍSKÁNÍ", descriptor);
            var total = await _context.SafeGetAsync<int?>(async () =>
     await applyFilter(BaseQuery()).SumAsync(te => te.EntryMinutes)
 ) ?? 0;

            Log("ZÍSKÁNÍ", $"HOTOVO ODPRACOVÁNO: {total} MINUT");
            return total;
        }

        #endregion

        #region CRUD Operations

        /// <summary>
        /// Adds a new <see cref="TimeEntry"/>.
        /// </summary>
        public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry timeEntry)
        {
            Log("PŘIDÁNÍ", $"'{FormatHelper.FormatTimeEntryToString(timeEntry)}'.");
            _context.TimeEntries.Add(timeEntry);
            await _context.SafeSaveAsync();
            _context.Entry(timeEntry).State = EntityState.Detached;
            Log("PŘIDÁNÍ", "HOTOVO.");
            return timeEntry;
        }

        /// <summary>
        /// Retrieves all time entries.
        /// </summary>
        public Task<List<TimeEntry>> GetAllTimeEntriesAsync()
            => FetchAsync("'VŠECHNY'", q => q);

        /// <summary>
        /// Retrieves all time entries for a specific user.
        /// </summary>
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
        public Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A TYPU PROJEKTU '{projectType}'",
                q => q.Where(te => te.UserId == user.Id && te.Project.ProjectType == projectType));

        /// <summary>
        /// Retrieves time entries for a user by project type and date.
        /// </summary>
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
        public Task<List<TimeEntry>> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{date:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Retrieves time entries between two dates.
        /// </summary>
        public Task<List<TimeEntry>> GetAllTimeEntriesBetweenDatesAsync(DateTime fromDate, DateTime toDate)
            => FetchAsync(
                $"'VŠECHNY' OBDOBÍ '{fromDate:yyyy-MM-dd} - {toDate:yyyy-MM-dd}'",
                q => q.Where(te => te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date >= fromDate.Date
                                   && te.Timestamp.Value.Date <= toDate.Date));

        /// <summary>
        /// Retrieves time entries for a user in the week of a given date (Mon-Sun).
        /// </summary>
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
        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            Log("ZÍSKÁNÍ", $"'KONKRÉTNÍ' PODLE ID '{id}'");
            var entry = await BaseQuery(noTracking: true)
                .SafeFirstOrDefaultAsync(te => te.Id == id);
            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO: '{FormatHelper.FormatTimeEntryToString(entry)}'");
            return entry;
        }

        /// <summary>
        /// Updates an existing <see cref="TimeEntry"/>.
        /// </summary>
        public async Task<bool> UpdateTimeEntryAsync(TimeEntry timeEntry)
        {
            var existing = await _context.TimeEntries.SafeFindAsync(new object?[] { timeEntry.Id });
            if (existing == null) return false;

            Log("AKTUALIZACE", $"'{FormatHelper.FormatTimeEntryToString(existing)}' NA '{FormatHelper.FormatTimeEntryToString(timeEntry)}'");
            _context.Entry(existing).CurrentValues.SetValues(timeEntry);

            await _context.SafeSaveAsync();
            Log("AKTUALIZACE", "HOTOVO");
            return true;
        }

        /// <summary>
        /// Deletes a <see cref="TimeEntry"/> by ID.
        /// </summary>
        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            Log("SMAZÁNÍ", $"PRO ID '{id}'");
            var entry = await _context.TimeEntries.SafeFindAsync(new object?[] { id });
            if (entry == null)
            {
                Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: NE");
                return false;
            }

            _context.TimeEntries.Remove(entry);
            await _context.SafeSaveAsync();
            Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: ANO");
            return true;
        }

        #endregion

        #region Summaries and Utilities

        /// <summary>
        /// Gets total minutes for a user on a specific day.
        /// </summary>
        public Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
            => SumAsync(
                $"'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{date:yyyy-MM-dd}'",
                q => q.Where(te => te.UserId == user.Id
                                   && te.Timestamp.HasValue
                                   && te.Timestamp.Value.Date == date.Date));

        /// <summary>
        /// Gets total minutes for a user on a specific day and project type.
        /// </summary>
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
        public async Task<List<TimeEntrySummary>> GetTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            Log("ZÍSKÁNÍ", $"'SUMMARY' OBDOBÍ '{fromDate:yyyy-MM-dd} - {toDate:yyyy-MM-dd}'");
            var summary = await _context.SafeGetAsync(async () =>
                await _context.TimeEntries
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
                    .ToListAsync()
            ) ?? new List<TimeEntrySummary>();

            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {summary.Count} ZÁZNAMŮ");
            return summary;
        }

        /// <summary>
        /// Locks all entries in the specified month.
        /// </summary>
        /// <param name="month">Month name in Czech (e.g., "Leden").</param>
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
                .SafeToListAsync();

            entries.ForEach(e => e.IsLocked = 1);
            await _context.SafeSaveAsync();
            Log("AKTUALIZACE", "HOTOVO");
        }

        /// <summary>
        /// Updates project ID for all entries from old to new project ID.
        /// </summary>
        public async Task<int> UpdateProjectIdForEntriesAsync(int oldProjectId, int newProjectId)
        {
            Log("AKTUALIZACE", $"ID PROJEKTU '{oldProjectId}' NA '{newProjectId}'");
            var entries = await _context.TimeEntries
                .Where(e => e.ProjectId == oldProjectId)
                .SafeToListAsync();

            entries.ForEach(e => e.ProjectId = newProjectId);
            await _context.SafeSaveAsync();
            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");
            return entries.Count;
        }

        /// <summary>
        /// Bulk change descriptions of all not-locked records for the specified user.
        /// </summary>
        public async Task<int> UpdateUnlockedDescriptionsForUserAsync(int userId, string oldDescription, string newDescription)
        {
            Log("AKTUALIZACE", $"VŠECHNY ODEMČENÉ PRO UŽIVATELE '{userId}' S POPISEM '{oldDescription}' ⇒ '{newDescription}'");

            var entries = await BaseQuery(noTracking: false)
                .Where(te => te.UserId == userId
                          && te.IsLocked == 0
                          && te.Description == oldDescription)
                .SafeToListAsync();

            foreach (var e in entries)
                e.Description = newDescription;

            await _context.SafeSaveAsync();
            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");
            return entries.Count;
        }

        /// <summary>
        /// Determines whether an entry exists for a given user, date, project, and entry type.
        /// </summary>
        public async Task<bool> ExistsEntryAsync(int userId, DateTime day, int projectId, int entryTypeId)
        {
            Log("ZÍSKÁNÍ", $"EXISTUJE? UŽIVATEL '{userId}', DEN '{day:yyyy-MM-dd}', PROJEKT '{projectId}', TYP '{entryTypeId}'");

            var exists = await _context.SafeGetAsync<bool?>(async () =>
       await BaseQuery(noTracking: true).AnyAsync(te =>
           te.UserId == userId &&
           te.Timestamp.HasValue &&
           te.Timestamp.Value.Date == day.Date &&
           te.ProjectId == projectId &&
           te.EntryTypeId == entryTypeId)
   ) ?? false;


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

        public sealed class ProjectUserCumulativeDto
        {
            public int ProjectId { get; set; }
            public int UserId { get; set; }
            public int MinutesToFullFilled { get; set; }
        }

        public async Task<List<ProjectUserCumulativeDto>> GetCumulativeToFullfilledAsync(IEnumerable<int> projectIds)
        {
            var ids = projectIds?.ToList() ?? new List<int>();

            return await _context.TimeEntries
                .Where(te => te.Timestamp.HasValue
                             && te.ProjectId != null
                             && te.UserId != null
                             && te.Project!.DateFullFilled != null
                             && (ids.Count == 0 || ids.Contains(te.ProjectId!.Value))
                             && te.Timestamp!.Value <= te.Project!.DateFullFilled!)
                .GroupBy(te => new { te.ProjectId, te.UserId })
                .Select(g => new ProjectUserCumulativeDto
                {
                    ProjectId = g.Key.ProjectId!.Value,
                    UserId = g.Key.UserId!.Value,
                    MinutesToFullFilled = g.Sum(x => x.EntryMinutes)
                })
                .ToListAsync();
        }

    }
}

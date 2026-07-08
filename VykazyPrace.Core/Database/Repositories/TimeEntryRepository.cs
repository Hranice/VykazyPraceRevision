using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntryRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public TimeEntryRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        #region Helpers

        private void Log(string action, string message)
            => AppLogger.Debug($"[ČAS.ZÁZNAM_{action}]: {message}");

        private static IQueryable<TimeEntry> BaseQuery(
            VykazyPraceContext context,
            bool noTracking = false)
        {
            var query = context.TimeEntries.AsQueryable();

            if (noTracking)
                query = query.AsNoTracking();

            return query
                .Include(te => te.User)
                    .ThenInclude(u => u.UserGroup)
                .Include(te => te.EntryType)
                .Include(te => te.Project);
        }

        private async Task<List<TimeEntry>> FetchAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", descriptor);

            var list = await applyFilter(BaseQuery(context, noTracking: true))
                .SafeToListAsync();

            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO {list.Count} ZÁZNAMŮ");

            return list;
        }

        private async Task<int> SumAsync(
            string descriptor,
            Func<IQueryable<TimeEntry>, IQueryable<TimeEntry>> applyFilter)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", descriptor);

            var total = await context.SafeGetAsync<int?>(async () =>
                await applyFilter(BaseQuery(context, noTracking: true))
                    .SumAsync(te => te.EntryMinutes)
            ) ?? 0;

            Log("ZÍSKÁNÍ", $"HOTOVO ODPRACOVÁNO: {total} MINUT");

            return total;
        }

        #endregion

        #region CRUD Operations

        public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry timeEntry)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("PŘIDÁNÍ", $"'{FormatHelper.FormatTimeEntryToString(timeEntry)}'.");

            context.TimeEntries.Add(timeEntry);
            await context.SafeSaveAsync();

            context.Entry(timeEntry).State = EntityState.Detached;

            Log("PŘIDÁNÍ", "HOTOVO.");

            return timeEntry;
        }

        public Task<List<TimeEntry>> GetAllTimeEntriesAsync()
            => FetchAsync("'VŠECHNY'", q => q);

        public Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, bool includeSnacks = false)
            => FetchAsync(
                includeSnacks
                    ? $"'VŠECHNY VČETNĚ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'"
                    : $"'VŠECHNY BEZ SVAČIN' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}'",
                q =>
                {
                    var filtered = q.Where(te => te.UserId == user.Id);

                    if (!includeSnacks)
                    {
                        filtered = filtered.Where(te =>
                            !(te.ProjectId == WorkLogIds.Projects.Snack && te.EntryTypeId == WorkLogIds.EntryTypes.Snack));
                    }

                    return filtered;
                });

        public Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
            => FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}' A TYPU PROJEKTU '{projectType}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Project != null &&
                    te.Project.ProjectType == projectType));

        public Task<List<TimeEntry>> GetTimeEntriesByProjectTypeAndDateAsync(
            User user,
            int projectType,
            DateTime date)
        {
            var dayStart = date.Date;
            var nextDay = dayStart.AddDays(1);

            return FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', PROJEKT '{projectType}', DEN '{dayStart:yyyy-MM-dd}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Project != null &&
                    te.Project.ProjectType == projectType &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= dayStart &&
                    te.Timestamp.Value < nextDay));
        }

        public Task<List<TimeEntry>> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
        {
            var dayStart = date.Date;
            var nextDay = dayStart.AddDays(1);

            return FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{dayStart:yyyy-MM-dd}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= dayStart &&
                    te.Timestamp.Value < nextDay));
        }

        public Task<List<TimeEntry>> GetAllTimeEntriesBetweenDatesAsync(DateTime fromDate, DateTime toDate)
        {
            var range = NormalizeInclusiveDateRange(fromDate, toDate);

            return FetchAsync(
                $"'VŠECHNY' OBDOBÍ '{range.FromInclusive:yyyy-MM-dd} - {range.ToInclusive:yyyy-MM-dd}'",
                q => q.Where(te =>
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= range.FromInclusive &&
                    te.Timestamp.Value < range.ToExclusive));
        }

        public Task<List<TimeEntry>> GetTimeEntriesByUserAndCurrentWeekAsync(User user, DateTime date)
        {
            var start = date.Date.AddDays(
                -(int)date.DayOfWeek + (date.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));

            var endInclusive = start.AddDays(6);
            var endExclusive = endInclusive.AddDays(1);

            return FetchAsync(
                $"'VŠECHNY' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', TÝDEN '{start:yyyy-MM-dd} - {endInclusive:yyyy-MM-dd}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= start &&
                    te.Timestamp.Value < endExclusive));
        }

        public async Task<TimeEntry?> GetTimeEntryByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("ZÍSKÁNÍ", $"'KONKRÉTNÍ' PODLE ID '{id}'");

            var entry = await BaseQuery(context, noTracking: true)
                .SafeFirstOrDefaultAsync(te => te.Id == id);

            Log("ZÍSKÁNÍ", $"HOTOVO VRÁCENO: '{FormatHelper.FormatTimeEntryToString(entry)}'");

            return entry;
        }

        public async Task<bool> UpdateTimeEntryAsync(TimeEntry timeEntry)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.TimeEntries
                .SafeFindAsync(new object?[] { timeEntry.Id });

            if (existing == null)
                return false;

            Log(
                "AKTUALIZACE",
                $"'{FormatHelper.FormatTimeEntryToString(existing)}' NA '{FormatHelper.FormatTimeEntryToString(timeEntry)}'");

            context.Entry(existing).CurrentValues.SetValues(timeEntry);

            await context.SafeSaveAsync();

            Log("AKTUALIZACE", "HOTOVO");

            return true;
        }

        public async Task<bool> DeleteTimeEntryAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("SMAZÁNÍ", $"PRO ID '{id}'");

            var entry = await context.TimeEntries
                .SafeFindAsync(new object?[] { id });

            if (entry == null)
            {
                Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: NE");
                return false;
            }

            context.TimeEntries.Remove(entry);
            await context.SafeSaveAsync();

            Log("SMAZÁNÍ", "HOTOVO ÚSPĚCH: ANO");

            return true;
        }

        #endregion

        #region Summaries and Utilities

        public Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
        {
            var dayStart = date.Date;
            var nextDay = dayStart.AddDays(1);

            return SumAsync(
                $"'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{dayStart:yyyy-MM-dd}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= dayStart &&
                    te.Timestamp.Value < nextDay));
        }

        public Task<int> GetTotalMinutesForUserByDayAsync(User user, DateTime date, int projectType)
        {
            var dayStart = date.Date;
            var nextDay = dayStart.AddDays(1);

            return SumAsync(
                $"'SUMA ODPRACOVANÝCH MINUT' PRO UŽIVATELE '{FormatHelper.FormatUserToString(user)}', DEN '{dayStart:yyyy-MM-dd}', TYP PROJEKTU '{projectType}'",
                q => q.Where(te =>
                    te.UserId == user.Id &&
                    te.Project != null &&
                    te.Project.ProjectType == projectType &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value >= dayStart &&
                    te.Timestamp.Value < nextDay));
        }

        public async Task<List<TimeEntrySummary>> GetTimeEntriesSummaryAsync(
            DateTime fromDate,
            DateTime toDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var range = NormalizeInclusiveDateRange(fromDate, toDate);

            Log("ZÍSKÁNÍ", $"'SUMMARY' OBDOBÍ '{range.FromInclusive:yyyy-MM-dd} - {range.ToInclusive:yyyy-MM-dd}'");

            var summary = await context.SafeGetAsync(async () =>
                await context.TimeEntries
                    .AsNoTracking()
                    .Where(te =>
                        te.Timestamp.HasValue &&
                        te.Timestamp.Value >= range.FromInclusive &&
                        te.Timestamp.Value < range.ToExclusive &&
                        !(te.ProjectId == WorkLogIds.Projects.Snack && te.EntryTypeId == WorkLogIds.EntryTypes.Snack))
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

        public async Task LockAllEntriesInMonth(string month)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

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

            var entries = await context.TimeEntries
                .Where(e =>
                    e.Timestamp.HasValue &&
                    e.Timestamp.Value.Month == monthNum)
                .SafeToListAsync();

            entries.ForEach(e => e.IsLocked = 1);

            await context.SafeSaveAsync();

            Log("AKTUALIZACE", "HOTOVO");
        }

        public async Task<int> UpdateProjectIdForEntriesAsync(int oldProjectId, int newProjectId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log("AKTUALIZACE", $"ID PROJEKTU '{oldProjectId}' NA '{newProjectId}'");

            var entries = await context.TimeEntries
                .Where(e => e.ProjectId == oldProjectId)
                .SafeToListAsync();

            entries.ForEach(e => e.ProjectId = newProjectId);

            await context.SafeSaveAsync();

            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");

            return entries.Count;
        }

        public async Task<int> UpdateUnlockedDescriptionsForUserAsync(
            int userId,
            string oldDescription,
            string newDescription)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            Log(
                "AKTUALIZACE",
                $"VŠECHNY ODEMČENÉ PRO UŽIVATELE '{userId}' S POPISEM '{oldDescription}' ⇒ '{newDescription}'");

            var entries = await BaseQuery(context, noTracking: false)
                .Where(te =>
                    te.UserId == userId &&
                    te.IsLocked == 0 &&
                    te.Description == oldDescription)
                .SafeToListAsync();

            foreach (var entry in entries)
            {
                entry.Description = newDescription;
            }

            await context.SafeSaveAsync();

            Log("AKTUALIZACE", $"HOTOVO UPRAVENO: {entries.Count}");

            return entries.Count;
        }

        public async Task<bool> ExistsEntryAsync(
            int userId,
            DateTime day,
            int projectId,
            int entryTypeId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var dayStart = day.Date;
            var nextDay = dayStart.AddDays(1);

            Log(
                "ZÍSKÁNÍ",
                $"EXISTUJE? UŽIVATEL '{userId}', DEN '{dayStart:yyyy-MM-dd}', PROJEKT '{projectId}', TYP '{entryTypeId}'");

            var exists = await context.SafeGetAsync<bool?>(async () =>
                await BaseQuery(context, noTracking: true)
                    .AnyAsync(te =>
                        te.UserId == userId &&
                        te.Timestamp.HasValue &&
                        te.Timestamp.Value >= dayStart &&
                        te.Timestamp.Value < nextDay &&
                        te.ProjectId == projectId &&
                        te.EntryTypeId == entryTypeId)
            ) ?? false;

            Log("ZÍSKÁNÍ", $"HOTOVO EXISTUJE: {exists}");

            return exists;
        }

        public async Task<List<ProjectUserCumulativeDto>> GetCumulativeToFullfilledAsync(
            IEnumerable<int> projectIds)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var ids = projectIds?.ToList() ?? new List<int>();

            return await context.TimeEntries
                .AsNoTracking()
                .Where(te =>
                    te.Timestamp.HasValue &&
                    te.ProjectId != null &&
                    te.UserId != null &&
                    te.Project != null &&
                    te.Project.DateFullFilled != null &&
                    (ids.Count == 0 || ids.Contains(te.ProjectId.Value)) &&
                    te.Timestamp.Value < te.Project.DateFullFilled.Value.Date.AddDays(1))
                .GroupBy(te => new { te.ProjectId, te.UserId })
                .Select(g => new ProjectUserCumulativeDto
                {
                    ProjectId = g.Key.ProjectId!.Value,
                    UserId = g.Key.UserId!.Value,
                    MinutesToFullFilled = g.Sum(x => x.EntryMinutes)
                })
                .ToListAsync();
        }

        #endregion

        public class TimeEntrySummary
        {
            public int? UserId { get; set; }
            public int? ProjectId { get; set; }
            public double TotalHours { get; set; }
        }

        public sealed class ProjectUserCumulativeDto
        {
            public int ProjectId { get; set; }
            public int UserId { get; set; }
            public int MinutesToFullFilled { get; set; }
        }

        private static (DateTime FromInclusive, DateTime ToInclusive, DateTime ToExclusive)
            NormalizeInclusiveDateRange(DateTime fromDate, DateTime toDate)
        {
            var fromInclusive = fromDate.Date;
            var toInclusive = toDate.Date;

            if (toInclusive < fromInclusive)
                throw new ArgumentException("Datum do nesmí být menší než datum od.");

            return (fromInclusive, toInclusive, toInclusive.AddDays(1));
        }
    }
}

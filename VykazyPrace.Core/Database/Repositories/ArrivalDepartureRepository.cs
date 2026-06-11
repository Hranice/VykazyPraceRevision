using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ArrivalDepartureRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public ArrivalDepartureRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Vytvoření nového záznamu příchodu/odchodu.
        /// </summary>
        public async Task<ArrivalDeparture> CreateArrivalDepartureAsync(ArrivalDeparture entry)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.ArrivalsDepartures.Add(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return entry;
        }

        public async Task<DateTime?> GetLatestWorkDateAsync(int userId)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.WorkDate)
                .Select(a => (DateTime?)a.WorkDate)
                .SafeFirstOrDefaultAsync();
        }

        public async Task<ArrivalDeparture?> GetExactMatchAsync(
            int userId,
            DateTime workDate,
            DateTime arrival,
            DateTime departure,
            double worked,
            double overtime)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .SafeFirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.WorkDate == workDate.Date &&
                    a.ArrivalTimestamp == arrival &&
                    a.DepartureTimestamp == departure &&
                    Math.Abs(a.HoursWorked - worked) < 0.01 &&
                    Math.Abs(a.HoursOvertime - overtime) < 0.01);
        }

        /// <summary>
        /// Vrátí všechny záznamy uživatele pro daný den.
        /// </summary>
        public async Task<List<ArrivalDeparture>> ListByUserAndDateAsync(int userId, DateTime date)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var day = date.Date;

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.WorkDate == day)
                .OrderBy(a => a.Id)
                .SafeToListAsync();
        }

        /// <summary>
        /// Přesná shoda záznamu včetně případných NULL hodnot a důvodu odchodu.
        /// </summary>
        public async Task<ArrivalDeparture?> GetExactMatchNullableAsync(
            int userId,
            DateTime workDate,
            DateTime? arrival,
            DateTime? departure,
            double worked,
            double overtime,
            string? reason)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var day = workDate.Date;
            var normalizedReason = (reason ?? string.Empty).ToLower();

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .SafeFirstOrDefaultAsync(a =>
                    a.UserId == userId &&
                    a.WorkDate == day &&
                    ((a.ArrivalTimestamp == null && arrival == null) || a.ArrivalTimestamp == arrival) &&
                    ((a.DepartureTimestamp == null && departure == null) || a.DepartureTimestamp == departure) &&
                    Math.Abs(a.HoursWorked - worked) < 0.01 &&
                    Math.Abs(a.HoursOvertime - overtime) < 0.01 &&
                    ((a.DepartureReason ?? string.Empty).ToLower() == normalizedReason));
        }

        /// <summary>
        /// Získání všech záznamů příchodů/odchodů.
        /// </summary>
        public async Task<List<ArrivalDeparture>> GetAllAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Include(a => a.User)
                .OrderBy(a => a.WorkDate)
                .SafeToListAsync();
        }

        /// <summary>
        /// Získání záznamu podle ID.
        /// </summary>
        public async Task<ArrivalDeparture?> GetByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Include(a => a.User)
                .SafeFirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Získání záznamu podle uživatele a data.
        /// Kdyby existovalo víc řádků pro stejný den, vrátí nejnovější.
        /// </summary>
        public async Task<ArrivalDeparture?> GetByUserAndDateAsync(int userId, DateTime date)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var day = date.Date;

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Where(a => a.UserId == userId && a.WorkDate == day)
                .OrderByDescending(a => a.Id)
                .SafeFirstOrDefaultAsync();
        }

        /// <summary>
        /// Uloží změny do předané entity.
        /// </summary>
        public async Task UpdateArrivalDepartureAsync(ArrivalDeparture entity)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existing = await context.ArrivalsDepartures
                .FirstOrDefaultAsync(a => a.Id == entity.Id);

            if (existing != null)
            {
                context.Entry(existing).CurrentValues.SetValues(entity);
            }
            else
            {
                context.ArrivalsDepartures.Attach(entity);
                context.Entry(entity).State = EntityState.Modified;
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(context);
        }

        /// <summary>
        /// Smazání záznamu podle ID.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var entry = await context.ArrivalsDepartures.FindAsync(id);

            if (entry == null)
                return false;

            context.ArrivalsDepartures.Remove(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Získání všech záznamů pro zadaný týden a uživatele.
        /// </summary>
        public async Task<List<ArrivalDeparture>> GetWeekEntriesForUserAsync(int userId, DateTime weekStart)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var weekEnd = weekStart.AddDays(7);

            return await context.ArrivalsDepartures
                .AsNoTracking()
                .Where(a =>
                    a.UserId == userId &&
                    a.WorkDate >= weekStart.Date &&
                    a.WorkDate < weekEnd.Date)
                .OrderBy(a => a.WorkDate)
                .SafeToListAsync();
        }
    }
}
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ArrivalDepartureRepository
    {
        private readonly VykazyPraceContext _context;

        public ArrivalDepartureRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Vytvoření nového záznamu příchodu/odchodu.
        /// </summary>
        public async Task<ArrivalDeparture> CreateArrivalDepartureAsync(ArrivalDeparture entry)
        {
            _context.ArrivalsDepartures.Add(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return entry;
        }

        public async Task<DateTime?> GetLatestWorkDateAsync(int userId)
        {
            return await _context.ArrivalsDepartures
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.WorkDate)
                .Select(a => (DateTime?)a.WorkDate)
                .SafeFirstOrDefaultAsync();
        }

        public async Task<ArrivalDeparture?> GetExactMatchAsync(int userId, DateTime workDate, DateTime arrival, DateTime departure, double worked, double overtime)
        {
            return await _context.ArrivalsDepartures.SafeFirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.WorkDate == workDate.Date &&
                a.ArrivalTimestamp == arrival &&
                a.DepartureTimestamp == departure &&
                Math.Abs(a.HoursWorked - worked) < 0.01 &&
                Math.Abs(a.HoursOvertime - overtime) < 0.01);
        }

        /// <summary>
        /// Vrátí všechny záznamy uživatele pro daný den (kvůli více párům za den).
        /// </summary>
        public async Task<List<ArrivalDeparture>> ListByUserAndDateAsync(int userId, DateTime date)
        {
            var day = date.Date;
            return await _context.ArrivalsDepartures
                .Where(a => a.UserId == userId && a.WorkDate == day)
                .OrderBy(a => a.Id)
                .SafeToListAsync();
        }

        /// <summary>
        /// Přesná shoda záznamu včetně případných NULL hodnot a důvodu odchodu.
        /// Užitečné pro deduplikaci jak kompletních, tak jednostranných záznamů.
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
            var day = workDate.Date;

            return await _context.ArrivalsDepartures.SafeFirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.WorkDate == day &&
                ((a.ArrivalTimestamp == null && arrival == null) || a.ArrivalTimestamp == arrival) &&
                ((a.DepartureTimestamp == null && departure == null) || a.DepartureTimestamp == departure) &&
                Math.Abs(a.HoursWorked - worked) < 0.01 &&
                Math.Abs(a.HoursOvertime - overtime) < 0.01 &&
                ((a.DepartureReason ?? string.Empty).ToLower() == (reason ?? string.Empty).ToLower())
            );
        }


        /// <summary>
        /// Získání všech záznamů příchodů/odchodů.
        /// </summary>
        public async Task<List<ArrivalDeparture>> GetAllAsync()
        {
            return await _context.ArrivalsDepartures
                .Include(a => a.User)
                .OrderBy(a => a.WorkDate)
                .SafeToListAsync();
        }

        /// <summary>
        /// Získání záznamu podle ID.
        /// </summary>
        public async Task<ArrivalDeparture?> GetByIdAsync(int id)
        {
            return await _context.ArrivalsDepartures
                .Include(a => a.User)
                .SafeFirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Získání záznamu podle uživatele a data.
        /// Kdyby existovalo víc řádků pro stejný den, vrátí nejnovější (nejvyšší Id).
        /// </summary>
        public async Task<ArrivalDeparture?> GetByUserAndDateAsync(int userId, DateTime date)
        {
            var day = date.Date;
            return await _context.ArrivalsDepartures
                .Where(a => a.UserId == userId && a.WorkDate == day)
                .OrderByDescending(a => a.Id)
                .SafeFirstOrDefaultAsync();
        }


        /// <summary>
        /// Uloží změny do předané entity. Pokud je entity detached, připojí ji.
        /// </summary>
        public async Task UpdateArrivalDepartureAsync(ArrivalDeparture entity)
        {
            var entry = _context.Entry(entity);
            if (entry.State == EntityState.Detached)
            {
                var existing = await _context.ArrivalsDepartures
                    .FirstOrDefaultAsync(a => a.Id == entity.Id);

                if (existing != null)
                {
                    _context.Entry(existing).CurrentValues.SetValues(entity);
                }
                else
                {
                    _context.ArrivalsDepartures.Attach(entity);
                    entry.State = EntityState.Modified;
                }
            }

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Smazání záznamu podle ID.
        /// </summary>
        public async Task<bool> DeleteAsync(int id)
        {
            var entry = await _context.ArrivalsDepartures.FindAsync(id);
            if (entry == null)
                return false;

            _context.ArrivalsDepartures.Remove(entry);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }

        /// <summary>
        /// Získání všech záznamů pro zadaný týden a uživatele.
        /// </summary>
        public async Task<List<ArrivalDeparture>> GetWeekEntriesForUserAsync(int userId, DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);
            return await _context.ArrivalsDepartures
                .Where(a => a.UserId == userId && a.WorkDate >= weekStart && a.WorkDate < weekEnd)
                .OrderBy(a => a.WorkDate)
                .SafeToListAsync();
        }
    }
}

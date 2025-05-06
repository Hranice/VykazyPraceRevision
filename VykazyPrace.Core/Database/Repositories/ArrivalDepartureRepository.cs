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
                .FirstOrDefaultAsync();
        }

        public async Task<ArrivalDeparture?> GetExactMatchAsync(int userId, DateTime workDate, DateTime arrival, DateTime departure, double worked, double overtime)
        {
            return await _context.ArrivalsDepartures.FirstOrDefaultAsync(a =>
                a.UserId == userId &&
                a.WorkDate == workDate.Date &&
                a.ArrivalTimestamp == arrival &&
                a.DepartureTimestamp == departure &&
                Math.Abs(a.HoursWorked - worked) < 0.01 &&
                Math.Abs(a.HoursOvertime - overtime) < 0.01);
        }

        /// <summary>
        /// Získání všech záznamů příchodů/odchodů.
        /// </summary>
        public async Task<List<ArrivalDeparture>> GetAllAsync()
        {
            return await _context.ArrivalsDepartures
                .Include(a => a.User)
                .OrderBy(a => a.WorkDate)
                .ToListAsync();
        }

        /// <summary>
        /// Získání záznamu podle ID.
        /// </summary>
        public async Task<ArrivalDeparture?> GetByIdAsync(int id)
        {
            return await _context.ArrivalsDepartures
                .Include(a => a.User)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        /// <summary>
        /// Získání záznamu podle uživatele a data.
        /// </summary>
        public async Task<ArrivalDeparture?> GetByUserAndDateAsync(int userId, DateTime date)
        {
            return await _context.ArrivalsDepartures
                .FirstOrDefaultAsync(a => a.UserId == userId && a.WorkDate.Date == date.Date);
        }

        /// <summary>
        /// Aktualizace existujícího záznamu.
        /// </summary>
        public async Task<bool> UpdateAsync(ArrivalDeparture updated)
        {
            var existing = await _context.ArrivalsDepartures.FindAsync(updated.Id);
            if (existing == null)
                return false;

            existing.UserId = updated.UserId;
            existing.WorkDate = updated.WorkDate;
            existing.ArrivalTimestamp = updated.ArrivalTimestamp;
            existing.DepartureTimestamp = updated.DepartureTimestamp;
            existing.DepartureReason = updated.DepartureReason;
            existing.HoursWorked = updated.HoursWorked;
            existing.HoursOvertime = updated.HoursOvertime;

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
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
                .ToListAsync();
        }
    }
}

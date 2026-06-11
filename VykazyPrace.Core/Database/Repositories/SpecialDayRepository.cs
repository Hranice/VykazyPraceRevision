using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class SpecialDayRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public SpecialDayRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Přidání nového speciálního dne.
        /// </summary>
        public async Task<SpecialDay> CreateSpecialDayAsync(SpecialDay specialDay)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            context.SpecialDays.Add(specialDay);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return specialDay;
        }

        /// <summary>
        /// Uzamkne všechny dny v daném měsíci.
        /// Pokud už SpecialDay na dané datum existuje, aktualizuje ho.
        /// </summary>
        public async Task<bool> LockEntireMonthAsync(int month, int year)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            try
            {
                int daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);

                    var existingSpecialDay = await context.SpecialDays
                        .FirstOrDefaultAsync(sd => sd.Date.Date == date.Date);

                    if (existingSpecialDay != null)
                    {
                        existingSpecialDay.Locked = true;
                        existingSpecialDay.Color = "#DCDCDC";
                        existingSpecialDay.Title = "Uzamčeno";
                    }
                    else
                    {
                        var newSpecialDay = new SpecialDay
                        {
                            Date = date,
                            Locked = true,
                            Color = "#DCDCDC",
                            Title = "Uzamčeno"
                        };

                        context.SpecialDays.Add(newSpecialDay);
                    }
                }

                await VykazyPraceContextExtensions.SafeSaveAsync(context);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Získání všech speciálních dnů.
        /// </summary>
        public async Task<List<SpecialDay>> GetAllSpecialDaysAsync()
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.SpecialDays
                .AsNoTracking()
                .OrderBy(sd => sd.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech speciálních dnů pro zadaný týden.
        /// </summary>
        public async Task<List<SpecialDay>> GetSpecialDaysForWeekAsync(DateTime weekStart)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var from = weekStart.Date;
            var to = from.AddDays(7);

            return await context.SpecialDays
                .AsNoTracking()
                .Where(sd => sd.Date.Date >= from && sd.Date.Date < to)
                .OrderBy(sd => sd.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Získání speciálního dne podle ID.
        /// </summary>
        public async Task<SpecialDay?> GetSpecialDayByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.SpecialDays
                .AsNoTracking()
                .FirstOrDefaultAsync(sd => sd.Id == id);
        }

        /// <summary>
        /// Získání speciálního dne podle data.
        /// </summary>
        public async Task<SpecialDay?> GetSpecialDayByDateAsync(DateTime date)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var day = date.Date;

            return await context.SpecialDays
                .AsNoTracking()
                .FirstOrDefaultAsync(sd => sd.Date.Date == day);
        }

        /// <summary>
        /// Aktualizace speciálního dne.
        /// </summary>
        public async Task<bool> UpdateSpecialDayAsync(SpecialDay specialDay)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingDay = await context.SpecialDays.FindAsync(specialDay.Id);

            if (existingDay == null)
                return false;

            existingDay.Date = specialDay.Date;
            existingDay.Title = specialDay.Title;
            existingDay.Locked = specialDay.Locked;
            existingDay.Color = specialDay.Color;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Smazání speciálního dne podle ID.
        /// </summary>
        public async Task<bool> DeleteSpecialDayAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var specialDay = await context.SpecialDays.FindAsync(id);

            if (specialDay == null)
                return false;

            context.SpecialDays.Remove(specialDay);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }
    }
}
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class SpecialDayRepository
    {
        private readonly VykazyPraceContext _context;

        public SpecialDayRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Přidání nového speciálního dne.
        /// </summary>
        public async Task<SpecialDay> CreateSpecialDayAsync(SpecialDay specialDay)
        {
            _context.SpecialDays.Add(specialDay);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return specialDay;
        }

        /// <summary>
        /// Uzamkne všechny dny v daném měsíci (vytvoří SpecialDay pro každý den).
        /// Pokud už SpecialDay na dané datum existuje, aktualizuje ho.
        /// </summary>
        /// <param name="month">Číslo měsíce (1-12)</param>
        /// <param name="year">Rok (např. 2025)</param>
        public async Task<bool> LockEntireMonthAsync(int month, int year)
        {
            try
            {
                var firstDayOfMonth = new DateTime(year, month, 1);
                var daysInMonth = DateTime.DaysInMonth(year, month);

                for (int day = 1; day <= daysInMonth; day++)
                {
                    var date = new DateTime(year, month, day);
                    var existingSpecialDay = await _context.SpecialDays
                        .FirstOrDefaultAsync(sd => sd.Date.Date == date.Date);

                    if (existingSpecialDay != null)
                    {
                        // Aktualizuj existující SpecialDay
                        existingSpecialDay.Locked = true;
                    }
                    else
                    {
                        // Přidej nový SpecialDay
                        var newSpecialDay = new SpecialDay
                        {
                            Date = date,
                            Locked = true,
                            Color = "#DCDCDC",
                            Title = "Uzamčeno"
                        };
                        _context.SpecialDays.Add(newSpecialDay);
                    }
                }

                await VykazyPraceContextExtensions.SafeSaveAsync(_context);
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
            return await _context.SpecialDays
                .OrderBy(sd => sd.Date)
                .ToListAsync();
        }

        /// <summary>
        /// Získání všech speciálních dnů pro zadaný týden (od pondělí do neděle).
        /// </summary>
        /// <param name="weekStart">Datum začátku týdne (pondělí).</param>
        public async Task<List<SpecialDay>> GetSpecialDaysForWeekAsync(DateTime weekStart)
        {
            var weekEnd = weekStart.AddDays(7);

            return await _context.SpecialDays
                .Where(sd => sd.Date.Date >= weekStart.Date && sd.Date.Date < weekEnd.Date)
                .OrderBy(sd => sd.Date)
                .ToListAsync();
        }


        /// <summary>
        /// Získání speciálního dne podle ID.
        /// </summary>
        public async Task<SpecialDay?> GetSpecialDayByIdAsync(int id)
        {
            return await _context.SpecialDays
                .FirstOrDefaultAsync(sd => sd.Id == id);
        }

        /// <summary>
        /// Získání speciálního dne podle data.
        /// </summary>
        public async Task<SpecialDay?> GetSpecialDayByDateAsync(DateTime date)
        {
            return await _context.SpecialDays
                .FirstOrDefaultAsync(sd => sd.Date.Date == date.Date);
        }

        /// <summary>
        /// Aktualizace speciálního dne.
        /// </summary>
        public async Task<bool> UpdateSpecialDayAsync(SpecialDay specialDay)
        {
            var existingDay = await _context.SpecialDays.FindAsync(specialDay.Id);
            if (existingDay == null)
                return false;

            existingDay.Date = specialDay.Date;
            existingDay.Title = specialDay.Title;
            existingDay.Locked = specialDay.Locked;
            existingDay.Color = specialDay.Color;

            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }

        /// <summary>
        /// Smazání speciálního dne podle ID.
        /// </summary>
        public async Task<bool> DeleteSpecialDayAsync(int id)
        {
            var specialDay = await _context.SpecialDays.FindAsync(id);
            if (specialDay == null)
                return false;

            _context.SpecialDays.Remove(specialDay);
            await VykazyPraceContextExtensions.SafeSaveAsync(_context);
            return true;
        }
    }
}

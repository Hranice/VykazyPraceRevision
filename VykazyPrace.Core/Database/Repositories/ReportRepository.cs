using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ReportRepository
    {
        private readonly VykazyPraceContext _context;

        public ReportRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Vrací přehled hodin pro konkrétního uživatele v rozmezí fromDate–toDate.
        /// </summary>
        public async Task<UserTimeReport?> GetUserTimeReportAsync(int userId, DateTime fromDate, DateTime toDate)
        {
            // 1) ReportedHours bez dovolené (6), svačiny (7) a lékaře (24)
            double reportedHours = await _context.TimeEntries
                .Where(te => te.UserId == userId
                          && te.Timestamp.HasValue
                          && te.Timestamp.Value.Date >= fromDate.Date
                          && te.Timestamp.Value.Date <= toDate.Date
                          && te.EntryTypeId != 6
                          && te.EntryTypeId != 7
                          && te.EntryTypeId != 24)
                .SumAsync(te => (double)te.EntryMinutes)
                / 60.0;

            // 2) Actual + Overtime beze změny
            var adData = await _context.ArrivalsDepartures
                .Where(ad => ad.UserId == userId
                          && ad.WorkDate.Date >= fromDate.Date
                          && ad.WorkDate.Date <= toDate.Date)
                .GroupBy(ad => ad.UserId)
                .Select(g => new
                {
                    Actual = g.Sum(ad => ad.HoursWorked),
                    Overtime = g.Sum(ad => ad.HoursOvertime)
                })
                .FirstOrDefaultAsync();

            // 3) Uživatel
            var user = await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => new { Name = u.FirstName + " " + u.Surname })
                .FirstOrDefaultAsync();
            if (user == null) return null;

            double actual = adData?.Actual ?? 0;
            double overtime = adData?.Overtime ?? 0;

            return new UserTimeReport
            {
                UserId = userId,
                UserName = user.Name,
                ReportedHours = reportedHours,
                ActualHours = actual,
                OvertimeHours = overtime,
                MissingHours = actual - reportedHours
            };
        }


        /// <summary>
        /// Vrátí fond hodin (pracovní dny * dailyHours minus dovolené a locked special days).
        /// </summary>
        /// <param name="userId">ID uživatele</param>
        /// <param name="fromDate">početod data (včetně)</param>
        /// <param name="toDate">do data (včetně)</param>
        /// <param name="dailyHours">počet hodin na jeden pracovní den (default 7.5)</param>
        public async Task<double> GetHourFundAsync(int userId, DateTime fromDate, DateTime toDate, double dailyHours = 7.5)
        {
            // 1) Spočítat počet pracovních dní (PO–PÁ) v rozsahu
            int businessDays = GetBusinessDays(fromDate, toDate);

            // 2) Spočítat počet dní dovolené (entryTypeId == 6) pro daného uživatele
            int vacationDays = await _context.TimeEntries
                .Where(te => te.UserId == userId
                          && te.EntryTypeId == 6
                          && te.Timestamp.HasValue
                          && te.Timestamp.Value.Date >= fromDate.Date
                          && te.Timestamp.Value.Date <= toDate.Date)
                .Select(te => te.Timestamp!.Value.Date)
                .Distinct()
                .CountAsync();

            // 3) Spočítat počet zablokovaných speciálních dnů (SpecialDay.Locked == true)
            int specialDays = await _context.SpecialDays
                .Where(sd => sd.Locked
                          && sd.Date.Date >= fromDate.Date
                          && sd.Date.Date <= toDate.Date)
                .CountAsync();

            // 4) Výsledný počet dní a fond hodin
            int effectiveDays = businessDays - vacationDays - specialDays;
            if (effectiveDays < 0) effectiveDays = 0;

            return effectiveDays * dailyHours;
        }

        /// <summary>
        /// Pomocná metoda pro spočtení počtu pracovních dní (PO–PÁ) mezi dvěma daty včetně.
        /// </summary>
        private int GetBusinessDays(DateTime start, DateTime end)
        {
            if (start > end) return 0;

            int totalDays = (end.Date - start.Date).Days + 1;
            int fullWeeks = totalDays / 7;
            int businessDays = fullWeeks * 5;

            int extraDays = totalDays % 7;
            int startDow = (int)start.DayOfWeek;
            for (int i = 0; i < extraDays; i++)
            {
                var dow = (DayOfWeek)((startDow + i) % 7);
                if (dow != DayOfWeek.Saturday && dow != DayOfWeek.Sunday)
                    businessDays++;
            }

            return businessDays;
        }
    }
}
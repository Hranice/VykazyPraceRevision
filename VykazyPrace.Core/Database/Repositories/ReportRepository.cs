using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ReportRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public ReportRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Vrací přehled hodin pro konkrétního uživatele v rozmezí fromDate–toDate.
        /// </summary>
        public async Task<UserTimeReport?> GetUserTimeReportAsync(
            int userId,
            DateTime fromDate,
            DateTime toDate)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var from = fromDate.Date;
            var to = toDate.Date;

            // 1) ReportedHours bez dovolené, svačiny, lékaře a bez IsValid==0
            double reportedMinutes = await context.TimeEntries
                .AsNoTracking()
                .Where(te =>
                    te.UserId == userId &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value.Date >= from &&
                    te.Timestamp.Value.Date <= to &&
                    te.IsValid != 0 &&
                    te.EntryTypeId != WorkLogIds.EntryTypes.Vacation &&
                    te.EntryTypeId != WorkLogIds.EntryTypes.SnackBreak &&
                    te.EntryTypeId != WorkLogIds.EntryTypes.Doctor)
                .SumAsync(te => (double?)te.EntryMinutes) ?? 0;

            double reportedHours = reportedMinutes / 60.0;

            // 2) Actual + Overtime
            var attendanceData = await context.ArrivalsDepartures
                .AsNoTracking()
                .Where(ad =>
                    ad.UserId == userId &&
                    ad.WorkDate.Date >= from &&
                    ad.WorkDate.Date <= to)
                .GroupBy(ad => ad.UserId)
                .Select(g => new
                {
                    Actual = g.Sum(ad => ad.HoursWorked),
                    Overtime = g.Sum(ad => ad.HoursOvertime)
                })
                .FirstOrDefaultAsync();

            // 3) Načíst uživatele
            var user = await context.Users
                .AsNoTracking()
                .Where(u => u.Id == userId)
                .Select(u => new
                {
                    Name = u.FirstName + " " + u.Surname
                })
                .FirstOrDefaultAsync();

            if (user == null)
                return null;

            double actual = attendanceData?.Actual ?? 0;
            double overtime = attendanceData?.Overtime ?? 0;

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
        public async Task<double> GetHourFundAsync(
            int userId,
            DateTime fromDate,
            DateTime toDate,
            double dailyHours = 7.5)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var from = fromDate.Date;
            var to = toDate.Date;

            // 1) Spočítat počet pracovních dní (PO–PÁ) v rozsahu
            int businessDays = GetBusinessDays(from, to);

            // 2) Spočítat počet dní dovolené (entryTypeId == 6) pro daného uživatele
            int vacationDays = await context.TimeEntries
                .AsNoTracking()
                .Where(te =>
                    te.UserId == userId &&
                    te.EntryTypeId == 6 &&
                    te.Timestamp.HasValue &&
                    te.Timestamp.Value.Date >= from &&
                    te.Timestamp.Value.Date <= to)
                .Select(te => te.Timestamp!.Value.Date)
                .Distinct()
                .CountAsync();

            // 3) Spočítat počet zablokovaných speciálních dnů
            int lockedSpecialDays = await context.SpecialDays
                .AsNoTracking()
                .Where(sd =>
                    sd.Locked &&
                    sd.Date.Date >= from &&
                    sd.Date.Date <= to)
                .CountAsync();

            // 4) Výsledný počet dní a fond hodin
            int effectiveDays = businessDays - vacationDays - lockedSpecialDays;

            if (effectiveDays < 0)
                effectiveDays = 0;

            return effectiveDays * dailyHours;
        }

        /// <summary>
        /// Pomocná metoda pro spočtení počtu pracovních dní (PO–PÁ) mezi dvěma daty včetně.
        /// </summary>
        private static int GetBusinessDays(DateTime start, DateTime end)
        {
            if (start > end)
                return 0;

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

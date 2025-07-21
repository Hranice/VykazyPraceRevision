using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Helpers
{
    public class DateRange
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public static class DateRangeHelper
    {
        /// <summary>
        /// Vrátí první a poslední den zadaného měsíce.
        /// </summary>
        /// <param name="year">Rok (např. 2025)</param>
        /// <param name="month">Měsíc (1–12)</param>
        public static DateRange GetMonthRange(int year, int month)
        {
            var firstDay = new DateTime(year, month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            return new DateRange
            {
                FromDate = firstDay.Date,
                ToDate = lastDay.Date
            };
        }

        /// <summary>
        /// Vrátí první a poslední den měsíce obsahujícího zadané datum.
        /// </summary>
        /// <param name="anyDate">Libovolné datum v požadovaném měsíci</param>
        public static DateRange GetMonthRange(DateTime anyDate)
        {
            return GetMonthRange(anyDate.Year, anyDate.Month);
        }
    }
}

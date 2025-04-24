using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar
    {
        private int GetStart(CalendarBlock block) => Grid.GetColumn(block);
        private int GetEnd(CalendarBlock block) => GetStart(block) + Grid.GetColumnSpan(block) - 1;
        private bool IsInRow(CalendarBlock block, int row) => Grid.GetRow(block) == row;

        private int GetColumnAt(double x)
        {
            double accumulatedWidth = 0;
            for (int i = 0; i < CalendarGrid.ColumnDefinitions.Count; i++)
            {
                accumulatedWidth += CalendarGrid.ColumnDefinitions[i].ActualWidth;
                if (x < accumulatedWidth) return i;
            }
            return -1;
        }

        private int GetRowAt(double y)
        {
            double accumulatedHeight = 0;
            for (int i = 0; i < CalendarGrid.RowDefinitions.Count; i++)
            {
                accumulatedHeight += CalendarGrid.RowDefinitions[i].ActualHeight;
                if (y < accumulatedHeight) return i;
            }
            return -1;
        }

        private bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }

        private (DateTime start, DateTime end) GetCurrentWeekBounds(DateTime reference)
        {
            int offset = (int)reference.DayOfWeek - 1;
            if (offset < 0) offset = 6; // pokud je neděle, začíná pondělí 6 dní zpět

            var startOfWeek = reference.Date.AddDays(-offset);
            var endOfWeek = startOfWeek.AddDays(7).AddSeconds(-1); // poslední vteřina neděle

            return (startOfWeek, endOfWeek);
        }

        private DateTime GetStartOfWeek(DateTime reference)
        {
            int offset = (int)reference.DayOfWeek - 1;
            if (offset < 0) offset = 6; // neděle → pondělí -6
            return reference.Date.AddDays(-offset);
        }
    }
}

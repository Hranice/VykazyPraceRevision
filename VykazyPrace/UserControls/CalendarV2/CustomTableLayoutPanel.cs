using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace VykazyPrace.UserControls.CalendarV2
{
    public class CustomTableLayoutPanel : TableLayoutPanel
    {
        public CustomTableLayoutPanel()
        {
            this.DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            int todayIndex = (int)DateTime.Now.DayOfWeek - 1;
            if (todayIndex < 0) todayIndex = 6;

            DateTime now = DateTime.Now;
            int halfHourIndex = (now.Hour * 2) + (now.Minute / 30);

            int[] colWidths = this.GetColumnWidths();
            int[] rowHeights = this.GetRowHeights();

            if (todayIndex < rowHeights.Length)
            {
                int yStart = rowHeights.Take(todayIndex).Sum();
                int rowHeight = rowHeights[todayIndex];

                using (Brush brush = new SolidBrush(Color.FromArgb(230, 230, 230)))
                {
                    e.Graphics.FillRectangle(brush, 0, yStart, this.Width, rowHeight);
                }
            }

            using (Pen pen = new Pen(Color.Gray))
            {
                for (int i = 0; i < this.ColumnCount; i++)
                {
                    int x = colWidths.Take(i).Sum();
                    e.Graphics.DrawLine(pen, x, 0, x, this.Height);
                }
                for (int j = 0; j < this.RowCount; j++)
                {
                    int y = rowHeights.Take(j).Sum();
                    e.Graphics.DrawLine(pen, 0, y, this.Width, y);
                }
            }

            // Vykreslení červené čáry podle aktuálního času
            if (todayIndex < rowHeights.Length && halfHourIndex < colWidths.Length)
            {
                int yPos = rowHeights.Take(todayIndex).Sum();
                int xPos = colWidths.Take(halfHourIndex).Sum();

                using (Pen redPen = new Pen(Color.Red, 1))
                {
                    e.Graphics.DrawLine(redPen, xPos, yPos + 1, xPos, yPos + rowHeights[todayIndex]);
                }
            }
        }
    }
}

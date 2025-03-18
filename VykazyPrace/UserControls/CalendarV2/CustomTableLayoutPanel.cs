using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            using (Pen pen = new Pen(Color.Gray))
            {
                for (int i = 0; i < this.ColumnCount; i++)
                {
                    int x = this.GetColumnWidths().Take(i).Sum();
                    e.Graphics.DrawLine(pen, x, 0, x, this.Height);
                }
                for (int j = 0; j < this.RowCount; j++)
                {
                    int y = this.GetRowHeights().Take(j).Sum();
                    e.Graphics.DrawLine(pen, 0, y, this.Width, y);
                }
            }
        }
    }

}

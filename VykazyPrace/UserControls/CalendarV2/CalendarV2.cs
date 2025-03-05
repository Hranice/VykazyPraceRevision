using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        public CalendarV2()
        {
            InitializeComponent();
        }

        private bool isResizing = false;
        private bool isResizingLeft = false;
        private int startMouseX;
        private int originalColumn;
        private int originalColumnSpan;
        private int tableX;
        private const int ResizeThreshold = 5; // Oblast pro zachycení myši na okraji

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (isResizing)
            {
                int currentMouseX = Cursor.Position.X; // Absolutní X-pozice kurzoru
                int deltaX = currentMouseX - startMouseX;
                int columnWidth = tableLayoutPanel1.Width / tableLayoutPanel1.ColumnCount;

                if (isResizingLeft)
                {
                    int newColumn = originalColumn + deltaX / columnWidth;
                    int newSpan = originalColumnSpan - (newColumn - originalColumn);

                    if (newColumn >= 0 && newSpan > 0 && newColumn + newSpan <= tableLayoutPanel1.ColumnCount)
                    {
                        tableLayoutPanel1.SuspendLayout();
                        tableLayoutPanel1.SetColumn(panel1, newColumn);
                        tableLayoutPanel1.SetColumnSpan(panel1, newSpan);
                        tableLayoutPanel1.ResumeLayout();
                    }
                }
                else
                {
                    int newSpan = originalColumnSpan + deltaX / columnWidth;
                    newSpan = Math.Max(1, Math.Min(tableLayoutPanel1.ColumnCount - originalColumn, newSpan));

                    tableLayoutPanel1.SuspendLayout();
                    tableLayoutPanel1.SetColumnSpan(panel1, newSpan);
                    tableLayoutPanel1.ResumeLayout();
                }
            }
            else
            {
                if (e.X <= ResizeThreshold)
                {
                    Cursor = Cursors.SizeWE;
                    isResizingLeft = true;
                }
                else if (e.X >= panel1.Width - ResizeThreshold)
                {
                    Cursor = Cursors.SizeWE;
                    isResizingLeft = false;
                }
                else
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (Cursor == Cursors.SizeWE)
            {
                isResizing = true;
                startMouseX = Cursor.Position.X; // Uložení absolutní X-pozice kliknutí
                originalColumn = tableLayoutPanel1.GetColumn(panel1);
                originalColumnSpan = tableLayoutPanel1.GetColumnSpan(panel1);
            }
        }

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            isResizing = false;
            Cursor = Cursors.Default;
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            Cursor = Cursors.Default;
        }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.UserControls.Calendar
{
    public partial class CalendarUC : UserControl
    {
        private Dictionary<int, int> _minutesDict = new Dictionary<int, int>();

        public CalendarUC(Dictionary<int, int> minutesDict)
        {
            InitializeComponent();

            _minutesDict = minutesDict;
        }

        private void CalendarUC_Load(object sender, EventArgs e)
        {
            GenerateCalendar(DateTime.Now.Year, DateTime.Now.Month);
        }

        private void AddWeekDaysHeader()
        {
            string[] days = { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle" };

            for (int i = 0; i < 7; i++)
            {
                Label lbl = new Label
                {
                    Text = days[i],
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Margin = new Padding(0),
                    Padding = new Padding(0),
                };
                tableLayoutPanel1.Controls.Add(lbl, i, 0);
            }
        }

        private void GenerateCalendar(int year, int month)
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.ColumnCount = 7;

            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            for (int i = 0; i < 7; i++)
            {
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 14.28f));
            }

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            for (int i = 1; i < 7; i++)
            {
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 16.66f));
            }

            CultureInfo czechCulture = new CultureInfo("cs-CZ");
            labelMonth.Text = czechCulture.DateTimeFormat.GetMonthName(month).ToUpper() + " " + year;

            DateTime startOfTheMonth = new DateTime(year, month, 1);
            int daysInMonth = DateTime.DaysInMonth(year, month);
            int startDayIndex = ((int)startOfTheMonth.DayOfWeek + 6) % 7; // Pondělí jako první den

            int totalCells = 42;
            int dayCounter = 1;

            AddWeekDaysHeader();

            DateTime today = DateTime.Now;

            // Přidání dní do kalendáře
            for (int i = 0; i < totalCells; i++)
            {
                DayUC dayCell = new DayUC();
                dayCell.Dock = DockStyle.Fill;
                dayCell.Margin = new Padding(2);
                dayCell.Padding = new Padding(0);



                if (i >= startDayIndex && dayCounter <= daysInMonth)
                {
                    // Doplníme záznam hodin, pokud existuje
                    if (_minutesDict.TryGetValue(i, out int minutes))
                    {
                        dayCell.labelHours.Text = $"{(minutes / 60.0).ToString("0.0")} h";

                        // Nastavíme barvy podle odpracovaných hodin
                        dayCell.BackColor = Color.FromArgb(255, 230, 230);

                        if (minutes == 450)
                        {
                            dayCell.BackColor = Color.FromArgb(230, 255, 230);
                        }

                        else if (minutes > 450)
                        {
                            dayCell.BackColor = Color.FromArgb(230, 230, 255);
                        }
                    }
                    else
                    {
                        dayCell.labelHours.Text = "";
                    }

                    dayCell.labelDay.Text = dayCounter.ToString();

                    DateTime currentDate = new DateTime(year, month, dayCounter);
                    DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                    // Pokud je sobota nebo neděle, nastavíme červený text
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    {
                        dayCell.labelDay.ForeColor = Color.Tomato;
                        dayCell.BackColor = Color.FromArgb(255, 240, 240);

                        // Pokud někdo šel v neděli, obarvíme modře
                        if (!String.IsNullOrEmpty(dayCell.labelHours.Text))
                        {
                            dayCell.BackColor = Color.FromArgb(240, 240, 255);
                        }
                    }

                    // Pokud je to dnešní den, nastavíme červený rámeček
                    if (dayCounter == today.Day && month == today.Month && year == today.Year)
                    {
                        dayCell.Paint += (sender, e) =>
                        {
                            Control control = sender as Control;
                            using (Pen pen = new Pen(Color.Tomato, 2))
                            {
                                e.Graphics.DrawRectangle(pen, 1, 1, control.Width - 3, control.Height - 3);
                            }
                        };
                    }

                    dayCounter++;
                }
                else
                {
                    dayCell.labelDay.Text = "";
                    dayCell.labelHours.Text = "";
                    dayCell.BackColor = Color.White;
                }

                int row = (i / 7) + 1; // +1 protože první řádek je hlavička
                int col = i % 7;
                tableLayoutPanel1.Controls.Add(dayCell, col, row);
            }
        }




    }
}

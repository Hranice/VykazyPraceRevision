using System.Globalization;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;

namespace VykazyPrace.UserControls.Calendar
{
    public partial class CalendarUC : UserControl
    {
        private int _currentMonth = DateTime.Now.Month;
        private int _currentYear = DateTime.Now.Year;
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly User _currentUser;
        private Dictionary<int, int> _minutesDict = new Dictionary<int, int>();
        private List<DayUC> _dayCells = new List<DayUC>();

        public CalendarUC(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            InitializeCalendarCells();
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

        private async Task LoadTimeEntriesAsync()
        {
            _minutesDict.Clear();

            var timeEntries = await _timeEntryRepo.GetAllTimeEntriesByUserAsync(_currentUser);

            foreach (var entry in timeEntries)
            {
                if (entry.Timestamp.HasValue &&
                    entry.Timestamp.Value.Year == _currentYear &&
                    entry.Timestamp.Value.Month == _currentMonth)
                {
                    int day = entry.Timestamp.Value.Day;

                    if (_minutesDict.ContainsKey(day))
                    {
                        _minutesDict[day] += entry.EntryMinutes;
                    }
                    else
                    {
                        _minutesDict[day] = entry.EntryMinutes;
                    }
                }
            }
        }

        private void CalendarUC_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            Task.Run(ReloadCalendar);
        }

        private async Task ReloadCalendar()
        {
            Invoke(() => _loadingUC.BringToFront());

            await LoadTimeEntriesAsync();
            Invoke(UpdateCalendarCells);

            Invoke(() => _loadingUC.Visible = false);
        }

        private void InitializeCalendarCells()
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

            AddWeekDaysHeader();

            for (int i = 0; i < 42; i++)
            {
                DayUC dayCell = new DayUC();
                dayCell.DoubleClick += DayCell_DoubleClick;
                dayCell.Dock = DockStyle.Fill;
                dayCell.Margin = new Padding(2);
                dayCell.Padding = new Padding(0);

                _dayCells.Add(dayCell);

                int row = (i / 7) + 1; // +1 protože první řádek je hlavička
                int col = i % 7;
                tableLayoutPanel1.Controls.Add(dayCell, col, row);
            }
        }

        private void UpdateCalendarCells()
        {
            CultureInfo czechCulture = new CultureInfo("cs-CZ");
            labelMonth.Text = czechCulture.DateTimeFormat.GetMonthName(_currentMonth).ToUpper() + " " + _currentYear;

            DateTime startOfTheMonth = new DateTime(_currentYear, _currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            int startDayIndex = ((int)startOfTheMonth.DayOfWeek + 6) % 7; // Pondělí jako první den

            int dayCounter = 1;
            DateTime today = DateTime.Now;

            for (int i = 0; i < _dayCells.Count; i++)
            {
                DayUC dayCell = _dayCells[i];

                if (i >= startDayIndex && dayCounter <= daysInMonth)
                {
                    dayCell.labelDay.Text = dayCounter.ToString();
                    dayCell.labelHours.Text = _minutesDict.TryGetValue(dayCounter, out int minutes) ? $"{(minutes / 60.0):0.0} h" : "";

                    DateTime currentDate = new DateTime(_currentYear, _currentMonth, dayCounter);
                    DayOfWeek dayOfWeek = currentDate.DayOfWeek;

                    // Pokud je den v budoucnosti a počet minut je 0, nastavíme barvu na Control
                    if (currentDate > today && minutes == 0)
                    {
                        dayCell.BackColor = SystemColors.Control;
                    }
                    else
                    {
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

                    // Pokud je sobota nebo neděle, nastavíme červený text
                    if (dayOfWeek == DayOfWeek.Saturday || dayOfWeek == DayOfWeek.Sunday)
                    {
                        dayCell.labelDay.ForeColor = Color.Tomato;
                        dayCell.BackColor = Color.FromArgb(255, 240, 240);
                        if (!string.IsNullOrEmpty(dayCell.labelHours.Text))
                        {
                            dayCell.BackColor = Color.FromArgb(240, 240, 255);
                        }
                    }

                    // Zvýraznění dnešního dne
                    dayCell.Paint -= HighlightToday;
                    if (dayCounter == today.Day
                        && _currentMonth == today.Month
                        && _currentYear == today.Year)
                    {
                        dayCell.Paint += HighlightToday;
                        dayCell.Invalidate();
                    }
                    else
                    {
                        dayCell.Paint -= HighlightToday;
                        dayCell.Invalidate();
                    }

                    dayCounter++;
                }
                else
                {
                    dayCell.labelDay.Text = "";
                    dayCell.labelHours.Text = "";
                    dayCell.BackColor = Color.White;
                }
            }
        }

        private void HighlightToday(object sender, PaintEventArgs e)
        {
            Control control = sender as Control;
            using (Pen pen = new Pen(Color.Tomato, 2))
            {
                e.Graphics.DrawRectangle(pen, 1, 1, control.Width - 3, control.Height - 3);
            }
        }


        private void DayCell_DoubleClick(object? sender, EventArgs e)
        {
            var dayCell = sender as DayUC;
            if (int.TryParse(dayCell?.labelDay.Text, out int day))
            {
                new TimeEntryDialog(_currentUser, new DateTime(_currentYear, _currentMonth, day)).ShowDialog();
            }

            Task.Run(ReloadCalendar);
        }

        private void labelPreviousMonth_Click(object sender, EventArgs e)
        {
            _currentMonth--;

            if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }

            Task.Run(ReloadCalendar);
        }

        private void labelNextMonth_Click(object sender, EventArgs e)
        {
            _currentMonth++;

            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }

            Task.Run(ReloadCalendar);
        }
    }
}

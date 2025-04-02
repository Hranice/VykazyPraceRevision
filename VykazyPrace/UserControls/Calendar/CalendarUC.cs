using System.Diagnostics;
using System.Globalization;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;

namespace VykazyPrace.UserControls.Calendar
{
    public partial class CalendarUC : UserControl
    {
        private int _currentMonth;
        private int _currentYear;
        private readonly LoadingUC _loadingUC = new();
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private User _selectedUser;
        private readonly Dictionary<int, int> _minutesDict = new();
        private readonly List<DayUC> _dayCells = new();

        public CalendarUC(User currentUser)
        {
            InitializeComponent();
            _selectedUser = currentUser;
            _currentMonth = DateTime.Now.Month;
            _currentYear = DateTime.Now.Year;
            InitializeCalendar();
        }

        private void InitializeCalendar()
        {
            InitializeCalendarCells();
            AddWeekDaysHeader();
        }

        private void AddWeekDaysHeader()
        {
            var days = new[] { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle" };

            for (int i = 0; i < days.Length; i++)
            {
                var lbl = new Label
                {
                    Text = days[i],
                    TextAlign = ContentAlignment.MiddleCenter,
                    Dock = DockStyle.Fill,
                    Margin = Padding.Empty,
                    Padding = Padding.Empty
                };
                tableLayoutPanel1.Controls.Add(lbl, i, 0);
            }
        }

        private async Task LoadTimeEntriesAsync()
        {
            _minutesDict.Clear();

            var timeEntries = await _timeEntryRepo.GetAllTimeEntriesByUserAsync(_selectedUser);

            foreach (var entry in timeEntries.Where(e => e.Timestamp.HasValue &&
                                                         e.Timestamp.Value.Year == _currentYear &&
                                                         e.Timestamp.Value.Month == _currentMonth))
            {
                int day = entry.Timestamp.Value.Day;
                _minutesDict.TryGetValue(day, out int currentMinutes);
                _minutesDict[day] = currentMinutes + entry.EntryMinutes;
            }
        }

        private void CalendarUC_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);
            Task.Run(ReloadCalendar);
        }

        public async Task ReloadCalendar()
        {
            Invoke(() => _loadingUC.Visible = true);

            await LoadTimeEntriesAsync();

            Invoke(() =>
            {
                UpdateCalendarCells();
                _loadingUC.Visible = false;
            });
        }


        private void InitializeCalendarCells()
        {
            tableLayoutPanel1.Controls.Clear();
            tableLayoutPanel1.RowCount = 7;
            tableLayoutPanel1.ColumnCount = 7;
            tableLayoutPanel1.ColumnStyles.Clear();
            tableLayoutPanel1.RowStyles.Clear();

            for (int i = 0; i < 7; i++)
                tableLayoutPanel1.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100f / 7));

            tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));

            for (int i = 1; i < 7; i++)
                tableLayoutPanel1.RowStyles.Add(new RowStyle(SizeType.Percent, 100f / 6));

            for (int i = 0; i < 42; i++)
            {
                var dayCell = new DayUC
                {
                    Dock = DockStyle.Fill,
                    Margin = new Padding(2),
                    Padding = Padding.Empty
                };
                dayCell.DoubleClick += DayCell_DoubleClick;
                _dayCells.Add(dayCell);
                tableLayoutPanel1.Controls.Add(dayCell, i % 7, (i / 7) + 1);
            }
        }

        private void UpdateCalendarCells()
        {
            var czechCulture = new CultureInfo("cs-CZ");
            labelMonth.Text = $"{czechCulture.DateTimeFormat.GetMonthName(_currentMonth).ToUpper()} {_currentYear}";

            var startOfTheMonth = new DateTime(_currentYear, _currentMonth, 1);
            int daysInMonth = DateTime.DaysInMonth(_currentYear, _currentMonth);
            int startDayIndex = ((int)startOfTheMonth.DayOfWeek + 6) % 7;

            int dayCounter = 1;
            DateTime today = DateTime.Today;

            for (int i = 0; i < _dayCells.Count; i++)
            {
                var dayCell = _dayCells[i];

                if (i >= startDayIndex && dayCounter <= daysInMonth)
                {
                    DateTime currentDate = new DateTime(_currentYear, _currentMonth, dayCounter);
                    _minutesDict.TryGetValue(dayCounter, out int minutes);

                    dayCell.labelDay.Text = dayCounter.ToString();
                    dayCell.labelHours.Text = minutes > 0 ? $"{minutes / 60.0:0.0} h" : string.Empty;

                    dayCell.BackColor = GetDayColor(currentDate, minutes);
                    dayCell.labelDay.ForeColor = (currentDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                        ? Color.Tomato
                        : SystemColors.ControlText;

                    dayCell.Paint -= HighlightToday;
                    if (currentDate == today)
                        dayCell.Paint += HighlightToday;

                    dayCell.Invalidate();
                    dayCounter++;
                }
                else
                {
                    dayCell.labelDay.Text = string.Empty;
                    dayCell.labelHours.Text = string.Empty;
                    dayCell.BackColor = Color.White;
                }
            }
        }

        private Color GetDayColor(DateTime date, int minutes)
        {
            if (date > DateTime.Today && minutes == 0)
                return SystemColors.Control;

            if (date.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
                return minutes > 0 ? Color.FromArgb(240, 240, 255) : Color.FromArgb(255, 240, 240);

            return minutes switch
            {
                450 => Color.FromArgb(230, 255, 230),
                > 450 => Color.FromArgb(230, 230, 255),
                _ => Color.FromArgb(255, 230, 230)
            };
        }

        private void HighlightToday(object sender, PaintEventArgs e)
        {
            if (sender is Control control)
            {
                using var pen = new Pen(Color.Tomato, 2);
                e.Graphics.DrawRectangle(pen, 1, 1, control.Width - 3, control.Height - 3);
            }
        }

        private void DayCell_DoubleClick(object? sender, EventArgs e)
        {
            if (sender is not DayUC dayCell || !int.TryParse(dayCell.labelDay.Text, out int day)) return;
            new TimeEntryDialog(_selectedUser, new DateTime(_currentYear, _currentMonth, day)).ShowDialog();
            Task.Run(ReloadCalendar);
        }

        private void labelPreviousMonth_Click(object sender, EventArgs e) => ChangeMonth(-1);
        private void labelNextMonth_Click(object sender, EventArgs e) => ChangeMonth(1);

        private void ChangeMonth(int offset)
        {
            _currentMonth += offset;
            if (_currentMonth > 12)
            {
                _currentMonth = 1;
                _currentYear++;
            }
            else if (_currentMonth < 1)
            {
                _currentMonth = 12;
                _currentYear--;
            }

            Task.Run(ReloadCalendar);
        }

        public void ChangeUser(User user)
        {
            _selectedUser = user;
            Task.Run(ReloadCalendar);
        }
    }
}

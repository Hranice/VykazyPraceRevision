using System.Globalization;
using System.Runtime.CompilerServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;
using VykazyPrace.UserControls.Calendar;
using VykazyPrace.UserControls.CalendarV2;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        private readonly NotifyIcon _trayIcon = new NotifyIcon();
        private readonly ContextMenuStrip _trayMenu = new ContextMenuStrip();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo = new TimeEntryTypeRepository();
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo = new TimeEntrySubTypeRepository();
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private User _selectedUser = new();
        private int _currentUserLoA = 0;
        private DateTime _selectedDate;
        private CalendarV2 _calendar;

        public MainForm()
        {
            InitializeComponent();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            _selectedDate = DateTime.Now;
            labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);

            _ = Task.Run(LoadDataAsync);
        }

        private async void MainForm_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                await _calendar.DeleteRecord();
            }
        }

        private async Task LoadDataAsync()
        {
            Invoke(() => _loadingUC.BringToFront());

            var users = await _userRepo.GetAllUsersAsync();
            _selectedUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();
            _currentUserLoA = _selectedUser.LevelOfAccess;

            if (_selectedUser.Id == 0)
            {
                AppLogger.Error("Nepodaøilo se naèíst aktuálního uživatele, pøístup bude omezen.");
                return;
            }

            Invoke(() => InitializeCalendar(users));
        }

        private void InitializeCalendar(List<User> users)
        {
            // Pùvodní CalendarUC do panelCalendarContainer
            var calendarUC = new CalendarUC(_selectedUser)
            {
                Dock = DockStyle.Fill
            };
            panelCalendarContainer.Controls.Add(calendarUC);

            // Nový CalendarV2 do panelContainer
            _calendar = new CalendarV2(_selectedUser, _timeEntryRepo, _timeEntryTypeRepo, _timeEntrySubTypeRepo, _projectRepo, _userRepo)
            {
                Dock = DockStyle.Fill,
                Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238),
                Location = new Point(0, 0),
                Name = "calendarV21",
                Size = new Size(1126, 620),
                TabIndex = 0
            };
            panelContainer.Controls.Add(_calendar);

            // Naplnìní comboboxu
            comboBoxUsers.Items.Clear();
            comboBoxUsers.Items.AddRange(users.Select(FormatHelper.FormatUserToString).ToArray());
            comboBoxUsers.SelectedItem = FormatHelper.FormatUserToString(users.FirstOrDefault(x => x.Id == _selectedUser.Id));

            comboBoxUsers.Enabled = _currentUserLoA > 1;

            panelCalendarContainer.Visible = false;
            _calendar.BringToFront();
            _loadingUC.Visible = false;
        }


        private void správaUživatelùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentUserLoA > 1)
            {
                new Dialogs.UserManagementDialog().ShowDialog();
            }

            else
            {
                AppLogger.Error("Na správu uživatelù nemáš dostateèná oprávnìní.");
            }
        }

        private void správaProjektùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentUserLoA > 1)
            {
                new Dialogs.ProjectManagementDialog(_selectedUser).ShowDialog();
            }

            else
            {
                AppLogger.Error("Na správu projektù nemáš dostateèná oprávnìní.");
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ExportDialog(_selectedUser).ShowDialog();
        }

        private void nastaveníToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.SettingsDialog(_selectedUser).ShowDialog();
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            bool showCalendarV2 = radioButton1.Checked;

            _calendar.Visible = showCalendarV2;
            panelCalendarContainer.Visible = !showCalendarV2;

            if (showCalendarV2)
                _calendar.BringToFront();
            else
                panelCalendarContainer.BringToFront();
        }

        private async void comboBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var users = await _userRepo.GetAllUsersAsync();

            var selectedName = comboBoxUsers.SelectedItem?.ToString();
            _selectedUser = users.FirstOrDefault(x => FormatHelper.FormatUserToString(x) == selectedName) ?? new User();

            _calendar?.ChangeUser(_selectedUser);
        }

        private async void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                _selectedDate = await _calendar.ChangeToPreviousWeek();
                labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);
            }
        }

        private async void buttonNext_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                _selectedDate = await _calendar.ChangeToNextWeek();
                labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);
            }
        }

        private void testToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            new TestDialog().ShowDialog();
        }

        private async void správaIndexùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new TimeEntrySubTypeManagement(_selectedUser, _timeEntrySubTypeRepo).ShowDialog();
            await _calendar.ForceReloadAsync();
        }
    }
}

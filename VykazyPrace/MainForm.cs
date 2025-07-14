using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Dialogs;
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
        private readonly SpecialDayRepository _specialDayRepo = new SpecialDayRepository();
        private readonly ArrivalDepartureRepository _arrivalDepartureRepo = new ArrivalDepartureRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private User _selectedUser = new();
        private int _currentUserLoA = 0;
        private DateTime _selectedDate;
        private CalendarV2 _calendar;
        private CalendarUC _monthlyCalendar;

        // Notifications
        private System.Windows.Forms.Timer _notificationTimer;
        private DateTime? _lastNotificationDate = null;

        public MainForm()
        {
            InitializeComponent();

            zobrazitToolStripMenuItem.Click += new System.EventHandler(zobrazitToolStripMenuItem_Click);
            ukoncitToolStripMenuItem.Click += new System.EventHandler(ukoncitToolStripMenuItem_Click);

        }

        private void zobrazitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            ShowFromTray();
        }

        private void ukoncitToolStripMenuItem_Click(object? sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Application.Exit();
        }


        private async void MainForm_Load(object sender, EventArgs e)
        {
            InitFormUI();

            await HandleUpdatesAsync();

            if (!ValidateDatabase())
            {
                ShowSettingsDialog("Databáze je neplatná nebo ji nelze naèíst. Chcete otevøít nastavení?", "Chyba databáze");
                Close();
                return;
            }

            _ = Task.Run(LoadDataAsync);

            _notificationTimer = new System.Windows.Forms.Timer();
            _notificationTimer.Interval = 60 * 1000;
            _notificationTimer.Tick += NotificationTimer_Tick;
            _notificationTimer.Start();

            Enabled = true;
        }

        private void NotificationTimer_Tick(object? sender, EventArgs e)
        {
            var config = ConfigService.Load();

            if (!config.NotificationOn)
                return;

            var now = DateTime.Now;
            var todayTarget = new DateTime(now.Year, now.Month, now.Day,
                                            config.NotificationTime.Hour,
                                            config.NotificationTime.Minute, 0);

            if (_lastNotificationDate != DateTime.Today && now >= todayTarget && now < todayTarget.AddMinutes(1))
            {
                notifyIcon1.BalloonTipTitle = config.NotificationTitle;
                notifyIcon1.BalloonTipText = config.NotificationText;
                notifyIcon1.BalloonTipIcon = ToolTipIcon.Warning;
                notifyIcon1.ShowBalloonTip(5000);

                _lastNotificationDate = DateTime.Today;
            }
        }


        private void InitFormUI()
        {
            var config = ConfigService.Load();
            WindowState = config.AppMaximized ? FormWindowState.Maximized : FormWindowState.Normal;

            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            _selectedDate = DateTime.Now;
            labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);

            Enabled = false;
        }

        private async Task HandleUpdatesAsync()
        {
            UpdateService.CheckForUpdateMessage();
            await UpdateService.CheckForUpdateAsync();
        }

        private bool ValidateDatabase()
        {
            try
            {
                using var testContext = new VykazyPraceContext();
                VykazyPrace.Core.Database.DatabaseValidator.ValidateStructure(testContext);
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Databáze je neplatná nebo ji nelze naèíst.", ex);
                return false;
            }
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
            try
            {
                Invoke(() => _loadingUC.BringToFront());

                var powerKeyHelper = new PowerKeyHelper();
                int totalRows = await powerKeyHelper.DownloadArrivalsDeparturesAsync(DateTime.Now);
                AppLogger.Information($"Staženo {totalRows} záznamù pro mìsíc è.{DateTime.Now.Month}.", false);

                var users = await _userRepo.GetAllUsersAsync();
                _selectedUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();
                _currentUserLoA = _selectedUser.LevelOfAccess;

                if (_selectedUser.Id == 0)
                {
                    AppLogger.Error("Nepodaøilo se naèíst aktuálního uživatele, pøístup bude omezen.");
                    return;
                }

                Invoke(() =>
                {
                    SetupUiForAccessLevel(_currentUserLoA);
                    InitializeCalendar(users);
                });
            }
            catch (Microsoft.Data.Sqlite.SqliteException ex)
            {
                AppLogger.Error("Databáze není dostupná nebo se ji nepodaøilo otevøít.", ex);
                ShowSettingsDialog("Pøejete si otevøít nastavení?", "Databáze není dostupná.");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Došlo k neoèekávané chybì pøi naèítání dat.", ex);
                ShowSettingsDialog("Pøejete si otevøít nastavení?", "Chyba pøi naèítání dat.");
            }
        }

        private void SetupUiForAccessLevel(int levelOfAccess)
        {
            if (levelOfAccess == 3)
            {
                dataToolStripMenuItem.Visible = true;
                uživateléToolStripMenuItem.Visible = true;
                správaProjektùToolStripMenuItem.Visible = true;
                comboBoxUsers.Visible = true;
                správceToolStripMenuItem.Visible = true;
            }
            else if (levelOfAccess == 2)
            {
                dataToolStripMenuItem.Visible = true;
                správaProjektùToolStripMenuItem.Visible = true;
                comboBoxUsers.Visible = true;
            }
        }

        private void ShowSettingsDialog(string message, string caption)
        {
            Invoke(() =>
            {
                var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    new Dialogs.SettingsDialog(_selectedUser).ShowDialog();
                }

                else
                {
                    AppLogger.Information("Aplikace se nyní ukonèí.", true);
                    Environment.Exit(0);
                }
            });
        }



        private void InitializeCalendar(List<User> users)
        {
            // Pùvodní CalendarUC do panelCalendarContainer
            _monthlyCalendar = new CalendarUC(_selectedUser)
            {
                Dock = DockStyle.Fill
            };
            panelCalendarContainer.Controls.Add(_monthlyCalendar);

            // Nový CalendarV2 do panelContainer
            _calendar = new CalendarV2(_selectedUser,
                                       _timeEntryRepo,
                                       _timeEntryTypeRepo,
                                       _timeEntrySubTypeRepo,
                                       _projectRepo,
                                       _userRepo,
                                       _specialDayRepo,
                                       _arrivalDepartureRepo)
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

        private async void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            bool showCalendarV2 = radioButton1.Checked;

            _calendar.Visible = showCalendarV2;
            panelCalendarContainer.Visible = !showCalendarV2;

            if (showCalendarV2)
            {
                _calendar.BringToFront();
                buttonPrevious.Visible = true;
                buttonNext.Visible = true;
                labelSelectedDate.Visible = true;
                buttonNow.Visible = true;
            }
            else
            {
                buttonPrevious.Visible = false;
                buttonNext.Visible = false;
                labelSelectedDate.Visible = false;
                buttonNow.Visible = false;
                panelCalendarContainer.BringToFront();
                await _monthlyCalendar.ReloadCalendar();
            }
        }

        private async void comboBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            var users = await _userRepo.GetAllUsersAsync();

            var selectedName = comboBoxUsers.SelectedItem?.ToString();
            _selectedUser = users.FirstOrDefault(x => FormatHelper.FormatUserToString(x) == selectedName) ?? new User();

            _calendar?.ChangeUser(_selectedUser);
            _monthlyCalendar.ChangeUser(_selectedUser);
        }

        private async void buttonPrevious_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                _selectedDate = await _calendar.ChangeToPreviousWeek();
                labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);
                if (_selectedDate.Date == DateTime.Today)
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Bold);
                }

                else
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Regular);
                }
            }
        }

        private async void buttonNext_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                _selectedDate = await _calendar.ChangeToNextWeek();
                labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);
                if (_selectedDate.Date == DateTime.Today)
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Bold);
                }

                else
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Regular);
                }
            }
        }

        private async void buttonNow_Click(object sender, EventArgs e)
        {
            if (radioButton1.Checked)
            {
                _selectedDate = await _calendar.ChangeToTodaysWeek();
                labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);
                if (_selectedDate.Date == DateTime.Today)
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Bold);
                }

                else
                {
                    labelSelectedDate.Font = new Font(labelSelectedDate.Font, FontStyle.Regular);
                }
            }
        }

        private void testToolStripMenuItem_Click_1(object sender, EventArgs e)
        {
            new TestDialog().ShowDialog();
        }

        private async void správaIndexùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new TimeEntrySubTypeManagement(_selectedUser, _timeEntrySubTypeRepo, _timeEntryRepo).ShowDialog();
            await _calendar.ForceReloadAsync();
        }

        private void oProgramuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = ConfigService.Load();
            config.AppMaximized = this.WindowState == FormWindowState.Maximized;
            ConfigService.Save(config);

            if (config.MinimizeToTray)
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            }
        }

        private void správceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new ManagerDialog().ShowDialog();
        }

        private void návrhProjektuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ProposeProjectDialog(_selectedUser).ShowDialog();
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowFromTray();
        }

        public void ShowFromTray()
        {
            this.Invoke(async () =>
            {
                this.Show();
                this.WindowState = FormWindowState.Normal;
                this.BringToFront();
                await _calendar.ForceReloadIndicators();
            });
        }

    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Import;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Dialogs;
using VykazyPrace.Enums;
using VykazyPrace.Updater;
using VykazyPrace.UserControls;
using VykazyPrace.UserControls.Calendar;
using VykazyPrace.UserControls.CalendarV2;
using DateRangeHelper = VykazyPrace.Core.Helpers.DateRangeHelper;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigService _configService;

        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        private readonly UserRepository _userRepo;
        private readonly UserGroupRepository _userGroupRepo;
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private readonly ProjectRepository _projectRepo;
        private readonly SpecialDayRepository _specialDayRepo;
        private readonly ArrivalDepartureRepository _arrivalDepartureRepo;
        private readonly PowerKeyHelper _powerKeyHelper;
        private readonly ExternalTimeEntryImportService _externalImportService;

        private readonly LoadingUC _loadingUC = new LoadingUC();
        private User _selectedUser = new();
        private User _loggedUser = new();
        private int _currentUserLoA = 0;
        private DateTime _selectedDate;
        private UserSelectionDialog? _userSelectionDialog;
        private CalendarV2 _calendar;
        private CalendarUC _monthlyCalendar;

        private List<User> _users = new();
        private bool _isSwitchingUser = false;


        // Notifications
        private System.Windows.Forms.Timer _notificationTimer;
        private DateTime? _lastNotificationDate = null;

        // Update
        private UpdateInfo? _cachedUpdateInfo;

        public MainForm(
     IServiceProvider serviceProvider,
     IConfigService configService,
     IDbContextFactory<VykazyPraceContext> contextFactory,
     UserRepository userRepo,
     UserGroupRepository userGroupRepo,
     TimeEntryRepository timeEntryRepo,
     TimeEntryTypeRepository timeEntryTypeRepo,
     TimeEntrySubTypeRepository timeEntrySubTypeRepo,
     ProjectRepository projectRepo,
     SpecialDayRepository specialDayRepo,
     ArrivalDepartureRepository arrivalDepartureRepo,
     PowerKeyHelper powerKeyHelper,
     ExternalTimeEntryImportService externalImportService)
        {
            _serviceProvider = serviceProvider;
            _configService = configService;
            _contextFactory = contextFactory;

            _userRepo = userRepo;
            _userGroupRepo = userGroupRepo;
            _timeEntryRepo = timeEntryRepo;
            _timeEntryTypeRepo = timeEntryTypeRepo;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepo;
            _projectRepo = projectRepo;
            _specialDayRepo = specialDayRepo;
            _arrivalDepartureRepo = arrivalDepartureRepo;
            _powerKeyHelper = powerKeyHelper;
            _externalImportService = externalImportService;

            InitializeComponent();

            zobrazitToolStripMenuItem.Click += zobrazitToolStripMenuItem_Click;
            ukoncitToolStripMenuItem.Click += ukoncitToolStripMenuItem_Click;
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

            _ = Task.Run(HandleUpdatesAsync);

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
            var config = _configService.Current;

            if (!config.NotificationOn)
                return;

            var now = DateTime.Now;
            var todayTarget = new DateTime(
                now.Year,
                now.Month,
                now.Day,
                config.NotificationTime.Hour,
                config.NotificationTime.Minute,
                0);

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
            var config = _configService.Current;

            RestoreWindowState(config);

            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);

            KeyPreview = true;
            KeyDown += MainForm_KeyDown;

            _selectedDate = DateTime.Now;
            labelSelectedDate.Text = FormatHelper.GetWeekNumberAndRange(_selectedDate);

            Enabled = false;
        }

        private void RestoreWindowState(AppConfig config)
        {
            var window = config.MainWindow;

            if (window.Width > 0 && window.Height > 0)
            {
                Size = new Size(window.Width, window.Height);
            }

            if (window.X >= 0 && window.Y >= 0)
            {
                var bounds = new Rectangle(window.X, window.Y, window.Width, window.Height);

                bool isVisibleOnAnyScreen = Screen.AllScreens
                    .Any(screen => screen.WorkingArea.IntersectsWith(bounds));

                if (isVisibleOnAnyScreen)
                {
                    StartPosition = FormStartPosition.Manual;
                    Location = new Point(window.X, window.Y);
                }
                else
                {
                    StartPosition = FormStartPosition.CenterScreen;
                }
            }
            else
            {
                StartPosition = FormStartPosition.CenterScreen;
            }

            WindowState = window.Maximized
                ? FormWindowState.Maximized
                : FormWindowState.Normal;
        }

        private async Task HandleUpdatesAsync()
        {
            _cachedUpdateInfo = await UpdateService.GetUpdateInfoAsync();

            if (_cachedUpdateInfo.UpdateAvailable)
            {
                buttonUpdate.BackColor = Color.FromKnownColor(KnownColor.LightGoldenrodYellow);
            }
        }

        private bool ValidateDatabase()
        {
            try
            {
                using var testContext = _contextFactory.CreateDbContext();

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
                Control? focused = this.ContainsFocus ? this.GetFocusedControl(this) : null;

                if (focused is TextBoxBase or ComboBox)
                {
                    return;
                }

                await _calendar.DeleteRecord();
            }
        }

        private Control? GetFocusedControl(Control control)
        {
            foreach (Control child in control.Controls)
            {
                if (child.ContainsFocus)
                {
                    if (child.HasChildren)
                        return GetFocusedControl(child);
                    else
                        return child;
                }
            }

            return control.Focused ? control : null;
        }

        private async Task LoadDataAsync()
        {
            try
            {
                Invoke(() => _loadingUC.BringToFront());

                _users = await _userRepo.GetAllUsersAsync();
                AppLogger.Debug($"Naèteno {_users.Count} záznamù.");
                string userName = Environment.UserName.ToLower();
                _loggedUser = await _userRepo.GetUserByWindowsUsernameAsync(userName) ?? new User();
                _selectedUser = _loggedUser;
                AppLogger.Debug($"Naètení uživatele podle windows už. jména: '{userName}'.");
                _currentUserLoA = _selectedUser.LevelOfAccess;
                AppLogger.Debug($"Naètení uživatelských práv: '{_currentUserLoA}'.");

                if (_selectedUser.Id == 0)
                {
                    AppLogger.Error("Nepodaøilo se naèíst aktuálního uživatele, pøístup bude omezen.");
                    return;
                }

                int totalRows = await _powerKeyHelper.DownloadForUserAsync(DateTime.Now, _selectedUser);
                AppLogger.Information($"Staženo {totalRows} záznamù pro mìsíc è.{DateTime.Now.Month} uživatele {FormatHelper.FormatUserToString(_selectedUser)}.", false);

                // Pokud den pøed tøemi dny spadal do jiného mìsíce, stáhni i ten mìsíc.
                var previousDay = DateTime.Now.AddDays(-3);
                if (previousDay.Month != DateTime.Today.Month || previousDay.Year != DateTime.Today.Year)
                {
                    await _powerKeyHelper.DownloadForUserAsync(previousDay, _selectedUser);
                    AppLogger.Information($"Staženo {totalRows} záznamù pro mìsíc è.{previousDay.Month} uživatele {FormatHelper.FormatUserToString(_selectedUser)}.", false);
                }

                Invoke(() =>
                {
                    SetupUiForAccessLevel(_currentUserLoA);
                    InitializeCalendar(_users);
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
                uživateléToolStripMenuItem.Visible = true;
                správaProjektùToolStripMenuItem.Visible = true;
                bChangeUser.Visible = true;
                správceToolStripMenuItem.Visible = true;
            }
            else if (levelOfAccess == 2)
            {
                správaProjektùToolStripMenuItem.Visible = true;
                bChangeUser.Visible = true;
            }
        }

        private void ShowSettingsDialog(string message, string caption)
        {
            Invoke(() =>
            {
                var result = MessageBox.Show(message, caption, MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Yes)
                {
                    OpenSettings();
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
            _monthlyCalendar = ActivatorUtilities.CreateInstance<CalendarUC>(
                _serviceProvider,
                _selectedUser);

            _monthlyCalendar.Dock = DockStyle.Fill;

            panelCalendarContainer.Controls.Add(_monthlyCalendar);

            _calendar = ActivatorUtilities.CreateInstance<CalendarV2>(
                _serviceProvider,
                _selectedUser);

            _calendar.Dock = DockStyle.Fill;
            _calendar.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            _calendar.Location = new Point(0, 0);
            _calendar.Name = "calendarV21";
            _calendar.Size = new Size(1226, 620);
            _calendar.TabIndex = 0;

            panelContainer.Controls.Add(_calendar);

            bChangeUser.Enabled = _currentUserLoA > 1;
            bChangeUser.Text = _selectedUser != null
                ? FormatHelper.FormatUserToString(_selectedUser)
                : "Vybrat uživatele";

            panelCalendarContainer.Visible = false;
            _calendar.BringToFront();

            HideLoading();
        }

        private void správaUživatelùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_currentUserLoA > 1)
            {
                OpenSettings();
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
                var dialog = ActivatorUtilities.CreateInstance<ProjectManagementDialog>(
    _serviceProvider,
    _selectedUser);

                dialog.ShowDialog(this);
            }

            else
            {
                AppLogger.Error("Na správu projektù nemáš dostateèná oprávnìní.");
            }
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = ActivatorUtilities.CreateInstance<ExportDialog>(
    _serviceProvider,
    _loggedUser);

            dialog.ShowDialog(this);
        }

        private void nastaveníToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenSettings();
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
            using var dialog = ActivatorUtilities.CreateInstance<TestDialog>(
    _serviceProvider);

            dialog.ShowDialog(this);
        }

        private async void správaIndexùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (_selectedUser.Id != _loggedUser.Id && _selectedUser.MasterUserId != _loggedUser.Id)
                return;

            using var dialog = ActivatorUtilities.CreateInstance<TimeEntrySubTypeManagement>(
    _serviceProvider,
    _selectedUser);

            dialog.ShowDialog(this);

            await _calendar.ForceReloadAsync();
        }


        private void oProgramuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            var config = _configService.Current;

            if (config.MinimizeToTray && e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            SaveWindowState(config);
            _configService.Save();
        }

        private void SaveWindowState(AppConfig config)
        {
            var bounds = WindowState == FormWindowState.Normal
                ? Bounds
                : RestoreBounds;

            config.MainWindow.Width = bounds.Width;
            config.MainWindow.Height = bounds.Height;
            config.MainWindow.X = bounds.X;
            config.MainWindow.Y = bounds.Y;
            config.MainWindow.Maximized = WindowState == FormWindowState.Maximized;
        }

        private void správceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = ActivatorUtilities.CreateInstance<ManagerDialog>(
    _serviceProvider);

            dialog.ShowDialog(this);
        }

        private void návrhProjektuToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var proposalProjectDialog = ActivatorUtilities.CreateInstance<ProposeProjectDialog>(
    _serviceProvider,
    _selectedUser);
            proposalProjectDialog.ShowDialog(this);
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            ShowFromTray();
        }

        public void ShowFromTray()
        {
            Invoke(async () =>
            {
                var config = _configService.Current;

                Show();

                WindowState = config.MainWindow.Maximized
                    ? FormWindowState.Maximized
                    : FormWindowState.Normal;

                BringToFront();
                Activate();

                if (_calendar != null)
                    await _calendar.ForceReloadIndicators();
            });
        }

        private async void buttonReloadData_Click(object sender, EventArgs e)
        {
            ShowLoading(); ;
            await _calendar.ForceReloadAsync();
            HideLoading();
        }

        private void pøehledToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = ActivatorUtilities.CreateInstance<OverviewDialog>(
    _serviceProvider,
    _selectedUser, DateRangeHelper.GetMonthRange(_selectedDate));

            dialog.ShowDialog(this);
        }

        private async void buttonOutlookEvents_Click(object sender, EventArgs e)
        {
            var outlookType = Type.GetTypeFromProgID("Outlook.Application");
            if (outlookType == null)
            {
                // Starý Outlook není k dispozici
                MessageBox.Show(
                    "Nebyl nalezen klasický Outlook pro Windows. " +
                    "Nový Outlook 2023+ nepodporuje pøímé propojení s kalendáøem pøes COM.\n\n" +
                    "Pro tuto funkci prosím použijte klasický Outlook pro Windows.",
                    "Outlook integrace",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning
                );
                return;
            }

            var dialog = ActivatorUtilities.CreateInstance<OutlookEvents>(
    _serviceProvider,
    _selectedUser);
            dialog.ShowDialog(this);

            await _calendar.ForceReloadAsync();
        }


        private void ShowLoading()
        {
            _loadingUC.Visible = true;
            _loadingUC.BringToFront();
            UseWaitCursor = true;
            Application.DoEvents();
        }

        private void HideLoading()
        {
            UseWaitCursor = false;
            _loadingUC.Visible = false;
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            new UpdateDialog().ShowDialog();
        }

        private async void importToolStripMenuItem_Click(object sender, EventArgs e)
        {
            using var dialog = new OpenFileDialog
            {
                Title = "Vyberte Excel s externími výkazy",
                Filter = "Excel soubory (*.xlsx)|*.xlsx",
                Multiselect = false
            };

            if (dialog.ShowDialog() != DialogResult.OK)
                return;

            var confirm = MessageBox.Show(
                "Import smaže pùvodní záznamy pro dotèené uživatele a dny a následnì vloží nové záznamy z Excelu. Pokraèovat?",
                "Potvrzení importu",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes)
                return;

            try
            {
                var result = await _externalImportService.ImportAsync(dialog.FileName);

                if (!result.Success)
                {
                    MessageBox.Show(
                        string.Join(Environment.NewLine, result.Errors),
                        "Import se nezdaøil",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);

                    return;
                }

                var message =
                    $"Import dokonèen.{Environment.NewLine}" +
                    $"Smazáno pùvodních záznamù: {result.DeletedCount}{Environment.NewLine}" +
                    $"Importováno nových záznamù: {result.ImportedCount}";

                if (result.Warnings.Count > 0)
                {
                    message += Environment.NewLine + Environment.NewLine +
                               "Upozornìní:" + Environment.NewLine +
                               string.Join(Environment.NewLine, result.Warnings.Take(20));

                    if (result.Warnings.Count > 20)
                        message += Environment.NewLine + $"... a dalších {result.Warnings.Count - 20} upozornìní.";
                }

                MessageBox.Show(
                    message,
                    "Import dokonèen",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ex.Message,
                    "Chyba importu",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void BChangeUser_Click(object sender, EventArgs e)
        {
            if (_isSwitchingUser)
                return;

            if (_userSelectionDialog != null && !_userSelectionDialog.IsDisposed)
            {
                _userSelectionDialog.BringToFront();
                _userSelectionDialog.Activate();
                return;
            }

            var config = _configService.Current;

            IEnumerable<int>? preselectedUserIds = null;

            if (config.UserViewSelection.SelectedUserId.HasValue)
            {
                preselectedUserIds = new[] { config.UserViewSelection.SelectedUserId.Value };
            }
            else if (_selectedUser != null && _selectedUser.Id != 0)
            {
                preselectedUserIds = new[] { _selectedUser.Id };
            }

            _userSelectionDialog = ActivatorUtilities.CreateInstance<UserSelectionDialog>(
                _serviceProvider,
                UserSelectionMode.Single,
                preselectedUserIds,
                config.UserViewSelection.SelectedUserGroupIds);

            _userSelectionDialog.SelectionChanged += UserSelectionDialog_SelectionChanged;
            _userSelectionDialog.FormClosed += UserSelectionDialog_FormClosed;

            _userSelectionDialog.Show(this);
            _userSelectionDialog.Activate();
        }

        private async void UserSelectionDialog_SelectionChanged(
    object? sender,
    UserSelectionChangedEventArgs e)
        {
            if (_isSwitchingUser)
                return;

            if (_selectedUser != null && e.SelectedUser.Id == _selectedUser.Id)
                return;

            var config = _configService.Current;

            config.UserViewSelection.SelectedUserId = e.SelectedUser.Id;
            config.UserViewSelection.SelectedUserGroupIds = e.SelectedUserGroupIds;

            _configService.Save();

            await SwitchSelectedUserAsync(e.SelectedUser);
        }

        private void UserSelectionDialog_FormClosed(object? sender, FormClosedEventArgs e)
        {
            _userSelectionDialog = null;
        }

        private async Task SwitchSelectedUserAsync(User newUser)
        {
            if (_isSwitchingUser) return;
            if (newUser == null || newUser.Id == 0) return;

            if (_selectedUser != null && newUser.Id == _selectedUser.Id)
                return;

            _isSwitchingUser = true;

            try
            {
                _userSelectionDialog?.SetSelectionEnabled(false);
                ShowLoading();

                _selectedUser = newUser;

                bChangeUser.Text = FormatHelper.FormatUserToString(_selectedUser);

                int totalRows = await _powerKeyHelper.DownloadForUserAsync(DateTime.Now, _selectedUser);

                AppLogger.Information(
                    $"Staženo {totalRows} záznamù pro mìsíc è.{DateTime.Now.Month} uživatele {FormatHelper.FormatUserToString(_selectedUser)}.",
                    false);

#if NOTDEBUG
        buttonOutlookEvents.Visible = _selectedUser.WindowsUsername == Environment.UserName;
#endif

                _calendar?.ChangeUser(_selectedUser);
                _monthlyCalendar.ChangeUser(_selectedUser);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba pøi pøepínání uživatele.", ex);
            }
            finally
            {
                HideLoading();
                _userSelectionDialog?.SetSelectionEnabled(true);
                _isSwitchingUser = false;
            }
        }

        private void OpenSettings()
        {
            using var dialog = ActivatorUtilities.CreateInstance<SettingsDialog>(
    _serviceProvider,
    _loggedUser);

            dialog.ShowDialog(this);
        }
    }
}

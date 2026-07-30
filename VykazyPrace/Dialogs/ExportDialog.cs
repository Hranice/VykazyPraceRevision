using ClosedXML.Excel;
using Microsoft.Extensions.DependencyInjection;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Enums;
using DataTable = System.Data.DataTable;
using Excel = Microsoft.Office.Interop.Excel;

/// <summary>
/// ChatGPT 5 credits:
///     názvy proměnných, komentáře, regiony a základ designu excel tabulek
/// </summary>
namespace VykazyPrace.Dialogs
{
    #region === UI vrstvička (WinForms) ===
    /// <summary>
    /// Dialog pro export časových záznamů do Excelu – zjednodušená UI vrstva.
    /// </summary>
    public partial class ExportDialog : Form
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IConfigService _configService;

        private readonly UserRepository _userRepository;
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly SpecialDayRepository _specialDayRepo;

        private readonly User _currentUser;

        private readonly DataTableFactory _tableFactory;
        private readonly ExcelStylingService _styling = new();

        private List<(RadioButton radioButton, Panel panel)> _options = new();
        private List<User> _selectedExportUsers = new();
        private List<User> _availableExportUsers = new();
        private List<User>? _previousAvailableExportUsers;
        private int _exportUsersRefreshVersion;

        private bool _isTreeViewChecking;

        public ExportDialog(
     IServiceProvider serviceProvider,
     IConfigService configService,
     User currentUser,
     TimeEntryRepository timeEntryRepo,
     SpecialDayRepository specialDayRepo,
     UserRepository userRepository,
     DataTableFactory tableFactory)
        {
            _serviceProvider = serviceProvider;
            _configService = configService;
            _currentUser = currentUser;

            _timeEntryRepo = timeEntryRepo;
            _specialDayRepo = specialDayRepo;
            _userRepository = userRepository;
            _tableFactory = tableFactory;

            InitializeComponent();
        }

        private async void ExportDialog_Load(object sender, EventArgs e)
        {
            InitializeDatePickers();
            RestoreExportRangeSelectionFromConfig();
            RegisterExportRangeChangeHandlers();
            await LoadExportUsersSelectionAsync();

            if (_currentUser.LevelOfAccess > 2)
            {
                gBLock.Visible = true;
            }
        }

        private void InitializeDatePickers()
        {
            var firstDayThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var previousMonth = firstDayThisMonth.AddMonths(-1);
            var range = DateRangeHelper.GetMonthRange(previousMonth);

            dtpFrom.Value = range.From;
            dtpTo.Value = range.To;
            cBMonth.SelectedIndex = previousMonth.Month - 1;

            _options = new List<(RadioButton, Panel)>
            {
                (rBSpecificTimePeriod, panelSpecificTimePeriod),
                (rBSpecificWeek, panelSpecificWeek),
                (rBSpecificMonth, panelSpecificMonth),
                (rBSpecificYear, panelSpecificYear)
            };

            nUDWeek.Value = ISOWeek.GetWeekOfYear(DateTime.Now);
            cBMonth2.SelectedIndex = DateTime.Now.Month - 1;
            nUDYear.Value = DateTime.Now.Year;
        }

        private void RestoreExportRangeSelectionFromConfig()
        {
            var config = _configService.Current;
            var exportConfig = config.ExportSelection;

            if (exportConfig == null)
                return;

            cBBuildEvaluationSheet.Checked = exportConfig.BuildEvaluationSheet;

            rBSpecificTimePeriod.Checked = false;
            rBSpecificWeek.Checked = false;
            rBSpecificMonth.Checked = false;
            rBSpecificYear.Checked = false;

            RadioButton targetRadioButton = rBSpecificTimePeriod;

            if (exportConfig.Year.HasValue)
            {
                int year = exportConfig.Year.Value;
                nUDYear.Value = Math.Min(nUDYear.Maximum, Math.Max(nUDYear.Minimum, year));
            }

            switch (exportConfig.SelectedRangeType)
            {
                case ExportRangeType.TimePeriod:
                    targetRadioButton = rBSpecificTimePeriod;

                    if (exportConfig.From.HasValue)
                        dtpFrom.Value = exportConfig.From.Value;

                    if (exportConfig.To.HasValue)
                        dtpTo.Value = exportConfig.To.Value;

                    break;

                case ExportRangeType.Week:
                    targetRadioButton = rBSpecificWeek;

                    if (exportConfig.Week.HasValue)
                    {
                        int week = exportConfig.Week.Value;
                        nUDWeek.Value = Math.Min(nUDWeek.Maximum, Math.Max(nUDWeek.Minimum, week));
                    }

                    break;

                case ExportRangeType.Month:
                    targetRadioButton = rBSpecificMonth;

                    if (exportConfig.Month.HasValue &&
                        exportConfig.Month.Value >= 1 &&
                        exportConfig.Month.Value <= 12)
                    {
                        cBMonth2.SelectedIndex = exportConfig.Month.Value - 1;
                    }

                    break;

                case ExportRangeType.Year:
                    targetRadioButton = rBSpecificYear;
                    break;
            }

            targetRadioButton.Checked = true;
            SelectOption(targetRadioButton);
        }

        private async void bSaveAs_Click(object sender, EventArgs e)
        {
            await RefreshAvailableExportUsersAsync(keepCurrentSelection: true);

            var selectedUsers = GetSelectedExportUsers();

            if (selectedUsers.Count == 0)
            {
                MessageBox.Show(
                    "Není vybraný žádný uživatel.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var sfd = new SaveFileDialog
            {
                Filter = "Excel Files|*.xlsx",
                FileName = "Export.xlsx"
            };

            if (sfd.ShowDialog(this) != DialogResult.OK)
                return;

            var exportService = new TimeEntryExportService(
                _timeEntryRepo,
                _specialDayRepo,
                _tableFactory,
                _styling);

            try
            {
                var (from, to) = GetSelectedExportRange();

                await exportService.ExportAsync(
                    sfd.FileName,
                    from,
                    to,
                    new List<int>(),
                    selectedUsers.Select(u => u.Id).Distinct().ToList(),
                    selectedUsers,
                    _currentUser,
                    cBBuildEvaluationSheet.Checked);

                SaveExportSelectionToConfig();

                MessageBox.Show(
                    "Export byl dokončen.",
                    "Export",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Export selhal. Podrobnosti v logu.\n{ex.Message}",
                    "Chyba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void cboMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var (from, to) = DateRangeHelper.GetMonthRangeByIndex(
                cBMonth.SelectedIndex,
                dtpFrom.Value.Year);

            dtpFrom.Value = from;
            dtpTo.Value = to;
        }

        private async void bLockEntries_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cBMonth.Text))
                return;

            var result = MessageBox.Show(
                $"Zamknout záznamy za měsíc {cBMonth.Text}?",
                "Zamknout data?",
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Warning);

            if (result != DialogResult.Yes)
                return;

            var exportService = new TimeEntryExportService(
                _timeEntryRepo,
                _specialDayRepo,
                _tableFactory,
                _styling);

            try
            {
                await exportService.LockMonthAsync(cBMonth.Text, dtpFrom.Value.Year);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Zámek selhal. Podrobnosti v logu.\n{ex.Message}",
                    "Chyba",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void SelectOption(RadioButton selectedRadioButton)
        {
            foreach (var option in _options)
            {
                bool isSelected = option.radioButton == selectedRadioButton;

                option.radioButton.Checked = isSelected;
                option.panel.BackColor = isSelected
                    ? Color.White
                    : SystemColors.Control;
            }
        }

        private void radioButtonTimePeriod_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                SelectOption(rb);
                _ = RefreshAvailableExportUsersAsync(keepCurrentSelection: true, notifyCountChange: true);
            }
        }

        private void panelTimePeriod_Click(object sender, EventArgs e)
        {
            if (sender == panelSpecificTimePeriod)
                SelectOption(rBSpecificTimePeriod);
            else if (sender == panelSpecificMonth)
                SelectOption(rBSpecificMonth);
            else if (sender == panelSpecificWeek)
                SelectOption(rBSpecificWeek);
            else if (sender == panelSpecificYear)
                SelectOption(rBSpecificYear);

        }

        private void bSetCurrentWeek_Click(object sender, EventArgs e)
        {
            nUDWeek.Value = ISOWeek.GetWeekOfYear(DateTime.Now);
        }

        private void bSetCurrentMonth_Click(object sender, EventArgs e)
        {
            cBMonth2.SelectedIndex = DateTime.Now.Month - 1;
        }

        private void bSetCurrentYear_Click(object sender, EventArgs e)
        {
            nUDYear.Value = DateTime.Now.Year;
        }

        private void tVUserGroupsUsers_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_isTreeViewChecking)
                return;

            try
            {
                _isTreeViewChecking = true;

                foreach (TreeNode child in e.Node.Nodes)
                {
                    child.Checked = e.Node.Checked;
                }

                if (e.Node.Parent != null)
                {
                    var parent = e.Node.Parent;
                    parent.Checked = parent.Nodes
                        .Cast<TreeNode>()
                        .Any(n => n.Checked);
                }
            }
            finally
            {
                _isTreeViewChecking = false;
            }
        }

        private async void bUserSelection_Click(object sender, EventArgs e)
        {
            if (_currentUser.LevelOfAccess == 1)
                return;

            await RefreshAvailableExportUsersAsync(keepCurrentSelection: true);

            if (_availableExportUsers.Count == 0)
            {
                MessageBox.Show(
                    "Ve zvoleném období nejsou žádní uživatelé s vykázanými hodinami.",
                    "Výběr uživatelů",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

                return;
            }

            using var dialog = ActivatorUtilities.CreateInstance<UserSelectionDialog>(
                _serviceProvider,
                UserSelectionMode.Multiple,
                _selectedExportUsers.Select(u => u.Id),
                Enumerable.Empty<int>());

            dialog.SetAvailableUsers(_availableExportUsers);

            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;

            _selectedExportUsers = dialog.SelectedUsers
                .DistinctBy(u => u.Id)
                .OrderBy(u => u.UserGroup?.Title)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();

            UpdateSelectedExportUsersLabel();
        }

        private async Task LoadExportUsersSelectionAsync()
        {
            await RefreshAvailableExportUsersAsync(keepCurrentSelection: false);
        }

        private void RegisterExportRangeChangeHandlers()
        {
            dtpFrom.ValueChanged += ExportRangeInputChanged;
            dtpTo.ValueChanged += ExportRangeInputChanged;
            nUDWeek.ValueChanged += ExportRangeInputChanged;
            nUDYear.ValueChanged += ExportRangeInputChanged;
            cBMonth2.SelectedIndexChanged += ExportRangeInputChanged;
        }

        private void ExportRangeInputChanged(object? sender, EventArgs e)
        {
            _ = RefreshAvailableExportUsersAsync(keepCurrentSelection: true, notifyCountChange: true);
        }

        private async Task RefreshAvailableExportUsersAsync(bool keepCurrentSelection, bool notifyCountChange = false)
        {
            if (_currentUser.LevelOfAccess == 1)
            {
                _availableExportUsers = new List<User> { _currentUser };
                _selectedExportUsers = new List<User> { _currentUser };

                bUserSelection.Enabled = false;
                bUserSelection.Text = "Vybrán aktuální uživatel";
                lSelected.Text = FormatHelper.FormatUserToString(_currentUser);

                return;
            }

            try
            {
                var refreshVersion = ++_exportUsersRefreshVersion;
                var previousAvailableUsers = _previousAvailableExportUsers;

                bUserSelection.Enabled = false;
                bUserSelection.Text = "Načítám uživatele...";

                var (from, to) = GetSelectedExportRange();

                var availableUsers = await _userRepository.GetUsersAvailableForExportAsync(from, to);

                if (refreshVersion != _exportUsersRefreshVersion)
                    return;

                _availableExportUsers = availableUsers;
                _previousAvailableExportUsers = _availableExportUsers.ToList();

                if (notifyCountChange)
                    ShowExportUsersCountChangeNotification(previousAvailableUsers, _availableExportUsers);

                var availableUserIds = _availableExportUsers
                    .Select(u => u.Id)
                    .ToHashSet();

                if (keepCurrentSelection)
                {
                    _selectedExportUsers = _selectedExportUsers
                        .Where(u => availableUserIds.Contains(u.Id))
                        .DistinctBy(u => u.Id)
                        .OrderBy(u => u.UserGroup?.Title)
                        .ThenBy(u => u.Surname)
                        .ThenBy(u => u.FirstName)
                        .ToList();

                    if (_selectedExportUsers.Count == 0)
                    {
                        _selectedExportUsers = _availableExportUsers
                            .DistinctBy(u => u.Id)
                            .OrderBy(u => u.UserGroup?.Title)
                            .ThenBy(u => u.Surname)
                            .ThenBy(u => u.FirstName)
                            .ToList();
                    }
                }
                else
                {
                    _selectedExportUsers = GetInitialExportUsersFromConfig(_availableExportUsers);
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při načítání uživatelů pro export.", ex);
                _availableExportUsers = new List<User>();
                _selectedExportUsers = new List<User>();
            }
            finally
            {
                bUserSelection.Enabled = _availableExportUsers.Count > 0;
                UpdateSelectedExportUsersLabel();
            }
        }

        private void ShowExportUsersCountChangeNotification(List<User>? previousUsers, List<User> currentUsers)
        {
            if (previousUsers == null)
                return;

            var previousUserIds = previousUsers.Select(u => u.Id).ToHashSet();
            var currentUserIds = currentUsers.Select(u => u.Id).ToHashSet();

            var addedUsers = currentUsers
                .Where(u => !previousUserIds.Contains(u.Id))
                .OrderBy(u => u.UserGroup?.Title)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();

            var removedUsers = previousUsers
                .Where(u => !currentUserIds.Contains(u.Id))
                .OrderBy(u => u.UserGroup?.Title)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();

            if (addedUsers.Count == 0 && removedUsers.Count == 0)
                return;

            var lines = new List<string>
            {
                $"Po změně období se změnil seznam uživatelů pro export.",
                $"Původně: {previousUsers.Count}, nyní: {currentUsers.Count}."
            };

            if (addedUsers.Count > 0)
            {
                lines.Add("");
                lines.Add($"Přidáni ({addedUsers.Count}):");
                lines.AddRange(addedUsers.Select(u => $"- {FormatHelper.FormatUserToString(u)}"));
            }

            if (removedUsers.Count > 0)
            {
                lines.Add("");
                lines.Add($"Odebráni ({removedUsers.Count}):");
                lines.AddRange(removedUsers.Select(u => $"- {FormatHelper.FormatUserToString(u)}"));
            }

            MessageBox.Show(
                string.Join(Environment.NewLine, lines),
                "Změna seznamu uživatelů",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private List<User> GetInitialExportUsersFromConfig(List<User> availableUsers)
        {
            var config = _configService.Current;

            var savedUserIds = config.ExportSelection?.SelectedUserIds?.ToHashSet()
                ?? new HashSet<int>();

            var savedGroupIds = config.ExportSelection?.SelectedUserGroupIds?.ToHashSet()
                ?? new HashSet<int>();

            bool anythingSaved = savedUserIds.Count > 0 || savedGroupIds.Count > 0;

            var selectedUsers = anythingSaved
                ? availableUsers
                    .Where(u =>
                        savedUserIds.Contains(u.Id) ||
                        (u.UserGroupId.HasValue && savedGroupIds.Contains(u.UserGroupId.Value)))
                    .DistinctBy(u => u.Id)
                    .OrderBy(u => u.UserGroup?.Title)
                    .ThenBy(u => u.Surname)
                    .ThenBy(u => u.FirstName)
                    .ToList()
                : new List<User>();

            if (selectedUsers.Count > 0)
                return selectedUsers;

            return availableUsers
                .DistinctBy(u => u.Id)
                .OrderBy(u => u.UserGroup?.Title)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();
        }

        private void UpdateSelectedExportUsersLabel()
        {
            if (_selectedExportUsers.Count == 0)
            {
                lSelected.Text = "Nevybrán žádný uživatel";
                bUserSelection.Text = "Vybrat uživatele...";
                return;
            }

            if (_selectedExportUsers.Count == 1)
            {
                lSelected.Text = FormatHelper.FormatUserToString(_selectedExportUsers[0]);
                bUserSelection.Text = "Změnit uživatele...";
                return;
            }

            lSelected.Text = $"Vybráno uživatelů: {_selectedExportUsers.Count}";
            bUserSelection.Text = "Změnit výběr uživatelů...";
        }

        private List<User> GetSelectedExportUsers()
        {
            return _selectedExportUsers
                .Where(u => u.Id > 0)
                .DistinctBy(u => u.Id)
                .ToList();
        }

        private void SaveExportSelectionToConfig()
        {
            var config = _configService.Current;

            config.ExportSelection ??= new ExportSelectionConfig();

            config.ExportSelection.SelectedUserIds = _selectedExportUsers
                .Where(u => u.Id > 0)
                .Select(u => u.Id)
                .Distinct()
                .ToList();

            config.ExportSelection.SelectedUserGroupIds = new List<int>();

            config.ExportSelection.From = null;
            config.ExportSelection.To = null;
            config.ExportSelection.Week = null;
            config.ExportSelection.Month = null;
            config.ExportSelection.Year = null;

            if (rBSpecificTimePeriod.Checked)
            {
                config.ExportSelection.SelectedRangeType = ExportRangeType.TimePeriod;
                config.ExportSelection.From = dtpFrom.Value.Date;
                config.ExportSelection.To = dtpTo.Value.Date;
            }
            else if (rBSpecificWeek.Checked)
            {
                config.ExportSelection.SelectedRangeType = ExportRangeType.Week;
                config.ExportSelection.Week = (int)nUDWeek.Value;
                config.ExportSelection.Year = (int)nUDYear.Value;
            }
            else if (rBSpecificMonth.Checked)
            {
                config.ExportSelection.SelectedRangeType = ExportRangeType.Month;
                config.ExportSelection.Month = cBMonth2.SelectedIndex + 1;
                config.ExportSelection.Year = (int)nUDYear.Value;
            }
            else if (rBSpecificYear.Checked)
            {
                config.ExportSelection.SelectedRangeType = ExportRangeType.Year;
                config.ExportSelection.Year = (int)nUDYear.Value;
            }

            config.ExportSelection.BuildEvaluationSheet = cBBuildEvaluationSheet.Checked;

            _configService.Save();
        }

        private (DateTime From, DateTime To) GetSelectedExportRange()
        {
            if (rBSpecificTimePeriod.Checked)
            {
                return (dtpFrom.Value.Date, dtpTo.Value.Date);
            }

            if (rBSpecificWeek.Checked)
            {
                int year = (int)nUDYear.Value;
                int week = (int)nUDWeek.Value;

                var from = ISOWeek.ToDateTime(year, week, DayOfWeek.Monday);
                var to = from.AddDays(6);

                return (from.Date, to.Date);
            }

            if (rBSpecificMonth.Checked)
            {
                int year = (int)nUDYear.Value;
                int month = cBMonth2.SelectedIndex + 1;

                if (month < 1 || month > 12)
                    throw new InvalidOperationException("Není vybraný měsíc pro export.");

                var from = new DateTime(year, month, 1);
                var to = new DateTime(year, month, DateTime.DaysInMonth(year, month));

                return (from.Date, to.Date);
            }

            if (rBSpecificYear.Checked)
            {
                int year = (int)nUDYear.Value;

                var from = new DateTime(year, 1, 1);
                var to = new DateTime(year, 12, 31);

                return (from.Date, to.Date);
            }

            throw new InvalidOperationException("Není vybraná možnost časového rozsahu exportu.");
        }
    }
    #endregion

    #region === Doménové konstanty ===
    /// <summary>
    /// Centralizované ID a další konstanty pro filtrování záznamů.
    /// Vyhneme se "magic numbers" rozesetým v kódu.
    /// </summary>
    internal static class ExportConstants
    {
        /// <summary>
        /// Projekt, který je vyloučen v kombinaci s <see cref="ExcludedEntryTypeId"/>.
        /// </summary>
        public const int ExcludedProjectId = WorkLogIds.Projects.Snack;

        /// <summary>
        /// Typ záznamu, který je spolu s <see cref="ExcludedProjectId"/> vyloučen z exportu.
        /// </summary>
        public const int ExcludedEntryTypeId = WorkLogIds.EntryTypes.Snack;

        /// <summary>
        /// Projekt reprezentující nepřítomnost – nezapočítává se do souhrnů podle uživatele.
        /// </summary>
        public const int AbsenceProjectId = WorkLogIds.Projects.Absence;

        /// <summary>
        /// Typ záznamu reprezentující outlook událost (nevalidní záznam) – nezapočítává se do souhrnů podle uživatele.
        /// </summary>
        public const int OutlookEventEntryTypeId = WorkLogIds.EntryTypes.OutlookEvent;

        // VYHODNOCENÍ
        public const int AutomationProjectId = WorkLogIds.Projects.Automation;

        public const int ProductionSdProjectId = WorkLogIds.Projects.ProductionSd;
        public const int ProductionHpProjectId = WorkLogIds.Projects.ProductionHp;
        public const int ProductionMetProjectId = WorkLogIds.Projects.ProductionMet;
        public const int ProductionKomProjectId = WorkLogIds.Projects.ProductionKom;
        public const int ProductionSorProjectId = WorkLogIds.Projects.ProductionSor;
        public const int ProductionOtherProjectId = WorkLogIds.Projects.ProductionOther;

        public const int ClubYoungTechnicianEntryTypeId = WorkLogIds.EntryTypes.ClubYoungTechnician;
    }
    #endregion

    #region === Pomocné utility ===
    /// <summary>
    /// Nástroje pro práci s datovým rozsahem exportu (měsíce, dny, atd.).
    /// </summary>
    internal static class DateRangeHelper
    {
        /// <summary>
        /// Vrátí první a poslední den měsíce podle vstupního data.
        /// </summary>
        public static (DateTime From, DateTime To) GetMonthRange(DateTime anchor)
        {
            var firstDay = new DateTime(anchor.Year, anchor.Month, 1);
            var lastDay = new DateTime(anchor.Year, anchor.Month, DateTime.DaysInMonth(anchor.Year, anchor.Month));
            return (firstDay, lastDay);
        }

        /// <summary>
        /// Vrátí (From, To) pro měsíc určený indexem 0..11 v daném roce.
        /// </summary>
        public static (DateTime From, DateTime To) GetMonthRangeByIndex(int monthIndex, int year)
        {
            var month = monthIndex + 1; // 0..11 -> 1..12
            var firstDay = new DateTime(year, month, 1);
            var lastDay = new DateTime(year, month, DateTime.DaysInMonth(year, month));
            return (firstDay, lastDay);
        }

        /// <summary>
        /// Normalizuje uživatelský rozsah "od dne včetně do dne včetně".
        /// Pro databázové filtrování vrací horní mez jako následující den exkluzivně.
        /// </summary>
        public static (DateTime FromInclusive, DateTime ToInclusive, DateTime ToExclusive)
            NormalizeInclusiveDateRange(DateTime from, DateTime to)
        {
            var fromInclusive = from.Date;
            var toInclusive = to.Date;

            if (toInclusive < fromInclusive)
                throw new InvalidOperationException("Datum do nesmí být menší než datum od.");

            var toExclusive = toInclusive.AddDays(1);

            return (fromInclusive, toInclusive, toExclusive);
        }
    }

    /// <summary>
    /// Zajišťuje bezpečné názvy listů pro Excel (omezení 31 znaků, nepovolené znaky).
    /// </summary>
    internal static class SheetNameSanitizer
    {
        public static string MakeSafe(string? name)
        {
            var input = string.IsNullOrWhiteSpace(name) ? "List" : name!;
            var invalid = System.IO.Path.GetInvalidFileNameChars();
            var safe = string.Join("_", input.Split(invalid, StringSplitOptions.RemoveEmptyEntries));
            if (safe.Length > 31) safe = safe[..31];
            return string.IsNullOrWhiteSpace(safe) ? "List" : safe;
        }
    }
    #endregion

    #region === Továrna na datové tabulky ===
    /// <summary>
    /// Vytváří <see cref="DataTable"/> pro jednotlivé listy exportu.
    /// </summary>
    public sealed class DataTableFactory
    {
        private readonly PowerKeyHelper _powerKeyHelper;

        public DataTableFactory(PowerKeyHelper powerKeyHelper)
        {
            _powerKeyHelper = powerKeyHelper;
        }

        /// <summary>
        /// Datová tabulka pro detailní záznamy (časové záznamy / projekty).
        /// </summary>
        public DataTable BuildTimeEntries(IEnumerable<TimeEntry> items)
        {
            var dt = new DataTable();
            dt.Columns.Add("Osobní číslo", typeof(int));
            dt.Columns.Add("Jméno", typeof(string));
            dt.Columns.Add("Skupina", typeof(string));
            dt.Columns.Add("Projekt", typeof(string));
            dt.Columns.Add("Popis projektu", typeof(string));
            dt.Columns.Add("Typ záznamu", typeof(string));
            dt.Columns.Add("Časový záznam", typeof(DateTime));
            dt.Columns.Add("Popis", typeof(string));
            dt.Columns.Add("Poznámky", typeof(string));
            dt.Columns.Add("Doba v hodinách", typeof(double));

            foreach (var e in items)
            {
                dt.Rows.Add(
                    e.User?.PersonalNumber ?? 0,
                    $"{e.User?.FirstName} {e.User?.Surname}".Trim(),
                    e.User?.UserGroup?.Title ?? "CHYBÍ DATA",
                    e.Project?.ProjectTitle ?? "N/A",
                    e.Project?.ProjectDescription ?? "N/A",
                    e.EntryType?.Title ?? "Neznámý typ",
                    e.Timestamp?.Date ?? (object)DBNull.Value!,
                    e.Description ?? "N/A",
                    e.Note ?? "N/A",
                    e.EntryMinutes / 60.0
                );
            }
            return dt;
        }

        /// <summary>
        /// Datová tabulka – souhrn podle uživatele. Souhrnné řádky uživatelů + rozpad na projekty.
        /// </summary>
        public async Task<DataTable> BuildUserSummary(
          IEnumerable<User> selectedUsers,
          IEnumerable<TimeEntry> timeEntries,
          DateTime from,
          DateTime to,
          IReadOnlyDictionary<(int ProjectId, int UserId), double> cumToFullfilledDict)
        {
            var dt = new DataTable();
            dt.Columns.Add("Osobní číslo", typeof(int));
            dt.Columns.Add("Jméno", typeof(string));
            dt.Columns.Add("Projekt", typeof(string));
            dt.Columns.Add("Popis projektu", typeof(string));
            dt.Columns.Add("Součet hodin", typeof(double));
            dt.Columns.Add("Suma (čas. úsek)", typeof(double));
            dt.Columns.Add("Docházka", typeof(double));
            dt.Columns.Add("Suma (před zplnohodnotněním projektu)", typeof(double));

            // Docházka z PowerKey podle konkrétního časového úseku.
            // Bere se z PowerKey spočítaných denních hodnot AD.WorkedHours,
            // aby zůstaly zachované systémové tolerance, pauzy a zaokrouhlení.
            Dictionary<int, double> powerKeyData;

            try
            {
                var personalNumbers = selectedUsers
                    .Where(u => u != null && u.PersonalNumber > 0)
                    .Select(u => u.PersonalNumber)
                    .Distinct()
                    .OrderBy(x => x)
                    .ToList();

                powerKeyData = await _powerKeyHelper
                    .GetWorkedHoursByPersonalNumberForRangeAsync(from, to, personalNumbers)
                    .ConfigureAwait(false);

                powerKeyData ??= new Dictionary<int, double>();
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    "PowerKey nedostupný – docházka bude nastavena na 0 a export pokračuje.",
                    ex);

                powerKeyData = new Dictionary<int, double>();
            }

            // Odkomentováno - Ignorovali jsme nepřítomnost v souhrnu
            //var filteredEntries = timeEntries.Where(e => e.ProjectId != ExportConstants.AbsenceProjectId).ToList();

            var entriesByUserId = timeEntries
      .Where(e => e.UserId.HasValue)
      .GroupBy(e => e.UserId!.Value)
      .ToDictionary(g => g.Key, g => g.ToList());

            var usersToExport = selectedUsers
                .Where(u => u != null)
                .GroupBy(u => u.Id)
                .Select(g => g.First())
                .OrderBy(u => u.PersonalNumber)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();

            foreach (var user in usersToExport)
            {
                entriesByUserId.TryGetValue(user.Id, out var userEntries);
                userEntries ??= new List<TimeEntry>();

                double totalHours = userEntries
                    .Where(e => e.ProjectId != ExportConstants.AbsenceProjectId)
                    .Sum(e => e.EntryMinutes) / 60.0;
                double attendance = powerKeyData.TryGetValue(user.PersonalNumber, out double h) ? h : 0;
                string fullName = $"{user.FirstName} {user.Surname}".Trim();

                // Souhrnný řádek uživatele — vznikne i když userEntries je prázdné
                dt.Rows.Add(
                    user.PersonalNumber,
                    fullName,
                    string.Empty,
                    string.Empty,
                    DBNull.Value!,
                    totalHours,
                    attendance,
                    DBNull.Value!
                );

                var projects = userEntries
                    .Where(e => e.Project != null)
                    .GroupBy(e => new
                    {
                        e.Project!.Id,
                        e.Project.ProjectTitle,
                        e.Project.ProjectDescription,
                        e.Project.DateFullFilled
                    })
                    .OrderBy(g => g.Key.ProjectTitle);

                foreach (var proj in projects)
                {
                    double monthlyHours = proj.Sum(e => e.EntryMinutes) / 60.0;
                    double? cumHours = null;

                    if (proj.Key.DateFullFilled.HasValue)
                    {
                        int pid = proj.Key.Id;
                        int uid = user.Id;

                        if (cumToFullfilledDict.TryGetValue((pid, uid), out var val))
                            cumHours = val;
                    }

                    dt.Rows.Add(
                        user.PersonalNumber,
                        fullName,
                        proj.Key.ProjectTitle ?? "N/A",
                        proj.Key.ProjectDescription ?? "N/A",
                        monthlyHours,
                        DBNull.Value!,
                        DBNull.Value!,
                        (object?)cumHours ?? DBNull.Value!
                    );
                }
            }

            return dt;
        }

        public async Task BuildEvaluationSheet(
           XLWorkbook wb,
           IEnumerable<TimeEntry> entries,
           IEnumerable<User> selectedUsers,
           DateTime from,
           DateTime to)
        {
            var ws = wb.AddWorksheet("VYHODNOCENÍ");

            ws.Position = 1;
            ws.TabColor = XLColor.Yellow;

            double totalPowerKeyWorkedHours = 0;

            try
            {
                var personalNumbers = selectedUsers
        .Where(u => u != null && u.PersonalNumber > 0)
        .Select(u => u.PersonalNumber)
        .Distinct()
        .OrderBy(x => x)
        .ToList();

                var powerKeyData = await _powerKeyHelper
                    .GetWorkedHoursByPersonalNumberForRangeAsync(from, to, personalNumbers)
                    .ConfigureAwait(false);

                totalPowerKeyWorkedHours = powerKeyData?.Values.Sum() ?? 0;
            }
            catch (Exception ex)
            {
                AppLogger.Error(
                    "PowerKey nedostupný – celková docházka pro vyhodnocení bude nastavena na 0 a export pokračuje.",
                    ex);

                totalPowerKeyWorkedHours = 0;
            }

            bool includeExternalBreakdown = selectedUsers.Any(user => user.UserGroupId == 6);
            var rows = new List<EvaluationRow>();

            if (includeExternalBreakdown)
            {
                rows.Add(new EvaluationRow(
                    "Projekty", "EXTERNÍ PROJEKTY",
                    entry => !IsExternalEntry(entry)
                        && entry.Project?.ProjectDescription?.Contains("E", StringComparison.OrdinalIgnoreCase) == true,
                    "INTERNÍ"));
                rows.Add(new EvaluationRow(
                    "Projekty", "EXTERNÍ PROJEKTY",
                    entry => IsExternalEntry(entry)
                        && entry.Project?.ProjectDescription?.Contains("E", StringComparison.OrdinalIgnoreCase) == true,
                    "EXTERNISTÉ"));
                rows.Add(new EvaluationRow(
                    "Projekty", "INTERNÍ PROJEKTY",
                    entry => !IsExternalEntry(entry)
                        && entry.Project?.ProjectDescription?.Contains("I", StringComparison.OrdinalIgnoreCase) == true,
                    "INTERNÍ"));
                rows.Add(new EvaluationRow(
                    "Projekty", "INTERNÍ PROJEKTY",
                    entry => IsExternalEntry(entry)
                        && entry.Project?.ProjectDescription?.Contains("I", StringComparison.OrdinalIgnoreCase) == true,
                    "EXTERNISTÉ"));
                rows.Add(new EvaluationRow(
                    "Automatizace", "Provoz Automatizace",
                    entry => !IsExternalEntry(entry)
                        && entry.ProjectId == ExportConstants.AutomationProjectId,
                    "INTERNÍ"));
                rows.Add(new EvaluationRow(
                    "Automatizace", "Provoz Automatizace",
                    entry => IsExternalEntry(entry)
                        && entry.ProjectId == ExportConstants.AutomationProjectId,
                    "EXTERNISTÉ"));
            }
            else
            {
                rows.Add(new EvaluationRow(
                    "Projekty", "EXTERNÍ PROJEKTY",
                    entry => entry.Project?.ProjectDescription?.Contains("E", StringComparison.OrdinalIgnoreCase) == true));
                rows.Add(new EvaluationRow(
                    "Projekty", "INTERNÍ PROJEKTY",
                    entry => entry.Project?.ProjectDescription?.Contains("I", StringComparison.OrdinalIgnoreCase) == true));
                rows.Add(new EvaluationRow(
                    "Automatizace", "Provoz Automatizace",
                    entry => entry.ProjectId == ExportConstants.AutomationProjectId));
            }

            rows.AddRange(new[]
            {
                new EvaluationRow("Provoz výroba", "Provoz SD",
                    entry => entry.ProjectId == ExportConstants.ProductionSdProjectId),
                new EvaluationRow("Provoz výroba", "Provoz HP",
                    entry => entry.ProjectId == ExportConstants.ProductionHpProjectId),
                new EvaluationRow("Provoz výroba", "Provoz MET",
                    entry => entry.ProjectId == ExportConstants.ProductionMetProjectId),
                new EvaluationRow("Provoz výroba", "Provoz KOM",
                    entry => entry.ProjectId == ExportConstants.ProductionKomProjectId),
                new EvaluationRow("Provoz výroba", "Provoz SOR",
                    entry => entry.ProjectId == ExportConstants.ProductionSorProjectId),
                new EvaluationRow("Ostatní", "Ostatní",
                    entry => entry.ProjectId == ExportConstants.ProductionOtherProjectId),
                new EvaluationRow("Ostatní", "Nepřítomnost",
                    entry => entry.ProjectId == ExportConstants.AbsenceProjectId),
                new EvaluationRow("Ostatní", "Kroužek MT",
                    entry => entry.EntryTypeId == ExportConstants.ClubYoungTechnicianEntryTypeId)
            });

            foreach (var row in rows)
            {
                row.SumHours = entries
                    .Where(row.Predicate)
                    .Sum(e => e.EntryMinutes) / 60.0;
            }

            double totalHours = rows.Sum(r => r.SumHours);
            int firstDataRow = 3;
            int lastDataRow = firstDataRow + rows.Count - 1;
            int totalRow = lastDataRow + 2;
            int powerKeyRow = totalRow + 2;
            int hoursColumn = includeExternalBreakdown ? 4 : 3;
            int itemPercentColumn = includeExternalBreakdown ? 5 : 4;
            int groupPercentColumn = includeExternalBreakdown ? 6 : 5;
            string hoursColumnLetter = includeExternalBreakdown ? "D" : "C";
            string itemPercentColumnLetter = includeExternalBreakdown ? "E" : "D";
            string groupPercentColumnLetter = includeExternalBreakdown ? "F" : "E";

            foreach (var row in rows)
            {
                row.Percent = totalHours > 0
                    ? row.SumHours / totalHours
                    : 0;
            }

            var groupPercents = rows
                .GroupBy(r => r.Group)
                .ToDictionary(
                    g => g.Key,
                    g => totalHours > 0 ? g.Sum(r => r.SumHours) / totalHours : 0
                );

            // 1. řádek prázdný, sloučený přes oblast A:M.
            ws.Range("A1:M1").Merge();
            ws.Row(1).Clear();

            // 2. řádek prázdný, výška 6 px => cca 4.5 pt.
            ws.Row(2).Clear();
            ws.Row(2).Height = 4.5;

            int currentRow = firstDataRow;

            // Budeme si pamatovat startovní řádky kategorií,
            // aby pomocná data pro graf mohla odkazovat na sloupec s podílem skupiny.
            var groupStartRows = new Dictionary<string, int>();

            foreach (var group in rows.GroupBy(r => r.Group))
            {
                int groupStartRow = currentRow;
                groupStartRows[group.Key] = groupStartRow;

                foreach (var item in group)
                {
                    ws.Cell(currentRow, 2).Value = item.Name;

                    if (includeExternalBreakdown)
                        ws.Cell(currentRow, 3).Value = item.WorkerType;

                    ws.Cell(currentRow, hoursColumn).Value = item.SumHours;
                    ws.Cell(currentRow, itemPercentColumn).Value = item.Percent;
                    ws.Cell(currentRow, 9).Value = string.IsNullOrEmpty(item.WorkerType)
                        ? item.Name
                        : $"{item.Name} / {item.WorkerType}";

                    currentRow++;
                }

                int groupEndRow = currentRow - 1;
                if (includeExternalBreakdown)
                {
                    int categoryStartRow = groupStartRow;

                    foreach (var category in group.GroupBy(item => item.Name))
                    {
                        int categoryEndRow = categoryStartRow + category.Count() - 1;

                        if (categoryEndRow > categoryStartRow)
                        {
                            ws.Range(categoryStartRow, 2, categoryEndRow, 2).Merge();
                            ws.Range(categoryStartRow, 2, categoryEndRow, 2)
                                .Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                        }

                        categoryStartRow = categoryEndRow + 1;
                    }
                }

                ws.Cell(groupStartRow, 1).Value = group.Key;
                ws.Cell(groupStartRow, groupPercentColumn).Value = groupPercents[group.Key];

                if (groupEndRow > groupStartRow)
                {
                    ws.Range(groupStartRow, 1, groupEndRow, 1).Merge();
                    ws.Range(groupStartRow, groupPercentColumn, groupEndRow, groupPercentColumn).Merge();
                }

                var groupRange = ws.Range(groupStartRow, 1, groupEndRow, groupPercentColumn);
                var firstColumnGroupRange = ws.Range(groupStartRow, 1, groupEndRow, 1);
                var lastColumnGroupRange = ws.Range(groupStartRow, groupPercentColumn, groupEndRow, groupPercentColumn);

                groupRange.Style.Fill.BackgroundColor = GetEvaluationGroupColor(group.Key);

                // Jemné vnitřní čáry v rámci sekce.
                groupRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                // Silné ohraničení celé sekce.
                groupRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                // První sloupec sekce má také silné ohraničení okolo.
                firstColumnGroupRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

                firstColumnGroupRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

                lastColumnGroupRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                lastColumnGroupRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            // Souhrnná data pro hlavní koláčový graf.
            int chartDataRow = 3;

            foreach (var groupName in new[] { "Projekty", "Automatizace", "Provoz výroba", "Ostatní" })
            {
                if (!groupStartRows.TryGetValue(groupName, out int sourceRow))
                    continue;

                ws.Cell(chartDataRow, 7).Value = groupName;            // G
                ws.Cell(chartDataRow, 8).FormulaA1 = $"={groupPercentColumnLetter}{sourceRow}"; // H
                chartDataRow++;
            }

            if (includeExternalBreakdown)
            {
                // Data pro tři samostatné koláče: interní pracovníci vs. externisté.
                AddCategoryChartData(ws, 3, 14, firstDataRow + 4, firstDataRow + 5, itemPercentColumnLetter);
                AddCategoryChartData(ws, 18, 7, firstDataRow, firstDataRow + 1, itemPercentColumnLetter);
                AddCategoryChartData(ws, 18, 14, firstDataRow + 2, firstDataRow + 3, itemPercentColumnLetter);
            }
            ws.Column(7).Style.NumberFormat.Format = "@";
            ws.Column(8).Style.NumberFormat.Format = "0.00%";

            ws.Column(hoursColumn).Style.NumberFormat.Format = "# ##0.0";
            ws.Column(itemPercentColumn).Style.NumberFormat.Format = "0.00%";
            ws.Column(groupPercentColumn).Style.NumberFormat.Format = "0.00%";

            ws.Column(hoursColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Column(itemPercentColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Column(groupPercentColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Range(firstDataRow, groupPercentColumn, lastDataRow, groupPercentColumn).Style.Font.Bold = true;
            ws.Range(firstDataRow, groupPercentColumn, lastDataRow, groupPercentColumn).Style.Font.FontSize = 14;

            // Součet hodin pod tabulkou
            ws.Cell(totalRow, hoursColumn - 1).Value = "∑";
            ws.Cell(totalRow, hoursColumn).FormulaA1 = $"=SUM({hoursColumnLetter}{firstDataRow}:{hoursColumnLetter}{lastDataRow})";

            ws.Cell(totalRow, hoursColumn - 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
            ws.Cell(totalRow, hoursColumn - 1).Style.Font.Bold = true;

            ws.Cell(totalRow, hoursColumn).Style.NumberFormat.Format = "# ##0.0";
            ws.Cell(totalRow, hoursColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(totalRow, hoursColumn).Style.Font.Bold = true;

            // Celkově odpracované hodiny z PowerKey za všechny vybrané uživatele v daném období
            ws.Cell(powerKeyRow, hoursColumn - 1).Value = "PowerKey";
            ws.Cell(powerKeyRow, hoursColumn).Value = totalPowerKeyWorkedHours;

            ws.Cell(powerKeyRow, hoursColumn - 1).Style.Font.Bold = true;
            ws.Cell(powerKeyRow, hoursColumn).Style.NumberFormat.Format = "# ##0.0";
            ws.Cell(powerKeyRow, hoursColumn).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(powerKeyRow, hoursColumn).Style.Font.Bold = true;

            // Nejdřív dopočítat podle obsahu.
            ws.Columns().AdjustToContents();

            // Přibližný převod pixelů na Excel šířku:
            // ExcelWidth = (pixels - 5) / 7
            ws.Column(1).Width = 17.57;
            ws.Column(2).Width = 20.14;

            if (includeExternalBreakdown)
            {
                ws.Column(3).Width = 13;    // typ pracovníka
                ws.Column(4).Width = 8.43;  // hodiny
                ws.Column(5).Width = 8.43;  // podíl položky
                ws.Column(6).Width = 9;     // podíl skupiny
            }
            else
            {
                ws.Column(3).Width = 8.43;  // hodiny
                ws.Column(4).Width = 8.43;  // podíl položky
                ws.Column(5).Width = 9;     // podíl skupiny
            }

            ws.SetTabActive();
        }

        private static bool IsExternalEntry(TimeEntry entry)
        {
            return entry.User?.UserGroupId == 6;
        }

        private static void AddCategoryChartData(
            IXLWorksheet worksheet,
            int startRow,
            int startColumn,
            int internalSourceRow,
            int externalSourceRow,
            string percentColumnLetter)
        {
            worksheet.Cell(startRow, startColumn).Value = "Interní";
            worksheet.Cell(startRow, startColumn + 1).FormulaA1 = $"={percentColumnLetter}{internalSourceRow}";
            worksheet.Cell(startRow + 1, startColumn).Value = "Externisté";
            worksheet.Cell(startRow + 1, startColumn + 1).FormulaA1 = $"={percentColumnLetter}{externalSourceRow}";
        }
        private static XLColor GetEvaluationGroupColor(string groupName)
        {
            return groupName switch
            {
                "Projekty" => XLColor.FromHtml("#FCE4D6"),
                "Automatizace" => XLColor.FromHtml("#DDEBF7"),
                "Provoz výroba" => XLColor.FromHtml("#D9D9D9"),
                "Ostatní" => XLColor.FromHtml("#EBF1DE"),
                _ => XLColor.White
            };
        }

        public void AddEvaluationChartsWithExcelInterop(string filePath, bool includeExternalBreakdown)
        {
            Excel.Application? excel = null;
            Excel.Workbook? workbook = null;
            Excel.Worksheet? ws = null;
            Excel.ChartObjects? chartObjects = null;

            try
            {
                excel = new Excel.Application
                {
                    DisplayAlerts = false,
                    Visible = false
                };

                workbook = excel.Workbooks.Open(filePath);
                ws = (Excel.Worksheet)workbook.Worksheets["VYHODNOCENÍ"];
                chartObjects = (Excel.ChartObjects)ws.ChartObjects(Type.Missing);

                AddEvaluationPieChart(chartObjects, ws);
                if (includeExternalBreakdown)
                    AddEvaluationCategoryPieCharts(chartObjects, ws);

                AddEvaluationColumnChart(chartObjects, ws, includeExternalBreakdown);

                workbook.Save();
            }
            finally
            {
                if (chartObjects != null) Marshal.ReleaseComObject(chartObjects);
                if (ws != null) Marshal.ReleaseComObject(ws);

                if (workbook != null)
                {
                    workbook.Close(SaveChanges: false);
                    Marshal.ReleaseComObject(workbook);
                }

                if (excel != null)
                {
                    excel.Quit();
                    Marshal.ReleaseComObject(excel);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
        }

        private static void AddEvaluationPieChart(Excel.ChartObjects chartObjects, Excel.Worksheet ws)
        {
            Excel.ChartObject? chartObject = null;
            Excel.Chart? chart = null;
            Excel.SeriesCollection? seriesCollection = null;
            Excel.Series? series = null;
            Excel.DataLabels? dataLabels = null;
            Excel.Range? topLeft = null;
            Excel.Range? chartArea = null;

            try
            {
                topLeft = (Excel.Range)ws.Range["G3"];
                chartArea = (Excel.Range)ws.Range["G3:M17"];

                chartObject = chartObjects.Add(
                    (double)topLeft.Left,
                    (double)topLeft.Top,
                    (double)chartArea.Width,
                    (double)chartArea.Height
                );

                chartObject.Name = "VyhodnoceniPieChart";

                chart = chartObject.Chart;
                chart.ChartType = Excel.XlChartType.xl3DPie;

                seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();
                series = seriesCollection.NewSeries();

                series.XValues = "='VYHODNOCENÍ'!$G$3:$G$6";
                series.Values = "='VYHODNOCENÍ'!$H$3:$H$6";
                series.Name = "Podíl";

                chart.HasTitle = true;
                chart.ChartTitle.Text = "Podíl využití časového fondu v rámci AUTOMATIZACE";
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Size = 14;

                chart.HasLegend = true;
                chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionBottom;
                chart.Legend.Font.Size = 10;

                chart.Rotation = 120;
                chart.Elevation = 25;
                chart.Perspective = 30;

                chart.ChartArea.Border.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                chart.ChartArea.Border.Weight = Excel.XlBorderWeight.xlThin;

                chart.ChartArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                chart.PlotArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                chart.PlotArea.Border.LineStyle = Excel.XlLineStyle.xlLineStyleNone;

                series.ApplyDataLabels();

                dataLabels = (Excel.DataLabels)series.DataLabels();
                dataLabels.ShowCategoryName = true;
                dataLabels.ShowPercentage = true;
                dataLabels.ShowValue = false;
                dataLabels.Separator = "; ";
                dataLabels.NumberFormatLocal = "0,00%";
                dataLabels.Font.Size = 8;
                dataLabels.Font.Bold = true;
                dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionBestFit;

                SetSeriesPointColor(series, 1, "#F8CBAD");
                SetSeriesPointColor(series, 2, "#BDD7EE");
                SetSeriesPointColor(series, 3, "#D9D9D9");
                SetSeriesPointColor(series, 4, "#E2F0D9");
            }
            finally
            {
                if (dataLabels != null) Marshal.ReleaseComObject(dataLabels);
                if (series != null) Marshal.ReleaseComObject(series);
                if (seriesCollection != null) Marshal.ReleaseComObject(seriesCollection);
                if (chart != null) Marshal.ReleaseComObject(chart);
                if (chartObject != null) Marshal.ReleaseComObject(chartObject);
                if (chartArea != null) Marshal.ReleaseComObject(chartArea);
                if (topLeft != null) Marshal.ReleaseComObject(topLeft);
            }
        }

        private static void AddEvaluationCategoryPieCharts(Excel.ChartObjects chartObjects, Excel.Worksheet ws)
        {
            AddEvaluationCategoryPieChart(
                chartObjects, ws, "AutomatizacePieChart", "Automatizace",
                "N3", "N3:T17", "$N$3:$N$4", "$O$3:$O$4", "#5B9BD5", "#BDD7EE");

            AddEvaluationCategoryPieChart(
                chartObjects, ws, "ExterniProjektyPieChart", "Externí projekty",
                "G18", "G18:M32", "$G$18:$G$19", "$H$18:$H$19", "#ED7D31", "#F4B183");

            AddEvaluationCategoryPieChart(
                chartObjects, ws, "InterniProjektyPieChart", "Interní projekty",
                "N18", "N18:T32", "$N$18:$N$19", "$O$18:$O$19", "#C55A11", "#F8CBAD");
        }

        private static void AddEvaluationCategoryPieChart(
            Excel.ChartObjects chartObjects,
            Excel.Worksheet ws,
            string chartName,
            string title,
            string topLeftAddress,
            string chartAreaAddress,
            string categoryRange,
            string valueRange,
            string internalColor,
            string externalColor)
        {
            Excel.ChartObject? chartObject = null;
            Excel.Chart? chart = null;
            Excel.SeriesCollection? seriesCollection = null;
            Excel.Series? series = null;
            Excel.DataLabels? dataLabels = null;
            Excel.Range? topLeft = null;
            Excel.Range? chartArea = null;

            try
            {
                topLeft = (Excel.Range)ws.Range[topLeftAddress];
                chartArea = (Excel.Range)ws.Range[chartAreaAddress];
                chartObject = chartObjects.Add(
                    (double)topLeft.Left,
                    (double)topLeft.Top,
                    (double)chartArea.Width,
                    (double)chartArea.Height);

                chartObject.Name = chartName;
                chart = chartObject.Chart;
                chart.ChartType = Excel.XlChartType.xlPie;

                seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();
                series = seriesCollection.NewSeries();
                series.XValues = $"='VYHODNOCENÍ'!{categoryRange}";
                series.Values = $"='VYHODNOCENÍ'!{valueRange}";

                chart.HasTitle = true;
                chart.ChartTitle.Text = title;
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Size = 12;
                chart.HasLegend = true;
                chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionBottom;
                chart.Legend.Font.Size = 9;

                int white = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                chart.ChartArea.Interior.Color = white;
                chart.PlotArea.Interior.Color = white;
                chart.ChartArea.Border.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                chart.ChartArea.Border.Weight = Excel.XlBorderWeight.xlThin;
                chart.PlotArea.Border.LineStyle = Excel.XlLineStyle.xlLineStyleNone;

                series.ApplyDataLabels();
                dataLabels = (Excel.DataLabels)series.DataLabels();
                dataLabels.ShowCategoryName = false;
                dataLabels.ShowPercentage = true;
                dataLabels.ShowValue = false;
                dataLabels.NumberFormatLocal = "0,0%";
                dataLabels.Font.Size = 9;
                dataLabels.Font.Bold = true;
                dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionBestFit;

                SetSeriesPointColor(series, 1, internalColor);
                SetSeriesPointColor(series, 2, externalColor);
            }
            finally
            {
                if (dataLabels != null) Marshal.ReleaseComObject(dataLabels);
                if (series != null) Marshal.ReleaseComObject(series);
                if (seriesCollection != null) Marshal.ReleaseComObject(seriesCollection);
                if (chart != null) Marshal.ReleaseComObject(chart);
                if (chartObject != null) Marshal.ReleaseComObject(chartObject);
                if (chartArea != null) Marshal.ReleaseComObject(chartArea);
                if (topLeft != null) Marshal.ReleaseComObject(topLeft);
            }
        }

        private static void AddEvaluationColumnChart(
            Excel.ChartObjects chartObjects,
            Excel.Worksheet ws,
            bool includeExternalBreakdown)
        {
            Excel.ChartObject? chartObject = null;
            Excel.Chart? chart = null;
            Excel.SeriesCollection? seriesCollection = null;
            Excel.Series? series = null;
            Excel.DataLabels? dataLabels = null;
            Excel.Axis? valueAxis = null;
            Excel.DataTable? dataTable = null;
            Excel.Range? topLeft = null;
            Excel.Range? chartArea = null;

            try
            {
                string topLeftAddress = includeExternalBreakdown ? "A34" : "A18";
                string chartAreaAddress = includeExternalBreakdown ? "A34:T51" : "A18:M35";
                topLeft = (Excel.Range)ws.Range[topLeftAddress];
                chartArea = (Excel.Range)ws.Range[chartAreaAddress];

                chartObject = chartObjects.Add(
                    (double)topLeft.Left,
                    (double)topLeft.Top,
                    (double)chartArea.Width,
                    (double)chartArea.Height
                );

                chartObject.Name = "OdpracovaneHodinyChart";

                chart = chartObject.Chart;
                chart.ChartType = Excel.XlChartType.xlColumnClustered;

                seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();
                series = seriesCollection.NewSeries();

                series.Name = "Odpracované hodiny";
                series.XValues = includeExternalBreakdown
                    ? "='VYHODNOCENÍ'!$I$3:$I$16"
                    : "='VYHODNOCENÍ'!$B$3:$B$13";
                series.Values = includeExternalBreakdown
                    ? "='VYHODNOCENÍ'!$D$3:$D$16"
                    : "='VYHODNOCENÍ'!$C$3:$C$13";

                chart.HasTitle = true;
                chart.ChartTitle.Text = "Odpracované hodiny";
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Size = 14;

                chart.HasLegend = false;

                int white = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);

                chart.ChartArea.Interior.Color = white;
                chart.PlotArea.Interior.Color = white;

                chart.ChartArea.Border.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                chart.ChartArea.Border.Weight = Excel.XlBorderWeight.xlThin;

                valueAxis = (Excel.Axis)chart.Axes(
                    Excel.XlAxisType.xlValue,
                    Excel.XlAxisGroup.xlPrimary
                );

                valueAxis.MinimumScale = 0;
                valueAxis.HasMajorGridlines = true;
                valueAxis.TickLabels.NumberFormatLocal = "# ##0,0";

                series.ApplyDataLabels();

                dataLabels = (Excel.DataLabels)series.DataLabels();
                dataLabels.ShowValue = true;
                dataLabels.ShowCategoryName = false;
                dataLabels.ShowSeriesName = false;
                dataLabels.NumberFormatLocal = "# ##0,0";
                dataLabels.Font.Size = 8;
                dataLabels.Font.Bold = true;
                dataLabels.Position = Excel.XlDataLabelPosition.xlLabelPositionOutsideEnd;

                chart.HasDataTable = true;
                dataTable = chart.DataTable;
                dataTable.ShowLegendKey = true;

                if (includeExternalBreakdown)
                {
                    SetSeriesPointColor(series, 1, "#ED7D31");
                    SetSeriesPointColor(series, 2, "#F4B183");
                    SetSeriesPointColor(series, 3, "#C55A11");
                    SetSeriesPointColor(series, 4, "#F8CBAD");
                    SetSeriesPointColor(series, 5, "#5B9BD5");
                    SetSeriesPointColor(series, 6, "#BDD7EE");
                    SetSeriesPointColor(series, 7, "#D9EAD3");
                    SetSeriesPointColor(series, 8, "#E7E6E6");
                    SetSeriesPointColor(series, 9, "#F4CCCC");
                    SetSeriesPointColor(series, 10, "#D9D9D9");
                    SetSeriesPointColor(series, 11, "#8EAADB");
                    SetSeriesPointColor(series, 12, "#C6E0B4");
                    SetSeriesPointColor(series, 13, "#E2F0D9");
                    SetSeriesPointColor(series, 14, "#D9D2E9");
                }
                else
                {
                    SetSeriesPointColor(series, 1, "#F8CBAD");
                    SetSeriesPointColor(series, 2, "#F4B183");
                    SetSeriesPointColor(series, 3, "#BDD7EE");
                    SetSeriesPointColor(series, 4, "#D9EAD3");
                    SetSeriesPointColor(series, 5, "#E7E6E6");
                    SetSeriesPointColor(series, 6, "#F4CCCC");
                    SetSeriesPointColor(series, 7, "#D9D9D9");
                    SetSeriesPointColor(series, 8, "#8EAADB");
                    SetSeriesPointColor(series, 9, "#C6E0B4");
                    SetSeriesPointColor(series, 10, "#E2F0D9");
                    SetSeriesPointColor(series, 11, "#D9D2E9");
                }
            }
            finally
            {
                if (dataTable != null) Marshal.ReleaseComObject(dataTable);
                if (valueAxis != null) Marshal.ReleaseComObject(valueAxis);
                if (dataLabels != null) Marshal.ReleaseComObject(dataLabels);
                if (series != null) Marshal.ReleaseComObject(series);
                if (seriesCollection != null) Marshal.ReleaseComObject(seriesCollection);
                if (chart != null) Marshal.ReleaseComObject(chart);
                if (chartObject != null) Marshal.ReleaseComObject(chartObject);
                if (chartArea != null) Marshal.ReleaseComObject(chartArea);
                if (topLeft != null) Marshal.ReleaseComObject(topLeft);
            }
        }

        private static void SetSeriesPointColor(Excel.Series series, int pointIndex, string htmlColor)
        {
            Excel.Point? point = null;

            try
            {
                point = (Excel.Point)series.Points(pointIndex);

                int fillColor = System.Drawing.ColorTranslator.ToOle(
                    System.Drawing.ColorTranslator.FromHtml(htmlColor)
                );

                int borderColor = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);

                point.Interior.Color = fillColor;
                point.Border.Color = borderColor;
                point.Border.Weight = Excel.XlBorderWeight.xlThin;
            }
            finally
            {
                if (point != null)
                    Marshal.ReleaseComObject(point);
            }
        }

        private sealed class EvaluationRow
        {
            public string Group { get; }
            public string Name { get; }
            public string WorkerType { get; }
            public Func<TimeEntry, bool> Predicate { get; }

            public double SumHours { get; set; }
            public double Percent { get; set; }

            public EvaluationRow(
                string group,
                string name,
                Func<TimeEntry, bool> predicate,
                string workerType = "")
            {
                Group = group;
                Name = name;
                WorkerType = workerType;
                Predicate = predicate;
            }
        }
    }


    #endregion

    #region === Styling Excelu ===
    /// <summary>
    /// Styly a formátování pro listy a tabulky v ClosedXML.
    /// </summary>
    internal sealed class ExcelStylingService
    {
        /// <summary>
        /// Detailní listy – formáty sloupců, čísla, datumy (bez změny theme).
        /// </summary>
        public void BeautifyDetailTable(IXLWorksheet ws, IXLTable table)
        {
            var colPopis = table.Field("Popis projektu").Column.ColumnNumber();
            var colDatum = table.Field("Časový záznam").Column.ColumnNumber();
            var colHod = table.Field("Doba v hodinách").Column.ColumnNumber();

            ws.Column(colPopis).Style.Alignment.WrapText = false;
            ws.Column(colDatum).Style.DateFormat.Format = "dd.mm.yyyy";
            ws.Column(colHod).Style.NumberFormat.Format = "0.00";
        }

        /// <summary>
        /// Souhrn – zvýraznění „uživatelských“ řádků, odsazení projektů, grouping, formáty.
        /// </summary>
        public void BeautifyUserSummarySheet(IXLWorksheet wsSummary, IXLTable tableSummary)
        {
            wsSummary.Outline.SummaryVLocation = XLOutlineSummaryVLocation.Top;

            var tblRange = tableSummary.AsRange();
            int firstCol = tblRange.FirstColumn().ColumnNumber();
            int lastCol = tblRange.LastColumn().ColumnNumber();

            var data = tableSummary.DataRange;
            int firstRow = data.FirstRow().RowNumber();
            int lastRow = data.LastRow().RowNumber();

            int colProjekt = tableSummary.Field("Projekt").Column.ColumnNumber();
            int colSoucet = tableSummary.Field("Součet hodin").Column.ColumnNumber();
            int colSuma = tableSummary.Field("Suma (čas. úsek)").Column.ColumnNumber();
            int colDoch = tableSummary.Field("Docházka").Column.ColumnNumber();

            // Vodorovné čáry mezi řádky (uvnitř tabulky)
            var gridColor = XLColor.FromHtml("#95B3D7");
            for (int rr = firstRow; rr <= lastRow; rr++)
            {
                var rowRange = wsSummary.Range(rr, firstCol, rr, lastCol);
                rowRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
                rowRange.Style.Border.BottomBorderColor = gridColor;
            }

            var userRows = new List<int>();
            for (int r = firstRow; r <= lastRow; r++)
                if (string.IsNullOrWhiteSpace(wsSummary.Cell(r, colProjekt).GetString()))
                    userRows.Add(r);

            if (userRows.Count == 0)
            {
                wsSummary.Column(colSoucet).Style.NumberFormat.Format = "0.0#";
                wsSummary.Column(colSuma).Style.NumberFormat.Format = "0.0#";
                wsSummary.Column(colDoch).Style.NumberFormat.Format = "0.0#";
                return;
            }

            var userBack = XLColor.FromHtml("#C0E6F5");
            var userTop = XLColor.FromHtml("#156082");

            var userNoHoursFont = XLColor.FromHtml("#CC0000");


            for (int i = 0; i < userRows.Count; i++)
            {
                int headerRow = userRows[i];
                int nextHeader = (i < userRows.Count - 1) ? userRows[i + 1] : lastRow + 1;
                int startDetail = headerRow + 1;
                int endDetail = nextHeader - 1;

                double reportedHours = 0;
                double attendanceHours = 0;

                var reportedCell = wsSummary.Cell(headerRow, colSuma);
                var attendanceCell = wsSummary.Cell(headerRow, colDoch);

                if (reportedCell.TryGetValue(out double parsedReportedHours))
                    reportedHours = parsedReportedHours;

                if (attendanceCell.TryGetValue(out double parsedAttendanceHours))
                    attendanceHours = parsedAttendanceHours;

                // Červeně označit uživatele, pokud se vykázané hodiny liší
                // od docházky z PowerKey o více než 5 %.
                bool hasInvalidHoursDifference;

                if (attendanceHours <= 0)
                {
                    hasInvalidHoursDifference = reportedHours > 0;
                }
                else
                {
                    double differenceRatio = Math.Abs(reportedHours - attendanceHours) / attendanceHours;
                    hasInvalidHoursDifference = differenceRatio > 0.1;
                }

                var headerRange = wsSummary.Range(headerRow, firstCol, headerRow, lastCol);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = userBack;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thick;
                headerRange.Style.Border.TopBorderColor = userTop;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                if (hasInvalidHoursDifference)
                {
                    headerRange.Style.Font.FontColor = userNoHoursFont;
                }

                wsSummary.Cell(headerRow, colSuma).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                wsSummary.Cell(headerRow, colDoch).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                // Detailní projektové řádky: odsazení + seskupení
                if (endDetail >= startDetail)
                {
                    for (int rr = startDetail; rr <= endDetail; rr++)
                    {
                        var projCell = wsSummary.Cell(rr, colProjekt);
                        projCell.Style.Alignment.Indent = 1;
                        projCell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Left;
                    }

                    wsSummary.Rows(startDetail, endDetail).Group();
                }

                wsSummary.Row(headerRow).Height = 18.5;
            }

            // Formáty čísel
            wsSummary.Column(colSoucet).Style.NumberFormat.Format = "0.0#";
            wsSummary.Column(colSuma).Style.NumberFormat.Format = "0.0#";
            wsSummary.Column(colDoch).Style.NumberFormat.Format = "0.0#";
        }

        /// <summary>
        /// Aplikuje vzhled "Excel TableStyleMedium2" (teal) s vlastním pruhováním.
        /// </summary>
        public void ApplyMedium2Teal(IXLTable table)
        {
            table.Theme = XLTableTheme.TableStyleMedium2;
            table.ShowRowStripes = false;
            table.ShowColumnStripes = false;
            table.ShowAutoFilter = true;

            var headerBg = XLColor.FromHtml("#156082");
            var stripeBg = XLColor.FromHtml("#EAF6FB");
            var white = XLColor.White;

            // Hlavička
            var hdr = table.HeadersRow().Cells();
            hdr.Style.Fill.BackgroundColor = headerBg;
            hdr.Style.Font.FontColor = white;
            hdr.Style.Font.Bold = true;

            var data = table.DataRange;
            for (int r = 1; r <= data.RowCount(); r++)
            {
                var row = data.Row(r);
                row.Style.Fill.BackgroundColor = (r % 2 == 1) ? white : stripeBg;
            }

            // Totals řádek – stejné jako header
            if (table.ShowTotalsRow)
            {
                var tot = table.TotalsRow().Cells();
                tot.Style.Fill.BackgroundColor = headerBg;
                tot.Style.Font.FontColor = white;
                tot.Style.Font.Bold = true;
            }
        }
    }
    #endregion

    #region === Služba pro export (logika mimo UI) ===
    /// <summary>
    /// Orchestruje načtení dat, sestavení sešitu a uložení souboru.
    /// </summary>
    internal sealed class TimeEntryExportService
    {
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly SpecialDayRepository _specialDayRepo;
        private readonly DataTableFactory _tableFactory;
        private readonly ExcelStylingService _styling;

        public TimeEntryExportService(
            TimeEntryRepository timeEntryRepo,
            SpecialDayRepository specialDayRepo,
            DataTableFactory tableFactory,
            ExcelStylingService styling)
        {
            _timeEntryRepo = timeEntryRepo;
            _specialDayRepo = specialDayRepo;
            _tableFactory = tableFactory;
            _styling = styling;
        }
        /// <summary>
        /// Načte data, vytvoří listy a uloží XLSX.
        /// Uživatelský rozsah je chápán jako "od dne včetně do dne včetně".
        /// Pro časové záznamy se horní hranice technicky filtruje jako následující den exkluzivně.
        /// </summary>
        public async Task ExportAsync(
            string filePath,
            DateTime from,
            DateTime to,
            IEnumerable<int> selectedUserGroupIds,
            IEnumerable<int> selectedUserIds,
            IEnumerable<User> selectedUsers,
            User currentUser,
            bool buildEvaluationSheet)
        {
            try
            {
                var range = DateRangeHelper.NormalizeInclusiveDateRange(from, to);

                var fromInclusive = range.FromInclusive;
                var toInclusive = range.ToInclusive;
                var toExclusive = range.ToExclusive;

                var selectedGroupIdSet = selectedUserGroupIds?.ToHashSet() ?? new HashSet<int>();
                var selectedUserIdSet = selectedUserIds?.ToHashSet() ?? new HashSet<int>();

                if (currentUser.LevelOfAccess == 1)
                {
                    selectedGroupIdSet.Clear();
                    selectedUserIdSet.Clear();
                    selectedUserIdSet.Add(currentUser.Id);
                }

                // Důležité:
                // časové záznamy načítáme od fromInclusive včetně
                // do toExclusive exkluzivně.
                var allEntries = await _timeEntryRepo
                    .GetAllTimeEntriesBetweenDatesAsync(fromInclusive, toExclusive)
                    .ConfigureAwait(false);

                // Bezpečnostní filtr i v paměti:
                // kdyby repozitář někdy filtroval špatně nebo vrátil širší rozsah.
                var filtered = allEntries
                    .Where(e =>
                        e.Timestamp.HasValue &&
                        e.Timestamp.Value >= fromInclusive &&
                        e.Timestamp.Value < toExclusive)
                    .Where(e =>
                        e.User != null &&
                        (
                            (e.User.UserGroup != null && selectedGroupIdSet.Contains(e.User.UserGroup.Id))
                            || selectedUserIdSet.Contains(e.User.Id)
                        ))
                    .Where(e => e.EntryTypeId != ExportConstants.OutlookEventEntryTypeId)
                    .Where(e => e.ProjectId != ExportConstants.ExcludedProjectId)
                    .ToList();

                var projects = filtered
                    .Where(e =>
                        e.Project?.ProjectType != 6
                        && e.Project?.ProjectType != 1
                        && e.Project?.ProjectType != 2
                        && e.Project?.ProjectType != null
                        && e.ProjectId != null)
                    .Select(e => e.Project!)
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .OrderBy(p => p.ProjectType)
                    .ThenBy(p => p.ProjectTitle)
                    .ToList();

                var projectIdsForSummary = filtered
                    .Where(e => e.ProjectId.HasValue && e.Project?.DateFullFilled != null)
                    .Select(e => e.ProjectId!.Value)
                    .Distinct()
                    .ToList();

                var cumulativeRows = await _timeEntryRepo
                    .GetCumulativeToFullfilledAsync(projectIdsForSummary)
                    .ConfigureAwait(false);

                var cumDict = cumulativeRows.ToDictionary(
                    k => (k.ProjectId, k.UserId),
                    v => v.MinutesToFullFilled / 60.0
                );

                using var wb = new XLWorkbook();

                // „Časové záznamy“
                var wsBase = wb.AddWorksheet("Časové záznamy");
                var dtBase = _tableFactory.BuildTimeEntries(filtered);
                var tableBase = wsBase.Cell(1, 1).InsertTable(dtBase, "CasoveZaznamy", true);

                _styling.ApplyMedium2Teal(tableBase);
                _styling.BeautifyDetailTable(wsBase, tableBase);
                wsBase.Columns().AdjustToContents();

                // „Souhrn podle uživatele“
                var wsSummary = wb.AddWorksheet("Souhrn podle uživatele");

                var usersForSummary = selectedUsers
                    .Where(u => u != null)
                    .GroupBy(u => u.Id)
                    .Select(g => g.First())
                    .ToList();

                var dtSummary = await _tableFactory
                    .BuildUserSummary(usersForSummary, filtered, fromInclusive, toInclusive, cumDict)
                    .ConfigureAwait(false);

                var tableSummary = wsSummary.Cell(1, 1).InsertTable(dtSummary, "SouhrnUzivatel", true);

                _styling.ApplyMedium2Teal(tableSummary);
                _styling.BeautifyUserSummarySheet(wsSummary, tableSummary);
                wsSummary.Columns().AdjustToContents();

                // „VYHODNOCENÍ“
                if (buildEvaluationSheet)
                {
                    await _tableFactory
                        .BuildEvaluationSheet(wb, filtered, usersForSummary, fromInclusive, toInclusive)
                        .ConfigureAwait(false);
                }

                // Listy podle projektů
                foreach (var proj in projects)
                {
                    var rows = filtered
                        .Where(e => e.Project?.Id == proj.Id)
                        .ToList();

                    if (rows.Count == 0)
                        continue;

                    var safeName = SheetNameSanitizer.MakeSafe(proj.ProjectTitle);
                    var ws = wb.AddWorksheet(safeName);

                    var dtProj = _tableFactory.BuildTimeEntries(rows);
                    var table = ws.Cell(1, 1).InsertTable(dtProj, $"Projekt_{proj.Id}", true);

                    table.ShowTotalsRow = true;

                    var hoursField = table.Fields.FirstOrDefault(f => f.Name == "Doba v hodinách");
                    if (hoursField != null)
                        hoursField.TotalsRowFunction = XLTotalsRowFunction.Sum;

                    _styling.ApplyMedium2Teal(table);
                    _styling.BeautifyDetailTable(ws, table);
                    ws.Columns().AdjustToContents();
                }

                wb.SaveAs(filePath);

                if (buildEvaluationSheet)
                {
                    _tableFactory.AddEvaluationChartsWithExcelInterop(
                        filePath,
                        usersForSummary.Any(user => user.UserGroupId == 6));
                }

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při exportu ClosedXML.", ex);
                throw;
            }
        }

        /// <summary>
        /// Zamkne zápisy za měsíc a speciální dny daného měsíce/roku.
        /// </summary>
        public async Task LockMonthAsync(string monthNameCz, int year)
        {
            await _timeEntryRepo.LockAllEntriesInMonth(monthNameCz).ConfigureAwait(false);
            await _specialDayRepo.LockEntireMonthAsync(FormatHelper.GetMonthNumberFromString(monthNameCz), year).ConfigureAwait(false);
        }
    }
    #endregion
}

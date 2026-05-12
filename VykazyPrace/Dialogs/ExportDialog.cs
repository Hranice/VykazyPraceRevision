using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using System.Diagnostics;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Helpers;
using ClosedXML.Excel;
using DataTable = System.Data.DataTable;
using System.Globalization;
using VykazyPrace.Core.Configuration;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

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
        // Repozitáře
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private readonly UserGroupRepository _userGroupRepository = new();
        private readonly SpecialDayRepository _specialDayRepo = new();

        private readonly User _currentUser;
        private readonly AppConfig _config;

        // Služby
        private readonly DataTableFactory _tableFactory = new();
        private readonly ExcelStylingService _styling = new();


        public ExportDialog(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
            _config = ConfigService.Load();
        }

        #region — Životní cyklus dialogu —
        private async void ExportDialog_Load(object sender, EventArgs e)
        {
            InitializeDatePickers();
            RestoreExportRangeSelectionFromConfig();
            await LoadUserGroupsToTreeViewAsync();

            if (_currentUser.LevelOfAccess > 2)
            {
                gBLock.Visible = true;
            }
        }
        #endregion

        #region — Inicializace UI —
        private List<(RadioButton radioButton, Panel panel)> options;

        /// <summary>
        /// Nastaví výchozí rozmezí (předchozí měsíc) a předvybere měsíc v ComboBoxu.
        /// </summary>
        private void InitializeDatePickers()
        {
            var firstDayThisMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var previousMonth = firstDayThisMonth.AddMonths(-1);
            var range = DateRangeHelper.GetMonthRange(previousMonth);

            dtpFrom.Value = range.From;
            dtpTo.Value = range.To;
            cBMonth.SelectedIndex = previousMonth.Month - 1; // 0..11

            options = new List<(RadioButton, Panel)>
            {
                (rBSpecificTimePeriod, panelSpecificTimePeriod),
                (rBSpecificWeek, panelSpecificWeek),
                (rBSpecificMonth, panelSpecificMonth),
                (rBSpecificYear, panelSpecificYear)
            };

            nUDWeek.Value = ISOWeek.GetWeekOfYear(DateTime.Now);
            cBMonth2.SelectedIndex = DateTime.Now.Month - 1; // 0..11
            nUDYear.Value = DateTime.Now.Year;
        }

        /// <summary>
        /// Načte skupiny a jejich uživatele do TreeView.
        /// Tag na group node = UserGroup
        /// Tag na user node = User
        /// </summary>
        private async Task LoadUserGroupsToTreeViewAsync()
        {
            var userGroups = await _userGroupRepository.GetAllUserGroupsAsync().ConfigureAwait(true);

            tVUserGroupsUsers.BeginUpdate();
            tVUserGroupsUsers.Nodes.Clear();
            tVUserGroupsUsers.CheckBoxes = true;

            if (_currentUser.LevelOfAccess == 1)
            {
                var node = new TreeNode($"{_currentUser.FirstName} {_currentUser.Surname}".Trim())
                {
                    Tag = _currentUser,
                    Checked = true
                };

                tVUserGroupsUsers.Nodes.Add(node);

                tVUserGroupsUsers.Enabled = false;

                tVUserGroupsUsers.EndUpdate();
                return;
            }

            foreach (var group in userGroups.OrderBy(g => g.Title))
            {
                var groupNode = new TreeNode(group.Title ?? "(bez názvu)")
                {
                    Tag = group,
                    Checked = true
                };

                foreach (var user in group.Users.OrderBy(u => u.Surname).ThenBy(u => u.FirstName))
                {
                    var userNode = new TreeNode($"{user.FirstName} {user.Surname}".Trim())
                    {
                        Tag = user,
                        Checked = true
                    };

                    groupNode.Nodes.Add(userNode);
                }

                tVUserGroupsUsers.Nodes.Add(groupNode);
            }

            tVUserGroupsUsers.ExpandAll();
            tVUserGroupsUsers.EndUpdate();

            ApplySavedSelectionToTreeView();
        }

        private void ApplySavedSelectionToTreeView()
        {
            var savedGroupIds = _config.ExportSelection?.SelectedUserGroupIds?.ToHashSet() ?? new HashSet<int>();
            var savedUserIds = _config.ExportSelection?.SelectedUserIds?.ToHashSet() ?? new HashSet<int>();

            bool anythingSaved = savedGroupIds.Count > 0 || savedUserIds.Count > 0;

            // když nic uložené není, defaultně vše vybrané
            if (!anythingSaved)
            {
                CheckAllTreeNodes();
                return;
            }

            _isTreeViewChecking = true;
            try
            {
                foreach (TreeNode groupNode in tVUserGroupsUsers.Nodes)
                {
                    if (groupNode.Tag is not UserGroup group)
                        continue;

                    bool groupChecked = savedGroupIds.Contains(group.Id);
                    groupNode.Checked = groupChecked;

                    foreach (TreeNode userNode in groupNode.Nodes)
                    {
                        if (userNode.Tag is not User user)
                            continue;

                        userNode.Checked = groupChecked || savedUserIds.Contains(user.Id);
                    }

                    // rodič checked pokud je checked skupina nebo aspoň jedno dítě
                    if (!groupChecked)
                        groupNode.Checked = groupNode.Nodes.Cast<TreeNode>().Any(n => n.Checked);
                }
            }
            finally
            {
                _isTreeViewChecking = false;
            }
        }

        private void RestoreExportRangeSelectionFromConfig()
        {
            var exportConfig = _config.ExportSelection;
            if (exportConfig == null)
                return;

            cBBuildEvaluationSheet.Checked = exportConfig.BuildEvaluationSheet;

            // Nejdřív vše odškrtnout, protože radio buttony mohou být v různých containerech
            rBSpecificTimePeriod.Checked = false;
            rBSpecificWeek.Checked = false;
            rBSpecificMonth.Checked = false;
            rBSpecificYear.Checked = false;

            RadioButton targetRadioButton = rBSpecificTimePeriod;

            // nejdřív rok, protože ho používá víc režimů
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

                    if (exportConfig.Month.HasValue && exportConfig.Month.Value >= 1 && exportConfig.Month.Value <= 12)
                        cBMonth2.SelectedIndex = exportConfig.Month.Value - 1;

                    break;

                case ExportRangeType.Year:
                    targetRadioButton = rBSpecificYear;
                    break;
            }

            targetRadioButton.Checked = true;
            SelectOption(targetRadioButton);
        }
        #endregion

        #region — Handlery —
        private async void bSaveAs_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "Export.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var selection = GetSelectedUsersAndGroupsFromTreeView();

            if (selection.SelectedGroupIds.Count == 0 && selection.SelectedUserIds.Count == 0)
            {
                MessageBox.Show("Není vybraná žádná skupina ani uživatel.", "Export", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var exportService = new TimeEntryExportService(_timeEntryRepo, _specialDayRepo, _tableFactory, _styling);

            try
            {
                var (from, to) = GetSelectedExportRange();

                await exportService.ExportAsync(
        sfd.FileName,
        from,
        to,
        selection.SelectedGroupIds,
        selection.SelectedUserIds,
        selection.SelectedUsers,
        _currentUser,
        cBBuildEvaluationSheet.Checked);

                SaveExportSelection(selection);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export selhal. Podrobnosti v logu.\n{ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var (from, to) = DateRangeHelper.GetMonthRangeByIndex(cBMonth.SelectedIndex, dtpFrom.Value.Year);
            dtpFrom.Value = from;
            dtpTo.Value = to;
        }

        private async void bLockEntries_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cBMonth.Text)) return;

            var result = MessageBox.Show($"Zamknout záznamy za měsíc {cBMonth.Text}?", "Zamknout data?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            var exportService = new TimeEntryExportService(_timeEntryRepo, _specialDayRepo, _tableFactory, _styling);
            try
            {
                await exportService.LockMonthAsync(cBMonth.Text, dtpFrom.Value.Year);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Zámek selhal. Podrobnosti v logu.\n{ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void SelectOption(RadioButton selectedRadioButton)
        {
            foreach (var option in options)
            {
                bool isSelected = option.radioButton == selectedRadioButton;
                option.radioButton.Checked = isSelected;
                option.panel.BackColor = isSelected ? Color.White : SystemColors.Control;
            }
        }

        private void radioButtonTimePeriod_CheckedChanged(object sender, EventArgs e)
        {
            var rb = (RadioButton)sender;
            if (rb.Checked)
                SelectOption(rb);
        }

        private void panelTimePeriod_Click(object sender, EventArgs e)
        {
            if (sender == panelSpecificTimePeriod) SelectOption(rBSpecificTimePeriod);
            else if (sender == panelSpecificMonth) SelectOption(rBSpecificMonth);
            else if (sender == panelSpecificWeek) SelectOption(rBSpecificWeek);
            else if (sender == panelSpecificYear) SelectOption(rBSpecificYear);
        }

        private void bSetCurrentWeek_Click(object sender, EventArgs e)
        {
            nUDWeek.Value = ISOWeek.GetWeekOfYear(DateTime.Now);
        }

        private void bSetCurrentMonth_Click(object sender, EventArgs e)
        {
            cBMonth2.SelectedIndex = DateTime.Now.Month - 1; // 0..11
        }

        private void bSetCurrentYear_Click(object sender, EventArgs e)
        {
            nUDYear.Value = DateTime.Now.Year;
        }

        private bool _isTreeViewChecking;
        private void tVUserGroupsUsers_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_isTreeViewChecking) return;

            try
            {
                _isTreeViewChecking = true;

                foreach (TreeNode child in e.Node.Nodes)
                    child.Checked = e.Node.Checked;

                // rodič checked = aspoň jedno dítě checked
                if (e.Node.Parent != null)
                {
                    var parent = e.Node.Parent;
                    parent.Checked = parent.Nodes.Cast<TreeNode>().Any(n => n.Checked);
                }
            }
            finally
            {
                _isTreeViewChecking = false;
            }
        }
        #endregion

        #region - Helpery -
        private void CheckAllTreeNodes()
        {
            _isTreeViewChecking = true;
            try
            {
                foreach (TreeNode groupNode in tVUserGroupsUsers.Nodes)
                {
                    groupNode.Checked = true;

                    foreach (TreeNode userNode in groupNode.Nodes)
                        userNode.Checked = true;
                }
            }
            finally
            {
                _isTreeViewChecking = false;
            }
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

        private void SaveExportSelection(UserSelection selection)
        {
            _config.ExportSelection ??= new ExportSelectionConfig();

            _config.ExportSelection.SelectedUserGroupIds = selection.SelectedGroupIds.ToList();
            _config.ExportSelection.SelectedUserIds = selection.SelectedUserIds.ToList();

            _config.ExportSelection.From = null;
            _config.ExportSelection.To = null;
            _config.ExportSelection.Week = null;
            _config.ExportSelection.Month = null;
            _config.ExportSelection.Year = null;

            _config.ExportSelection.BuildEvaluationSheet = false;

            if (rBSpecificTimePeriod.Checked)
            {
                _config.ExportSelection.SelectedRangeType = ExportRangeType.TimePeriod;
                _config.ExportSelection.From = dtpFrom.Value.Date;
                _config.ExportSelection.To = dtpTo.Value.Date;
            }
            else if (rBSpecificWeek.Checked)
            {
                _config.ExportSelection.SelectedRangeType = ExportRangeType.Week;
                _config.ExportSelection.Week = (int)nUDWeek.Value;
                _config.ExportSelection.Year = (int)nUDYear.Value;
            }
            else if (rBSpecificMonth.Checked)
            {
                _config.ExportSelection.SelectedRangeType = ExportRangeType.Month;
                _config.ExportSelection.Month = cBMonth2.SelectedIndex + 1;
                _config.ExportSelection.Year = (int)nUDYear.Value;
            }
            else if (rBSpecificYear.Checked)
            {
                _config.ExportSelection.SelectedRangeType = ExportRangeType.Year;
                _config.ExportSelection.Year = (int)nUDYear.Value;
            }

            _config.ExportSelection.BuildEvaluationSheet = cBBuildEvaluationSheet.Checked;

            ConfigService.Save(_config);
        }

        private sealed class UserSelection
        {
            public HashSet<int> SelectedGroupIds { get; } = new();
            public HashSet<int> SelectedUserIds { get; } = new();
            public List<User> SelectedUsers { get; } = new();
        }

        private UserSelection GetSelectedUsersAndGroupsFromTreeView()
        {
            var result = new UserSelection();

            if (_currentUser.LevelOfAccess == 1)
            {
                result.SelectedUserIds.Add(_currentUser.Id);
                result.SelectedUsers.Add(_currentUser);
                return result;
            }

            foreach (TreeNode groupNode in tVUserGroupsUsers.Nodes)
            {
                if (groupNode.Tag is not UserGroup group)
                    continue;

                var checkedUserNodes = groupNode.Nodes
                    .Cast<TreeNode>()
                    .Where(n => n.Checked && n.Tag is User)
                    .ToList();

                foreach (var userNode in checkedUserNodes)
                {
                    if (userNode.Tag is User user)
                    {
                        result.SelectedUserIds.Add(user.Id);
                        result.SelectedUsers.Add(user);
                    }
                }

                bool allUsersChecked = groupNode.Nodes.Count > 0 && checkedUserNodes.Count == groupNode.Nodes.Count;

                if (groupNode.Checked && (groupNode.Nodes.Count == 0 || allUsersChecked))
                {
                    result.SelectedGroupIds.Add(group.Id);
                }
            }

            return result;
        }
        #endregion
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
        public const int ExcludedProjectId = 132;

        /// <summary>
        /// Typ záznamu, který je spolu s <see cref="ExcludedProjectId"/> vyloučen z exportu.
        /// </summary>
        public const int ExcludedEntryTypeId = 24;

        /// <summary>
        /// Projekt reprezentující nepřítomnost – nezapočítává se do souhrnů podle uživatele.
        /// </summary>
        public const int AbsenceProjectId = 23;

        /// <summary>
        /// Typ záznamu reprezentující outlook událost (nevalidní záznam) – nezapočítává se do souhrnů podle uživatele.
        /// </summary>
        public const int OutlookEventEntryTypeId = 25;

        // VYHODNOCENÍ
        public const int AutomationProjectId = 31;

        public const int ProductionSdProjectId = 19;
        public const int ProductionHpProjectId = 20;
        public const int ProductionMetProjectId = 17;
        public const int ProductionKomProjectId = 21;
        public const int ProductionSorProjectId = 140;
        public const int ProductionOtherProjectId = 22;

        public const int ClubYoungTechnicianEntryTypeId = 8;
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
    internal sealed class DataTableFactory
    {
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
            DateTime exportMonth,
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

            // Docházka z PowerKey
            Dictionary<int, double> powerKeyData;

            try
            {
                var pkHelper = new PowerKeyHelper();
                powerKeyData = await pkHelper
                    .GetWorkedHoursByPersonalNumberForMonthAsync(exportMonth)
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

            // Ignorujeme svačinu v souhrnu
            var filteredEntries = timeEntries.Where(e => e.ProjectId != ExportConstants.ExcludedProjectId).ToList();

            var entriesByUserId = filteredEntries
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

                double totalHours = userEntries.Sum(e => e.EntryMinutes) / 60.0;
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

        public void BuildEvaluationSheet(XLWorkbook wb, IEnumerable<TimeEntry> entries)
        {
            var ws = wb.AddWorksheet("VYHODNOCENÍ");

            ws.Position = 1;
            ws.TabColor = XLColor.Yellow;

            var rows = new List<EvaluationRow>
    {
        new("Projekty", "EXTERNÍ PROJEKTY",
            e => e.Project?.ProjectDescription?.Contains("E", StringComparison.OrdinalIgnoreCase) == true),

        new("Projekty", "INTERNÍ PROJEKTY",
            e => e.Project?.ProjectDescription?.Contains("I", StringComparison.OrdinalIgnoreCase) == true),

        new("Automatizace", "Provoz Automatizace",
            e => e.ProjectId == ExportConstants.AutomationProjectId),

        new("Provoz výroba", "Provoz SD",
            e => e.ProjectId == ExportConstants.ProductionSdProjectId),

        new("Provoz výroba", "Provoz HP",
            e => e.ProjectId == ExportConstants.ProductionHpProjectId),

        new("Provoz výroba", "Provoz MET",
            e => e.ProjectId == ExportConstants.ProductionMetProjectId),

        new("Provoz výroba", "Provoz KOM",
            e => e.ProjectId == ExportConstants.ProductionKomProjectId),

        new("Provoz výroba", "Provoz SOR",
            e => e.ProjectId == ExportConstants.ProductionSorProjectId),

        new("Ostatní", "Ostatní",
            e => e.ProjectId == ExportConstants.ProductionOtherProjectId),

        new("Ostatní", "Nepřítomnost",
            e => e.ProjectId == ExportConstants.AbsenceProjectId),

        new("Ostatní", "Kroužek MT",
            e => e.EntryTypeId == ExportConstants.ClubYoungTechnicianEntryTypeId)
    };

            foreach (var row in rows)
            {
                row.SumHours = entries
                    .Where(row.Predicate)
                    .Sum(e => e.EntryMinutes) / 60.0;
            }

            double totalHours = rows.Sum(r => r.SumHours);

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

            int currentRow = 3;

            // Budeme si pamatovat startovní řádky kategorií,
            // aby pomocná data pro graf mohla odkazovat na sloupec E.
            var groupStartRows = new Dictionary<string, int>();

            foreach (var group in rows.GroupBy(r => r.Group))
            {
                int groupStartRow = currentRow;
                groupStartRows[group.Key] = groupStartRow;

                foreach (var item in group)
                {
                    ws.Cell(currentRow, 2).Value = item.Name;
                    ws.Cell(currentRow, 3).Value = item.SumHours;
                    ws.Cell(currentRow, 4).Value = item.Percent;

                    currentRow++;
                }

                int groupEndRow = currentRow - 1;

                ws.Cell(groupStartRow, 1).Value = group.Key;
                ws.Cell(groupStartRow, 5).Value = groupPercents[group.Key];

                if (groupEndRow > groupStartRow)
                {
                    ws.Range(groupStartRow, 1, groupEndRow, 1).Merge();
                    ws.Range(groupStartRow, 5, groupEndRow, 5).Merge();
                }

                var groupRange = ws.Range(groupStartRow, 1, groupEndRow, 5);
                var firstColumnGroupRange = ws.Range(groupStartRow, 1, groupEndRow, 1);
                var lastColumnGroupRange = ws.Range(groupStartRow, 5, groupEndRow, 5);

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

            // Pomocná data pro koláčový graf.
            // Jsou přímo pod grafem v oblasti G3:H6, takže v Excelu nebudou vidět,
            // protože graf leží přes oblast G3:M17.
            int chartDataRow = 3;

            foreach (var groupName in new[] { "Projekty", "Automatizace", "Provoz výroba", "Ostatní" })
            {
                if (!groupStartRows.TryGetValue(groupName, out int sourceRow))
                    continue;

                ws.Cell(chartDataRow, 7).Value = groupName;            // G
                ws.Cell(chartDataRow, 8).FormulaA1 = $"=E{sourceRow}"; // H

                chartDataRow++;
            }

            ws.Column(7).Style.NumberFormat.Format = "@";
            ws.Column(8).Style.NumberFormat.Format = "0.00%";

            ws.Column(3).Style.NumberFormat.Format = "# ##0.0";
            ws.Column(4).Style.NumberFormat.Format = "0.00%";
            ws.Column(5).Style.NumberFormat.Format = "0.00%";

            ws.Column(3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Column(4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Column(5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

            ws.Column(5).Style.Font.Bold = true;
            ws.Column(5).Style.Font.FontSize = 14;

            // Nejdřív dopočítat podle obsahu.
            ws.Columns().AdjustToContents();

            // Přibližný převod pixelů na Excel šířku:
            // ExcelWidth = (pixels - 5) / 7
            ws.Column(1).Width = 17.57; // cca 128 px - podle šablony od MV
            ws.Column(2).Width = 20.14; // cca 146 px - podle šablony od MV
            ws.Column(3).Width = 8.43;  // cca 64 px - podle šablony od MV
            ws.Column(4).Width = 8.43;  // cca 64 px - podle šablony od MV
            ws.Column(5).Width = 9;     // cca 68 px - podle šablony od MV

            ws.SetTabActive();
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

        public void AddEvaluationPieChartWithExcelInterop(string filePath)
        {
            Excel.Application? excel = null;
            Excel.Workbook? workbook = null;
            Excel.Worksheet? ws = null;
            Excel.ChartObjects? chartObjects = null;
            Excel.ChartObject? chartObject = null;
            Excel.Chart? chart = null;
            Excel.SeriesCollection? seriesCollection = null;
            Excel.Series? series = null;
            Excel.DataLabels? dataLabels = null;

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

                // Umístění grafu přesně do oblasti G3:M17
                var topLeft = (Excel.Range)ws.Range["G3"];
                var chartArea = (Excel.Range)ws.Range["G3:M17"];

                chartObject = chartObjects.Add(
                    (double)topLeft.Left,
                    (double)topLeft.Top,
                    (double)chartArea.Width,
                    (double)chartArea.Height
                );

                chartObject.Name = "VyhodnoceniPieChart";

                chart = chartObject.Chart;
                chart.ChartType = Excel.XlChartType.xl3DPie;

                // Zdroj dat: pomocná oblast pod grafem
                // G3:G6 = názvy kategorií
                // H3:H6 = procenta ze sloupce E
                seriesCollection = (Excel.SeriesCollection)chart.SeriesCollection();
                series = seriesCollection.NewSeries();

                series.XValues = "='VYHODNOCENÍ'!$G$3:$G$6";
                series.Values = "='VYHODNOCENÍ'!$H$3:$H$6";
                series.Name = "Podíl";

                // Titulek
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Podíl využití časového fondu v rámci" + Environment.NewLine + "AUTOMATIZACE";
                chart.ChartTitle.Font.Bold = true;
                chart.ChartTitle.Font.Size = 14;

                // Legenda dole
                chart.HasLegend = true;
                chart.Legend.Position = Excel.XlLegendPosition.xlLegendPositionBottom;
                chart.Legend.Font.Size = 10;

                // 3D natočení podobné předloze
                chart.Rotation = 0;
                chart.Elevation = 25;
                chart.Perspective = 30;

                // Vzhled oblasti grafu - bílé pozadí + černý rámeček
                chart.ChartArea.Border.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.Black);
                chart.ChartArea.Border.Weight = Excel.XlBorderWeight.xlThin;

                chart.ChartArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                chart.PlotArea.Interior.Color = System.Drawing.ColorTranslator.ToOle(System.Drawing.Color.White);
                chart.PlotArea.Border.LineStyle = Excel.XlLineStyle.xlLineStyleNone;

                // Datové popisky
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

                // Barvy výsečí podle předlohy:
                // 1 Projekty - béžová
                // 2 Automatizace - světle modrá
                // 3 Provoz výroba - světle šedá
                // 4 Ostatní - světle zelená
                SetPiePointColor(series, 1, "#F8CBAD");
                SetPiePointColor(series, 2, "#BDD7EE");
                SetPiePointColor(series, 3, "#D9D9D9");
                SetPiePointColor(series, 4, "#E2F0D9");

                workbook.Save();
            }
            finally
            {
                if (dataLabels != null) Marshal.ReleaseComObject(dataLabels);
                if (series != null) Marshal.ReleaseComObject(series);
                if (seriesCollection != null) Marshal.ReleaseComObject(seriesCollection);
                if (chart != null) Marshal.ReleaseComObject(chart);
                if (chartObject != null) Marshal.ReleaseComObject(chartObject);
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

        private static void SetPiePointColor(Excel.Series series, int pointIndex, string htmlColor)
        {
            Excel.Point? point = null;

            try
            {
                point = (Excel.Point)series.Points(pointIndex);

                int fillColor = System.Drawing.ColorTranslator.ToOle(
                    System.Drawing.ColorTranslator.FromHtml(htmlColor)
                );

                int borderColor = System.Drawing.ColorTranslator.ToOle(
                    System.Drawing.Color.Black
                );

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
            public Func<TimeEntry, bool> Predicate { get; }

            public double SumHours { get; set; }
            public double Percent { get; set; }

            public EvaluationRow(string group, string name, Func<TimeEntry, bool> predicate)
            {
                Group = group;
                Name = name;
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

                double totalHours = 0;

                var totalCell = wsSummary.Cell(headerRow, colSuma);

                if (totalCell.TryGetValue(out double parsedTotalHours))
                    totalHours = parsedTotalHours;

                bool hasNoHours = totalHours <= 0;

                var headerRange = wsSummary.Range(headerRow, firstCol, headerRow, lastCol);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = userBack;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thick;
                headerRange.Style.Border.TopBorderColor = userTop;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

                if (hasNoHours)
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
                var selectedGroupIdSet = selectedUserGroupIds?.ToHashSet() ?? new HashSet<int>();
                var selectedUserIdSet = selectedUserIds?.ToHashSet() ?? new HashSet<int>();

                if (currentUser.LevelOfAccess == 1)
                {
                    selectedGroupIdSet.Clear();
                    selectedUserIdSet.Clear();
                    selectedUserIdSet.Add(currentUser.Id);
                }

                var allEntries = await _timeEntryRepo.GetAllTimeEntriesBetweenDatesAsync(from, to).ConfigureAwait(false);

                var filtered = allEntries
                    .Where(e =>
                        e.User != null &&
                        (
                            (e.User.UserGroup != null && selectedGroupIdSet.Contains(e.User.UserGroup.Id))
                            || selectedUserIdSet.Contains(e.User.Id)
                        ))
                    // Exkludované záznamy nepřítomnosti
                    //.Where(e => !(e.ProjectId == ExportConstants.ExcludedProjectId && e.EntryTypeId == ExportConstants.ExcludedEntryTypeId))
                    .Where(e => e.EntryTypeId != ExportConstants.OutlookEventEntryTypeId)
                    .ToList();

                // Projekty pro jednotlivé listy (bez svačiny)
                var projects = filtered
                    // odkomentováno - sestavovali jsme bez nepřítomnosti atd
                    //.Where(e => e.Project?.ProjectType == 0 && e.ProjectId != null)
                    .Where(e => e.Project?.ProjectType != 6 && e.ProjectId != null)
                    .Select(e => e.Project!)
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .ToList();

                // Podklady pro cumulativní hodiny do zplnohodnocení
                var projectIdsForSummary = filtered
                    .Where(e => e.ProjectId.HasValue && e.Project?.DateFullFilled != null)
                    .Select(e => e.ProjectId!.Value)
                    .Distinct()
                    .ToList();

                var cumulativeRows = await _timeEntryRepo.GetCumulativeToFullfilledAsync(projectIdsForSummary).ConfigureAwait(false);
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
    .GroupBy(u => u.Id)
    .Select(g => g.First())
    .ToList();

                var dtSummary = await _tableFactory
                    .BuildUserSummary(usersForSummary, filtered, from, cumDict)
                    .ConfigureAwait(false);
                var tableSummary = wsSummary.Cell(1, 1).InsertTable(dtSummary, "SouhrnUzivatel", true);
                _styling.ApplyMedium2Teal(tableSummary);
                _styling.BeautifyUserSummarySheet(wsSummary, tableSummary);
                wsSummary.Columns().AdjustToContents();

                // „VYHODNOCENÍ“
                if(buildEvaluationSheet) _tableFactory.BuildEvaluationSheet(wb, filtered);

                // Listy podle projektů
                foreach (var proj in projects)
                {
                    var rows = filtered.Where(e => e.Project?.Id == proj.Id).ToList();
                    if (rows.Count == 0) continue;

                    var safeName = SheetNameSanitizer.MakeSafe(proj.ProjectTitle);
                    var ws = wb.AddWorksheet(safeName);

                    var dtProj = _tableFactory.BuildTimeEntries(rows);
                    var table = ws.Cell(1, 1).InsertTable(dtProj, $"Projekt_{proj.Id}", true);
                    table.ShowTotalsRow = true;
                    var hoursField = table.Fields.FirstOrDefault(f => f.Name == "Doba v hodinách");
                    if (hoursField != null) hoursField.TotalsRowFunction = XLTotalsRowFunction.Sum;

                    _styling.ApplyMedium2Teal(table);
                    _styling.BeautifyDetailTable(ws, table);
                    ws.Columns().AdjustToContents();
                }

                wb.SaveAs(filePath);

                if(buildEvaluationSheet) _tableFactory.AddEvaluationPieChartWithExcelInterop(filePath);

                Process.Start(new ProcessStartInfo { FileName = filePath, UseShellExecute = true });
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při exportu ClosedXML.", ex);
                throw; // nechť UI rozhodne, jak zobrazit
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
using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using System.Diagnostics;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Helpers;
using ClosedXML.Excel;
using DataTable = System.Data.DataTable;

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
        // Repozitáře a servisní vrstvy – injektovatelné (pro jednoduchost zde new())
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private readonly UserGroupRepository _userGroupRepository = new();
        private readonly SpecialDayRepository _specialDayRepo = new();

        // Služby
        private readonly DataTableFactory _tableFactory = new();
        private readonly ExcelStylingService _styling = new();

        public ExportDialog()
        {
            InitializeComponent();
        }

        #region — Životní cyklus dialogu —
        private async void ExportDialog_Load(object sender, EventArgs e)
        {
            InitializeDatePickers();
            await LoadUserGroupsToCheckedListAsync();
        }
        #endregion

        #region — Inicializace UI —
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
            cboMonth.SelectedIndex = previousMonth.Month - 1; // 0..11
        }

        /// <summary>
        /// Načte skupiny uživatelů, naplní a celé předvybere v CheckedListBoxu.
        /// </summary>
        private async Task LoadUserGroupsToCheckedListAsync()
        {
            var userGroups = await _userGroupRepository.GetAllUserGroupsAsync().ConfigureAwait(false);

            clbUserGroups.Items.Clear();
            clbUserGroups.Items.AddRange(userGroups.ToArray());
            clbUserGroups.DisplayMember = nameof(UserGroup.Title);

            for (int i = 0; i < clbUserGroups.Items.Count; i++)
                clbUserGroups.SetItemChecked(i, true);
        }
        #endregion

        #region — Handlery —
        private async void btnSaveAs_Click(object sender, EventArgs e)
        {
            using var sfd = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "Export.xlsx" };
            if (sfd.ShowDialog() != DialogResult.OK) return;

            var selectedGroupIds = clbUserGroups.CheckedItems
                .Cast<UserGroup>()
                .Select(g => g.Id)
                .ToList();

            var exportService = new TimeEntryExportService(_timeEntryRepo, _specialDayRepo, _tableFactory, _styling);

            try
            {
                await exportService.ExportAsync(sfd.FileName, dtpFrom.Value, dtpTo.Value, selectedGroupIds);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Export selhal. Podrobnosti v logu.\n{ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var (from, to) = DateRangeHelper.GetMonthRangeByIndex(cboMonth.SelectedIndex, dtpFrom.Value.Year);
            dtpFrom.Value = from;
            dtpTo.Value = to;
        }

        private async void btnLockEntries_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(cboMonth.Text)) return;

            var result = MessageBox.Show($"Zamknout záznamy za měsíc {cboMonth.Text}?", "Zamknout data?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result != DialogResult.Yes) return;

            var exportService = new TimeEntryExportService(_timeEntryRepo, _specialDayRepo, _tableFactory, _styling);
            try
            {
                await exportService.LockMonthAsync(cboMonth.Text, dtpFrom.Value.Year);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Zámek selhal. Podrobnosti v logu.\n{ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            dt.Columns.Add("Suma (měsíc)", typeof(double));
            dt.Columns.Add("Docházka", typeof(double));
            dt.Columns.Add("Suma (před zplnohodnotněním projektu)", typeof(double));

            // Docházka z PowerKey
            var pkHelper = new PowerKeyHelper();
            var powerKeyData = await pkHelper.GetWorkedHoursByPersonalNumberForMonthAsync(exportMonth).ConfigureAwait(false);

            // Ignorujeme nepřítomnost v souhrnu
            var filteredEntries = timeEntries.Where(e => e.ProjectId != ExportConstants.AbsenceProjectId).ToList();

            var groupedUsers = filteredEntries
                .Where(e => e.User != null)
                .GroupBy(e => new
                {
                    e.User!.Id,
                    e.User!.PersonalNumber,
                    FullName = $"{e.User.FirstName} {e.User.Surname}".Trim()
                })
                .OrderBy(g => g.Key.PersonalNumber)
                .ThenBy(g => g.Key.FullName);

            foreach (var userGroup in groupedUsers)
            {
                double totalHours = userGroup.Sum(e => e.EntryMinutes) / 60.0;
                double attendance = powerKeyData.TryGetValue(userGroup.Key.PersonalNumber, out double h) ? h : 0;

                // Souhrnný řádek (uživatel) – Projekt/Popis prázdné
                dt.Rows.Add(
                    userGroup.Key.PersonalNumber,
                    userGroup.Key.FullName,
                    string.Empty,
                    string.Empty,
                    DBNull.Value!,
                    totalHours,
                    attendance,
                    DBNull.Value!
                );

                // Projekty pod uživatelem
                var projects = userGroup
                    .Where(e => e.Project != null)
                    .GroupBy(e => new { e.Project!.Id, e.Project.ProjectTitle, e.Project.ProjectDescription, e.Project.DateFullFilled })
                    .OrderBy(g => g.Key.ProjectTitle);

                foreach (var proj in projects)
                {
                    double monthlyHours = proj.Sum(e => e.EntryMinutes) / 60.0;
                    double? cumHours = null;

                    if (proj.Key.DateFullFilled.HasValue)
                    {
                        int pid = proj.Key.Id;
                        int uid = userGroup.Key.Id;
                        if (cumToFullfilledDict.TryGetValue((pid, uid), out var val))
                            cumHours = val;
                    }

                    dt.Rows.Add(
                        userGroup.Key.PersonalNumber,
                        userGroup.Key.FullName,
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
            int colSuma = tableSummary.Field("Suma (měsíc)").Column.ColumnNumber();
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

            for (int i = 0; i < userRows.Count; i++)
            {
                int headerRow = userRows[i];
                int nextHeader = (i < userRows.Count - 1) ? userRows[i + 1] : lastRow + 1;
                int startDetail = headerRow + 1;
                int endDetail = nextHeader - 1;

                var headerRange = wsSummary.Range(headerRow, firstCol, headerRow, lastCol);
                headerRange.Style.Font.Bold = true;
                headerRange.Style.Fill.BackgroundColor = userBack;
                headerRange.Style.Border.TopBorder = XLBorderStyleValues.Thick;
                headerRange.Style.Border.TopBorderColor = userTop;
                headerRange.Style.Border.BottomBorder = XLBorderStyleValues.Thin;

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
            IEnumerable<int> selectedUserGroupIds)
        {
            try
            {
                // 1) Načtení záznamů v rozsahu
                var allEntries = await _timeEntryRepo.GetAllTimeEntriesBetweenDatesAsync(from, to).ConfigureAwait(false);

                // 2) Filtrování dle skupin a vyloučené kombinace (projekt+typ)
                var filtered = allEntries
                    .Where(e => e.User?.UserGroup != null && selectedUserGroupIds.Contains(e.User.UserGroup.Id))
                    .Where(e => !(e.ProjectId == ExportConstants.ExcludedProjectId && e.EntryTypeId == ExportConstants.ExcludedEntryTypeId))
                    .ToList();

                // 3) Projekty pro jednotlivé listy (jen reálné projekty typu 0)
                var projects = filtered
                    .Where(e => e.Project?.ProjectType == 0 && e.ProjectId != null)
                    .Select(e => e.Project!)
                    .GroupBy(p => p.Id)
                    .Select(g => g.First())
                    .ToList();

                // 4) Podklady pro cumulativní hodiny do zplnohodnocení
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

                // A) „Časové záznamy“
                var wsBase = wb.AddWorksheet("Časové záznamy");
                var dtBase = _tableFactory.BuildTimeEntries(filtered);
                var tableBase = wsBase.Cell(1, 1).InsertTable(dtBase, "CasoveZaznamy", true);
                _styling.ApplyMedium2Teal(tableBase);
                _styling.BeautifyDetailTable(wsBase, tableBase);
                wsBase.Columns().AdjustToContents();

                // B) „Souhrn podle uživatele“
                var wsSummary = wb.AddWorksheet("Souhrn podle uživatele");
                var dtSummary = await _tableFactory.BuildUserSummary(filtered, from, cumDict).ConfigureAwait(false);
                var tableSummary = wsSummary.Cell(1, 1).InsertTable(dtSummary, "SouhrnUzivatel", true);
                _styling.ApplyMedium2Teal(tableSummary);
                _styling.BeautifyUserSummarySheet(wsSummary, tableSummary);
                wsSummary.Columns().AdjustToContents();

                // C) Listy podle projektů
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

                wsSummary.SetTabActive();
                wb.SaveAs(filePath);
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
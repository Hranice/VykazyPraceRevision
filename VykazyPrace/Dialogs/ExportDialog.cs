using System.Data;
using System.Runtime.InteropServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using Microsoft.Office.Interop.Excel;
using Range = Microsoft.Office.Interop.Excel.Range;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.Diagnostics;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.Dialogs
{
    public partial class ExportDialog : Form
    {
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private readonly UserGroupRepository _userGroupRepository = new();
        private readonly SpecialDayRepository _specialDayRepo = new();

        public ExportDialog()
        {
            InitializeComponent();
        }

        private async void ExportDialog_Load(object sender, EventArgs e)
        {
            InitializeDatePickers();
            await UpdateCheckedListBox();
        }

        private async Task UpdateCheckedListBox()
        {
            var userGroups = await _userGroupRepository.GetAllUserGroupsAsync();

            checkedListBoxUserGroups.Items.Clear();
            checkedListBoxUserGroups.Items.AddRange(userGroups.ToArray());
            checkedListBoxUserGroups.DisplayMember = nameof(UserGroup.Title);

            for (int i = 0; i < checkedListBoxUserGroups.Items.Count; i++)
            {
                checkedListBoxUserGroups.SetItemChecked(i, true);
            }
        }

        private void InitializeDatePickers()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePicker1.Value = firstDayOfMonth.AddMonths(-1);
            dateTimePicker2.Value = new DateTime(firstDayOfMonth.Year, dateTimePicker1.Value.Month, DateTime.DaysInMonth(firstDayOfMonth.Year, dateTimePicker1.Value.Month));
            comboBoxMonth.SelectedIndex = firstDayOfMonth.Month - 2;
        }

        private async Task ExportToExcel(string filePath)
        {
            var excelApp = new Application();
            if (excelApp == null)
            {
                AppLogger.Error("Excel nebyl nalezen na tomto počítači.");
                return;
            }

            try
            {
                var workbook = excelApp.Workbooks.Add();

                var allEntries = await _timeEntryRepo
                    .GetAllTimeEntriesBetweenDatesAsync(dateTimePicker1.Value, dateTimePicker2.Value);

                var selectedGroupIds = checkedListBoxUserGroups.CheckedItems
                    .Cast<UserGroup>()
                    .Select(g => g.Id)
                    .ToList();

                var timeEntries = allEntries
                    .Where(e =>
                        e.User?.UserGroup != null
                        && selectedGroupIds.Contains(e.User.UserGroup.Id)
                        && !(e.ProjectId == 132 && e.EntryTypeId == 24)
                    )
                    .ToList();



                var projects = timeEntries
                    .Where(e => e.Project?.ProjectType == 0)
                    .Select(e => e.Project!)
                    .DistinctBy(p => p.Id)
                    .ToList();

                Worksheet baseSheet = (Worksheet)workbook.Sheets[1];
                GenerateTimeEntriesSheet(baseSheet, timeEntries);

                Worksheet summarySheet = (Worksheet)workbook.Sheets.Add(After: baseSheet);
                await GenerateUserSummarySheet(summarySheet, timeEntries, dateTimePicker1.Value);


                await GenerateProjectSheets(workbook, timeEntries, projects, summarySheet);

                summarySheet.Activate();
                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();
                Marshal.ReleaseComObject(workbook);

                Process.Start(new ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při exportu do Excelu.", ex);
            }
            finally
            {
                GC.Collect();
                GC.WaitForPendingFinalizers();
                Marshal.ReleaseComObject(excelApp);
            }
        }
        private void GenerateTimeEntriesSheet(Worksheet sheet, List<TimeEntry> timeEntries)
        {
            sheet.Name = "Časové záznamy";
            string[] headers = {
        "Osobní číslo", "Jméno", "Skupina", "Projekt", "Popis projektu",
        "Typ záznamu", "Časový záznam", "Popis", "Poznámky", "Doba v hodinách"
    };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1] = headers[i];

            int row = 2;
            foreach (var e in timeEntries)
            {
                sheet.Cells[row, 1] = e.User?.PersonalNumber ?? 0;
                sheet.Cells[row, 2] = $"{e.User?.FirstName} {e.User?.Surname}".Trim();
                sheet.Cells[row, 3] = e.User?.UserGroup?.Title ?? "CHYBÍ DATA";
                sheet.Cells[row, 4] = e.Project?.ProjectTitle ?? "N/A";

                var descCell = (Range)sheet.Cells[row, 5];
                descCell.NumberFormat = "@";
                descCell.Value2 = $"'{e.Project?.ProjectDescription ?? "N/A"}";

                sheet.Cells[row, 6] = e.EntryType?.Title ?? "Neznámý typ";
                sheet.Cells[row, 7] = e.Timestamp?.ToString("yyyy-MM-dd") ?? "N/A";
                sheet.Cells[row, 8] = e.Description ?? "N/A";
                sheet.Cells[row, 9] = e.Note ?? "N/A";
                sheet.Cells[row, 10] = e.EntryMinutes / 60.0;
                row++;
            }

            sheet.Columns.AutoFit();
        }

        private async Task GenerateProjectSheets(Workbook workbook, List<TimeEntry> entries, List<Project> projects, Worksheet insertAfter)
        {
            foreach (var proj in projects)
            {
                var projRows = entries.Where(e => e.Project?.Id == proj.Id).ToList();
                if (!projRows.Any()) continue;

                Worksheet pSheet = (Worksheet)workbook.Sheets.Add(After: insertAfter);
                string safeName = string.Join("_", proj.ProjectTitle.Split(Path.GetInvalidFileNameChars()));
                pSheet.Name = safeName.Length > 31 ? safeName[..31] : safeName;

                string[] headers = {
            "Osobní číslo", "Jméno", "Skupina", "Projekt", "Popis projektu",
            "Typ záznamu", "Časový záznam", "Popis", "Poznámky", "Doba v hodinách"
        };

                for (int i = 0; i < headers.Length; i++)
                    pSheet.Cells[1, i + 1] = headers[i];

                int row = 2;
                foreach (var e in projRows)
                {
                    pSheet.Cells[row, 1] = e.User?.PersonalNumber ?? 0;
                    pSheet.Cells[row, 2] = $"{e.User?.FirstName} {e.User?.Surname}".Trim();
                    pSheet.Cells[row, 3] = e.User?.UserGroup?.Title ?? "CHYBÍ DATA";
                    pSheet.Cells[row, 4] = e.Project?.ProjectTitle ?? "N/A";

                    var descCell = (Range)pSheet.Cells[row, 5];
                    descCell.NumberFormat = "@";
                    descCell.Value2 = $"'{e.Project?.ProjectDescription ?? "N/A"}";

                    pSheet.Cells[row, 6] = e.EntryType?.Title ?? "Neznámý typ";
                    pSheet.Cells[row, 7] = e.Timestamp?.ToString("yyyy-MM-dd") ?? "N/A";
                    pSheet.Cells[row, 8] = e.Description ?? "N/A";
                    pSheet.Cells[row, 9] = e.Note ?? "N/A";
                    pSheet.Cells[row, 10] = e.EntryMinutes / 60.0;
                    row++;
                }

                pSheet.Columns.AutoFit();
                var range = pSheet.Range[$"A1:J{row - 1}"];
                var table = pSheet.ListObjects.AddEx(XlListObjectSourceType.xlSrcRange, range, XlYesNoGuess.xlYes);
                table.ShowTotals = true;
                table.ListColumns["Doba v hodinách"].TotalsCalculation = XlTotalsCalculation.xlTotalsCalculationSum;
            }
        }

        private async Task GenerateUserSummarySheet(Worksheet sheet, List<TimeEntry> timeEntries, DateTime exportMonth)
        {
            sheet.Name = "Souhrn podle uživatele";

            string[] headers = {
        "Osobní číslo", "Jméno", "Projekt", "Popis projektu",
        "Součet hodin", "Suma", "Docházka"
    };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1] = headers[i];

            // Ignorovat nepřítomnost
            var filteredEntries = timeEntries
                .Where(e => e.ProjectId != 23)
                .ToList();

            var grouped = filteredEntries
                .GroupBy(e => new
                {
                    e.User!.PersonalNumber,
                    FullName = $"{e.User.FirstName} {e.User.Surname}".Trim()
                })
                .OrderBy(g => g.Key.PersonalNumber)
                .ThenBy(g => g.Key.FullName)
                .ToList();


            // načtení docházky z PowerKey do Dictionary
            var pkHelper = new PowerKeyHelper();
            var powerKeyData = await pkHelper.GetWorkedHoursByPersonalNumberForMonthAsync(exportMonth);

            int row = 2;

            foreach (var userGroup in grouped)
            {
                var userKey = userGroup.Key;

                var projects = userGroup
                    .GroupBy(e => new
                    {
                        e.Project!.ProjectTitle,
                        e.Project.ProjectDescription
                    })
                    .OrderBy(g => g.Key.ProjectTitle);

                double totalHours = userGroup.Sum(e => e.EntryMinutes) / 60.0;
                double attendance = powerKeyData.TryGetValue(userKey.PersonalNumber, out double h) ? h : 0;

                // souhrnný řádek
                sheet.Cells[row, 1] = userKey.PersonalNumber;
                sheet.Cells[row, 2] = userKey.FullName;
                sheet.Cells[row, 6] = totalHours;
                sheet.Cells[row, 7] = attendance;

                ((Range)sheet.Cells[row, 1]).Font.Bold = true;
                ((Range)sheet.Cells[row, 2]).Font.Bold = true;
                ((Range)sheet.Cells[row, 6]).Font.Bold = true;
                ((Range)sheet.Cells[row, 7]).Font.Bold = true;

                row++;

                // detailní projekty
                foreach (var proj in projects)
                {
                    sheet.Cells[row, 3] = proj.Key.ProjectTitle;

                    var descCell = (Range)sheet.Cells[row, 4];
                    descCell.NumberFormat = "@";
                    descCell.Value2 = $"'{proj.Key.ProjectDescription}";

                    sheet.Cells[row, 5] = proj.Sum(e => e.EntryMinutes) / 60.0;

                    row++;
                }

                // seskupení projektových řádků pod daného uživatele
                if (projects.Any())
                {
                    int startRow = row - projects.Count();
                    int endRow = row - 1;
                    sheet.Range[$"A{startRow}:G{endRow}"].Rows.Group();
                }
            }

            // vytvoření tabulky (pro filtry + vzhled)
            var tableRange = sheet.Range[$"A1:G{row - 1}"];
            var table = sheet.ListObjects.AddEx(
                XlListObjectSourceType.xlSrcRange,
                tableRange,
                XlYesNoGuess.xlYes
            );
            table.Name = "SouhrnUzivatel";
            table.ShowTotals = false;

            sheet.Columns.AutoFit();
        }

        private async void ButtonSaveAs_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "Export.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                await ExportToExcel(sfd.FileName);
            }
        }

        private void ComboBoxMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var firstDayOfYear = new DateTime(dateTimePicker1.Value.Year, 1, 1);
            dateTimePicker1.Value = firstDayOfYear.AddMonths(comboBoxMonth.SelectedIndex);
            dateTimePicker2.Value = new DateTime(firstDayOfYear.Year, dateTimePicker1.Value.Month, DateTime.DaysInMonth(firstDayOfYear.Year, dateTimePicker1.Value.Month));
        }

        private async void buttonLockEntries_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show($"Zamknout záznamy za měsíc {comboBoxMonth.Text}?", "Zamknout data?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
            if (result == DialogResult.Yes)
            {
                await _timeEntryRepo.LockAllEntriesInMonth(comboBoxMonth.Text);
                await _specialDayRepo.LockEntireMonthAsync(FormatHelper.GetMonthNumberFromString(comboBoxMonth.Text), dateTimePicker1.Value.Year);
            }
        }
    }
}

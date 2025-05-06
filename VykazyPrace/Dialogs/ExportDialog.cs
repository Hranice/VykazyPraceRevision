using System.Data;
using System.Runtime.InteropServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;
using Microsoft.Office.Interop.Excel;
using Action = System.Action;
using Range = Microsoft.Office.Interop.Excel.Range;
using Application = Microsoft.Office.Interop.Excel.Application;
using System.Diagnostics;
using VykazyPrace.Core.PowerKey;

namespace VykazyPrace.Dialogs
{
    public partial class ExportDialog : Form
    {
        private readonly User _selectedUser;
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private readonly UserRepository _userRepo = new();
        private readonly UserGroupRepository _userGroupRepository = new();
        private readonly ProjectRepository _projectRepo = new();
        private readonly SpecialDayRepository _specialDayRepo = new();
        private readonly LoadingUC _loadingUC = new();

        public ExportDialog(User selectedUser)
        {
            InitializeComponent();
            _selectedUser = selectedUser;
        }

        private async Task LoadTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            //Invoke(() => _loadingUC.BringToFront());
            //var summaryList = await _timeEntryRepo.GetTimeEntriesSummaryAsync(fromDate, toDate);

            //var data = await Task.WhenAll(summaryList.Select(async summary =>
            //{
            //    var user = await _userRepo.GetUserByIdAsync(summary.UserId ?? 0) ?? new User { FirstName = "Neznámý", Surname = "uživatel" };
            //    var project = await _projectRepo.GetProjectByIdAsync(summary.ProjectId ?? 0) ?? new Project { ProjectTitle = "Neznámý projekt" };

            //    return new
            //    {
            //        UserName = $"{user.FirstName} {user.Surname}",
            //        ProjectName = project.ProjectTitle,
            //        TotalHours = $"{summary.TotalHours:F1} h"
            //    };
            //}));

            //UpdateDataGrid(data);

            Invoke(() => _loadingUC.Visible = false);
        }

        private void UpdateDataGrid(object data)
        {
            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => dataGridView1.DataSource = data));
            }
            else
            {
                dataGridView1.DataSource = data;
            }
            Invoke(() => _loadingUC.Visible = false);
        }

        private void ExportDialog_Load(object sender, EventArgs e)
        {
            InitializeLoadingControl();
            InitializeDatePickers();
            InitializeUsers();
            this.Shown += async (s, ev) => await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
        }

        private async Task InitializeUsers()
        {
            var users = await _userRepo.GetAllUsersAsync();
            UpdateCheckedListBox(users);
        }

        private void UpdateCheckedListBox(IEnumerable<User> users)
        {
            var userNames = users.Select(FormatHelper.FormatUserToString).ToArray();
            if (checkedListBoxUsers.InvokeRequired)
            {
                checkedListBoxUsers.Invoke(new Action(() => checkedListBoxUsers.Items.AddRange(userNames)));
            }
            else
            {
                checkedListBoxUsers.Items.AddRange(userNames);
            }
        }

        private void InitializeLoadingControl()
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);
        }

        private void InitializeDatePickers()
        {
            var firstDayOfMonth = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePicker1.Value = firstDayOfMonth.AddMonths(-1);
            dateTimePicker2.Value = new DateTime(firstDayOfMonth.Year, dateTimePicker1.Value.Month, DateTime.DaysInMonth(firstDayOfMonth.Year, dateTimePicker1.Value.Month));
            comboBoxMonth.SelectedIndex = firstDayOfMonth.Month - 2;
        }

        private async void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
        }

        private async Task ExportToExcelWithRate(string filePath)
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
                var worksheet = (Worksheet)workbook.Sheets[1];
                worksheet.Name = "Časové záznamy";

                // Přidání hlaviček
                string[] headers = { "Uživatel", "Skupina", "Projekt", "Typ záznamu", "Časový záznam", "Popis", "Doba v hodinách", "Sazba (Kč/h)", "Celkové náklady" };
                for (int i = 0; i < headers.Length; i++)
                {
                    worksheet.Cells[1, i + 1] = headers[i];
                }

                var timeEntries = await _timeEntryRepo.GetAllTimeEntriesAsync();

                for (int i = 0; i < timeEntries.Count; i++)
                {
                    var entry = timeEntries[i];
                    if (entry.ProjectId == 132 && entry.EntryTypeId == 24) continue;
                    string userDisplay = $"{entry.User?.PersonalNumber ?? 0} - {entry.User?.FirstName ?? "N/A"} {entry.User?.Surname ?? "N/A"}";
                    worksheet.Cells[i + 2, 1] = userDisplay;
                    worksheet.Cells[i + 2, 2] = entry.User?.UserGroup?.Title?.Trim() ?? "CHYBÍ DATA";
                    worksheet.Cells[i + 2, 3] = entry.Project?.ProjectTitle ?? "N/A";
                    worksheet.Cells[i + 2, 4] = entry.EntryType?.Title ?? "Neznámý typ";
                    worksheet.Cells[i + 2, 5] = entry.Timestamp?.ToString("yyyy-MM-dd") ?? "N/A";
                    worksheet.Cells[i + 2, 6] = entry.Description ?? "N/A";
                    worksheet.Cells[i + 2, 7] = entry.EntryMinutes / 60.0;

                    // Výpočet sazby podle skupiny + celkových nákladů
                    ((Range)worksheet.Cells[i + 2, 8]).Formula = $"=IFERROR(VLOOKUP(TRIM(B{i + 2}), NákladyTabulka, 2, FALSE), 0)";
                    ((Range)worksheet.Cells[i + 2, 9]).Formula = $"=G{i + 2}*H{i + 2}";
                }

                worksheet.Columns.AutoFit();

                // List "Sazby"
                Worksheet costSheet = (Worksheet)workbook.Sheets.Add();
                costSheet.Name = "Sazby";
                costSheet.Cells[1, 1] = "Skupina";
                costSheet.Cells[1, 2] = "Sazba (Kč/h)";

                var userGroups = await _userGroupRepository.GetAllUserGroupsAsync();
                int row = 2;
                foreach (var group in userGroups)
                {
                    costSheet.Cells[row, 1] = group.Title.Trim();
                    costSheet.Cells[row, 2] = 500;
                    row++;
                }
                costSheet.Columns.AutoFit();

                Range namedRange = costSheet.Range[$"A1:B{userGroups.Count + 1}"];
                workbook.Names.Add("NákladyTabulka", namedRange);

                // KONTINGENČNÍ TABULKY
                // Kontingenční tabulka "Souhrn podle projektu"
                Worksheet summarySheet = (Worksheet)workbook.Sheets.Add();
                summarySheet.Name = "Souhrn podle projektu";

                PivotTable pivotTable1 = (PivotTable)summarySheet.PivotTableWizard(
                    XlPivotTableSourceType.xlDatabase,
                    worksheet.Range["A1:I" + (timeEntries.Count + 1)],
                    summarySheet.Range["A3"],
                    "TimeSummaryPivot"
                );

                ((PivotField)pivotTable1.PivotFields("Projekt")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable1.PivotFields("Skupina")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable1.PivotFields("Typ záznamu")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable1.PivotFields("Uživatel")).Orientation = XlPivotFieldOrientation.xlRowField;

                PivotField hoursField1 = (PivotField)pivotTable1.PivotFields("Doba v hodinách");
                hoursField1.Orientation = XlPivotFieldOrientation.xlDataField;
                hoursField1.Function = XlConsolidationFunction.xlSum;

                PivotField costField1 = (PivotField)pivotTable1.PivotFields("Celkové náklady");
                costField1.Orientation = XlPivotFieldOrientation.xlDataField;
                costField1.Function = XlConsolidationFunction.xlSum;

                pivotTable1.RefreshTable();
                summarySheet.Columns.AutoFit();

                // Kontingenční tabulka "Souhrn podle oddělení"
                Worksheet costSummarySheet = (Worksheet)workbook.Sheets.Add();
                costSummarySheet.Name = "Souhrn podle oddělení";

                PivotTable pivotTable2 = (PivotTable)costSummarySheet.PivotTableWizard(
                    XlPivotTableSourceType.xlDatabase,
                    worksheet.Range["A1:I" + (timeEntries.Count + 1)],
                    costSummarySheet.Range["A3"],
                    "CostSummaryPivot"
                );

                ((PivotField)pivotTable2.PivotFields("Skupina")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable2.PivotFields("Projekt")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable2.PivotFields("Typ záznamu")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable2.PivotFields("Uživatel")).Orientation = XlPivotFieldOrientation.xlRowField;

                PivotField hoursField2 = (PivotField)pivotTable2.PivotFields("Doba v hodinách");
                hoursField2.Orientation = XlPivotFieldOrientation.xlDataField;
                hoursField2.Function = XlConsolidationFunction.xlSum;

                PivotField costField2 = (PivotField)pivotTable2.PivotFields("Celkové náklady");
                costField2.Orientation = XlPivotFieldOrientation.xlDataField;
                costField2.Function = XlConsolidationFunction.xlSum;

                pivotTable2.RefreshTable();
                costSummarySheet.Columns.AutoFit();

                // Kontingenční tabulka "Souhrn podle uživatele"
                Worksheet userSummarySheet = (Worksheet)workbook.Sheets.Add();
                userSummarySheet.Name = "Souhrn podle uživatele";

                PivotTable pivotTable3 = (PivotTable)userSummarySheet.PivotTableWizard(
                    XlPivotTableSourceType.xlDatabase,
                    worksheet.Range["A1:I" + (timeEntries.Count + 1)],
                    userSummarySheet.Range["A3"],
                    "UserSummaryPivot"
                );

                ((PivotField)pivotTable3.PivotFields("Uživatel")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable3.PivotFields("Projekt")).Orientation = XlPivotFieldOrientation.xlRowField;
                ((PivotField)pivotTable3.PivotFields("Typ záznamu")).Orientation = XlPivotFieldOrientation.xlRowField;

                PivotField hoursField3 = (PivotField)pivotTable3.PivotFields("Doba v hodinách");
                hoursField3.Orientation = XlPivotFieldOrientation.xlDataField;
                hoursField3.Function = XlConsolidationFunction.xlSum;

                PivotField costField3 = (PivotField)pivotTable3.PivotFields("Celkové náklady");
                costField3.Orientation = XlPivotFieldOrientation.xlDataField;
                costField3.Function = XlConsolidationFunction.xlSum;

                pivotTable3.RefreshTable();
                userSummarySheet.Columns.AutoFit();

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
                Marshal.ReleaseComObject(excelApp);
                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
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
                var timeEntries = (await _timeEntryRepo.GetAllTimeEntriesBetweenDatesAsync(dateTimePicker1.Value, dateTimePicker2.Value))
                    .Where(e => !(e.ProjectId == 132 && e.EntryTypeId == 24))
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
        "Součet hodin", "Suma", "Docházka (hod)"
    };

            for (int i = 0; i < headers.Length; i++)
                sheet.Cells[1, i + 1] = headers[i];

            // group by uživatel
            var grouped = timeEntries
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

        private async void ComboBoxMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var firstDayOfYear = new DateTime(dateTimePicker1.Value.Year, 1, 1);
            dateTimePicker1.Value = firstDayOfYear.AddMonths(comboBoxMonth.SelectedIndex);
            dateTimePicker2.Value = new DateTime(firstDayOfYear.Year, dateTimePicker1.Value.Month, DateTime.DaysInMonth(firstDayOfYear.Year, dateTimePicker1.Value.Month));

            await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
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

        private void checkedListBoxUsers_SelectedValueChanged(object sender, EventArgs e)
        {

        }
    }
}

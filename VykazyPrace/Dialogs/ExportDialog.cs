using System.Data;
using System.Runtime.InteropServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class ExportDialog : Form
    {
        private readonly User _selectedUser;
        private readonly TimeEntryRepository _timeEntryRepo = new();
        private readonly UserRepository _userRepo = new();
        private readonly ProjectRepository _projectRepo = new();
        private readonly LoadingUC _loadingUC = new();

        public ExportDialog(User selectedUser)
        {
            InitializeComponent();
            _selectedUser = selectedUser;
        }

        private async Task LoadTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            Invoke(() => _loadingUC.BringToFront());
            var summaryList = await _timeEntryRepo.GetTimeEntriesSummaryAsync(fromDate, toDate);

            var data = await Task.WhenAll(summaryList.Select(async summary =>
            {
                var user = await _userRepo.GetUserByIdAsync(summary.UserId ?? 0) ?? new User { FirstName = "Neznámý", Surname = "uživatel" };
                var project = await _projectRepo.GetProjectByIdAsync(summary.ProjectId ?? 0) ?? new Project { ProjectTitle = "Neznámý projekt" };

                return new { UserName = $"{user.FirstName} {user.Surname}", ProjectName = project.ProjectTitle, TotalHours = $"{summary.TotalHours:F1} h" };
            }));

            UpdateDataGrid(data);
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
            this.Shown += async (s, ev) => await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
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
        }

        private async void DateTimePicker_ValueChanged(object sender, EventArgs e)
        {
            await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
        }

        private void ExportToExcel(string filePath)
        {
            var excelApp = new Microsoft.Office.Interop.Excel.Application();
            if (excelApp == null)
            {
                AppLogger.Error("Excel nebyl nalezen na tomto počítači.");
                return;
            }

            try
            {
                var workbook = excelApp.Workbooks.Add();
                var worksheet = (Microsoft.Office.Interop.Excel.Worksheet)workbook.Sheets[1];

                FillWorksheetWithData(worksheet);

                workbook.SaveAs(filePath);
                workbook.Close();
                excelApp.Quit();
                AppLogger.Information("Export do Excelu dokončen.", true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při exportu do Excelu.", ex);
            }
            finally
            {
                ReleaseExcelObjects(excelApp);
            }
        }

        private void FillWorksheetWithData(Microsoft.Office.Interop.Excel.Worksheet worksheet)
        {
            for (int i = 0; i < dataGridView1.Columns.Count; i++)
            {
                worksheet.Cells[1, i + 1] = dataGridView1.Columns[i].HeaderText;
            }

            for (int i = 0; i < dataGridView1.Rows.Count; i++)
            {
                for (int j = 0; j < dataGridView1.Columns.Count; j++)
                {
                    worksheet.Cells[i + 2, j + 1] = dataGridView1.Rows[i].Cells[j].Value?.ToString() ?? "";
                }
            }
        }

        private void ReleaseExcelObjects(Microsoft.Office.Interop.Excel.Application excelApp)
        {
            Marshal.ReleaseComObject(excelApp);
        }

        private void ButtonSaveAs_Click(object sender, EventArgs e)
        {
            using SaveFileDialog sfd = new SaveFileDialog { Filter = "Excel Files|*.xlsx", FileName = "Export.xlsx" };
            if (sfd.ShowDialog() == DialogResult.OK)
            {
                ExportToExcel(sfd.FileName);
            }
        }

        private async void ComboBoxMonth_SelectionChangeCommitted(object sender, EventArgs e)
        {
            var firstDayOfYear = new DateTime(dateTimePicker1.Value.Year, 1, 1);
            dateTimePicker1.Value = firstDayOfYear.AddMonths(comboBoxMonth.SelectedIndex);
            dateTimePicker2.Value = new DateTime(firstDayOfYear.Year, dateTimePicker1.Value.Month, DateTime.DaysInMonth(firstDayOfYear.Year, dateTimePicker1.Value.Month));

            await LoadTimeEntriesSummaryAsync(dateTimePicker1.Value, dateTimePicker2.Value);
        }
    }
}

using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class ExportDialog : Form
    {
        private readonly User _selectedUser;
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();

        public ExportDialog(User selectedUser)
        {
            InitializeComponent();

            _selectedUser = selectedUser;
        }

        private async Task LoadTimeEntriesSummary(DateTime fromDate, DateTime toDate)
        {
            Invoke(() => _loadingUC.BringToFront());
            var summaryList = await _timeEntryRepo.GetTimeEntriesSummaryAsync(fromDate, toDate);

            var tasks = summaryList.Select(async summary =>
            {
                var userTask = _userRepo.GetUserByIdAsync(summary.UserId ?? 0);
                var projectTask = _projectRepo.GetProjectByIdAsync(summary.ProjectId ?? 0);

                await Task.WhenAll(userTask, projectTask);

                var user = await userTask;
                var project = await projectTask;

                return new
                {
                    UserName = user != null ? $"{user.FirstName} {user.Surname}" : "Neznámý uživatel",
                    ProjectName = project != null ? project.ProjectTitle : "Neznámý projekt",
                    TotalHours = $"{summary.TotalHours:F1} h"
                };
            });

            var data = await Task.WhenAll(tasks);

            if (dataGridView1.InvokeRequired)
            {
                dataGridView1.Invoke(new Action(() => dataGridView1.DataSource = data.ToList()));
            }
            else
            {
                dataGridView1.DataSource = data.ToList();
            }

            Invoke(() => _loadingUC.Visible = false);
        }

        private void ExportDialog_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            var dateFirstDay = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dateTimePicker1.Value = dateFirstDay.AddMonths(-1);

            int dateLastDay = DateTime.DaysInMonth(dateFirstDay.Year, dateTimePicker1.Value.Month);
            dateTimePicker2.Value = new DateTime(dateFirstDay.Year, dateTimePicker1.Value.Month, dateLastDay);


            Task.Run(() => LoadTimeEntriesSummary(dateTimePicker1.Value, dateTimePicker2.Value));
        }

        private void dateTimePicker2_ValueChanged(object sender, EventArgs e)
        {
            Task.Run(() => LoadTimeEntriesSummary(dateTimePicker1.Value, dateTimePicker2.Value));
        }

        private void dateTimePicker1_ValueChanged(object sender, EventArgs e)
        {
            Task.Run(() => LoadTimeEntriesSummary(dateTimePicker1.Value, dateTimePicker2.Value));
        }
    }
}

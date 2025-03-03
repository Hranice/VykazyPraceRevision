using System.Runtime.CompilerServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;
using VykazyPrace.UserControls.Calendar;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private User _selectedUser = new User();

        public MainForm()
        {
            InitializeComponent();
        }

        private void správaUživatelùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.UserManagementDialog().ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            Task.Run(LoadData);
        }

        private async Task LoadData()
        {
            Invoke(() => _loadingUC.BringToFront());

            _selectedUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();

            if (_selectedUser == null)
            {
                AppLogger.Error("Nepodaøilo se naèíst aktuálního uživatele, pøístup bude omezen.");
                return;
            }

            Invoke(() => panelCalendarContainer.Controls.Add(new CalendarUC(_selectedUser)
            {
                Dock = DockStyle.Fill
            }));

            Invoke(() => _loadingUC.Visible = false);
        }

        private void správaProjektùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ProjectManagementDialog(_selectedUser).ShowDialog();
        }

        private void exportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ExportDialog(_selectedUser).ShowDialog();
        }
    }
}

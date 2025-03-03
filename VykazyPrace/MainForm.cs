using System.Runtime.CompilerServices;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.UserControls.Calendar;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private User _selectedUser = new User();

        public MainForm()
        {
            InitializeComponent();
        }

        private void správaUživatelùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.UserManagementDialog().ShowDialog();
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            _selectedUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName);

            panelCalendarContainer.Controls.Add(new CalendarUC(_selectedUser)
            {
                Dock = DockStyle.Fill
            });

        }

        private void správaProjektùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ProjectManagementDialog().ShowDialog();
        }
    }
}

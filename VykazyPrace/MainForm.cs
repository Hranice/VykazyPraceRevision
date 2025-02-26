using VykazyPrace.UserControls.Calendar;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void spr�vaU�ivatel�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.UserManagementDialog().ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelCalendarContainer.Controls.Add(new CalendarUC(new Dictionary<int, int> { { 5, 360 }, { 24, 30 }, { 17, 90 }, { 8, 900 } })
            {
                Dock = DockStyle.Fill
            });

        }

        private void spr�vaProjekt�ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            new Dialogs.ProjectManagementDialog().ShowDialog();
        }
    }
}

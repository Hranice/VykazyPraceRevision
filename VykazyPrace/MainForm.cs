using VykazyPrace.UserControls.Calendar;

namespace VykazyPrace
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void správaUživatelùToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var userMgmt = new Dialogs.UserManagementDialog();
            userMgmt.ShowDialog();
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            panelCalendarContainer.Controls.Add(new CalendarUC(new Dictionary<int, int> { { 5, 360 }, { 24, 30 }, { 17, 90 }, { 8, 900 } })
            {
                Dock = DockStyle.Fill
            });

        }
    }
}

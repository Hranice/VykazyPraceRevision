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
            var userMgmt = new Dialogs.UserManagementDialog();
            userMgmt.ShowDialog();
        }
    }
}

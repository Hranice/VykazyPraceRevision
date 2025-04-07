using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class SettingsDialog : Form
    {
        private AppConfig _config;
        private readonly User _selectedUser;

        public SettingsDialog(User selectedUser)
        {
            InitializeComponent();

            _config = ConfigService.Load();
            labelDatabaseFilePath.Text = _config.DatabasePath;

            _selectedUser = selectedUser;
        }

        private void buttonPathToDatabase_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog
            {
                Title = "Vyberte databázový soubor",
                Filter = "SQLite databáze (*.db)|*.db|Všechny soubory (*.*)|*.*"
            };

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                labelDatabaseFilePath.Text = ofd.FileName;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _config.DatabasePath = labelDatabaseFilePath.Text;
            ConfigService.Save(_config);
            MessageBox.Show("Nové nastavení bylo uloženo. Restartujte aplikaci.", "Uloženo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            Close();
        }
    }
}

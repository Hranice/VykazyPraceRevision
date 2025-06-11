using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class SettingsDialog : Form
    {
        private AppConfig _config;
        private bool databaseChanged = false;
        private readonly User _selectedUser;

        public SettingsDialog(User selectedUser)
        {
            InitializeComponent();

            _config = ConfigService.Load();
            labelDatabaseFilePath.Text = _config.DatabasePath;
            dateTimePicker1.Value = _config.NotificationTime;
            checkBoxEnableNotification.Checked = _config.NotificationOn;
            checkBoxMinimizeToTray.Checked = _config.MinimizeToTray;
            textBoxNotificationTitle.Text = _config.NotificationTitle;
            textBoxNotificationText.Text = _config.NotificationText;

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
                if (_config.DatabasePath != ofd.FileName)
                {
                    databaseChanged = true;
                }
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            _config.DatabasePath = labelDatabaseFilePath.Text;
            _config.NotificationTime = dateTimePicker1.Value;
            _config.NotificationOn = checkBoxEnableNotification.Checked;
            _config.MinimizeToTray = checkBoxMinimizeToTray.Checked;
            _config.NotificationTitle = textBoxNotificationTitle.Text;
            _config.NotificationText = textBoxNotificationText.Text;
            ConfigService.Save(_config);

            if (databaseChanged)
            {
                MessageBox.Show("Nové nastavení bylo uloženo.\n\nZměna databáze se projeví až po restartu aplikace.", "Uloženo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }

            else
            {
                MessageBox.Show("Nové nastavení bylo uloženo.", "Uloženo", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            Close();
        }
    }
}

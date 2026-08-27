using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class SettingsDialog : Form
    {
        private readonly IConfigService _configService;

        private bool _databaseChanged;

        public SettingsDialog(
            IConfigService configService)
        {
            _configService = configService;

            InitializeComponent();

            LoadConfigToUi();
        }

        private void LoadConfigToUi()
        {
            var config = _configService.Current;

            labelDatabaseFilePath.Text = config.DatabasePath;
            dateTimePicker1.Value = config.NotificationTime;
            checkBoxEnableNotification.Checked = config.NotificationOn;
            checkBoxMinimizeToTray.Checked = config.MinimizeToTray;
            checkBoxRememberLastResolutionPosition.Checked = config.RememberLastResolutionPosition;
            textBoxNotificationTitle.Text = config.NotificationTitle;
            textBoxNotificationText.Text = config.NotificationText;
        }

        private void buttonPathToDatabase_Click(object sender, EventArgs e)
        {
            var config = _configService.Current;

            using var ofd = new OpenFileDialog
            {
                Title = "Vyberte databázový soubor",
                Filter = "SQLite databáze (*.db)|*.db|Všechny soubory (*.*)|*.*"
            };

            if (ofd.ShowDialog(this) != DialogResult.OK)
                return;

            labelDatabaseFilePath.Text = ofd.FileName;

            if (config.DatabasePath != ofd.FileName)
            {
                _databaseChanged = true;
            }
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            var config = _configService.Current;

            config.DatabasePath = labelDatabaseFilePath.Text;
            config.NotificationTime = dateTimePicker1.Value;
            config.NotificationOn = checkBoxEnableNotification.Checked;
            config.MinimizeToTray = checkBoxMinimizeToTray.Checked;
            config.RememberLastResolutionPosition = checkBoxRememberLastResolutionPosition.Checked;
            config.NotificationTitle = textBoxNotificationTitle.Text;
            config.NotificationText = textBoxNotificationText.Text;

            _configService.Save();

            if (_databaseChanged)
            {
                MessageBox.Show(
                    "Nové nastavení bylo uloženo.\n\nZměna databáze se projeví až po restartu aplikace.",
                    "Uloženo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(
                    "Nové nastavení bylo uloženo.",
                    "Uloženo",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }

            Close();
        }
    }
}

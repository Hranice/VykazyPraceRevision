using VykazyPrace.Core.Logging;
using VykazyPrace.Updater;

namespace VykazyPrace.Dialogs
{
    public partial class UpdateDialog : Form
    {
        private UpdateInfo? _updateInfo;

        public UpdateDialog()
        {
            InitializeComponent();
        }

        private async void UpdateDialog_Load(object sender, EventArgs e)
        {
            labelCurrentVersion.Text = "Načítám...";
            labelLatestVersion.Text = "Načítám...";
            buttonUpdate.Enabled = false;
            buttonShowChangelog.Enabled = false;

            _updateInfo = await UpdateService.GetUpdateInfoAsync();

            labelCurrentVersion.Text = _updateInfo.CurrentVersion.ToString();
            labelLatestVersion.Text = _updateInfo.LatestVersion?.ToString() ?? "Neznámá";

            buttonShowChangelog.Enabled = UpdateService.CanShowChangelog();
            buttonUpdate.Enabled = _updateInfo.UpdateAvailable && _updateInfo.UpdateFilesAvailable;

            if (!string.IsNullOrWhiteSpace(_updateInfo.ErrorMessage))
            {
                labelLatestVersion.Text = $"Chyba: {_updateInfo.ErrorMessage}";
            }
        }

        private void buttonShowChangelog_Click(object sender, EventArgs e)
        {
            try
            {
                UpdateService.OpenChangelog();
            }
            catch (Exception ex)
            {
                AppLogger.Error("Nepodařilo se otevřít changelog.", ex);
            }
        }

        private void buttonUpdate_Click(object sender, EventArgs e)
        {
            if (_updateInfo == null || !_updateInfo.UpdateAvailable)
            {
                AppLogger.Error("Aktualizace není k dispozici.");
                return;
            }

            var result = MessageBox.Show(
                $"Bude spuštěn instalátor nové verze ({_updateInfo.CurrentVersion} → {_updateInfo.LatestVersion}).\n\n" +
                "Pro dokončení aktualizace bude potřeba zavřít aplikaci.\n\n" +
                "Chcete pokračovat?",
                "Potvrzení aktualizace",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (result != DialogResult.Yes)
                return;

            if (UpdateService.TryStartUpdate(out string? errorMessage))
            {
                AppLogger.Information("Instalátor byl spuštěn.\n\nPo dokončení aktualizace aplikaci zavřete a spusťte znovu.");
            }
            else
            {
                AppLogger.Error("Aktualizaci se nepodařilo spustit: " + errorMessage);
            }
        }
    }
}
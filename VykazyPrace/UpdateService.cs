using System.Diagnostics;
using System.Reflection;
using VykazyPrace.Core.Logging;
using VykazyPrace.Logging;

namespace VykazyPrace
{
    public static class UpdateService
    {
        private const string UpdateFolderPath = @"\\cze-sfs01\data\TS\jprochazka-sw\WorkLog\Updates";
        private const string VersionFile = "latest.txt";
        private const string InstallerFile = "WorkLog_Installer.exe";

        public static async Task CheckForUpdateAsync()
        {
            try
            {
                string versionPath = Path.Combine(UpdateFolderPath, VersionFile);
                string installerPath = Path.Combine(UpdateFolderPath, InstallerFile);

                if (!File.Exists(versionPath) || !File.Exists(installerPath))
                    return;

                string latest = await File.ReadAllTextAsync(versionPath);
                Version latestVersion = new Version(latest.Trim());
                Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;


                if (latestVersion > currentVersion)
                {
                    AppLogger.Information($"Byla zjištěna nová verze ({currentVersion} -> {latestVersion}), aplikace se nyní aktualizuje.", true);

                    string tempInstaller = Path.Combine(Path.GetTempPath(), InstallerFile);
                    File.Copy(installerPath, tempInstaller, true);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempInstaller,
                        Arguments = "/silent /promptrestart /closeapplications /restartapplications",
                        UseShellExecute = true
                    });

                    Application.Exit();
                }
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při kontrole aktualizace.", ex);
            }
        }


        public static void CheckForUpdateMessage()
        {
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "WorkLog");
            string versionFile = Path.Combine(appDataPath, "version.txt");
            string currentVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "0.0.0";

            if (!Directory.Exists(appDataPath))
                Directory.CreateDirectory(appDataPath);

            string lastVersion = File.Exists(versionFile) ? File.ReadAllText(versionFile).Trim() : "";

            if (lastVersion != currentVersion)
            {
                File.WriteAllText(versionFile, currentVersion);

                string changelogPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changelog.docx");
                string message = $"Aplikace byla aktualizována na verzi {currentVersion}.";

                if (File.Exists(changelogPath))
                {
                    DialogResult result = MessageBox.Show(
                        message + "\n\nChcete si zobrazit seznam změn (changelog)?",
                        "Aktualizace dokončena",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Information
                    );

                    if (result == DialogResult.Yes)
                    {
                        try
                        {
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = changelogPath,
                                UseShellExecute = true
                            });
                        }
                        catch (Exception ex)
                        {
                            AppLogger.Error("Nepodařilo se otevřít Changelog.docx", ex);
                        }
                    }
                }
                else
                {
                    MessageBox.Show(message, "Aktualizace dokončena", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }


    }
}

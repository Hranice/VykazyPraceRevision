using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
                    string tempInstaller = Path.Combine(Path.GetTempPath(), InstallerFile);
                    File.Copy(installerPath, tempInstaller, true);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = tempInstaller,
                        Arguments = "/verysilent",
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
    }
}

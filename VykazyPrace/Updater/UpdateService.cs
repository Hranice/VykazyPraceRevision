using System.Diagnostics;
using System.Reflection;
using VykazyPrace.Core.Logging;
using VykazyPrace.Updater;

namespace VykazyPrace
{
    public static class UpdateService
    {
        private const string UpdateFolderPath = @"\\cze-sfs01\data\TS\jprochazka-sw\WorkLog\Updates";
        private const string LatestVersionFileName = "latest.txt";
        private const string InstallerFileName = "WorkLog_Installer.msi";
        private const string ChangelogFileName = "Changelog.docx";

        public static Version GetCurrentVersion()
        {
            return Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
        }

        public static string GetInstallerPath()
        {
            return Path.Combine(UpdateFolderPath, InstallerFileName);
        }

        public static string GetLatestVersionFilePath()
        {
            return Path.Combine(UpdateFolderPath, LatestVersionFileName);
        }

        public static string GetNetworkChangelogPath()
        {
            return Path.Combine(UpdateFolderPath, ChangelogFileName);
        }

        public static string GetLocalChangelogPath()
        {
            return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ChangelogFileName);
        }

        public static bool CanShowChangelog()
        {
            return File.Exists(GetNetworkChangelogPath()) || File.Exists(GetLocalChangelogPath());
        }

        public static void OpenChangelog()
        {
            string networkChangelogPath = GetNetworkChangelogPath();
            string localChangelogPath = GetLocalChangelogPath();

            if (File.Exists(networkChangelogPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = networkChangelogPath,
                    UseShellExecute = true
                });
                return;
            }

            if (File.Exists(localChangelogPath))
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = localChangelogPath,
                    UseShellExecute = true
                });
                return;
            }

            throw new FileNotFoundException("Changelog nebyl nalezen ani na síti, ani lokálně.");
        }

        public static async Task<UpdateInfo> GetUpdateInfoAsync()
        {
            var info = new UpdateInfo
            {
                CurrentVersion = GetCurrentVersion()
            };

            try
            {
                string versionPath = GetLatestVersionFilePath();
                string installerPath = GetInstallerPath();

                bool versionExists = File.Exists(versionPath);
                bool installerExists = File.Exists(installerPath);

                info.UpdateFilesAvailable = versionExists && installerExists;

                if (!versionExists)
                {
                    info.ErrorMessage = "Soubor latest.txt nebyl na serveru nalezen.";
                    return info;
                }

                string latest = await File.ReadAllTextAsync(versionPath);
                info.LatestVersion = new Version(latest.Trim());

                if (!installerExists)
                {
                    info.ErrorMessage = "Instalátor aktualizace nebyl na serveru nalezen.";
                }

                return info;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při načítání informací o aktualizaci.", ex);
                info.ErrorMessage = "Nepodařilo se načíst informace o aktualizaci.";
                return info;
            }
        }

        public static bool TryStartUpdate(out string? errorMessage)
        {
            errorMessage = null;

            try
            {
                string installerPath = GetInstallerPath();

                if (!File.Exists(installerPath))
                {
                    errorMessage = "Instalátor aktualizace nebyl nalezen.";
                    return false;
                }

                var process = Process.Start(new ProcessStartInfo
                {
                    FileName = "msiexec.exe",
                    Arguments = $"/i \"{installerPath}\" /promptrestart",
                    UseShellExecute = true,
                    WorkingDirectory = Path.GetDirectoryName(installerPath)
                });

                if (process == null)
                {
                    errorMessage = "Instalátor se nepodařilo spustit.";
                    return false;
                }

                AppLogger.Information($"Byl spuštěn instalátor aktualizace přímo z umístění: {installerPath}");
                return true;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při spuštění aktualizace.", ex);
                errorMessage = "Při spuštění aktualizace došlo k chybě.";
                return false;
            }
        }
    }
}

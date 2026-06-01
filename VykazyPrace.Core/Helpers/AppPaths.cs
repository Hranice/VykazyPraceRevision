namespace VykazyPrace.Core.Helpers
{
    public static class AppPaths
    {
        // USER DATA
        public static string RoamingDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "WorkLog");

        public static string LocalDirectory =>
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "WorkLog");

        public static string ConfigFile =>
            Path.Combine(RoamingDirectory, "config.json");

        public static string LogsDirectory =>
            Path.Combine(LocalDirectory, "logs");

        public static string VersionFile =>
            Path.Combine(LocalDirectory, "version.txt");


        // UPDATE
        public static string UpdateFolder =>
            @"\\cze-sfs01\data\TS\jprochazka-sw\WorkLog\Updates";

        public static string LatestVersionFile =>
            Path.Combine(UpdateFolder, "latest.txt");

        public static string InstallerNetworkPath =>
            Path.Combine(UpdateFolder, "WorkLog_Installer.msi");

        public static string ChangelogNetworkPath =>
            Path.Combine(UpdateFolder, "Changelog.docx");


        // LOCAL TEMP
        public static string InstallerTempPath =>
            Path.Combine(Path.GetTempPath(), "WorkLog_Installer.msi");


        // LOCAL INSTALL
        public static string ChangelogLocalPath =>
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Changelog.docx");
    }
}

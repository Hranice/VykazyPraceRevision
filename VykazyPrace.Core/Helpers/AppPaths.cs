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

    }
}

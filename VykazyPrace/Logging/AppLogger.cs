using Serilog;

namespace VykazyPrace.Logging
{
    public static class AppLogger
    {
        public static ILogger Logger { get; private set; }

        static AppLogger()
        {
            Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        public static void Information(string message, bool showDialog = false)
        {
            Logger.Information(message);
            if (showDialog)
            {
                ShowInformationPopup(message);
            }
        }

        public static void Error(string message, Exception ex)
        {
            Logger.Error(ex, message);
            ShowErrorPopup(message, ex);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
            ShowErrorPopup(message);
        }

        private static void ShowErrorPopup(string message, Exception ex)
        {
            string errorMessage = $"Došlo k chybě:\n{message}\n\n{ex.Message}";
            MessageBox.Show(errorMessage, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ShowErrorPopup(string message)
        {
            string errorMessage = $"Došlo k chybě:\n{message}";
            MessageBox.Show(errorMessage, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private static void ShowInformationPopup(string message)
        {
            MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }

}

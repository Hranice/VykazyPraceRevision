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

        public static void Information(string message)
        {
            Logger.Information(message);
        }

        public static void Error(string message, Exception ex)
        {
            Logger.Error(ex, message);

            // **Zobrazit popup okno při chybě**
            ShowErrorPopup(message, ex);
        }

        private static void ShowErrorPopup(string message, Exception ex)
        {
            string errorMessage = $"Došlo k chybě:\n{message}\n\n{ex.Message}";

            // Pro WinForms:
            MessageBox.Show(errorMessage, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);

            // Pro WPF:
            // System.Windows.MessageBox.Show(errorMessage, "Chyba", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

}

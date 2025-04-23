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

        private static readonly SynchronizationContext? _syncContext = SynchronizationContext.Current;

        private static void ShowErrorPopup(string message, Exception ex)
        {
            string errorMessage = $"Došlo k chybě:\n{message}\n\n{ex.Message}";
            ShowMessage(errorMessage, "Chyba", MessageBoxIcon.Error);
        }

        private static void ShowErrorPopup(string message)
        {
            string errorMessage = $"Došlo k chybě:\n{message}";
            ShowMessage(errorMessage, "Chyba", MessageBoxIcon.Error);
        }

        private static void ShowInformationPopup(string message)
        {
            ShowMessage(message, "Info", MessageBoxIcon.Information);
        }

        private static void ShowMessage(string text, string caption, MessageBoxIcon icon)
        {
            if (_syncContext != null && SynchronizationContext.Current != _syncContext)
            {
                _syncContext.Post(_ => MessageBox.Show(text, caption, MessageBoxButtons.OK, icon), null);
            }
            else
            {
                MessageBox.Show(text, caption, MessageBoxButtons.OK, icon);
            }
        }

    }

}

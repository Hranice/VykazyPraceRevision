using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog;
using Serilog.Events;
using VykazyPrace.Core.Configuration;

namespace VykazyPrace.Core.Logging
{
    public static class AppLogger
    {
        public static ILogger Logger { get; private set; }

        private static ILoggerPopupService? _popupService;

        public static void RegisterPopupService(ILoggerPopupService service)
        {
            _popupService = service;
        }

        static AppLogger()
        {
            var config = ConfigService.Load();

            var level = ParseLogLevel(config.LogLevel);

            Logger = new LoggerConfiguration()
                .MinimumLevel.Is(level)
                .WriteTo.Console()
                .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();
        }

        private static LogEventLevel ParseLogLevel(string level)
        {
            return level?.ToLower() switch
            {
                "debug" => LogEventLevel.Debug,
                "information" => LogEventLevel.Information,
                "warning" => LogEventLevel.Warning,
                "error" => LogEventLevel.Error,
                "fatal" => LogEventLevel.Fatal,
                "verbose" => LogEventLevel.Verbose,
                _ => LogEventLevel.Information
            };
        }

        public static void Debug(string message)
        {
            Logger.Debug(message);
        }

        public static void Information(string message, bool showDialog = false)
        {
            Logger.Information(message);
            if (showDialog && _popupService != null)
            {
                _popupService.ShowInformation(message);
            }
        }

        public static void Error(string message, Exception ex)
        {
            Logger.Error(ex, $"{message}\n\nInner exception: {ex.InnerException}");
            _popupService?.ShowError(message, ex);
        }

        public static void Error(string message)
        {
            Logger.Error(message);
            _popupService?.ShowError(message);
        }
    }
}

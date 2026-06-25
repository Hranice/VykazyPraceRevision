using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Pipes;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Import;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Core.Services;
using VykazyPrace.Dialogs;
using VykazyPrace.Logging.VykazyPrace;
using VykazyPrace.UserControls.Calendar;
using VykazyPrace.UserControls.CalendarV2;
using VykazyPrace.UserControls.Outlook;

namespace VykazyPrace
{
    internal static class Program
    {
        public static MainForm? MainFormInstance;
        public static IServiceProvider Services { get; private set; } = null!;

        private const string PipeName = "VykazyPrace_IPC";
        private const string MutexName = "VykazyPrace_Mutex";

        [STAThread]
        static void Main()
        {
            ApplicationConfiguration.Initialize();

            Services = ConfigureServices();

            var configService = Services.GetRequiredService<IConfigService>();

            AppLogger.Initialize(configService);
            AppLogger.RegisterPopupService(new WinFormsLoggerPopupService());

            using Mutex mutex = new(true, MutexName, out bool isFirstInstance);

            if (!isFirstInstance)
            {
                RestoreExistingInstance();
                return;
            }

            AppLogger.Debug("Aplikace spuštìna.");

            WarmupDatabaseAsync().GetAwaiter().GetResult();

            StartPipeServer();

            try
            {
                MainFormInstance = Services.GetRequiredService<MainForm>();
                Application.Run(MainFormInstance);
            }
            finally
            {
                AppLogger.CloseAndFlush();
            }
        }

        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<IConfigService, ConfigService>();

            services.AddDbContextFactory<VykazyPraceContext>((provider, options) =>
            {
                var configService = provider.GetRequiredService<IConfigService>();
                var config = configService.Current;

                options.UseSqlite($"Data Source={config.DatabasePath}");
            });

            // Repositories
            services.AddTransient<UserRepository>();
            services.AddTransient<UserGroupRepository>();
            services.AddTransient<TimeEntryRepository>();
            services.AddTransient<TimeEntryTypeRepository>();
            services.AddTransient<TimeEntrySubTypeRepository>();
            services.AddTransient<ProjectRepository>();
            services.AddTransient<SpecialDayRepository>();
            services.AddTransient<ArrivalDepartureRepository>();
            services.AddTransient<CalendarRepository>();
            services.AddTransient<ReportRepository>();

            // Services / helpers
            services.AddTransient<PowerKeyHelper>();
            services.AddTransient<OutlookMeetingImportService>();
            services.AddTransient<DataTableFactory>();
            services.AddTransient<ExternalTimeEntryImportService>();

            // Forms
            services.AddTransient<MainForm>();
            services.AddTransient<SettingsDialog>();
            services.AddTransient<ExportDialog>();
            services.AddTransient<UserSelectionDialog>();
            services.AddTransient<ManagerDialog>();
            services.AddTransient<OutlookEvents>();
            services.AddTransient<OverviewDialog>();

            // UserControls
            services.AddTransient<CalendarV2>();
            services.AddTransient<CalendarUC>();
            services.AddTransient<OutlookEvent>();

            return services.BuildServiceProvider();
        }

        private static async Task WarmupDatabaseAsync()
        {
            AppLogger.Debug("Zahøívací dotaz na databázi...");

            try
            {
                var userRepository = Services.GetRequiredService<UserRepository>();
                var users = await userRepository.GetAllUsersAsync();

                _ = users.FirstOrDefault();

                AppLogger.Debug("Databáze zahøáta.");
            }
            catch (Exception ex)
            {
                AppLogger.Error("Zahøívací dotaz selhal: " + ex.Message);
            }
        }

        private static void RestoreExistingInstance()
        {
            AppLogger.Debug("Aplikace již bìží, obnovuji pùvodní instanci.");

            try
            {
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);

                client.Connect(200);

                using var writer = new StreamWriter(client);
                writer.WriteLine("show");
                writer.Flush();

                AppLogger.Debug("Pùvodní instance obnovena.");
            }
            catch
            {
                AppLogger.Debug("Pùvodní instance nereaguje.");
            }
        }

        private static void StartPipeServer()
        {
            Thread serverThread = new(() =>
            {
                while (true)
                {
                    try
                    {
                        using var server = new NamedPipeServerStream(PipeName, PipeDirection.In);

                        server.WaitForConnection();

                        using var reader = new StreamReader(server);
                        string? message = reader.ReadLine();

                        if (message == "show" && MainFormInstance != null)
                        {
                            MainFormInstance.BeginInvoke(() =>
                            {
                                MainFormInstance.ShowFromTray();
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        AppLogger.Error("Pipe server selhal: " + ex.Message);
                    }
                }
            });

            serverThread.IsBackground = true;
            serverThread.Start();
        }
    }
}
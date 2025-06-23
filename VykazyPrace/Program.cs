using System.IO.Pipes;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using VykazyPrace.Logging.VykazyPrace;
using VykazyPrace.Core.Logging.VykazyPrace.Logging;

namespace VykazyPrace
{
    internal static class Program
    {
        public static MainForm? MainFormInstance;
        private const string PipeName = "VykazyPrace_IPC";

        [STAThread]
        static void Main()
        {
            bool isFirstInstance;
            using (Mutex mutex = new Mutex(true, "VykazyPrace_Mutex", out isFirstInstance))
            {
                if (isFirstInstance)
                {
                    AppLogger.RegisterPopupService(new WinFormsLoggerPopupService());
                    AppLogger.Debug("Aplikace spu�t�na.");

                    StartPipeServer();

                    ApplicationConfiguration.Initialize();
                    MainFormInstance = new MainForm();
                    Application.Run(MainFormInstance);
                }

                else
                {
                    AppLogger.Debug("Aplikace ji� b��, obnovuji p�vodn� instanci.");
                    // Druh� instance => po�li po�adavek na zobrazen� hlavn�ho okna
                    try
                    {
                        using (NamedPipeClientStream client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out))
                        {
                            client.Connect(200);
                            using (StreamWriter writer = new StreamWriter(client))
                            {
                                writer.WriteLine("show");
                                writer.Flush();
                            }
                        }
                    }
                    catch
                    {
                        AppLogger.Debug("P�vodn� instance nereaguje.");
                    }

                    AppLogger.Debug("P�vodn� instance obnovena.");
                }
            }
        }

        private static void StartPipeServer()
        {
            Thread serverThread = new Thread(() =>
            {
                try
                {
                    while (true)
                    {
                        using (NamedPipeServerStream server = new NamedPipeServerStream(PipeName, PipeDirection.In))
                        {
                            server.WaitForConnection();
                            using (StreamReader reader = new StreamReader(server))
                            {
                                string? message = reader.ReadLine();
                                if (message == "show" && MainFormInstance != null)
                                {
                                    MainFormInstance.BeginInvoke(() =>
                                    {
                                        MainFormInstance.ShowFromTray();
                                    });
                                }
                            }
                        }
                    }
                }
                catch
                {
                    // Server thread selhal, ignorujeme
                }
            });

            serverThread.IsBackground = true;
            serverThread.Start();
        }
    }
}

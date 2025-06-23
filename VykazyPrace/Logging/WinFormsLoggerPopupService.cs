using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.Logging
{
    namespace VykazyPrace
    {
        public class WinFormsLoggerPopupService : ILoggerPopupService
        {
            public void ShowError(string message, Exception? exception = null)
            {
                var fullMessage = exception == null
                    ? $"Došlo k chybě:\n{message}"
                    : $"Došlo k chybě:\n{message}\n\n{exception.Message}";

                MessageBox.Show(fullMessage, "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            public void ShowInformation(string message)
            {
                MessageBox.Show(message, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VykazyPrace.Core.Logging
{
    public interface ILoggerPopupService
    {
        void ShowError(string message, Exception? exception = null);
        void ShowInformation(string message);
    }
}

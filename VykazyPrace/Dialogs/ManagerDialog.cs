using VykazyPrace.Core.Logging.VykazyPrace.Logging;
using VykazyPrace.Core.PowerKey;
using VykazyPrace.Logging;

namespace VykazyPrace.Dialogs
{
    public partial class ManagerDialog : Form
    {
        public ManagerDialog()
        {
            InitializeComponent();
        }

        private async void buttonDownloadArrivalsDepartures_Click(object sender, EventArgs e)
        {
            try
            {
                var powerKeyHelper = new PowerKeyHelper();
                int totalRows = await powerKeyHelper.DownloadArrivalsDeparturesAsync(dateTimePicker1.Value);
                AppLogger.Information($"Staženo {totalRows} záznamů pro měsíc č.{dateTimePicker1.Value.Month}.", true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při stahování příchodů a odchodů.", ex);
            }
        }
    }
}

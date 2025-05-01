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
                int totalRows = await powerKeyHelper.DownloadArrivalsDeparturesAsync();
                AppLogger.Information($"Staženo {totalRows} záznamů.", true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při stahování příchodů a odchodů.", ex);
            }
        }
    }
}

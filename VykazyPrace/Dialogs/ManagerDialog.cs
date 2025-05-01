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
                var month = DateTime.Now.AddMonths(-1);
                int totalRows = await powerKeyHelper.DownloadArrivalsDeparturesAsync(month);
                AppLogger.Information($"Staženo {totalRows} záznamů pro měsíc č.{month.Month}.", true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při stahování příchodů a odchodů.", ex);
            }
        }
    }
}

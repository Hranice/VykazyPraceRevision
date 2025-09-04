using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;
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
                var userRepo = new UserRepository();

                var allUsers = await userRepo.GetAllUsersAsync();

                var targetUsers = checkBox1.Checked
                    ? allUsers
                    : allUsers
                        .Where(u => u.PersonalNumber == (int)numericUpDown1.Value)
                        .ToList();

                if (targetUsers.Count == 0)
                {
                    AppLogger.Error("Nebyl nalezen žádný uživatel pro stažení.");
                    return;
                }

                var totalRows = 0;
                foreach (var user in targetUsers)
                {
                    totalRows += await powerKeyHelper.DownloadForUserAsync(dateTimePicker1.Value, user);
                }

                AppLogger.Information($"Staženo pro {targetUsers.Count} uživatelů, celkem {totalRows} řádků.", true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při stahování příchodů a odchodů.", ex);
            }
        }
    }
}

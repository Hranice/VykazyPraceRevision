using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.PowerKey;

namespace VykazyPrace.Dialogs
{
    public partial class ManagerDialog : Form
    {
        private readonly UserRepository _userRepo;
        private readonly PowerKeyHelper _powerKeyHelper;

        public ManagerDialog(
            UserRepository userRepo,
            PowerKeyHelper powerKeyHelper)
        {
            InitializeComponent();

            _userRepo = userRepo;
            _powerKeyHelper = powerKeyHelper;
        }

        private async void buttonDownloadArrivalsDepartures_Click(object sender, EventArgs e)
        {
            try
            {
                var allUsers = await _userRepo.GetAllUsersAsync();

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
                    totalRows += await _powerKeyHelper.DownloadForUserAsync(
                        dateTimePicker1.Value,
                        user);
                }

                AppLogger.Information(
                    $"Staženo pro {targetUsers.Count} uživatelů, celkem {totalRows} řádků.",
                    true);
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při stahování příchodů a odchodů.", ex);
            }
        }
    }
}
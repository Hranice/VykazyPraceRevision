using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class SettingsDialog : Form
    {
        private readonly User _selectedUser;

        public SettingsDialog(User selectedUser)
        {
            InitializeComponent();

            _selectedUser = selectedUser;
        }
    }
}

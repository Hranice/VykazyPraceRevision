using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class UserManagementDialog : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();

        public UserManagementDialog()
        {
            InitializeComponent();
        }

        private void UserManagementDialog_Load(object sender, EventArgs e)
        {
            listBoxUsers.Items.Clear();
            listBoxUsers.Items.Add("Načítání...");

            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);
            _loadingUC.BringToFront();

            Task.Run(() => LoadUsersAsync());
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                var users = await _userRepo.GetAllUsersAsync();

                Invoke(new Action(() =>
                {
                    listBoxUsers.Items.Clear();
                    foreach (var user in users)
                    {
                        listBoxUsers.Items.Add($"{user.Id}: {user.FirstName} {user.Surname}");
                    }
                    _loadingUC.Visible = false;
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    MessageBox.Show($"Chyba při načítání: {ex.Message}", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    _loadingUC.Visible = false;
                }));
            }
        }


        private async void button1_Click(object sender, EventArgs e)
        {
            var newUser = new User
            {
                FirstName = "Jan",
                Surname = "Novák",
                PersonalNumber = 12345,
                WindowsUsername = "jnovak",
                LevelOfAccess = 1
            };

            await _userRepo.CreateUserAsync(newUser);
            MessageBox.Show("Uživatel přidán!");
        }
    }
}

using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
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
                        listBoxUsers.Items.Add(FormatUserToString(user));
                    }
                    _loadingUC.Visible = false;
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error($"Chyba při načítání uživatelů.", ex);
                    _loadingUC.Visible = false;
                }));
            }
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (buttonAdd.Text == "Konec prohlížení")
            {
                groupBox1.Text = $"Přidání uživatele";
                textBoxFirstName.Enabled = true;
                textBoxSurname.Enabled = true;
                textBoxWindowsUsername.Enabled = true;
                maskedTextBoxPersonalNumber.Enabled = true;
                numericUpDownLevelOfAccess.Enabled = true;
                ClearFields();
                buttonAdd.Text = "Přidat";
            }

            else
            {
                var dataCheck = CheckForEmptyOrIncorrectFields();

                if (dataCheck.Item1)
                {
                    var newUser = new User
                    {
                        FirstName = textBoxFirstName.Text,
                        Surname = textBoxSurname.Text,
                        PersonalNumber = Int32.Parse(maskedTextBoxPersonalNumber.Text),
                        WindowsUsername = textBoxWindowsUsername.Text,
                        LevelOfAccess = (int)numericUpDownLevelOfAccess.Value
                    };

                    var addedUser = await _userRepo.CreateUserAsync(newUser);
                    if (addedUser is not null)
                    {
                        AppLogger.Information($"Uživatel {FormatUserToString(addedUser)} byl přidán do databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Uživatel {FormatUserToString(newUser)} nebyl přidán do databáze.");
                    }

                    await LoadUsersAsync();
                }

                else
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {dataCheck.Item2}");
                }
            }
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var user = await GetUserBySelectedItem();

            if (user != null)
            {
                var dialogResult = MessageBox.Show($"Smazat uživatele {FormatUserToString(user)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _userRepo.DeleteUserAsync(user.Id))
                    {
                        AppLogger.Information($"Uživatel {FormatUserToString(user)} byl smazán z databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat uživatele {FormatUserToString(user)} z databáze.");
                    }

                    await LoadUsersAsync();
                }
            }
        }

        private async Task<User?> GetUserBySelectedItem()
        {
            User? user = new User();

            if (listBoxUsers.SelectedItem != null)
            {
                string? selectedItem = listBoxUsers.SelectedItem?.ToString();
                string? idString = selectedItem?.Split(' ')[0];

                if (int.TryParse(idString, out int userId))
                {
                    user = await _userRepo.GetUserByIdAsync(userId);
                }

                else
                {
                    AppLogger.Error($"Nepodařilo se získat uživatele {selectedItem} z databáze, id '{idString}' je neplatné.");
                }

                if (user == null)
                {
                    AppLogger.Error($"Nepodařilo se získat uživatele {selectedItem} z databáze.");
                }
            }

            return user;
        }

        private string FormatUserToString(User user)
        {
            return $"{user.Id} ({user.PersonalNumber}): {user.FirstName} {user.Surname}";
        }

        private void GenerateWindowsUsername()
        {
            if (!String.IsNullOrEmpty(textBoxFirstName.Text) && !String.IsNullOrEmpty(textBoxSurname.Text))
            {
                textBoxWindowsUsername.Text = $"{RemoveDiacritics(textBoxFirstName.Text[0].ToString())}{RemoveDiacritics(textBoxSurname.Text)}";
            }
        }

        private string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Specifické nahrazení pro německé přehlásky
            input = input.Replace("ü", "ue")
                         .Replace("Ü", "Ue")
                         .Replace("ö", "oe")
                         .Replace("Ö", "Oe")
                         .Replace("ä", "ae")
                         .Replace("Ä", "Ae")
                         .Replace("ß", "ss");

            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);
                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
        }

        private void buttonGenerateWindowsUsername_Click(object sender, EventArgs e)
        {
            GenerateWindowsUsername();
        }

        private async void listBoxUsers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxUsers.SelectedIndex >= listBoxUsers.Items.Count - 1)
            {
                var user = await GetUserBySelectedItem();

                if (user != null)
                {
                    groupBox1.Text = $"Zobrazení uživatele ({user.Id})";
                    textBoxFirstName.Text = user.FirstName;
                    textBoxFirstName.Enabled = false;
                    textBoxSurname.Text = user.Surname;
                    textBoxSurname.Enabled = false;
                    textBoxWindowsUsername.Text = user.WindowsUsername;
                    textBoxWindowsUsername.Enabled = false;
                    maskedTextBoxPersonalNumber.Text = user.PersonalNumber.ToString();
                    maskedTextBoxPersonalNumber.Enabled = false;
                    numericUpDownLevelOfAccess.Value = user.LevelOfAccess;
                    numericUpDownLevelOfAccess.Enabled = false;

                    buttonAdd.Text = "Konec prohlížení";
                }
            }
        }

        private (bool, string) CheckForEmptyOrIncorrectFields()
        {
            if (string.IsNullOrEmpty(textBoxFirstName.Text)) return (false, "Jméno");
            if (string.IsNullOrEmpty(textBoxSurname.Text)) return (false, "Příjmení");
            if (string.IsNullOrEmpty(textBoxWindowsUsername.Text)) return (false, "Windows login");
            if (string.IsNullOrEmpty(maskedTextBoxPersonalNumber.Text)) return (false, "Osobní číslo");

            return (true, "");
        }

        private void ClearFields()
        {
            textBoxFirstName.Text = "";
            textBoxSurname.Text = "";
            textBoxWindowsUsername.Text = "";
            maskedTextBoxPersonalNumber.Text = "";
            numericUpDownLevelOfAccess.Value = 0;
        }
    }
}

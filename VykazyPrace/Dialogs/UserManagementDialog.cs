using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class UserManagementDialog : Form
    {
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly UserGroupRepository _userGroupRepo = new UserGroupRepository();
        private List<User> _users = new List<User>();
        private List<UserGroup> _userGroups = new List<UserGroup>();
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
                _users = await _userRepo.GetAllUsersAsync();
                _userGroups = await _userGroupRepo.GetAllUserGroupsAsync();

                Invoke(() =>
                {
                    listBoxUsers.Items.Clear();
                    listBoxUsers.Items.AddRange(_users.Select(FormatHelper.FormatUserToString).ToArray());

                    comboBoxGroup.Items.Clear();
                    comboBoxGroup.Items.AddRange(_userGroups.Select(FormatHelper.FormatUserGroupToString).ToArray());
                    comboBoxGroup.SelectedIndex = 0;

                    _loadingUC.Visible = false;
                });
            }
            catch (Exception ex)
            {
                Invoke(() =>
                {
                    AppLogger.Error($"Chyba při načítání uživatelů.", ex);
                    _loadingUC.Visible = false;
                });
            }
        }

        private User? GetUserBySelectedItem()
        {
            int index = listBoxUsers.SelectedIndex;
            if (index >= 0 && index < _users.Count)
                return _users[index];

            return null;
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
                comboBoxGroup.Enabled = true;
                ClearFields();
                buttonAdd.Text = "Přidat";
            }

            else
            {
                var dataCheck = CheckForEmptyOrIncorrectFields();

                if (dataCheck.Item1)
                {
                    UserGroup? selectedGroup = comboBoxGroup.SelectedIndex >= 0 && comboBoxGroup.SelectedIndex < _userGroups.Count
                        ? _userGroups[comboBoxGroup.SelectedIndex]
                        : null;

                    if (selectedGroup == null)
                    {
                        AppLogger.Error("Není vybrána žádná platná skupina.");
                        return;
                    }

                    var newUser = new User
                    {
                        FirstName = textBoxFirstName.Text,
                        Surname = textBoxSurname.Text,
                        PersonalNumber = int.Parse(maskedTextBoxPersonalNumber.Text),
                        WindowsUsername = textBoxWindowsUsername.Text,
                        LevelOfAccess = (int)numericUpDownLevelOfAccess.Value,
                        UserGroupId = selectedGroup.Id
                    };


                    var addedUser = await _userRepo.CreateUserAsync(newUser);
                    if (addedUser is not null)
                    {
                        AppLogger.Information($"Uživatel {FormatHelper.FormatUserToString(addedUser)} byl přidán do databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Uživatel {FormatHelper.FormatUserToString(newUser)} nebyl přidán do databáze.");
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
            var user = GetUserBySelectedItem();

            if (user != null)
            {
                var dialogResult = MessageBox.Show($"Smazat uživatele {FormatHelper.FormatUserToString(user)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _userRepo.DeleteUserAsync(user.Id))
                    {
                        AppLogger.Information($"Uživatel {FormatHelper.FormatUserToString(user)} byl smazán z databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat uživatele {FormatHelper.FormatUserToString(user)} z databáze.");
                    }

                    await LoadUsersAsync();
                }
            }
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
            var user = GetUserBySelectedItem();
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

                comboBoxGroup.SelectedIndex = _userGroups.FindIndex(g => g.Id == user.UserGroupId);
                comboBoxGroup.Enabled = false;

                buttonAdd.Text = "Konec prohlížení";
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
            comboBoxGroup.Text = "";
            comboBoxGroup.SelectedIndex = 0;
        }
    }
}

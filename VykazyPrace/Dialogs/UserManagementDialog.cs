using System.ComponentModel;
using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class UserManagementDialog : Form
    {
        private readonly UserRepository _userRepo;
        private readonly UserGroupRepository _userGroupRepo;

        private List<User> _users = new();
        private List<UserGroup> _userGroups = new();

        private BindingList<UserGridRow> _gridRows = new();

        private readonly LoadingUC _loadingUC = new();

        private bool _isInternalGridChange;

        public UserManagementDialog(
            UserRepository userRepo,
            UserGroupRepository userGroupRepo)
        {
            InitializeComponent();

            _userRepo = userRepo;
            _userGroupRepo = userGroupRepo;
        }

        private async void UserManagementDialog_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);
            _loadingUC.BringToFront();

            SetupDataGridView();

            await LoadUsersAsync();
        }

        private void SetupDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = true;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.MultiSelect = false;
            dataGridView1.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;

            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(UserGridRow.FirstName),
                HeaderText = "Jméno",
                DataPropertyName = nameof(UserGridRow.FirstName),
                Width = 120
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(UserGridRow.Surname),
                HeaderText = "Příjmení",
                DataPropertyName = nameof(UserGridRow.Surname),
                Width = 140
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(UserGridRow.WindowsUsername),
                HeaderText = "Windows login",
                DataPropertyName = nameof(UserGridRow.WindowsUsername),
                ReadOnly = true,
                Width = 140
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(UserGridRow.PersonalNumber),
                HeaderText = "Osobní číslo",
                DataPropertyName = nameof(UserGridRow.PersonalNumber),
                Width = 100
            });

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = nameof(UserGridRow.LevelOfAccess),
                HeaderText = "LoA",
                DataPropertyName = nameof(UserGridRow.LevelOfAccess),
                Width = 60
            });

            dataGridView1.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = nameof(UserGridRow.UserGroupId),
                HeaderText = "Skupina",
                DataPropertyName = nameof(UserGridRow.UserGroupId),
                DisplayMember = nameof(ComboItem<int>.Text),
                ValueMember = nameof(ComboItem<int>.Id),
                Width = 180,
                FlatStyle = FlatStyle.Flat
            });

            dataGridView1.Columns.Add(new DataGridViewComboBoxColumn
            {
                Name = nameof(UserGridRow.MasterUserId),
                HeaderText = "Sekundární vlastník",
                DataPropertyName = nameof(UserGridRow.MasterUserId),
                DisplayMember = nameof(ComboItem<int?>.Text),
                ValueMember = nameof(ComboItem<int?>.Id),
                Width = 240,
                FlatStyle = FlatStyle.Flat
            });

            dataGridView1.Columns.Add(new DataGridViewCheckBoxColumn
            {
                Name = nameof(UserGridRow.IsArchived),
                HeaderText = "Archivován",
                DataPropertyName = nameof(UserGridRow.IsArchived),
                Width = 90
            });
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                _loadingUC.Visible = true;
                _loadingUC.BringToFront();

                _users = await _userRepo.GetAllUsersAsync();
                _userGroups = await _userGroupRepo.GetAllUserGroupsAsync();

                var rows = _users.Select(u => new UserGridRow
                {
                    Id = u.Id,
                    FirstName = u.FirstName,
                    Surname = u.Surname,
                    WindowsUsername = u.WindowsUsername,
                    PersonalNumber = u.PersonalNumber,
                    LevelOfAccess = u.LevelOfAccess,
                    UserGroupId = u.UserGroupId,
                    MasterUserId = u.MasterUserId,
                    IsArchived = u.IsArchived
                }).ToList();

                _gridRows = new BindingList<UserGridRow>(rows);

                var groupColumn = (DataGridViewComboBoxColumn)dataGridView1.Columns[nameof(UserGridRow.UserGroupId)];
                groupColumn.DataSource = _userGroups
                    .Select(g => new ComboItem<int>
                    {
                        Id = g.Id,
                        Text = FormatHelper.FormatUserGroupToString(g)
                    })
                    .ToList();

                var masterUsers = new List<ComboItem<int?>>
        {
            new()
            {
                Id = null,
                Text = ""
            }
        };

                masterUsers.AddRange(_users.Select(u => new ComboItem<int?>
                {
                    Id = u.Id,
                    Text = FormatHelper.FormatUserToString(u)
                }));

                var masterColumn = (DataGridViewComboBoxColumn)dataGridView1.Columns[nameof(UserGridRow.MasterUserId)];
                masterColumn.DataSource = masterUsers;

                dataGridView1.DataSource = _gridRows;

                _loadingUC.Visible = false;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při načítání uživatelů.", ex);
                _loadingUC.Visible = false;
            }
        }

        private bool HasUserChanged(UserGridRow row)
        {
            var originalUser = _users.FirstOrDefault(u => u.Id == row.Id);

            if (originalUser is null)
                return false;

            var generatedWindowsUsername = GenerateWindowsUsername(row.FirstName, row.Surname);

            return originalUser.FirstName != row.FirstName.Trim()
                   || originalUser.Surname != row.Surname.Trim()
                   || originalUser.WindowsUsername != generatedWindowsUsername.Trim()
                   || originalUser.PersonalNumber != row.PersonalNumber
                   || originalUser.LevelOfAccess != row.LevelOfAccess
                   || originalUser.UserGroupId != row.UserGroupId
                   || originalUser.MasterUserId != row.MasterUserId
                   || originalUser.IsArchived != row.IsArchived;
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            dataGridView1.EndEdit();

            var rowsToSave = _gridRows
                .Where(r => !IsEmptyRow(r))
                .ToList();

            if (rowsToSave.Count == 0)
            {
                AppLogger.Information("Není zadán žádný uživatel k uložení.", true);
                return;
            }

            var newRows = rowsToSave
                .Where(r => r.Id == 0)
                .ToList();

            var changedExistingRows = rowsToSave
                .Where(r => r.Id > 0)
                .Where(HasUserChanged)
                .ToList();

            if (newRows.Count == 0 && changedExistingRows.Count == 0)
            {
                AppLogger.Information("Nejsou žádné změny k uložení.", true);
                return;
            }

            foreach (var row in newRows)
            {
                row.WindowsUsername = GenerateWindowsUsername(row.FirstName, row.Surname);

                var dataCheck = CheckGridRow(row);

                if (!dataCheck.IsValid)
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {dataCheck.Parameter}");
                    return;
                }

                var newUser = new User
                {
                    FirstName = row.FirstName.Trim(),
                    Surname = row.Surname.Trim(),
                    PersonalNumber = row.PersonalNumber,
                    WindowsUsername = row.WindowsUsername.Trim(),
                    LevelOfAccess = row.LevelOfAccess,
                    UserGroupId = row.UserGroupId,
                    MasterUserId = row.MasterUserId,
                    IsArchived = row.IsArchived
                };

                var addedUser = await _userRepo.CreateUserAsync(newUser);

                if (addedUser is not null)
                {
                    AppLogger.Information($"Uživatel {FormatHelper.FormatUserToString(addedUser)} byl přidán do databáze.", true);
                }
                else
                {
                    AppLogger.Error($"Uživatel {FormatHelper.FormatUserToString(newUser)} nebyl přidán do databáze.");
                    return;
                }
            }

            foreach (var row in changedExistingRows)
            {
                row.WindowsUsername = GenerateWindowsUsername(row.FirstName, row.Surname);

                var dataCheck = CheckGridRow(row);

                if (!dataCheck.IsValid)
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {dataCheck.Parameter}");
                    return;
                }

                var updatedUser = new User
                {
                    Id = row.Id,
                    FirstName = row.FirstName.Trim(),
                    Surname = row.Surname.Trim(),
                    PersonalNumber = row.PersonalNumber,
                    WindowsUsername = row.WindowsUsername.Trim(),
                    LevelOfAccess = row.LevelOfAccess,
                    UserGroupId = row.UserGroupId,
                    MasterUserId = row.MasterUserId,
                    IsArchived = row.IsArchived
                };

                var success = await _userRepo.UpdateUserAsync(updatedUser);

                if (success)
                {
                    AppLogger.Information($"Uživatel {FormatHelper.FormatUserToString(updatedUser)} byl upraven.", true);
                }
                else
                {
                    AppLogger.Error($"Uživatele {FormatHelper.FormatUserToString(updatedUser)} se nepodařilo upravit.");
                    return;
                }
            }

            await LoadUsersAsync();
        }

        private void dataGridView1_CellValueChanged(object? sender, DataGridViewCellEventArgs e)
        {
            if (_isInternalGridChange)
                return;

            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (columnName != nameof(UserGridRow.FirstName) &&
                columnName != nameof(UserGridRow.Surname))
            {
                return;
            }

            if (dataGridView1.Rows[e.RowIndex].DataBoundItem is not UserGridRow row)
                return;

            row.WindowsUsername = GenerateWindowsUsername(row.FirstName, row.Surname);

            _isInternalGridChange = true;
            dataGridView1.Refresh();
            _isInternalGridChange = false;
        }

        private void dataGridView1_CurrentCellDirtyStateChanged(object? sender, EventArgs e)
        {
            if (dataGridView1.IsCurrentCellDirty)
            {
                dataGridView1.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        }

        private void dataGridView1_CellValidating(object? sender, DataGridViewCellValidatingEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0)
                return;

            var columnName = dataGridView1.Columns[e.ColumnIndex].Name;

            if (dataGridView1.Rows[e.RowIndex].IsNewRow)
                return;

            if (columnName == nameof(UserGridRow.PersonalNumber))
            {
                if (!int.TryParse(e.FormattedValue?.ToString(), out var value) || value < 0)
                {
                    AppLogger.Error("Osobní číslo musí být číslo.");
                    e.Cancel = true;
                }
            }

            if (columnName == nameof(UserGridRow.LevelOfAccess))
            {
                if (!int.TryParse(e.FormattedValue?.ToString(), out var value) || value < 0)
                {
                    AppLogger.Error("LoA musí být číslo větší nebo rovno 0.");
                    e.Cancel = true;
                }
            }
        }

        private void dataGridView1_DataError(object? sender, DataGridViewDataErrorEventArgs e)
        {
            AppLogger.Error($"Chyba v tabulce uživatelů: {e.Exception.Message}");
            e.ThrowException = false;
        }

        private bool IsEmptyRow(UserGridRow row)
        {
            return string.IsNullOrWhiteSpace(row.FirstName)
                   && string.IsNullOrWhiteSpace(row.Surname)
                   && row.PersonalNumber == 0
                   && row.LevelOfAccess == 0
                   && row.UserGroupId == 0
                   && row.MasterUserId is null;
        }

        private (bool IsValid, string Parameter) CheckGridRow(UserGridRow row)
        {
            if (string.IsNullOrWhiteSpace(row.FirstName))
                return (false, "Jméno");

            if (string.IsNullOrWhiteSpace(row.Surname))
                return (false, "Příjmení");

            if (string.IsNullOrWhiteSpace(row.WindowsUsername))
                return (false, "Windows login");

            if (row.PersonalNumber < 0)
                return (false, "Osobní číslo");

            if (row.LevelOfAccess < 0)
                return (false, "Úroveň oprávnění");

            if (row.UserGroupId <= 0)
                return (false, "Skupina");

            return (true, "");
        }

        private string GenerateWindowsUsername(string firstName, string surname)
        {
            if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(surname))
                return "";

            return $"{RemoveDiacritics(firstName[0].ToString())}{RemoveDiacritics(surname)}";
        }

        private string RemoveDiacritics(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            input = input.Replace("ü", "ue")
                         .Replace("Ü", "Ue")
                         .Replace("ö", "oe")
                         .Replace("Ö", "Oe")
                         .Replace("ä", "ae")
                         .Replace("Ä", "Ae")
                         .Replace("ß", "ss");

            string normalized = input.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new();

            foreach (char c in normalized)
            {
                UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

                if (category != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString()
                .Normalize(NormalizationForm.FormC)
                .ToLowerInvariant();
        }

        private class UserGridRow
        {
            public int Id { get; set; }

            public string FirstName { get; set; } = "";
            public string Surname { get; set; } = "";
            public string WindowsUsername { get; set; } = "";

            public int PersonalNumber { get; set; }
            public int LevelOfAccess { get; set; }

            public int? UserGroupId { get; set; }

            public int? MasterUserId { get; set; }

            public bool IsArchived { get; set; }

            public bool IsExistingUser => Id > 0;
        }

        private class ComboItem<T>
        {
            public T? Id { get; set; }
            public string Text { get; set; } = "";
        }
    }
}
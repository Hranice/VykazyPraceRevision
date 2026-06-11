using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Enums;

namespace VykazyPrace.Dialogs
{
    public partial class UserSelectionDialog : Form
    {
        private readonly UserSelectionMode _mode;
        private readonly UserRepository _userRepository;
        private readonly UserGroupRepository _userGroupRepository;

        private List<User> _allUsers = new();
        private List<UserGroup> _allGroups = new();

        private bool _isInternalChange;

        public List<User> SelectedUsers { get; private set; } = new();

        public User? SelectedUser => SelectedUsers.FirstOrDefault();

        public UserSelectionDialog(
            UserSelectionMode mode,
            IEnumerable<int>? preselectedUserIds = null)
        {
            _mode = mode;

            _userRepository = new UserRepository();
            _userGroupRepository = new UserGroupRepository();

            InitializeComponent();
            BuildUi();

            if (preselectedUserIds != null)
            {
                _preselectedUserIds = preselectedUserIds.ToHashSet();
            }
        }

        private readonly HashSet<int> _preselectedUserIds = new();

        private async void UserSelectionDialog_Load(object? sender, EventArgs e)
        {
            await LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            _allUsers = await _userRepository.GetAllUsersAsync();
            _allGroups = await _userGroupRepository.GetAllUserGroupsAsync();

            _allUsers = _allUsers
                .OrderBy(u => u.UserGroup!.Title)
                .ThenBy(u => u.Surname)
                .ThenBy(u => u.FirstName)
                .ToList();

            _allGroups = _allGroups
                .OrderBy(g => g.Title)
                .ToList();

            FillGroups();
            FillUsers();

            ApplyPreselection();
        }

        private void BuildUi()
        {
            Text = _mode == UserSelectionMode.Single
                ? "Výběr uživatele"
                : "Výběr uživatelů";

            MinimizeBox = false;
            MaximizeBox = false;
            FormBorderStyle = FormBorderStyle.FixedDialog;

            if (_mode == UserSelectionMode.Single)
            {
                label1.Text = "Filtr skupin";
            }
            else
            {
                label1.Text = "Skupiny";
            }

            cLBUserGroups.CheckOnClick = true;
            cLBUsers.CheckOnClick = true;
        }

        private void FillGroups()
        {
            cLBUserGroups.Items.Clear();

            foreach (var group in _allGroups)
            {
                bool isChecked = _mode == UserSelectionMode.Single;

                cLBUserGroups.Items.Add(new UserGroupListItem(group), isChecked);
            }
        }

        private void FillUsers()
        {
            var search = FormatHelper.RemoveDiacritics(tBSearch.Text.Trim().ToLowerInvariant());

            var users = _allUsers.AsEnumerable();

            if (_mode == UserSelectionMode.Single)
            {
                var checkedGroupIds = GetCurrentlyCheckedGroupIds();

                users = users.Where(u =>
                    u.UserGroupId.HasValue &&
                    checkedGroupIds.Contains(u.UserGroupId.Value));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                {
                    var text = FormatHelper.RemoveDiacritics(
                        $"{u.FirstName} {u.Surname} {u.PersonalNumber} {u.WindowsUsername} {u.Email} {u.UserGroup?.Title}"
                        .ToLowerInvariant());

                    return text.Contains(search);
                });
            }

            var checkedUserIds = GetCurrentlyCheckedUserIds();

            _isInternalChange = true;

            cLBUsers.Items.Clear();

            foreach (var user in users)
            {
                var item = new UserListItem(user);
                var isChecked = checkedUserIds.Contains(user.Id) || _preselectedUserIds.Contains(user.Id);

                cLBUsers.Items.Add(item, isChecked);
            }

            _isInternalChange = false;
        }

        private HashSet<int> GetCurrentlyCheckedUserIds()
        {
            var ids = new HashSet<int>();

            foreach (var checkedItem in cLBUsers.CheckedItems)
            {
                if (checkedItem is UserListItem userItem)
                    ids.Add(userItem.User.Id);
            }

            return ids;
        }

        private HashSet<int> GetCurrentlyCheckedGroupIds()
        {
            var ids = new HashSet<int>();

            foreach (var checkedItem in cLBUserGroups.CheckedItems)
            {
                if (checkedItem is UserGroupListItem groupItem)
                    ids.Add(groupItem.Group.Id);
            }

            return ids;
        }

        private void ApplyPreselection()
        {
            if (_preselectedUserIds.Count == 0)
                return;

            _isInternalChange = true;

            for (int i = 0; i < cLBUsers.Items.Count; i++)
            {
                if (cLBUsers.Items[i] is UserListItem item)
                {
                    cLBUsers.SetItemChecked(i, _preselectedUserIds.Contains(item.User.Id));
                }
            }

            _isInternalChange = false;

            UpdateGroupsBySelectedUsers();
        }

        private void TxtSearch_TextChanged(object? sender, EventArgs e)
        {
            FillUsers();
        }

        private void ClbGroups_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isInternalChange)
                return;

            if (_mode == UserSelectionMode.Single)
            {
                BeginInvoke(new Action(FillUsers));
                return;
            }

            BeginInvoke(new Action(() =>
            {
                if (cLBUserGroups.Items[e.Index] is not UserGroupListItem groupItem)
                    return;

                bool shouldCheck = e.NewValue == CheckState.Checked;
                int groupId = groupItem.Group.Id;

                _isInternalChange = true;

                for (int i = 0; i < cLBUsers.Items.Count; i++)
                {
                    if (cLBUsers.Items[i] is UserListItem userItem &&
                        userItem.User.UserGroupId == groupId)
                    {
                        cLBUsers.SetItemChecked(i, shouldCheck);
                    }
                }

                _isInternalChange = false;
            }));
        }

        private void ClbUsers_ItemCheck(object? sender, ItemCheckEventArgs e)
        {
            if (_isInternalChange)
                return;

            if (_mode == UserSelectionMode.Single && e.NewValue == CheckState.Checked)
            {
                BeginInvoke(new Action(() =>
                {
                    _isInternalChange = true;

                    for (int i = 0; i < cLBUsers.Items.Count; i++)
                    {
                        if (i != e.Index)
                            cLBUsers.SetItemChecked(i, false);
                    }

                    _isInternalChange = false;
                }));
            }

            BeginInvoke(new Action(UpdateGroupsBySelectedUsers));
        }

        private void UpdateGroupsBySelectedUsers()
        {
            if (_mode == UserSelectionMode.Single)
                return;

            var selectedUserIds = GetCurrentlyCheckedUserIds();

            _isInternalChange = true;

            for (int i = 0; i < cLBUserGroups.Items.Count; i++)
            {
                if (cLBUserGroups.Items[i] is not UserGroupListItem groupItem)
                    continue;

                var groupUserIds = _allUsers
                    .Where(u => u.UserGroupId == groupItem.Group.Id)
                    .Select(u => u.Id)
                    .ToList();

                bool allSelected = groupUserIds.Count > 0 &&
                                   groupUserIds.All(id => selectedUserIds.Contains(id));

                cLBUserGroups.SetItemChecked(i, allSelected);
            }

            _isInternalChange = false;
        }

        private void BOk_Click(object? sender, EventArgs e)
        {
            SelectedUsers = cLBUsers.CheckedItems
                .OfType<UserListItem>()
                .Select(i => i.User)
                .DistinctBy(u => u.Id)
                .ToList();

            if (_mode == UserSelectionMode.Single && SelectedUsers.Count != 1)
            {
                MessageBox.Show(
                    "Vyberte právě jednoho uživatele.",
                    "Výběr uživatele",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            if (_mode == UserSelectionMode.Multiple && SelectedUsers.Count == 0)
            {
                MessageBox.Show(
                    "Vyberte alespoň jednoho uživatele.",
                    "Výběr uživatelů",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);

                return;
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        private sealed class UserListItem
        {
            public User User { get; }

            public UserListItem(User user)
            {
                User = user;
            }

            public override string ToString()
            {
                return FormatHelper.FormatUserToString(User);
            }
        }

        private sealed class UserGroupListItem
        {
            public UserGroup Group { get; }

            public UserGroupListItem(UserGroup group)
            {
                Group = group;
            }

            public override string ToString()
            {
                return FormatHelper.FormatUserGroupToString(Group);
            }
        }
    }
}
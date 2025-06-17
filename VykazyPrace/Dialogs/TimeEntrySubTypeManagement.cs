using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging.VykazyPrace.Logging;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntrySubTypeManagement : Form
    {
        private User _selectedUser;
        private TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private TimeEntryRepository _timeEntryRepo;
        private List<TimeEntrySubType> _timeEntrySubTypes = new List<TimeEntrySubType>();

        public TimeEntrySubTypeManagement(User selectedUser, TimeEntrySubTypeRepository timeEntrySubTypeRepository, TimeEntryRepository timeEntryRepo)
        {
            InitializeComponent();

            _selectedUser = selectedUser;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepository;
            _timeEntryRepo = timeEntryRepo;

            this.KeyPreview = true;
            this.KeyDown += TimeEntrySubTypeManagement_KeyDown;
        }

        private async void TimeEntrySubTypeManagement_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.Control && e.KeyCode == Keys.S)
            {
                e.SuppressKeyPress = true;

                if (HasOrderChanged())
                {
                    await SaveOrderChangesAsync();
                }
            }
        }


        private async Task LoadTimeEntrySubTypesAsync()
        {
            listBoxTimeEntrySubTypes.Items.Clear();
            _timeEntrySubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);
            listBoxTimeEntrySubTypes.Items.AddRange(
                _timeEntrySubTypes
                    .Where(t => t.IsArchived == 0)
                    .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                    .ToArray());

        }

        private async void TimeEntrySubTypeManagement_Load(object sender, EventArgs e)
        {
            await LoadTimeEntrySubTypesAsync();
        }

        private void listBoxTimeEntrySubTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxTitle.Text = listBoxTimeEntrySubTypes.Text?.ToString();

            if (listBoxTimeEntrySubTypes.SelectedItems.Count > 1)
            {
                textBoxTitle.Text = string.Empty;
            }
        }

        private async void buttonRename_Click(object sender, EventArgs e)
        {
            if (listBoxTimeEntrySubTypes.SelectedIndex < 0)
                return;

            if (listBoxTimeEntrySubTypes.SelectedItems.Count > 1)
            {
                AppLogger.Error("Nelze přepisovat více položek najednou.");
                return;
            }

            var result = MessageBox.Show(
                "Opravdu chcete přejmenovat tento index? Změna se projeví na všech položkách (s výjimkou uzamčených měsíců).",
                "Přejmenovat index?",
                MessageBoxButtons.YesNoCancel
            );

            if (result != DialogResult.Yes)
                return;

            var selectedSubType = _timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex];
            string oldTitle = selectedSubType.Title;
            string newTitle = textBoxTitle.Text?.Trim();

            if (string.IsNullOrWhiteSpace(newTitle))
            {
                AppLogger.Error("Název indexu nesmí být prázdný.");
                return;
            }

            if (oldTitle == newTitle)
                return;

            selectedSubType.Title = newTitle;
            bool success = await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(selectedSubType);

            if (success)
            {
                int affected = await _timeEntryRepo.UpdateUnlockedDescriptionsForUserAsync(_selectedUser.Id, oldTitle, newTitle);

                AppLogger.Information($"Změněn typ záznamu: '{oldTitle}' -> '{newTitle}'. Přejmenováno {affected} záznamů.", true);
                await LoadTimeEntrySubTypesAsync();
                textBoxTitle.Text = string.Empty;
            }
            else
            {
                AppLogger.Error("Nepodařilo se aktualizovat záznam.");
            }
        }


        private async void buttonDelete_Click(object sender, EventArgs e)
        {
            var selectedIndices = listBoxTimeEntrySubTypes.SelectedIndices.Cast<int>().ToList();

            if (!selectedIndices.Any())
                return;

            foreach (int index in selectedIndices)
            {
                var subType = _timeEntrySubTypes[index];
                subType.IsArchived = 1;
                await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(subType);
            }

            await LoadTimeEntrySubTypesAsync();
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            int index = listBoxTimeEntrySubTypes.SelectedIndex;
            if (index <= 0) return;

            var item = _timeEntrySubTypes[index];
            _timeEntrySubTypes.RemoveAt(index);
            _timeEntrySubTypes.Insert(index - 1, item);

            RefreshListBox();
            listBoxTimeEntrySubTypes.SelectedIndex = index - 1;
        }


        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            int index = listBoxTimeEntrySubTypes.SelectedIndex;
            if (index < 0 || index >= _timeEntrySubTypes.Count - 1) return;

            var item = _timeEntrySubTypes[index];
            _timeEntrySubTypes.RemoveAt(index);
            _timeEntrySubTypes.Insert(index + 1, item);

            RefreshListBox();
            listBoxTimeEntrySubTypes.SelectedIndex = index + 1;
        }


        private void RefreshListBox()
        {
            listBoxTimeEntrySubTypes.Items.Clear();
            listBoxTimeEntrySubTypes.Items.AddRange(
                _timeEntrySubTypes
                    .Where(t => t.IsArchived == 0)
                    .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                    .ToArray());

            this.Text = HasOrderChanged() ? "Správa indexů*" : "Správa indexů";
        }


        private async void TimeEntrySubTypeManagement_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (HasOrderChanged())
            {
                var result = MessageBox.Show("Byly provedeny změny v pořadí. Chcete je uložit?", "Uložit změny?", MessageBoxButtons.YesNoCancel);
                if (result == DialogResult.Cancel)
                {
                    e.Cancel = true;
                    this.Text = "Správa indexů*";
                    return;
                }
                if (result == DialogResult.Yes)
                {
                    await SaveOrderChangesAsync();
                    this.Text = "Správa indexů";
                }
            }
        }

        private async Task SaveOrderChangesAsync()
        {
            for (int i = 0; i < _timeEntrySubTypes.Count; i++)
            {
                _timeEntrySubTypes[i].Order = i;
                await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(_timeEntrySubTypes[i]);
            }

            AppLogger.Information("Změny pořadí byly úspěšně uloženy.");
        }



        private bool HasOrderChanged()
        {
            for (int i = 0; i < _timeEntrySubTypes.Count; i++)
            {
                if ((_timeEntrySubTypes[i].Order ?? -1) != i)
                    return true;
            }
            return false;
        }


    }
}

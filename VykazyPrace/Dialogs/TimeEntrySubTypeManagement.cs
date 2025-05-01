using System.Data;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;

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

            if(listBoxTimeEntrySubTypes.SelectedItems.Count > 1)
            {
                textBoxTitle.Text = string.Empty;
            }
        }

        private async void buttonRename_Click(object sender, EventArgs e)
        {
            if (listBoxTimeEntrySubTypes.SelectedIndex < 0)
                return;

            if(listBoxTimeEntrySubTypes.SelectedItems.Count > 1)
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

    }
}

using System.Data;
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

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var newEntrySubType = _timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex];
            newEntrySubType.IsArchived = 1;
            await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(newEntrySubType);
            await LoadTimeEntrySubTypesAsync();
        }

        private async void TimeEntrySubTypeManagement_Load(object sender, EventArgs e)
        {
            await LoadTimeEntrySubTypesAsync();
        }

        private void listBoxTimeEntrySubTypes_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxTitle.Text = listBoxTimeEntrySubTypes.Text?.ToString();
        }

        private async void buttonSave_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Změnou označení přepíšete všechny dosavadní záznamy, chcete pokračovat?", "Přepsat záznamy?", MessageBoxButtons.YesNoCancel);

            if (result == DialogResult.Yes)
            {
                string originalSubTypeTitle = listBoxTimeEntrySubTypes.Text.ToString();
                var newEntrySubType = _timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex];
                newEntrySubType.Title = textBoxTitle.Text;
                await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(newEntrySubType);

                int rowsAffected = await _timeEntryRepo.ReplaceDescriptionForUserAsync(_selectedUser.Id, originalSubTypeTitle, newEntrySubType.Title);
                AppLogger.Information($"Přepsáno ({rowsAffected}) záznamů: '{originalSubTypeTitle}' -> '{newEntrySubType.Title}'.", true);
                await LoadTimeEntrySubTypesAsync();
            }
        }
    }
}

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
            var timeEntrySubTypesResult = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);

            if (timeEntrySubTypesResult.Success)
            {
                _timeEntrySubTypes = timeEntrySubTypesResult.TimeEntrySubTypes;
                listBoxTimeEntrySubTypes.Items.AddRange(
                    _timeEntrySubTypes
                        .Where(t => t.IsArchived == 0)
                        .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                        .ToArray());
            }

            else
            {
                AppLogger.Error("Nepodařilo se načíst indexy.", new Exception(timeEntrySubTypesResult.Error));
            }
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
            var dialogResult = MessageBox.Show("Změnou označení přepíšete všechny dosavadní záznamy, chcete pokračovat?", "Přepsat záznamy?", MessageBoxButtons.YesNoCancel);

            if (dialogResult == DialogResult.Yes)
            {
                string originalSubTypeTitle = listBoxTimeEntrySubTypes.Text.ToString();
                var newEntrySubType = _timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex];
                newEntrySubType.Title = textBoxTitle.Text;
                await _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(newEntrySubType);

                var result = await _timeEntryRepo.ReplaceDescriptionForUserAsync(_selectedUser.Id, originalSubTypeTitle, newEntrySubType.Title);
                if (result.Success)
                {
                    AppLogger.Information($"Přepsáno ({result.AffectedRecords}) záznamů: '{originalSubTypeTitle}' -> '{newEntrySubType.Title}'.", true);
                    await LoadTimeEntrySubTypesAsync();

                }

                else
                {
                    AppLogger.Error($"Nepodařilo se přejmenovat záznamy: '{originalSubTypeTitle}' -> '{newEntrySubType.Title}'.", new Exception(result.ErrorMessage));
                }
            }
        }
    }
}

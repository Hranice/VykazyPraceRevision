using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntrySubTypeManagement : Form
    {
        private User _selectedUser;
        private TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private List<TimeEntrySubType> _timeEntrySubTypes = new List<TimeEntrySubType>();

        public TimeEntrySubTypeManagement(User selectedUser, TimeEntrySubTypeRepository timeEntrySubTypeRepository)
        {
            InitializeComponent();

            _selectedUser = selectedUser;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepository;
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
            _timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex].IsArchived = 1;
            _timeEntrySubTypeRepo.UpdateTimeEntrySubTypeAsync(_timeEntrySubTypes[listBoxTimeEntrySubTypes.SelectedIndex]);
            await LoadTimeEntrySubTypesAsync();
        }

        private async void TimeEntrySubTypeManagement_Load(object sender, EventArgs e)
        {
            await LoadTimeEntrySubTypesAsync();
        }
    }
}

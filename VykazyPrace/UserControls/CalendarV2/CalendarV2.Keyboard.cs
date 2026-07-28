using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private void CopySelectedPanel()
        {
            if (_selectedTimeEntryId <= 0) return;

            if (_selectedUser.Id != _loggedUser.Id && _selectedUser.MasterUserId != _loggedUser.Id) return;

            var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);

            // snack
            if (entry.ProjectId == WorkLogIds.Projects.Snack && entry.EntryTypeId == WorkLogIds.EntryTypes.Snack) return;

            if (entry != null)
            {
                copiedEntry = new TimeEntry
                {
                    EntryTypeId = entry.EntryTypeId,
                    ProjectId = entry.ProjectId,
                    Description = entry.Description,
                    Note = entry.Note,
                    EntryMinutes = entry.EntryMinutes,
                    AfterCare = entry.AfterCare,
                    UserId = entry.UserId,
                    IsValid = entry.IsValid
                };

                var panel = panels.FirstOrDefault(p => p.EntryId == _selectedTimeEntryId);
                if (panel != null)
                {
                    copyToolTip.ToolTipTitle = "Zkopírováno";
                    copyToolTip.Show("Záznam byl zkopírován", panel, panel.Width / 2, panel.Height / 2, 2000);
                }
            }
        }

        private bool CanEditSelectedUser()
        {
            return _selectedUser.Id == _loggedUser.Id
                || _selectedUser.MasterUserId == _loggedUser.Id;
        }

        private bool CanEditOwner(int ownerId)
        {
            if (ownerId == _loggedUser.Id) return true;

            if (ownerId == _selectedUser.Id)
                return _selectedUser.MasterUserId == _loggedUser.Id;

            return false;
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            Control? focused = this.ContainsFocus ? this.GetFocusedControl(this) : null;

            if (focused is TextBoxBase or ComboBox)
                return base.ProcessCmdKey(ref msg, keyData);

            if (keyData == (Keys.Control | Keys.C))
            {
                CopySelectedPanel();
                return true;
            }

            if (keyData == (Keys.Control | Keys.V))
            {
                PasteCopiedPanel();
                return true;
            }

            return base.ProcessCmdKey(ref msg, keyData);
        }
    }
}

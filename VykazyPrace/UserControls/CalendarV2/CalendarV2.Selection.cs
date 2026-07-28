using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            // Ctrl-klik => toggle ve výběru
            if ((ModifierKeys & Keys.Control) == Keys.Control)
            {
                if (_selectedEntryIds.Contains(panel.EntryId))
                    RemoveFromSelection(panel.EntryId);
                else
                    AddToSelection(panel.EntryId);

                panel.Activate();
            }
            else
            {
                ClearSelection();
                DeactivateAllPanels();
                AddToSelection(panel.EntryId);
                panel.Activate();
                _selectedTimeEntryId = panel.EntryId;
            }

            UpdateBulkEditIndicator();
        }

        private void AddToSelection(int entryId)
        {
            if (_selectedEntryIds.Add(entryId))
                SetSelectedUi(entryId, true);
        }

        private void RemoveFromSelection(int entryId)
        {
            if (_selectedEntryIds.Remove(entryId))
                SetSelectedUi(entryId, false);
        }

        /// <summary>
        /// Nastaví stav "selected" pro daný panel, pokud není typu "snack" nebo "locked".
        /// </summary>
        private void SetSelectedUi(int entryId, bool selected)
        {
            var panel = GetPanelByEntryId(entryId);
            if (panel == null)
                return;

            var tag = panel.Tag as string;

            if (tag is "snack" or "locked")
                panel.Selected = false;

            else
                panel.Selected = selected;
        }



        private void DeactivateAllPanels()
        {
            foreach (var ctrl in tableLayoutPanelCalendar.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    pan.Deactivate();
                    //SetSelectedUi(pan.EntryId, false);
                }
            }
        }

        private void DeselectAllPanels()
        {
            foreach (var ctrl in tableLayoutPanelCalendar.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    SetSelectedUi(pan.EntryId, false);
                }
            }
        }


        public async Task DeleteRecord()
        {
            if (_selectedUser.Id != _loggedUser.Id && _selectedUser.MasterUserId != _loggedUser.Id) return;

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            // zamčeno nebo svačina
            if (timeEntry.IsLocked == 1 ||
                (timeEntry.ProjectId == WorkLogIds.Projects.Snack && timeEntry.EntryTypeId == WorkLogIds.EntryTypes.Snack))
                return;

            if (!ShowDeleteConfirmation(timeEntry)) return;

            bool success = await _timeEntryRepo.DeleteTimeEntryAsync(_selectedTimeEntryId);
            if (!success)
            {
                AppLogger.Error($"Nepodařilo se smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} z DB.");
                return;
            }

            AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl smazán z DB.");

            _currentEntries.Remove(timeEntry);
            _selectedTimeEntryId = -1;

            int scrollX = panelContainer.HorizontalScroll.Value;
            BeginInvoke((Action)(() =>
            {
                RemoveEntryPanel(timeEntry.Id);

                panelContainer.HorizontalScroll.Value =
                    Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));

                UpdateHourLabels();
            }));

            await LoadSidebar();
        }

        private bool ShowDeleteConfirmation(TimeEntry entry)
        {
            var result = MessageBox.Show(
                $"Smazat záznam {(entry.IsValid == 1 ? FormatHelper.FormatTimeEntryToString(entry) : "")}?",
                "Smazat?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            return result == DialogResult.Yes;
        }

        private Control? GetFocusedControl(Control control)
        {
            foreach (Control child in control.Controls)
            {
                if (child.ContainsFocus)
                {
                    if (child.HasChildren)
                        return GetFocusedControl(child);
                    else
                        return child;
                }
            }

            return control.Focused ? control : null;
        }
    }
}

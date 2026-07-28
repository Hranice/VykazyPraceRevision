using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private async Task LoadInitialDataAsync()
        {
            await LoadReferenceDataAsync();

            SafeInvoke(() =>
            {
                customComboBoxProjects.SetItems(_projects
                    .Select(FormatHelper.FormatProjectToString)
                    .ToArray());

                UpdateEntryTypeControls(_currentProjectType);

                customComboBoxSubTypes.SetItems(_timeEntrySubTypes
                    .Where(t => t.IsArchived == 0)
                    .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                    .ToArray());
            });

            var specialTask = LoadSpecialDaysAsync();
            var arrivalTask = LoadArrivalDeparturesAsync();

            await Task.WhenAll(specialTask, arrivalTask);

            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
        }

        private async Task LoadReferenceDataAsync()
        {
            if (_cacheTypes == null)
                _cacheTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(DefaultProjectType);
            _timeEntryTypes = _cacheTypes;

            if (_cacheSubTypes == null)
                _cacheSubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);
            _timeEntrySubTypes = _cacheSubTypes;

            if (_cacheProjects == null)
                _cacheProjects = DefaultProjectType == 1
                    ? await _projectRepo.GetAllFullProjectsAndPreProjectsAsync(checkBoxArchivedProjects.Checked)
                    : await _projectRepo.GetAllProjectsAsyncByProjectType(DefaultProjectType);
            _projects = _cacheProjects;
        }


        private async Task LoadArrivalDeparturesAsync()
        {
            try
            {
                _arrivalDepartures = await _arrivalDepartureRepo.GetWeekEntriesForUserAsync(_selectedUser.Id, _selectedDate);
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání speciálních dnů.", ex));
            }
        }

        private async Task LoadSpecialDaysAsync()
        {
            try
            {
                _specialDays = await _specialDayRepo.GetSpecialDaysForWeekAsync(_selectedDate);
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání speciálních dnů.", ex));
            }
        }

        public async Task ChangeUser(User newUser)
        {
            _selectedUser = newUser;

            var arrivalTask = LoadArrivalDeparturesAsync();
            var renderTask = RenderCalendar();

            await Task.WhenAll(arrivalTask, renderTask);

            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);

            // úklid UI
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;

            _ = LoadSidebar();
        }



        internal async Task<DateTime> ChangeToPreviousWeek()
        {
            _selectedDate = _selectedDate.AddDays(-7);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToNextWeek()
        {
            _selectedDate = _selectedDate.AddDays(7);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToTodaysWeek()
        {
            DateTime today = DateTime.Today;
            int offset = ((int)today.DayOfWeek + 6) % 7;
            _selectedDate = today.AddDays(-offset);
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            this.Focus();
            return _selectedDate;
        }

        private async Task LoadTimeEntryTypesAsync(int projectType)
        {
            try
            {
                _currentProjectType = projectType;

                var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
                bool isArchived = entry?.AfterCare == 1;

                _timeEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);

                SafeInvoke(() => UpdateEntryTypeControls(projectType));
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání typů časových záznamů.", ex));
            }
        }

        private void UpdateEntryTypeControls(int projectType)
        {
            bool useRadioButtons = projectType is 0 or 1 or 2;

            comboBoxEntryType.Visible = !useRadioButtons;
            comboBoxEntryType.Enabled = !useRadioButtons;
            comboBoxEntryType.Items.Clear();

            ClearPreviousEntryTypeControls();

            if (useRadioButtons)
            {
                AddRadioButtonsForEntryTypes();
            }
            else
            {
                FillComboBoxWithEntryTypes();
            }
        }

        private void AddRadioButtonsForEntryTypes()
        {
            var layout = CreateRadioButtonLayout();

            for (int i = 0; i < _timeEntryTypes.Count; i++)
            {
                var entryType = _timeEntryTypes[i];
                var radio = CreateRadioButton(entryType.Title);

                if (i == 0)
                {
                    radio.Checked = true;
                }

                layout.Controls.Add(radio);
            }

            tableLayoutPanelEntryType.Controls.Add(layout);
        }


        private TableLayoutPanel CreateRadioButtonLayout()
        {
            var panel = new TableLayoutPanel
            {
                ColumnCount = 3,
                RowCount = 1,
                Dock = DockStyle.Fill,
                Height = comboBoxEntryType.Height,
                Padding = Padding.Empty,
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                Margin = Padding.Empty
            };

            for (int i = 0; i < panel.ColumnCount; i++)
            {
                panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            }

            return panel;
        }

        private RadioButton CreateRadioButton(string text)
        {
            return new RadioButton
            {
                Text = text,
                TextAlign = ContentAlignment.MiddleCenter,
                Appearance = Appearance.Button,
                Dock = DockStyle.Fill,
                AutoSize = true,
                Font = new Font(this.Font.FontFamily, 9.0f),
                BackColor = Color.White
            };
        }

        private void FillComboBoxWithEntryTypes()
        {
            var items = _timeEntryTypes.Select(type =>
                checkBoxArchivedProjects.Checked
                    ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(type)
                    : FormatHelper.FormatTimeEntryTypeToString(type));

            comboBoxEntryType.Items.AddRange(items.ToArray());
            comboBoxEntryType.Text = string.Empty;
        }

        private void ClearPreviousEntryTypeControls()
        {
            var tablePanels = tableLayoutPanelEntryType.Controls
                .OfType<TableLayoutPanel>()
                .ToList();

            foreach (var ctrl in tablePanels)
            {
                tableLayoutPanelEntryType.Controls.Remove(ctrl);
                ctrl.Dispose();
            }
        }

        private async Task LoadTimeEntrySubTypesAsync()
        {
            try
            {
                _timeEntrySubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);

                SafeInvoke(() =>
                {
                    customComboBoxSubTypes.SetItems(_timeEntrySubTypes
                                .Where(t => t.IsArchived == 0)
                                .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                                .ToArray());
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání sub-typů (indexů) časových záznamů.", ex));
            }
        }

        private async Task LoadProjectsAsync(int projectType)
        {
            try
            {
                bool includeArchived = checkBoxArchivedProjects.Checked;

                if (projectType == 1)
                {
                    _projects = await _projectRepo.GetAllFullProjectsAndPreProjectsAsync(checkBoxArchivedProjects.Checked);
                }
                else
                {
                    _projects = await _projectRepo.GetAllProjectsAsyncByProjectType(projectType);
                }

                SafeInvoke(() =>
                {
                    customComboBoxProjects.SetItems(_projects
                            .Select(FormatHelper.FormatProjectToString)
                            .ToArray());
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání projektů.", ex));
            }
        }

    }
}

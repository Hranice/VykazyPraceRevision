﻿using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Configuration;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using Timer = System.Windows.Forms.Timer;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        // Constants
        private const int ResizeThreshold = 12;
        private const int TimeSlotLengthInMinutes = 30;
        private const int DefaultProjectType = 0;

        // UI + State
        private readonly LoadingUC _loadingUC = new();
        private readonly Timer _resizeTimer = new() { Interval = 50 };
        private bool userHasScrolled = false;

        // Repositories
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private readonly ProjectRepository _projectRepo;
        private readonly SpecialDayRepository _specialDayRepo;
        private readonly UserRepository _userRepo;
        private readonly ArrivalDepartureRepository _arrivalDepartureRepo;

        // Data cache
        private List<Project> _projects = new();
        private List<TimeEntryType> _timeEntryTypes = new();
        private List<TimeEntrySubType> _timeEntrySubTypes = new();
        private List<SpecialDay> _specialDays = new();
        private List<ArrivalDeparture> _arrivalDepartures = new();
        private List<DayPanel> panels = new();
        private List<TimeEntry> _currentEntries = new();


        // Context
        private User _selectedUser;
        private DateTime _selectedDate;
        private int _selectedTimeEntryId = -1;
        private int _currentProjectType;

        // Drag & drop
        private DayPanel? activePanel = null;
        private DayPanel? lastPanel = null;
        private bool isResizing = false;
        private bool isMoving = false;
        private bool isResizingLeft = false;
        private int startMouseX;
        private int startPanelX;
        private int originalColumn;
        private int originalColumnSpan;

        // Copy & paste
        private TimeEntry? copiedEntry;
        private TableLayoutPanelCellPosition? pasteTargetCell;
        private ToolTip copyToolTip = new();

        // Right click context
        private ContextMenuStrip dayPanelMenu;
        private ContextMenuStrip tableLayoutMenu;

        // Configuration
        private AppConfig _config;
        private int arrivalColumn = 12;
        private int leaveColumn = 28;


        public CalendarV2(User currentUser,
                          TimeEntryRepository timeEntryRepo,
                          TimeEntryTypeRepository timeEntryTypeRepo,
                          TimeEntrySubTypeRepository timeEntrySubTypeRepo,
                          ProjectRepository projectRepo,
                          UserRepository userRepo,
                          SpecialDayRepository specialDayRepo,
                          ArrivalDepartureRepository arrivalDepartureRepo)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _selectedDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            _selectedUser = currentUser;

            _timeEntryRepo = timeEntryRepo;
            _timeEntryTypeRepo = timeEntryTypeRepo;
            _timeEntrySubTypeRepo = timeEntrySubTypeRepo;
            _projectRepo = projectRepo;
            _userRepo = userRepo;
            _specialDayRepo = specialDayRepo;
            _arrivalDepartureRepo = arrivalDepartureRepo;

            _resizeTimer.Tick += async (_, _) =>
            {
                _resizeTimer.Stop();
                await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);
            };

            _specialDayRepo = specialDayRepo;

            _config = ConfigService.Load();
        }

        private void InitializeContextMenus()
        {
            dayPanelMenu = new ContextMenuStrip();
            dayPanelMenu.Items.Add("Kopírovat", null, (_, _) => CopySelectedPanel());
            dayPanelMenu.Items.Add("Odstranit", null, async (_, _) => await DeleteRecord());

            tableLayoutMenu = new ContextMenuStrip();
            tableLayoutMenu.Items.Add("Vložit", null, (_, _) => PasteCopiedPanel());
        }


        public async Task ForceReloadAsync()
        {
            await LoadInitialDataAsync();
            await LoadSidebar();
        }

        private void SafeInvoke(Action action)
        {
            if (InvokeRequired) Invoke(action);
            else action();
        }

        private void CalendarV2_Resize(object sender, EventArgs e)
        {
            _resizeTimer.Stop();
            _resizeTimer.Start();
        }

        private void CalendarV2_Load(object sender, EventArgs e)
        {
            InitializeContextMenus();

            panelContainer.Scroll += PanelContainer_Scroll;
            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);
            _ = LoadInitialDataAsync();
        }


        private void PanelContainer_Scroll(object? sender, ScrollEventArgs e)
        {
            userHasScrolled = true;
        }

        private async Task LoadInitialDataAsync()
        {
            await Task.WhenAll(
                LoadTimeEntryTypesAsync(DefaultProjectType),
                LoadTimeEntrySubTypesAsync(),
                LoadProjectsAsync(DefaultProjectType),
                LoadSpecialDaysAsync(),
                LoadArrivalDeparturesAsync(),
                RenderCalendar(),
                AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate)
            );
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
            await LoadArrivalDeparturesAsync();
            await RenderCalendar();
            await AdjustIndicatorsAsync(panelContainer.AutoScrollPosition, _selectedUser.Id, _selectedDate);

            // Reset vybraného záznamu a sidebaru po změně uživatele
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
                Font = new Font(this.Font.FontFamily, 9.0f)
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

        private async Task LoadSidebar()
        {
            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            if (timeEntry.ProjectId == 132 && timeEntry.EntryTypeId == 24)
            {
                flowLayoutPanel2.Visible = false;
            }

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;

            flowLayoutPanel2.Enabled = timeEntry.IsLocked == 0;

            if (timeEntry.IsValid == 1)
            {
                checkBoxArchivedProjects.Checked = timeEntry.Project.IsArchived == 1;

                var proj = await _projectRepo.GetProjectByIdAsync(timeEntry.ProjectId ?? 0);
                if (proj == null) return;

                await LoadTimeEntryTypesAsync(proj.ProjectType);

                // Speciální projekty – override radio button
                switch (proj.Id)
                {
                    case 25:
                        SelectRadioButtonByText("OSTATNÍ");
                        break;
                    case 23:
                        SelectRadioButtonByText("NEPŘÍTOMNOST");
                        break;
                    case 26:
                        SelectRadioButtonByText("ŠKOLENÍ");
                        break;
                    default:
                        int index = proj.ProjectType + 1;

                        // Po sloučení projektů a zakázek mají společnou záložku
                        if (index == 2 || index == 3)
                            index = 2;

                        if (flowLayoutPanel2.Controls.Find($"radioButton{index}", false).FirstOrDefault() is RadioButton rb)
                            rb.Checked = true;
                        break;
                }

                BeginInvoke((Action)(() =>
                {
                    comboBoxStart.SelectedIndex = minutesStart / 30;
                    comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

                    if (lastPanel?.EntryId != -1)
                    {
                        customComboBoxSubTypes.SetText(timeEntry.Description);
                        customComboBoxProjects.SetText(FormatHelper.FormatProjectToString(timeEntry.Project));
                        textBoxNote.Text = timeEntry.Note;

                        // Výběr EntryType podle ProjectType
                        if (proj.ProjectType is 0 or 1 or 2)
                        {
                            int baseId = proj.ProjectType switch
                            {
                                0 => 1,
                                1 => 10,
                                2 => 13,
                                _ => 0
                            };

                            int radioIndex = (int)(timeEntry.EntryTypeId - baseId);
                            var radioPanel = tableLayoutPanelEntryType.Controls
                                .OfType<TableLayoutPanel>()
                                .FirstOrDefault();

                            var radios = radioPanel?.Controls.OfType<RadioButton>().ToList();
                            if (radios != null && radioIndex >= 0 && radioIndex < radios.Count)
                            {
                                radios[radioIndex].Checked = true;
                            }
                        }
                        else
                        {
                            var selectedType = _timeEntryTypes.FirstOrDefault(x => x.Id == timeEntry.EntryTypeId);
                            comboBoxEntryType.Text = timeEntry.AfterCare == 1
                                ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(selectedType)
                                : FormatHelper.FormatTimeEntryTypeToString(selectedType);
                        }
                    }

                }));
            }
            else
            {
                BeginInvoke((Action)(() =>
                {
                    comboBoxStart.SelectedIndex = minutesStart / 30;
                    comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

                    customComboBoxSubTypes.SetText(string.Empty);
                    textBoxNote.Text = string.Empty;
                    //comboBoxProjects.Text = string.Empty;
                    comboBoxEntryType.Text = string.Empty;

                    foreach (var radio in flowLayoutPanel2.Controls.OfType<RadioButton>())
                    {
                        radio.Checked = false;
                    }

                    tableLayoutPanel4.Visible = false;
                    tableLayoutPanel6.Visible = false;
                    tableLayoutPanelProject.Visible = false;
                    tableLayoutPanelEntryType.Visible = false;
                    tableLayoutPanelEntrySubType.Visible = false;
                    panel4.Visible = false;

                }));
            }
        }


        private void SelectRadioButtonByText(string text)
        {
            var rb = flowLayoutPanel2.Controls
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));

            if (rb != null) rb.Checked = true;
        }



        private TableLayoutPanelCellPosition GetCellAt(TableLayoutPanel panel, Point clickPosition)
        {
            int width = panel.Width / panel.ColumnCount;
            int height = panel.Height / panel.RowCount;

            int col = Math.Min(clickPosition.X / width, panel.ColumnCount - 1);
            int row = Math.Min(clickPosition.Y / height, panel.RowCount - 1);

            return new TableLayoutPanelCellPosition(col, row);
        }

        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            var cell = GetCellAt(tableLayoutPanelCalendar, e.Location);
            pasteTargetCell = cell;
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;
            _ = LoadSidebar();
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pasteTargetCell = GetCellAt(tableLayoutPanelCalendar, e.Location);

                if (tableLayoutMenu.Items.Count > 0)
                {
                    var pasteItem = tableLayoutMenu.Items[0];
                    pasteItem.Enabled = copiedEntry != null;
                }

                tableLayoutMenu.Show(tableLayoutPanelCalendar, e.Location);
            }
        }

        private async void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TableLayoutPanelCellPosition cell = GetCellAt(tableLayoutPanelCalendar, e.Location);

            if (_projects.Count == 0 || _timeEntryTypes.Count == 0)
            {
                AppLogger.Error("Nelze vytvořit záznam: nejsou načteny projekty nebo typy záznamů.");
                return;
            }

            int column = cell.Column;
            int row = cell.Row;
            int span = 1;

            // Najdi první volné místo od zadané pozice doprava
            while (column + span <= tableLayoutPanelCalendar.ColumnCount)
            {
                bool overlapping = panels.Any(p =>
                {
                    int r = tableLayoutPanelCalendar.GetRow(p);
                    if (r != row) return false;
                    int c = tableLayoutPanelCalendar.GetColumn(p);
                    int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                    return !(column + span - 1 < c || column > c + s - 1);
                });

                if (!overlapping) break;
                column++;
            }

            if (column + span > tableLayoutPanelCalendar.ColumnCount)
            {
                AppLogger.Error("V daném řádku už není místo pro nový záznam.");
                return;
            }

            DateTime clickedDate = _selectedDate.AddDays(row).AddMinutes(column * 30);

            var specialDay = _specialDays.Find(x => x.Date.Date == clickedDate.Date);
            if (specialDay is not null)
            {
                if (specialDay.Locked) return;
            }

            var newTimeEntry = new TimeEntry()
            {
                ProjectId = _projects[0].Id,
                EntryTypeId = _timeEntryTypes[0].Id,
                UserId = _selectedUser.Id,
                Timestamp = clickedDate,
                EntryMinutes = 30
            };

            if (customComboBoxProjects.SelectedIndex > -1)
            {
                newTimeEntry.ProjectId = _projects[(customComboBoxProjects.SelectedIndex == -1 ? 0 : customComboBoxProjects.SelectedIndex)].Id;
            }

            newTimeEntry.AfterCare = _projects.Find(x => x.Id == newTimeEntry.ProjectId).IsArchived;
            newTimeEntry.IsLocked = 0;

            var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
            if (addedTimeEntry == null)
            {
                AppLogger.Error("Chyba při vytváření nového časového záznamu.");
                return;
            }

            _selectedTimeEntryId = addedTimeEntry.Id;
            await RenderCalendar();
            await LoadSidebar();
        }


        private async Task RenderCalendar()
        {
            tableLayoutPanelCalendar.SuspendLayout();
            panelContainer.SuspendLayout();

            tableLayoutPanelCalendar.SetDate(_selectedDate);
            var scrollPosition = panelContainer.AutoScrollPosition;
            panelContainer.AutoScroll = false;
            _loadingUC.BringToFront();

            var currentUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName);
            bool isCurrentUser = _selectedUser.WindowsUsername == currentUser.WindowsUsername;
            tableLayoutPanelCalendar.Enabled = isCurrentUser;
            flowLayoutPanel2.Enabled = isCurrentUser;

            tableLayoutPanelCalendar.Controls.Clear();
            panels.Clear();

            var allProjects = await _projectRepo.GetAllProjectsAsync();
            var projectDict = allProjects.ToDictionary(p => p.Id);
            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();


            // 1. Načtení speciálních dnů
            await LoadSpecialDaysAsync();
            tableLayoutPanelCalendar.SetSpecialDays(_specialDays);

            // 2. Vytvoření chybějících "svačin"
            for (int row = 0; row < 7; row++)
            {
                DateTime day = _selectedDate.AddDays(row);
                bool exists = await _timeEntryRepo.ExistsEntryAsync(_selectedUser.Id, day, 132, 24);
                if (!exists)
                {
                    var defaultSnackTime = day.AddMinutes(18 * 30); // 9:00
                    var newSnack = new TimeEntry
                    {
                        ProjectId = 132,
                        EntryTypeId = 24,
                        UserId = _selectedUser.Id,
                        Timestamp = defaultSnackTime,
                        EntryMinutes = 30,
                        IsValid = 1,
                        IsLocked = 1
                    };
                    await _timeEntryRepo.CreateTimeEntryAsync(newSnack);
                }
            }

            // 3. Znovu načti všechny záznamy (včetně právě vytvořených)
            _currentEntries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);


            // 4. Vykresli panely
            foreach (var entry in _currentEntries)
            {
                if (entry.Project == null && entry.ProjectId.HasValue && projectDict.TryGetValue(entry.ProjectId.Value, out var proj))
                    entry.Project = proj;

                await CreatePanelForEntry(entry, allEntryTypes);
            }

            BeginInvoke((Action)(() =>
            {
                UpdateDateLabels();
                UpdateHourLabels();
                panelContainer.AutoScroll = true;

                if (!userHasScrolled)
                {
                    int[] columnWidths = tableLayoutPanelCalendar.GetColumnWidths();
                    int currentHourColumn = (DateTime.Now.Hour * 60 + DateTime.Now.Minute) / 30;
                    int scrollX = columnWidths.Take(currentHourColumn).Sum();
                    scrollX -= panelContainer.ClientSize.Width / 2;
                    scrollX = Math.Max(scrollX, 0);
                    panelContainer.AutoScrollPosition = new Point(scrollX, 0);
                }

                DeactivateAllPanels();

                var panelToActivate = tableLayoutPanelCalendar.Controls
                    .OfType<DayPanel>()
                    .FirstOrDefault(p => p.EntryId == _selectedTimeEntryId);
                panelToActivate?.Activate();

                tableLayoutPanelCalendar.ResumeLayout(true);
                panelContainer.ResumeLayout(true);
                _loadingUC.Visible = false;
            }));
        }




        private void UpdateHourLabels()
        {
            Label[] hourLabels = { labelHours01, labelHours02, labelHours03, labelHours04, labelHours05, labelHours06, labelHours07 };

            for (int row = 0; row < 7; row++)
            {
                DateTime day = _selectedDate.AddDays(row);

                int totalMinutes = _currentEntries
                    .Where(entry =>
                        entry.Timestamp.HasValue &&
                        entry.Timestamp.Value.Date == day.Date &&
                        entry.IsValid == 1 &&
                        !(entry.ProjectId == 132 && entry.EntryTypeId == 24) && // není svačina
                        !(entry.ProjectId == 23)) // není nepřítomnost
                    .Sum(entry => entry.EntryMinutes);

                double vykazanoHodin = totalMinutes / 60.0;

                var dochazka = _arrivalDepartures.FirstOrDefault(a => a.WorkDate.Date == day.Date);
                double hoursWorked = 0;

                if (dochazka != null)
                {
                    hoursWorked = dochazka.HoursWorked;
                }

                switch (_config.PanelDayView)
                {
                    case PanelDayView.Default:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";
                        hourLabels[row].ForeColor = Color.Black;
                        break;
                    case PanelDayView.Range:
                        hourLabels[row].Text = $"{vykazanoHodin:F1} / {hoursWorked:F1} h";
                        hourLabels[row].ForeColor = Color.Black;
                        break;
                    case PanelDayView.ColorWithinRange:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";

                        if (Math.Abs(vykazanoHodin - hoursWorked) < 0.01)
                            hourLabels[row].ForeColor = Color.Green;
                        else
                            hourLabels[row].ForeColor = Color.Red;
                        break;
                    case PanelDayView.ColorOvertime:
                        hourLabels[row].Text = $"{vykazanoHodin:F1}";

                        if (vykazanoHodin == 7.5)
                            hourLabels[row].ForeColor = Color.Green;
                        else if(vykazanoHodin > 7.5)
                            hourLabels[row].ForeColor = Color.Blue;
                        else
                            hourLabels[row].ForeColor = Color.Red;
                        break;
                }

                if (dochazka == null)
                {
                    hourLabels[row].ForeColor = Color.Black;
                }
            }
        }



        private int GetColumnBasedOnTimeEntry(DateTime? timeStamp)
        {
            var minutes = timeStamp.Value.Hour * 60 + timeStamp.Value.Minute;
            return minutes / TimeSlotLengthInMinutes;
        }

        private int GetColumnSpanBasedOnTimeEntry(int entryMinutes)
        {
            return Math.Max(1, entryMinutes / TimeSlotLengthInMinutes);
        }

        private int GetRowBasedOnTimeEntry(DateTime? timeStamp)
        {
            return ((int)timeStamp.Value.DayOfWeek + 6) % 7;
        }

        private void UpdateDateLabels()
        {
            Color special = Color.FromArgb(255, 98, 92);
            Color regular = Color.FromArgb(0, 0, 0);

            Label[] dateLabels = { labelDate01, labelDate02, labelDate03, labelDate04, labelDate05, labelDate06, labelDate07 };
            Label[] dayLabels = { labelDay01, labelDay02, labelDay03, labelDay04, labelDay05, labelDay06, labelDay07 };

            for (int i = 0; i < 7; i++)
            {
                DateTime date = _selectedDate.AddDays(i);
                bool isSpecial = _specialDays.Any(x => x.Date.Date == date);

                dateLabels[i].Text = date.ToString("d.M.yyyy");
                dateLabels[i].ForeColor = isSpecial ? special : regular;
                dayLabels[i].ForeColor = isSpecial ? special : regular;
            }
        }


        private async Task AdjustIndicatorsAsync(Point scrollPosition, int userId, DateTime weekStart)
        {
            var oldIndicators = panelContainer.Controls.OfType<Panel>().Where(p => p.Name == "indicator").ToList();
            foreach (var ctrl in oldIndicators)
            {
                panelContainer.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            var arrivalDepartures = await _arrivalDepartureRepo.GetWeekEntriesForUserAsync(userId, weekStart);

            int[] rowHeights = tableLayoutPanelCalendar.GetRowHeights();
            int[] columnWidths = tableLayoutPanelCalendar.GetColumnWidths();
            int[] headerRowHeights = customTableLayoutPanel1.GetRowHeights();
            int minutesPerColumn = 30;

            var toolTip = new ToolTip();

            foreach (var arrivalDeparture in arrivalDepartures)
            {
                if (!arrivalDeparture.ArrivalTimestamp.HasValue || !arrivalDeparture.DepartureTimestamp.HasValue)
                    continue;

                TimeSpan rawArrival = arrivalDeparture.ArrivalTimestamp.Value.TimeOfDay;
                TimeSpan rawDeparture = arrivalDeparture.DepartureTimestamp.Value.TimeOfDay;

                (TimeSpan roundedArrival, TimeSpan roundedDeparture) = RoundWorkTimeToNearestHalfHour(rawArrival, rawDeparture);

                int arrivalCol = GetColumnIndexFromTime(roundedArrival, minutesPerColumn);
                int leaveCol = GetColumnIndexFromTime(roundedDeparture, minutesPerColumn);

                int dayIndex = ((int)arrivalDeparture.WorkDate.DayOfWeek - 1 + 7) % 7;
                int rowHeight = (dayIndex < rowHeights.Length) ? rowHeights[dayIndex] : 69;
                int yPos = rowHeights.Take(dayIndex).Sum() + headerRowHeights[0];

                int arrivalX = (columnWidths[0] * arrivalCol) - Math.Abs(scrollPosition.X);
                int leaveX = (columnWidths[0] * leaveCol) - Math.Abs(scrollPosition.X);

                var arrivalIndicator = new Panel
                {
                    Name = "indicator",
                    Size = new Size(2, rowHeight),
                    Location = new Point(arrivalX, yPos),
                    BackColor = Color.Green
                };
                toolTip.SetToolTip(arrivalIndicator, $"{rawArrival:hh\\:mm}\n-\n{rawDeparture:hh\\:mm}");

                var leaveIndicator = new Panel
                {
                    Name = "indicator",
                    Size = new Size(2, rowHeight),
                    Location = new Point(leaveX, yPos),
                    BackColor = Color.Red
                };
                toolTip.SetToolTip(leaveIndicator, $"{rawArrival:hh\\:mm}\n-\n{rawDeparture:hh\\:mm}");

                panelContainer.Controls.Add(arrivalIndicator);
                panelContainer.Controls.Add(leaveIndicator);
                arrivalIndicator.BringToFront();
                leaveIndicator.BringToFront();
            }
        }


        private (TimeSpan, TimeSpan) RoundWorkTimeToNearestHalfHour(TimeSpan rawArrival, TimeSpan rawDeparture)
        {
            double arrivalMinutes = rawArrival.TotalMinutes;
            double roundedArrivalMinutes = Math.Round(arrivalMinutes / 30.0) * 30;
            var roundedArrival = TimeSpan.FromMinutes(roundedArrivalMinutes);

            var realDuration = rawDeparture - rawArrival;

            double roundedDurationMinutes = Math.Floor(realDuration.TotalMinutes / 30.0) * 30;
            var roundedDeparture = roundedArrival + TimeSpan.FromMinutes(roundedDurationMinutes);

            return (roundedArrival, roundedDeparture);
        }

        private int GetColumnIndexFromTime(TimeSpan timeOfDay, int minutesPerColumn)
        {
            return (int)(timeOfDay.TotalMinutes / minutesPerColumn);
        }

        private void AttachTooltipToPanel(DayPanel panel, TimeEntry entry)
        {
            if (entry.IsValid == 1)
            {
                string projectName = entry.Project?.ProjectTitle ?? "Projekt neznámý";
                string note = string.IsNullOrWhiteSpace(entry.Note) ? "Bez poznámky" : entry.Note;

                var tooltip = new ToolTip()
                {
                    AutoPopDelay = 5000,
                    InitialDelay = 300,
                    ReshowDelay = 100,
                    ShowAlways = true
                };

                string text = $"{projectName}\n{note}";
                tooltip.SetToolTip(panel, text);
            }
        }

        private async Task CreatePanelForEntry(TimeEntry entry, List<TimeEntryType> entryTypes)
        {
            if (entry.Project == null) return;

            var entryType = entryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);
            string color = entryType?.Color ?? "#ADD8E6";

            if (entry.IsValid == 0)
                color = "#FF6957";

            var panel = new DayPanel
            {
                Dock = DockStyle.Fill,
                BackColor = ColorTranslator.FromHtml(color),
                BorderStyle = BorderStyle.FixedSingle,
                EntryId = entry.Id
            };

            if (entry.IsValid == 1)
            {
                panel.UpdateUi(
                    (entry.Project?.IsArchived == 1 ? "(AFTERCARE) " : "") +
                    (entry.Project.ProjectType == 1 ? entry.Project.ProjectDescription : entry.Project.ProjectTitle),
                    entry.Description);
            }

            panel.ContextMenuStrip = dayPanelMenu;

            panel.MouseMove += dayPanel_MouseMove;
            panel.MouseDown += dayPanel_MouseDown;
            panel.MouseUp += dayPanel_MouseUp;
            panel.MouseLeave += dayPanel_MouseLeave;
            panel.MouseClick += dayPanel_MouseClick;

            AttachTooltipToPanel(panel, entry);

            int column = GetColumnBasedOnTimeEntry(entry.Timestamp);
            int row = GetRowBasedOnTimeEntry(entry.Timestamp);
            int columnSpan = GetColumnSpanBasedOnTimeEntry(entry.EntryMinutes);

            tableLayoutPanelCalendar.Controls.Add(panel, column, row);
            tableLayoutPanelCalendar.SetColumnSpan(panel, columnSpan);
            panel.Tag = (entry.ProjectId == 132 && entry.EntryTypeId == 24) ? "snack" : entry.IsLocked == 1 ? "locked" : null;

            panels.Add(panel);
        }



        #region DayPanel events
        private void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (mouseMoved) return;

            if (sender is not DayPanel panel) return;

            DeactivateAllPanels();
            panel.Activate();

            pasteTargetCell = new TableLayoutPanelCellPosition(
                tableLayoutPanelCalendar.GetColumn(panel),
                tableLayoutPanelCalendar.GetRow(panel)
            );

            tableLayoutPanelCalendar.ClearSelection();
        }


        private void DeactivateAllPanels()
        {
            foreach (var ctrl in tableLayoutPanelCalendar.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    pan.Deactivate();
                }
            }
        }

        private void dayPanel_MouseMove(object? sender, MouseEventArgs e)
        {


            if (sender is not DayPanel panel) return;

            int rowHeight = tableLayoutPanelCalendar.Height / tableLayoutPanelCalendar.RowCount;
            int currentMouseY = tableLayoutPanelCalendar.PointToClient(Cursor.Position).Y;
            int newRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanelCalendar.RowCount - 1));

            int currentMouseX = Cursor.Position.X;
            int deltaX = currentMouseX - startMouseX;
            int columnWidth = tableLayoutPanelCalendar.Width / tableLayoutPanelCalendar.ColumnCount;

            if (isResizing && activePanel == panel)
            {
                HandleResize(panel, deltaX, columnWidth);
            }
            else if (isMoving && activePanel == panel)
            {
                HandleMove(panel, deltaX, columnWidth);
            }
            else
            {
                UpdateCursor(e, panel);
            }
        }


        private bool mouseMoved = false;

        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (panel.Tag as string == "locked")
                return;

            mouseMoved = false;
            DeactivateAllPanels();
            panel.Activate();

            isResizing = Cursor == Cursors.SizeWE;
            isMoving = !isResizing;

            activePanel = panel;
            startMouseX = Cursor.Position.X;
            originalColumn = tableLayoutPanelCalendar.GetColumn(panel);
            originalColumnSpan = tableLayoutPanelCalendar.GetColumnSpan(panel);

            panel.Capture = true;

            if (panel.Tag as string == "snack")
                return;

            panel.BackColor = Color.LightCoral;
        }


        private void dayPanel_MouseLeave(object? sender, EventArgs e)
        {
            if (!isResizing && !isMoving)
            {
                Cursor = Cursors.Default;
            }
        }


        private void HandleResize(DayPanel panel, int deltaX, int columnWidth)
        {
            if (isResizingLeft)
            {
                int newColumn = originalColumn + deltaX / columnWidth;
                int newSpan = originalColumnSpan - (newColumn - originalColumn);
                int minColumn = GetNearestLeftColumn(originalColumn, tableLayoutPanelCalendar.GetRow(panel), panel);

                if (newColumn >= minColumn && newSpan > 0 && newColumn + newSpan <= tableLayoutPanelCalendar.ColumnCount && !IsOverlapping(newColumn, newSpan, tableLayoutPanelCalendar.GetRow(panel), panel))
                {
                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumn(panel, newColumn);
                    tableLayoutPanelCalendar.SetColumnSpan(panel, newSpan);
                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
            else
            {
                int newSpan = originalColumnSpan + deltaX / columnWidth;
                int maxSpan = GetNearestRightColumn(originalColumn, originalColumnSpan, tableLayoutPanelCalendar.GetRow(panel), panel);

                if (newSpan > 0 && originalColumn + newSpan <= maxSpan && !IsOverlapping(originalColumn, newSpan, tableLayoutPanelCalendar.GetRow(panel), panel))
                {
                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumnSpan(panel, newSpan);
                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
        }

        private void HandleMove(DayPanel panel, int deltaX, int columnWidth)
        {
            int targetColumn = originalColumn + deltaX / columnWidth;
            int rowHeight = tableLayoutPanelCalendar.Height / tableLayoutPanelCalendar.RowCount;
            int currentMouseY = tableLayoutPanelCalendar.PointToClient(Cursor.Position).Y;
            int targetRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanelCalendar.RowCount - 1));

            int span = originalColumnSpan;

            // Kontrola rozsahu tabulky
            if (targetColumn < 0 || targetColumn + span > tableLayoutPanelCalendar.ColumnCount)
                return;

            if (!IsOverlapping(targetColumn, span, targetRow, panel))
            {
                // Zjisti aktuální pozici panelu
                int currentColumn = tableLayoutPanelCalendar.GetColumn(panel);
                int currentRow = tableLayoutPanelCalendar.GetRow(panel);

                // Pokud se opravdu mění pozice
                bool hasMoved = currentColumn != targetColumn || currentRow != targetRow;

                if (hasMoved)
                {
                    tableLayoutPanelCalendar.SuspendLayout();

                    tableLayoutPanelCalendar.SetColumn(panel, targetColumn);
                    if (panel.Tag as string != "snack")
                        tableLayoutPanelCalendar.SetRow(panel, targetRow);
                    tableLayoutPanelCalendar.SetColumnSpan(panel, span);

                    mouseMoved = true;

                    tableLayoutPanelCalendar.ResumeLayout();
                }
            }
        }


        private void UpdateCursor(MouseEventArgs e, DayPanel panel)
        {
            if (panel.Tag as string == "snack")
            {
                Cursor = Cursors.SizeAll;
                return;
            }

            else if (panel.Tag as string == "locked")
                return;

            if (e.X <= ResizeThreshold)
            {
                Cursor = Cursors.SizeWE;
                isResizingLeft = true;
            }
            else if (e.X >= panel.Width - ResizeThreshold)
            {
                Cursor = Cursors.SizeWE;
                isResizingLeft = false;
            }
            else
            {
                Cursor = Cursors.SizeAll;
            }
        }

        private async void dayPanel_MouseUp(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            mouseMoved = false;
            isResizing = false;
            isMoving = false;
            activePanel = null;
            Cursor = Cursors.Default;

            var previousTimeEntryId = _selectedTimeEntryId;
            _selectedTimeEntryId = panel.EntryId;

            var allEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();

            var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (entry == null) return;

            var newTimestamp = _selectedDate
                .AddDays(tableLayoutPanelCalendar.GetRow(panel))
                .AddMinutes(tableLayoutPanelCalendar.GetColumn(panel) * TimeSlotLengthInMinutes);

            var newDuration = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanelCalendar.GetColumnSpan(panel));

            if (entry.Timestamp != newTimestamp || entry.EntryMinutes != newDuration)
            {
                entry.Timestamp = newTimestamp;
                entry.EntryMinutes = newDuration;
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);

                UpdateHourLabels();
            }

            var entryType = allEntryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);
            string color = entryType?.Color ?? "#ADD8E6";

            if (entry.IsValid == 0) color = "#FF6957";
            panel.BackColor = ColorTranslator.FromHtml(color);

            int minutesStart = newTimestamp.Hour * 60 + newTimestamp.Minute;
            int minutesEnd = minutesStart + entry.EntryMinutes;

            comboBoxStart.SelectedIndex = minutesStart / 30;
            comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

            if (_selectedTimeEntryId != previousTimeEntryId)
            {
                await LoadSidebar();
            }
        }

        #endregion

        private bool IsOverlapping(int column, int span, int row, DayPanel currentPanel)
        {
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel) continue;

                int pRow = tableLayoutPanelCalendar.GetRow(p);
                if (pRow != row) continue;

                int pCol = tableLayoutPanelCalendar.GetColumn(p);
                int pSpan = tableLayoutPanelCalendar.GetColumnSpan(p);

                int pEnd = pCol + pSpan - 1;
                int thisEnd = column + span - 1;

                bool overlaps = !(thisEnd < pCol || column > pEnd);
                if (overlaps) return true;
            }

            return false;
        }

        private int GetNearestLeftColumn(int currentColumn, int row, DayPanel currentPanel)
        {
            int minColumn = 0;
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanelCalendar.GetRow(p) != row) continue;

                int pCol = tableLayoutPanelCalendar.GetColumn(p);
                int pSpan = tableLayoutPanelCalendar.GetColumnSpan(p);
                int rightEdge = pCol + pSpan;

                if (rightEdge <= currentColumn)
                {
                    minColumn = Math.Max(minColumn, rightEdge);
                }
            }
            return minColumn;
        }

        private int GetNearestRightColumn(int currentColumn, int currentSpan, int row, DayPanel currentPanel)
        {
            int maxColumn = tableLayoutPanelCalendar.ColumnCount;
            int panelRightEdge = currentColumn + currentSpan;

            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanelCalendar.GetRow(p) != row) continue;

                int pCol = tableLayoutPanelCalendar.GetColumn(p);
                if (pCol >= panelRightEdge)
                {
                    maxColumn = Math.Min(maxColumn, pCol);
                }
            }
            return maxColumn;
        }

        private DateTime GetTimestampBasedOnColumn(int column)
        {
            int totalMinutes = column * 30;
            return _selectedDate.AddMinutes(totalMinutes);
        }

        private int GetEntryMinutesBasedOnColumnSpan(int columnSpan)
        {
            return columnSpan * 30;
        }

        #region Timespans
        private int GetNumberOfMinutesFromDateSpan(DateTime start, DateTime end)
        {
            return (int)(end - start).TotalMinutes;
        }

        private void comboBoxEnd_SelectionChangeCommitted(object sender, EventArgs e)
        {
            //if (comboBoxStart.SelectedIndex > 0)
            //{
            //    if (TimeDifferenceOutOfRange())
            //    {
            //        comboBoxEnd.SelectedIndex = comboBoxStart.SelectedIndex + 1;
            //    }
            //}
        }

        private void comboBoxStart_SelectedIndexChanged(object sender, EventArgs e)
        {
            //if (comboBoxEnd.SelectedIndex > 0)
            //{
            //    if (TimeDifferenceOutOfRange())
            //    {
            //        comboBoxStart.SelectedIndex = comboBoxEnd.SelectedIndex - 1;
            //    }
            //}
        }

        private bool TimeDifferenceOutOfRange()
        {
            int minutesStart = comboBoxStart.SelectedIndex * 30;
            int minutesEnd = comboBoxEnd.SelectedIndex * 30;

            return minutesEnd - minutesStart <= 0;
        }
        #endregion

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            var (valid, reason) = CheckForEmptyOrIncorrectFields();
            if (!valid)
            {
                AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                return;
            }

            int selectedEntryTypeId = 0;

            if (_currentProjectType is 0 or 1 or 2)
            {
                // Projektový typ s radio buttony
                var radioButtons = tableLayoutPanelEntryType.Controls
                    .OfType<TableLayoutPanel>()
                    .SelectMany(panel => panel.Controls.OfType<RadioButton>())
                    .ToList();

                for (int i = 0; i < radioButtons.Count; i++)
                {
                    if (radioButtons[i].Checked)
                    {
                        selectedEntryTypeId = _currentProjectType switch
                        {
                            0 => 1 + i,
                            1 => 10 + i,
                            2 => 13 + i,
                            _ => 0
                        };
                        break;
                    }
                }
            }
            else if (comboBoxEntryType.SelectedIndex > -1)
            {
                selectedEntryTypeId = _timeEntryTypes[comboBoxEntryType.SelectedIndex].Id;
            }

            var newSubType = new TimeEntrySubType
            {
                Title = customComboBoxSubTypes.GetText(),
                UserId = _selectedUser.Id
            };

            var addedTimeEntrySubType = await _timeEntrySubTypeRepo.CreateTimeEntrySubTypeAsync(newSubType);

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            timeEntry.Description = addedTimeEntrySubType.Title;
            timeEntry.EntryTypeId = selectedEntryTypeId;
            timeEntry.Note = textBoxNote.Text;

            var selectedProject = _projects.FirstOrDefault(p =>
                FormatHelper.FormatProjectToString(p).Equals(customComboBoxProjects.SelectedItem, StringComparison.InvariantCultureIgnoreCase));

            if (selectedProject != null)
            {
                timeEntry.ProjectId = selectedProject.Id;
            }

            if (radioButton5.Checked)
            {
                timeEntry.ProjectId = 25;
            }
            else if (radioButton4.Checked)
            {
                timeEntry.ProjectId = 23;
            }
            else if (radioButton3.Checked)
            {
                timeEntry.ProjectId = 26;
                timeEntry.EntryTypeId = 16;
            }

            timeEntry.AfterCare = _projects.FirstOrDefault(x => x.Id == timeEntry.ProjectId)?.IsArchived ?? 0;
            timeEntry.IsValid = 1;

            bool success = await _timeEntryRepo.UpdateTimeEntryAsync(timeEntry);
            if (success)
            {
                AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl úspěšně aktualizován.");
                await LoadTimeEntrySubTypesAsync();
                await RenderCalendar();
                await LoadSidebar();
            }
        }

        private (bool valid, string reason) CheckForEmptyOrIncorrectFields()
        {
            var rb = flowLayoutPanel2.Controls
               .OfType<RadioButton>()
               .FirstOrDefault(r => r.Checked);

            bool ProjectTextMatches = _projects.Any(p =>
                FormatHelper.FormatProjectToString(p).Equals(customComboBoxProjects.SelectedItem, StringComparison.InvariantCultureIgnoreCase));

            bool EntryTypeMatches = _timeEntryTypes.Any(t =>
                FormatHelper.FormatTimeEntryTypeToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase) ||
                FormatHelper.FormatTimeEntryTypeWithAfterCareToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase));

            switch (rb?.Text)
            {
                case "PROVOZ":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Nákladové středisko neodpovídá žádné možnosti");
                    break;
                case "PROJEKT":
                case "PŘEDPROJEKT":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Projekt neodpovídá žádné možnosti");
                    break;
                case "ŠKOLENÍ":
                    if (string.IsNullOrWhiteSpace(textBoxNote.Text))
                        return (false, "Poznámka");
                    break;
                case "NEPŘÍTOMNOST":
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Důvod neodpovídá žádné možnosti");
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Činnost neodpovídá žádné možnosti");
                    break;
            }

            return (true, "");
        }



        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            await DeleteRecord();
        }

        public async Task DeleteRecord()
        {
            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            // zamčeno
            if (timeEntry.IsLocked == 1) return;

            // svačina
            if (timeEntry.ProjectId == 132 && timeEntry.EntryTypeId == 24) return;

            bool confirmed = ShowDeleteConfirmation(timeEntry);
            if (!confirmed) return;

            bool success = await _timeEntryRepo.DeleteTimeEntryAsync(_selectedTimeEntryId);
            if (success)
            {
                AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl smazán z databáze.");
            }
            else
            {
                AppLogger.Error($"Nepodařilo se smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} z databáze.");
            }

            _selectedTimeEntryId = -1;
            await RenderCalendar();
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

        private async void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                int index = 0;
                label4.Text = "Poznámka";

                tableLayoutPanel4.Visible = true;
                tableLayoutPanel6.Visible = true;
                tableLayoutPanelProject.Visible = true;
                tableLayoutPanelEntryType.Visible = true;
                tableLayoutPanelEntrySubType.Visible = true;
                customComboBoxSubTypes.SetText(string.Empty);
                customComboBoxProjects.SetText(string.Empty);
                panel4.Visible = true;

                switch (rb.Text)
                {
                    case "PROVOZ":
                        index = 0;
                        labelProject.Text = "Nákladové středisko*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        break;
                    case "PROJEKT":
                        index = 1;
                        labelProject.Text = "Projekt*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = true;
                        break;
                    case "ŠKOLENÍ":
                        index = 3;
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = false;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        label4.Text = "Poznámka*";
                        break;
                    case "NEPŘÍTOMNOST":
                        labelType.Text = "Důvod*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        index = 4;
                        break;
                    default:
                        index = 5;
                        labelType.Text = "Činnost*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        break;
                }

                await LoadProjectsAsync(index);
                await LoadTimeEntryTypesAsync(index);
            }
        }

        private async void checkBoxArchivedProjects_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(1);
            await LoadTimeEntryTypesAsync(1);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            // Zjisti aktuálně fokusovaný prvek uvnitř tohoto UserControl
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




        private async void CopySelectedPanel()
        {
            if (_selectedTimeEntryId <= 0) return;

            var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);


            // svačina
            if (entry.ProjectId == 132 && entry.EntryTypeId == 24) return;

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

        private async void PasteCopiedPanel()
        {
            if (copiedEntry == null || pasteTargetCell == null) return;

            int column = pasteTargetCell.Value.Column;
            int row = pasteTargetCell.Value.Row;

            int span = copiedEntry.EntryMinutes / TimeSlotLengthInMinutes;
            int lastColumn = column + span - 1;

            if (lastColumn >= tableLayoutPanelCalendar.ColumnCount)
            {
                MessageBox.Show("Záznam nelze vložit, nevejde se do daného dne.", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool overlapping = panels.Any(p =>
            {
                int r = tableLayoutPanelCalendar.GetRow(p);
                int c = tableLayoutPanelCalendar.GetColumn(p);
                int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                return r == row && !(column + span - 1 < c || column > c + s - 1);
            });

            if (overlapping)
            {
                var dialog = new ReplaceOrMoveDialog().ShowDialog();

                if (dialog == DialogResult.Cancel) return;

                if (dialog == DialogResult.No)
                {
                    bool success = await ShiftRightFrom(column, row, span);
                    if (!success) return;
                }
                else if (dialog == DialogResult.Yes)
                {
                    await RemoveOverlappingPanels(column, span, row);
                }
            }

            DateTime newTimestamp = _selectedDate
                .AddDays(row)
                .AddMinutes(column * TimeSlotLengthInMinutes);

            var specialDay = _specialDays.Find(x => x.Date.Date == newTimestamp.Date.Date);
            if (specialDay != null)
            {
                if (specialDay.Locked) return;
            }

            var newEntry = new TimeEntry
            {
                EntryTypeId = copiedEntry.EntryTypeId,
                ProjectId = copiedEntry.ProjectId,
                Description = copiedEntry.Description,
                Note = copiedEntry.Note,
                EntryMinutes = copiedEntry.EntryMinutes,
                AfterCare = copiedEntry.AfterCare,
                UserId = _selectedUser.Id,
                Timestamp = newTimestamp,
                IsValid = copiedEntry.IsValid
            };

            var created = await _timeEntryRepo.CreateTimeEntryAsync(newEntry);
            if (created != null)
            {
                _selectedTimeEntryId = created.Id;
                await RenderCalendar();
                await LoadSidebar();
            }
        }

        private async Task RemoveOverlappingPanels(int fromCol, int span, int row)
        {
            var toRemove = panels.Where(p =>
            {
                int r = tableLayoutPanelCalendar.GetRow(p);
                int c = tableLayoutPanelCalendar.GetColumn(p);
                int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                return r == row && !(fromCol + span - 1 < c || fromCol > c + s - 1);
            }).ToList();

            foreach (var panel in toRemove)
            {
                await _timeEntryRepo.DeleteTimeEntryAsync(panel.EntryId);
            }
        }

        private async Task<bool> ShiftRightFrom(int fromCol, int row, int requiredSpan)
        {
            var toShift = panels
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .OrderBy(p => tableLayoutPanelCalendar.GetColumn(p))
                .ToList();

            var layoutWidth = tableLayoutPanelCalendar.ColumnCount;
            Dictionary<DayPanel, (int oldCol, int span)> shifts = new();
            int cursor = fromCol + requiredSpan;

            foreach (var panel in toShift)
            {
                int col = tableLayoutPanelCalendar.GetColumn(panel);
                int span = tableLayoutPanelCalendar.GetColumnSpan(panel);
                if (col >= fromCol)
                {
                    if (cursor + span > layoutWidth)
                    {
                        AppLogger.Error("Posun není možný, došlo by k přetečení dne.");
                        return false;
                    }
                    shifts[panel] = (col, span);
                    cursor += span;
                    tableLayoutPanelCalendar.SetColumn(panel, cursor - span);
                }
            }

            foreach (var kvp in shifts)
            {
                var panel = kvp.Key;
                var (newCol, span) = (tableLayoutPanelCalendar.GetColumn(panel), kvp.Value.span);
                var entry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
                if (entry == null) continue;
                entry.Timestamp = _selectedDate.AddDays(row).AddMinutes(newCol * TimeSlotLengthInMinutes);
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);
            }

            return true;
        }

        private void flowLayoutPanel2_SizeChanged(object sender, EventArgs e)
        {
            int newWidth = flowLayoutPanel2.ClientSize.Width - 10;
            tableLayoutPanelProject.Width = newWidth;
            tableLayoutPanelEntryType.Width = newWidth;
            tableLayoutPanelEntrySubType.Width = newWidth;
            tableLayoutPanel6.Width = newWidth;

            ClearComboBoxSelections(flowLayoutPanel2);
        }

        private void ClearComboBoxSelections(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is ComboBox cb)
                {
                    cb.SelectionStart = cb.Text.Length;
                    cb.SelectionLength = 0;
                }
                else
                {
                    ClearComboBoxSelections(control);
                }
            }
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelDay2_Paint(object sender, PaintEventArgs e)
        {

        }

        private void panelDay_Click(object sender, EventArgs e)
        {
            var current = _config.PanelDayView;
            var enumValues = Enum.GetValues(typeof(PanelDayView)).Cast<PanelDayView>().ToArray();
            int index = Array.IndexOf(enumValues, current);
            int nextIndex = (index + 1) % enumValues.Length;
            _config.PanelDayView = enumValues[nextIndex];

            UpdateHourLabels();
            ConfigService.Save(_config);

        }
    }
}
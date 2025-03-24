using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
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
        private const int ResizeThreshold = 7;
        private const int TimeSlotLengthInMinutes = 30;
        private const int DefaultProjectType = 0;

        // UI + State
        private readonly LoadingUC _loadingUC = new();
        private readonly Timer _resizeTimer = new() { Interval = 50 };

        // Repositories
        private readonly TimeEntryRepository _timeEntryRepo;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepo;
        private readonly ProjectRepository _projectRepo;
        private readonly UserRepository _userRepo;

        // Data cache
        private List<Project> _projects = new();
        private List<TimeEntryType> _timeEntryTypes = new();
        private List<TimeEntrySubType> _timeEntrySubTypes = new();
        private List<DayPanel> panels = new();

        // Context
        private User _selectedUser;
        private DateTime _selectedDate;
        private int _selectedTimeEntryId = -1;
        private bool comboBoxProjectsLoading = false;
        private bool isUpdating = false;

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

        // Configuration
        private int arrivalColumn = 12;
        private int leaveColumn = 28;

        public CalendarV2(User currentUser,
                          TimeEntryRepository timeEntryRepo,
                          TimeEntryTypeRepository timeEntryTypeRepo,
                          TimeEntrySubTypeRepository timeEntrySubTypeRepo,
                          ProjectRepository projectRepo,
                          UserRepository userRepo)
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

            _resizeTimer.Tick += (_, _) =>
            {
                _resizeTimer.Stop();
                AdjustIndicators(panelContainer.AutoScrollPosition);
            };
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

        private async void CalendarV2_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = Size;
            Controls.Add(_loadingUC);
            await LoadInitialDataAsync();
        }

        private async Task LoadInitialDataAsync()
        {
            await Task.WhenAll(
                LoadTimeEntryTypesAsync(DefaultProjectType),
                LoadTimeEntrySubTypesAsync(),
                LoadProjectsAsync(DefaultProjectType),
                RenderCalendar()
            );
        }

        public async void ChangeUser(User newUser)
        {
            _selectedUser = newUser;
            await RenderCalendar();
            await LoadSidebar();
        }

        public async Task<DateTime> ChangeToPreviousWeek()
        {
            _selectedDate = _selectedDate.AddDays(-7);
            await RenderCalendar();
            return _selectedDate;
        }

        public async Task<DateTime> ChangeToNextWeek()
        {
            _selectedDate = _selectedDate.AddDays(7);
            await RenderCalendar();
            return _selectedDate;
        }


        private async Task LoadTimeEntryTypesAsync(int projectType)
        {
            try
            {
                var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
                bool afterCare = timeEntry?.AfterCare == 1;

                _timeEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);

                SafeInvoke(() =>
                {
                    comboBoxEntryType.Items.Clear();
                    comboBoxEntryType.Items.AddRange(afterCare
                        ? _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeWithAfterCareToString).ToArray()
                        : _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());

                    if (comboBoxEntryType.Items.Count > 0)
                        comboBoxEntryType.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání typů časových záznamů.", ex));
            }
        }

        private async Task LoadTimeEntrySubTypesAsync()
        {
            try
            {
                _timeEntrySubTypes = await _timeEntrySubTypeRepo.GetAllTimeEntrySubTypesByUserIdAsync(_selectedUser.Id);

                SafeInvoke(() =>
                {
                    comboBoxIndex.Items.Clear();
                    comboBoxIndex.Items.AddRange(
                        _timeEntrySubTypes.Select(FormatHelper.FormatTimeEntrySubTypeToString).ToArray());

                    if (comboBoxIndex.Items.Count > 0)
                        comboBoxIndex.SelectedIndex = 0;
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
                _projects = await _projectRepo.GetAllProjectsAsyncByProjectType(projectType, includeArchived);

                SafeInvoke(() =>
                {
                    comboBoxProjectsLoading = true;

                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(
                        _projects.Select(FormatHelper.FormatProjectToString).ToArray());

                    if (_projects.Count > 0)
                    {
                        comboBoxProjects.SelectedIndex = 0;
                    }
                    else
                    {
                        comboBoxProjects.Text = string.Empty;
                    }

                    comboBoxProjectsLoading = false;
                });
            }
            catch (Exception ex)
            {
                SafeInvoke(() => AppLogger.Error("Chyba při načítání projektů.", ex));
            }
        }

        private async Task LoadSidebar()
        {
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;
            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (timeEntry == null) return;

            flowLayoutPanel2.Enabled = timeEntry.IsLocked == 0;

            var proj = await _projectRepo.GetProjectByIdAsync(timeEntry.ProjectId ?? 0);
            if (proj == null) return;

            await LoadTimeEntryTypesAsync(proj.ProjectType);

            int index = proj.ProjectType + 1;
            var radioButton = flowLayoutPanel2.Controls.Find($"radioButton{index}", false).FirstOrDefault() as RadioButton;
            if (radioButton != null)
                radioButton.Checked = true;

            comboBoxProjectsLoading = true;

            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;

            BeginInvoke((Action)(() =>
            {
                comboBoxStart.SelectedIndex = minutesStart / 30;
                comboBoxEnd.SelectedIndex = minutesEnd / 30;

                if (lastPanel?.EntryId != -1)
                {
                    comboBoxIndex.Text = timeEntry.Description;
                    textBoxNote.Text = timeEntry.Note;
                    comboBoxProjects.Text = FormatHelper.FormatProjectToString(timeEntry.Project);
                    var selectedType = _timeEntryTypes.FirstOrDefault(x => x.Id == timeEntry.EntryTypeId);
                    comboBoxEntryType.Text = selectedType != null ? FormatHelper.FormatTimeEntryTypeToString(selectedType) : "";
                    checkBoxArchivedProjects.Checked = timeEntry.Project.IsArchived == 1;
                }

                comboBoxProjectsLoading = false;
            }));
        }


        private TableLayoutPanelCellPosition GetCellAt(TableLayoutPanel panel, Point clickPosition)
        {
            int width = panel.Width / panel.ColumnCount;
            int height = panel.Height / panel.RowCount;

            int col = Math.Min(clickPosition.X / width, panel.ColumnCount - 1);
            int row = Math.Min(clickPosition.Y / height, panel.RowCount - 1);

            return new TableLayoutPanelCellPosition(col, row);
        }

        private async void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TableLayoutPanelCellPosition cell = GetCellAt(tableLayoutPanel1, e.Location);

            DateTime clickedDate = _selectedDate
                .AddDays(cell.Row)
                .AddMinutes(cell.Column * 30);

            if (_projects.Count == 0 || _timeEntryTypes.Count == 0)
            {
                AppLogger.Error("Nelze vytvořit záznam: nejsou načteny projekty nebo typy záznamů.");
                return;
            }

            var newTimeEntry = new TimeEntry()
            {
                ProjectId = _projects[0].Id,
                EntryTypeId = _timeEntryTypes[0].Id,
                UserId = _selectedUser.Id,
                Timestamp = clickedDate,
                EntryMinutes = 30
            };

            if (comboBoxProjects.SelectedIndex > -1)
            {
                newTimeEntry.ProjectId = _projects[(comboBoxProjects.SelectedIndex == -1 ? 0 : comboBoxProjects.SelectedIndex)].Id;
            }

            newTimeEntry.AfterCare = _projects.Find(x => x.Id == newTimeEntry.ProjectId).IsArchived;
            if (newTimeEntry.AfterCare == null) newTimeEntry.AfterCare = 0;

            newTimeEntry.IsLocked = 0;

            var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
            if (addedTimeEntry == null)
            {
                AppLogger.Error("Chyba při vytváření nového časového záznamu.");
                return;
            }

            _selectedTimeEntryId = addedTimeEntry.Id;
            await LoadSidebar();
            await RenderCalendar();
        }


        private async Task RenderCalendar()
        {
            var currentUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName);
            bool isCurrentUser = _selectedUser.WindowsUsername == currentUser.WindowsUsername;
            tableLayoutPanel1.Enabled = isCurrentUser;
            flowLayoutPanel2.Enabled = isCurrentUser;

            tableLayoutPanel1.SuspendLayout();
            panelContainer.SuspendLayout();

            var scrollPosition = panelContainer.AutoScrollPosition;
            panelContainer.AutoScroll = false;
            _loadingUC.BringToFront();

            tableLayoutPanel1.Controls.Clear();
            panels.Clear();

            var entries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);
            var allProjects = await _projectRepo.GetAllProjectsAsync();
            var projectDict = allProjects.ToDictionary(p => p.Id);

            foreach (var entry in entries)
            {
                if (entry.Project == null)
                    continue;


                var entryType = _timeEntryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);
                string color = entryType?.Color ?? "#ADD8E6";

                var newPanel = new DayPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = ColorTranslator.FromHtml(color),
                    BorderStyle = BorderStyle.FixedSingle,
                    EntryId = entry.Id
                };

                newPanel.UpdateUi(
                    (entry.Project?.IsArchived == 1 ? "(AFTERCARE) " : "") + entry.Project?.ProjectDescription,
                    entry.Description);

                int column = GetColumnBasedOnTimeEntry(entry.Timestamp);
                int row = GetRowBasedOnTimeEntry(entry.Timestamp);
                int columnSpan = GetColumnSpanBasedOnTimeEntry(entry.EntryMinutes);

                newPanel.MouseMove += dayPanel_MouseMove;
                newPanel.MouseDown += dayPanel_MouseDown;
                newPanel.MouseUp += dayPanel_MouseUp;
                newPanel.MouseLeave += dayPanel_MouseLeave;
                newPanel.MouseClick += dayPanel_MouseClick;

                tableLayoutPanel1.Controls.Add(newPanel, column, row);
                tableLayoutPanel1.SetColumnSpan(newPanel, columnSpan);
                panels.Add(newPanel);
            }

            BeginInvoke((Action)(() =>
            {
                UpdateDateLabels();
                panelContainer.AutoScroll = true;
                panelContainer.AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), 0);

                tableLayoutPanel1.ResumeLayout(true);
                panelContainer.ResumeLayout(true);
                _loadingUC.Visible = false;
            }));
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
            labelDate01.Text = _selectedDate.ToString("d.M.yyyy");
            labelDate02.Text = _selectedDate.AddDays(1).ToString("d.M.yyyy");
            labelDate03.Text = _selectedDate.AddDays(2).ToString("d.M.yyyy");
            labelDate04.Text = _selectedDate.AddDays(3).ToString("d.M.yyyy");
            labelDate05.Text = _selectedDate.AddDays(4).ToString("d.M.yyyy");
            labelDate06.Text = _selectedDate.AddDays(5).ToString("d.M.yyyy");
            labelDate07.Text = _selectedDate.AddDays(6).ToString("d.M.yyyy");
        }


        private void AdjustIndicators(Point scrollPosition)
        {
            // Odstranění starých indikátorů
            var oldIndicators = panelContainer.Controls.OfType<Panel>().Where(p => p.Name == "indicator").ToList();
            foreach (var ctrl in oldIndicators)
            {
                panelContainer.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            int[] rowHeights = tableLayoutPanel1.GetRowHeights();
            int[] columnWidths = tableLayoutPanel1.GetColumnWidths();
            int[] headerRowHeights = customTableLayoutPanel1.GetRowHeights();

            int todayIndex = (int)DateTime.Now.DayOfWeek - 1;
            if (todayIndex < 0) todayIndex = 6; // Oprava, aby pondělí bylo 0 a neděle 6

            // Přidání indikátorů
            for (int j = 0; j < 7; j++)
            {
                int rowHeight = (j < rowHeights.Length) ? rowHeights[j] : 69;
                int yPos = tableLayoutPanel1.GetRowHeights().Take(j).Sum() + headerRowHeights[0];

                int arrivalXPos = (columnWidths[0] * arrivalColumn) - Math.Abs(scrollPosition.X);

                var arrivalIndicator = new Panel
                {
                    Name = "indicator",
                    Size = new Size(2, rowHeight),
                    Location = new Point(arrivalXPos, yPos),
                    BackColor = Color.Green
                };

                panelContainer.Controls.Add(arrivalIndicator);
                arrivalIndicator.BringToFront();

                // Vykreslení leaveColumn pouze pokud to není aktuální den
                if (j != todayIndex)
                {
                    int leaveXPos = (columnWidths[0] * leaveColumn) - Math.Abs(scrollPosition.X);

                    var leaveIndicator = new Panel
                    {
                        Name = "indicator",
                        Size = new Size(2, rowHeight),
                        Location = new Point(leaveXPos, yPos),
                        BackColor = Color.Red
                    };

                    panelContainer.Controls.Add(leaveIndicator);
                    leaveIndicator.BringToFront();
                }
            }
        }



        #region DayPanel events
        private void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            DeactivateAllPanels();

            panel.Activate();
            tableLayoutPanel1.ClearSelection();

            _selectedTimeEntryId = panel.EntryId;
            _ = LoadSidebar();
        }

        private void DeactivateAllPanels()
        {
            foreach (var ctrl in tableLayoutPanel1.Controls)
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

            int currentMouseX = Cursor.Position.X;
            int deltaX = currentMouseX - startMouseX;
            int columnWidth = tableLayoutPanel1.Width / tableLayoutPanel1.ColumnCount;

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

        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            isResizing = Cursor == Cursors.SizeWE;
            isMoving = !isResizing;

            activePanel = panel;
            startMouseX = Cursor.Position.X;
            originalColumn = tableLayoutPanel1.GetColumn(panel);
            originalColumnSpan = tableLayoutPanel1.GetColumnSpan(panel);

            panel.Capture = true;
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
                int minColumn = GetNearestLeftColumn(originalColumn, tableLayoutPanel1.GetRow(panel), panel);

                if (newColumn >= minColumn && newSpan > 0 && newColumn + newSpan <= tableLayoutPanel1.ColumnCount && !IsOverlapping(newColumn, newSpan, tableLayoutPanel1.GetRow(panel), panel))
                {
                    tableLayoutPanel1.SuspendLayout();
                    tableLayoutPanel1.SetColumn(panel, newColumn);
                    tableLayoutPanel1.SetColumnSpan(panel, newSpan);
                    tableLayoutPanel1.ResumeLayout();
                }
            }
            else
            {
                int newSpan = originalColumnSpan + deltaX / columnWidth;
                int maxSpan = GetNearestRightColumn(originalColumn, originalColumnSpan, tableLayoutPanel1.GetRow(panel), panel);

                if (newSpan > 0 && originalColumn + newSpan <= maxSpan && !IsOverlapping(originalColumn, newSpan, tableLayoutPanel1.GetRow(panel), panel))
                {
                    tableLayoutPanel1.SuspendLayout();
                    tableLayoutPanel1.SetColumnSpan(panel, newSpan);
                    tableLayoutPanel1.ResumeLayout();
                }
            }
        }

        private void HandleMove(DayPanel panel, int deltaX, int columnWidth)
        {
            int newColumn = originalColumn + deltaX / columnWidth;
            int minColumn = GetNearestLeftColumn(originalColumn, tableLayoutPanel1.GetRow(panel), panel);
            int maxColumn = GetNearestRightColumn(originalColumn, originalColumnSpan, tableLayoutPanel1.GetRow(panel), panel) - originalColumnSpan;

            if (newColumn >= minColumn && newColumn <= maxColumn)
            {
                tableLayoutPanel1.SuspendLayout();
                tableLayoutPanel1.SetColumn(panel, newColumn);
                tableLayoutPanel1.ResumeLayout();
            }
        }

        private void UpdateCursor(MouseEventArgs e, DayPanel panel)
        {
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

            isResizing = false;
            isMoving = false;
            activePanel = null;
            Cursor = Cursors.Default;

            _selectedTimeEntryId = panel.EntryId;
            var entry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (entry == null) return;

            var entryType = _timeEntryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);
            panel.BackColor = ColorTranslator.FromHtml(entryType?.Color ?? "#ADD8E6");

            var newTimestamp = _selectedDate
                .AddMinutes(tableLayoutPanel1.GetColumn(panel) * TimeSlotLengthInMinutes)
                .AddDays(tableLayoutPanel1.GetRow(panel));
            var newDuration = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanel1.GetColumnSpan(panel));

            if (entry.Timestamp != newTimestamp || entry.EntryMinutes != newDuration)
            {
                entry.Timestamp = newTimestamp;
                entry.EntryMinutes = newDuration;
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);
            }

            await LoadSidebar();
        }
        #endregion

        private bool IsOverlapping(int newColumn, int newSpan, int row, DayPanel currentPanel)
        {
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel) continue;

                int pRow = tableLayoutPanel1.GetRow(p);
                if (pRow != row) continue;

                int pCol = tableLayoutPanel1.GetColumn(p);
                int pSpan = tableLayoutPanel1.GetColumnSpan(p);

                if ((newColumn >= pCol && newColumn < pCol + pSpan) ||
                    (newColumn + newSpan > pCol && newColumn + newSpan <= pCol + pSpan))
                {
                    return true;
                }
            }
            return false;
        }

        private int GetNearestLeftColumn(int currentColumn, int row, DayPanel currentPanel)
        {
            int minColumn = 0;
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanel1.GetRow(p) != row) continue;

                int pCol = tableLayoutPanel1.GetColumn(p);
                int pSpan = tableLayoutPanel1.GetColumnSpan(p);
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
            int maxColumn = tableLayoutPanel1.ColumnCount;
            int panelRightEdge = currentColumn + currentSpan;

            foreach (DayPanel p in panels)
            {
                if (p == currentPanel || tableLayoutPanel1.GetRow(p) != row) continue;

                int pCol = tableLayoutPanel1.GetColumn(p);
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

        private async void buttonPreviousWeek_Click(object sender, EventArgs e)
        {
            _selectedDate = _selectedDate.AddDays(-7);
            await RenderCalendar();
        }

        private async void buttonNextWeek_Click(object sender, EventArgs e)
        {
            _selectedDate = _selectedDate.AddDays(7);
            await RenderCalendar();
        }

        #region Timespans
        private int GetNumberOfMinutesFromDateSpan(DateTime start, DateTime end)
        {
            return (int)(end - start).TotalMinutes;
        }

        private void comboBoxEnd_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxStart.SelectedIndex > 0)
            {
                if (TimeDifferenceOutOfRange())
                {
                    comboBoxEnd.SelectedIndex = comboBoxStart.SelectedIndex + 1;
                }
            }
        }

        private void comboBoxStart_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEnd.SelectedIndex > 0)
            {
                if (TimeDifferenceOutOfRange())
                {
                    comboBoxStart.SelectedIndex = comboBoxEnd.SelectedIndex - 1;
                }
            }
        }

        private bool TimeDifferenceOutOfRange()
        {
            int minutesStart = comboBoxStart.SelectedIndex * 30;
            int minutesEnd = comboBoxEnd.SelectedIndex * 30;

            return minutesEnd - minutesStart <= 0;
        }
        #endregion

        #region ComboBox Projects

        private void comboBoxProjects_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxProjectsLoading || isUpdating) return;

            isUpdating = true;
            try
            {
                if (comboBoxProjects.SelectedItem != null)
                {
                    comboBoxProjects.Text = comboBoxProjects.SelectedItem.ToString();
                    comboBoxProjects.SelectionStart = comboBoxProjects.Text.Length;
                    comboBoxProjects.SelectionLength = 0;
                    comboBoxProjects.DroppedDown = false;
                }
            }
            finally { isUpdating = false; }
        }

        private void comboBoxProjects_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxProjectsLoading || isUpdating || !comboBoxProjects.Enabled) return;

            isUpdating = true;
            try
            {
                string query = FormatHelper.RemoveDiacritics(comboBoxProjects.Text);
                int selectionStart = comboBoxProjects.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    ResetProjectComboBox();
                    return;
                }

                var filteredItems = _projects
                    .Select(FormatHelper.FormatProjectToString)
                    .Where(x => FormatHelper.RemoveDiacritics(x)
                        .IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(filteredItems.ToArray());
                    comboBoxProjects.Text = comboBoxProjects.Text;
                    comboBoxProjects.SelectionStart = selectionStart;
                    comboBoxProjects.SelectionLength = 0;

                    if (!comboBoxProjects.DroppedDown)
                    {
                        BeginInvoke(() =>
                        {
                            comboBoxProjects.DroppedDown = true;
                            Cursor = Cursors.Default;
                        });
                    }
                }
                else
                {
                    comboBoxProjects.DroppedDown = false;
                }
            }
            finally { isUpdating = false; }
        }

        private void ResetProjectComboBox()
        {
            comboBoxProjects.Items.Clear();
            comboBoxProjects.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
            comboBoxProjects.DroppedDown = false;
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

            int selectedProjectIndex = comboBoxProjects.SelectedIndex >= 0 ? comboBoxProjects.SelectedIndex : 0;
            int projectType = _projects.ElementAtOrDefault(selectedProjectIndex)?.ProjectType ?? 0;

            var newType = new TimeEntryType
            {
                Title = comboBoxEntryType.Text,
                ForProjectType = projectType
            };

            var addedTimeEntryType = await _timeEntryTypeRepo.CreateTimeEntryTypeAsync(newType);
            if (addedTimeEntryType == null)
            {
                AppLogger.Error("Chyba při vytváření typu časového záznamu.");
                return;
            }

            var newSubType = new TimeEntrySubType
            {
                Title = comboBoxIndex.Text,
                UserId = _selectedUser.Id
            };

            var addedTimeEntrySubType = await _timeEntrySubTypeRepo.CreateTimeEntrySubTypeAsync(newSubType);

            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (timeEntry == null) return;

            timeEntry.Description = addedTimeEntrySubType.Title;
            timeEntry.EntryTypeId = addedTimeEntryType.Id;
            timeEntry.Note = textBoxNote.Text;

            if (comboBoxProjects.SelectedIndex >= 0)
                timeEntry.ProjectId = _projects[comboBoxProjects.SelectedIndex].Id;

            timeEntry.AfterCare = _projects.FirstOrDefault(x => x.Id == timeEntry.ProjectId)?.IsArchived ?? 0;

            bool success = await _timeEntryRepo.UpdateTimeEntryAsync(timeEntry);
            if (success)
            {
                AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl úspěšně aktualizován.");
                await RenderCalendar();
            }
        }

        private (bool valid, string reason) CheckForEmptyOrIncorrectFields()
        {
            if (string.IsNullOrWhiteSpace(comboBoxProjects.Text))
                return (false, "Projekt");
            if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text))
                return (false, "Typ zápisu");
            if (string.IsNullOrWhiteSpace(comboBoxIndex.Text))
                return (false, "Index");

            return (true, "");
        }


        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            await DeleteRecord();
        }

        public async Task DeleteRecord()
        {
            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (timeEntry == null) return;

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
                $"Smazat záznam {FormatHelper.FormatTimeEntryToString(entry)}?",
                "Smazat?",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Exclamation);

            return result == DialogResult.Yes;
        }


        private async void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            foreach (var ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    pan.Deactivate();
                }
            }

            _selectedTimeEntryId = -1;
            await LoadSidebar();
        }

        private async void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                int index = 0;

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
                    case "PŘEDPROJEKT":
                        index = 2;
                        labelProject.Text = "Předprojekt*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        break;
                    case "ŠKOLENÍ":
                        index = 3;
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = false;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
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
        }
    }
}

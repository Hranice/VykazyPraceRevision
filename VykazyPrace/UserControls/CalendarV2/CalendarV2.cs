using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
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
        private const int ResizeThreshold = 12;
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

        // Copy & paste
        private TimeEntry? copiedEntry;
        private TableLayoutPanelCellPosition? pasteTargetCell;
        private ToolTip copyToolTip = new();

        // Right click context
        private ContextMenuStrip dayPanelMenu;
        private ContextMenuStrip tableLayoutMenu;

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
            InitializeContextMenus();
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

        public async Task ChangeUser(User newUser)
        {
            _selectedUser = newUser;
            await RenderCalendar();

            // Reset vybraného záznamu a sidebaru po změně uživatele
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;
            _ = LoadSidebar();
        }

        internal async Task<DateTime> ChangeToPreviousWeek()
        {
            _selectedDate = _selectedDate.AddDays(-7);
            await RenderCalendar();
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToNextWeek()
        {
            _selectedDate = _selectedDate.AddDays(7);
            await RenderCalendar();
            this.Focus();
            return _selectedDate;
        }

        internal async Task<DateTime> ChangeToTodaysWeek()
        {
            DateTime today = DateTime.Today;
            int offset = ((int)today.DayOfWeek + 6) % 7;
            _selectedDate = today.AddDays(-offset);
            await RenderCalendar();
            this.Focus();
            return _selectedDate;
        }



        private async Task LoadTimeEntryTypesAsync(int projectType)
        {
            try
            {
                var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
                bool isArchived = timeEntry?.AfterCare == 1;

                _timeEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);

                SafeInvoke(() =>
                {
                    comboBoxEntryType.Items.Clear();
                    comboBoxEntryType.Items.AddRange(checkBoxArchivedProjects.Checked
                        ? _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeWithAfterCareToString).ToArray()
                        : _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());

                    comboBoxEntryType.Text = string.Empty;
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
                        _timeEntrySubTypes
                                .Where(t => t.IsArchived == 0)
                                .Select(FormatHelper.FormatTimeEntrySubTypeToString)
                                .ToArray());

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

                    comboBoxProjects.Text = string.Empty;

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
            comboBoxProjectsLoading = true;

            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;

            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);

            if (timeEntry == null) return;

            if (timeEntry.ProjectId == 132 && timeEntry.EntryTypeId == 24)
            {
                flowLayoutPanel2.Visible = false;
            }

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;

            if (timeEntry.IsValid == 1)
            {

                checkBoxArchivedProjects.Checked = timeEntry.Project.IsArchived == 1;
                flowLayoutPanel2.Enabled = timeEntry.IsLocked == 0;

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
                        comboBoxIndex.Text = timeEntry.Description;
                        textBoxNote.Text = timeEntry.Note;
                        suppressDropdownTemporarily = true;
                        comboBoxProjects.Text = FormatHelper.FormatProjectToString(timeEntry.Project);
                        suppressDropdownTemporarily = false;
                        var selectedType = _timeEntryTypes.FirstOrDefault(x => x.Id == timeEntry.EntryTypeId);
                        comboBoxEntryType.Text = timeEntry.AfterCare == 1
                            ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(selectedType)
                            : FormatHelper.FormatTimeEntryTypeToString(selectedType);
                    }

                    comboBoxProjectsLoading = false;
                }));
            }

            else
            {
                BeginInvoke((Action)(() =>
                {
                    comboBoxStart.SelectedIndex = minutesStart / 30;
                    comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

                    comboBoxIndex.Text = string.Empty;
                    textBoxNote.Text = string.Empty;
                    comboBoxProjects.Text = string.Empty;
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

                    comboBoxProjectsLoading = false;
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
            var cell = GetCellAt(tableLayoutPanel1, e.Location);
            pasteTargetCell = cell;
            DeactivateAllPanels();
            _selectedTimeEntryId = -1;
            _ = LoadSidebar();
        }

        private void tableLayoutPanel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                pasteTargetCell = GetCellAt(tableLayoutPanel1, e.Location);

                if (tableLayoutMenu.Items.Count > 0)
                {
                    var pasteItem = tableLayoutMenu.Items[0];
                    pasteItem.Enabled = copiedEntry != null;
                }

                tableLayoutMenu.Show(tableLayoutPanel1, e.Location);
            }
        }

        private async void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TableLayoutPanelCellPosition cell = GetCellAt(tableLayoutPanel1, e.Location);

            if (_projects.Count == 0 || _timeEntryTypes.Count == 0)
            {
                AppLogger.Error("Nelze vytvořit záznam: nejsou načteny projekty nebo typy záznamů.");
                return;
            }

            int column = cell.Column;
            int row = cell.Row;
            int span = 1;

            // Najdi první volné místo od zadané pozice doprava
            while (column + span <= tableLayoutPanel1.ColumnCount)
            {
                bool overlapping = panels.Any(p =>
                {
                    int r = tableLayoutPanel1.GetRow(p);
                    if (r != row) return false;
                    int c = tableLayoutPanel1.GetColumn(p);
                    int s = tableLayoutPanel1.GetColumnSpan(p);
                    return !(column + span - 1 < c || column > c + s - 1);
                });

                if (!overlapping) break;
                column++;
            }

            if (column + span > tableLayoutPanel1.ColumnCount)
            {
                AppLogger.Error("V daném řádku už není místo pro nový záznam.");
                return;
            }

            DateTime clickedDate = _selectedDate.AddDays(row).AddMinutes(column * 30);

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

            // snack entries
            for (int row = 0; row < 7; row++)
            {
                bool snackExists = entries.Any(entry =>
                    entry.ProjectId == 132 &&
                    entry.EntryTypeId == 24 &&
                    GetRowBasedOnTimeEntry(entry.Timestamp) == row);

                if (snackExists) continue;

                DateTime defaultSnackTime = _selectedDate.AddDays(row).AddMinutes(18 * 30);

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

                var created = await _timeEntryRepo.CreateTimeEntryAsync(newSnack);
                if (created == null)
                {
                    AppLogger.Error($"Nepodařilo se vytvořit výchozí svačinu pro den {defaultSnackTime.ToShortDateString()}.");
                }
            }

            foreach (var entry in entries)
            {
                if (entry.Project == null)
                    continue;

                await CreatePanelForEntry(entry);
            }

            BeginInvoke((Action)(() =>
            {
                UpdateDateLabels();
                panelContainer.AutoScroll = true;
                panelContainer.AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), 0);

                DeactivateAllPanels();

                var panelToActivate = tableLayoutPanel1.Controls
                  .OfType<DayPanel>()
                  .FirstOrDefault(p => p.EntryId == _selectedTimeEntryId);

                panelToActivate?.Activate();

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

        private async Task CreatePanelForEntry(TimeEntry entry)
        {
            if (entry.Project == null) return;

            var entryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            var entryType = entryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);
            string color = entryType?.Color ?? "#ADD8E6";

            if (entry.IsValid == 0)
            {
                color = "#FF6957";
            }

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
              (entry.Project?.IsArchived == 1 ? "(AFTERCARE) " : "") + (entry.Project.ProjectType == 1 ? entry.Project.ProjectDescription : entry.Project.ProjectTitle),
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

            tableLayoutPanel1.Controls.Add(panel, column, row);
            tableLayoutPanel1.SetColumnSpan(panel, columnSpan);
            panel.Tag = (entry.ProjectId == 132 && entry.EntryTypeId == 24) ? "snack" : null;

            panels.Add(panel);
        }


        #region DayPanel events
        private async void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (mouseMoved) return;

            if (sender is not DayPanel panel) return;

            DeactivateAllPanels();
            panel.Activate();
            _selectedTimeEntryId = panel.EntryId;

            pasteTargetCell = new TableLayoutPanelCellPosition(
                tableLayoutPanel1.GetColumn(panel),
                tableLayoutPanel1.GetRow(panel)
            );

            tableLayoutPanel1.ClearSelection();

            await LoadSidebar();
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

            int rowHeight = tableLayoutPanel1.Height / tableLayoutPanel1.RowCount;
            int currentMouseY = tableLayoutPanel1.PointToClient(Cursor.Position).Y;
            int newRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanel1.RowCount - 1));

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


        private bool mouseMoved = false;

        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            mouseMoved = false;
            DeactivateAllPanels();
            panel.Activate();

            isResizing = Cursor == Cursors.SizeWE;
            isMoving = !isResizing;

            activePanel = panel;
            startMouseX = Cursor.Position.X;
            originalColumn = tableLayoutPanel1.GetColumn(panel);
            originalColumnSpan = tableLayoutPanel1.GetColumnSpan(panel);

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

            mouseMoved = false;
        }

        private void HandleMove(DayPanel panel, int deltaX, int columnWidth)
        {
            int targetColumn = originalColumn + deltaX / columnWidth;
            int rowHeight = tableLayoutPanel1.Height / tableLayoutPanel1.RowCount;
            int currentMouseY = tableLayoutPanel1.PointToClient(Cursor.Position).Y;
            int targetRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanel1.RowCount - 1));

            int span = originalColumnSpan;

            // Kontrola rozsahu tabulky
            if (targetColumn < 0 || targetColumn + span > tableLayoutPanel1.ColumnCount)
                return;

            if (!IsOverlapping(targetColumn, span, targetRow, panel))
            {
                // Zjisti aktuální pozici panelu
                int currentColumn = tableLayoutPanel1.GetColumn(panel);
                int currentRow = tableLayoutPanel1.GetRow(panel);

                // Pokud se opravdu mění pozice
                bool hasMoved = currentColumn != targetColumn || currentRow != targetRow;

                if (hasMoved)
                {
                    tableLayoutPanel1.SuspendLayout();

                    tableLayoutPanel1.SetColumn(panel, targetColumn);
                    if (panel.Tag as string != "snack")
                        tableLayoutPanel1.SetRow(panel, targetRow);
                    tableLayoutPanel1.SetColumnSpan(panel, span);

                    mouseMoved = true;

                    tableLayoutPanel1.ResumeLayout();
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

            _selectedTimeEntryId = panel.EntryId;

            var entry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (entry == null) return;

            var entryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesAsync();
            var entryType = entryTypes.FirstOrDefault(x => x.Id == entry.EntryTypeId);

            var newTimestamp = _selectedDate
                .AddDays(tableLayoutPanel1.GetRow(panel))
                .AddMinutes(tableLayoutPanel1.GetColumn(panel) * TimeSlotLengthInMinutes);

            var newDuration = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanel1.GetColumnSpan(panel));

            if (entry.Timestamp != newTimestamp || entry.EntryMinutes != newDuration)
            {
                entry.Timestamp = newTimestamp;
                entry.EntryMinutes = newDuration;
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);
            }

            string color = entryType?.Color ?? "#ADD8E6";

            if (entry.IsValid == 0)
            {
                color = "#FF6957";
            }
            panel.BackColor = ColorTranslator.FromHtml(color);

            BeginInvoke(() =>
            {
                int minutesStart = entry.Timestamp?.Hour * 60 + entry.Timestamp?.Minute ?? 0;
                int minutesEnd = minutesStart + entry.EntryMinutes;

                comboBoxStart.SelectedIndex = minutesStart / 30;
                comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);
            });

            await LoadSidebar();

        }
        #endregion

        private bool IsOverlapping(int column, int span, int row, DayPanel currentPanel)
        {
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel) continue;

                int pRow = tableLayoutPanel1.GetRow(p);
                if (pRow != row) continue;

                int pCol = tableLayoutPanel1.GetColumn(p);
                int pSpan = tableLayoutPanel1.GetColumnSpan(p);

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

        private bool suppressDropdownTemporarily = false;


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

                    if (!comboBoxProjects.DroppedDown && !suppressDropdownTemporarily)
                    {
                        BeginInvoke(() =>
                        {
                            if (!suppressDropdownTemporarily) // double check inside async invoke
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

            int selectedEntryTypeId = 0;
            if (comboBoxEntryType.SelectedIndex > -1)
            {
                selectedEntryTypeId = _timeEntryTypes[comboBoxEntryType.SelectedIndex].Id;
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
            timeEntry.EntryTypeId = selectedEntryTypeId;
            timeEntry.Note = textBoxNote.Text;

            var selectedProject = _projects.FirstOrDefault(p =>
                FormatHelper.FormatProjectToString(p).Equals(comboBoxProjects.Text, StringComparison.InvariantCultureIgnoreCase));

            if (selectedProject != null)
            {
                timeEntry.ProjectId = selectedProject.Id;
            }

            if (radioButton6.Checked)
            {
                timeEntry.ProjectId = 25;
            }

            else if (radioButton5.Checked)
            {
                timeEntry.ProjectId = 23;
            }

            else if (radioButton4.Checked)
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
                FormatHelper.FormatProjectToString(p).Equals(comboBoxProjects.Text, StringComparison.InvariantCultureIgnoreCase));

            bool EntryTypeMatches = _timeEntryTypes.Any(t =>
                FormatHelper.FormatTimeEntryTypeToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase) ||
                FormatHelper.FormatTimeEntryTypeWithAfterCareToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase));

            switch (rb?.Text)
            {
                case "PROVOZ":
                    if (string.IsNullOrWhiteSpace(comboBoxProjects.Text) || !ProjectTextMatches)
                        return (false, "Nákladové středisko neodpovídá žádné možnosti");
                    break;
                case "PROJEKT":
                case "PŘEDPROJEKT":
                    if (string.IsNullOrWhiteSpace(comboBoxProjects.Text) || !ProjectTextMatches)
                        return (false, "Projekt neodpovídá žádné možnosti");
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Typ záznamu neodpovídá žádné možnosti");
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
            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);
            if (timeEntry == null) return;

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

            var entry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);

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

            if (lastColumn >= tableLayoutPanel1.ColumnCount)
            {
                MessageBox.Show("Záznam nelze vložit, nevejde se do daného dne.", "Chyba", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            bool overlapping = panels.Any(p =>
            {
                int r = tableLayoutPanel1.GetRow(p);
                int c = tableLayoutPanel1.GetColumn(p);
                int s = tableLayoutPanel1.GetColumnSpan(p);
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
                int r = tableLayoutPanel1.GetRow(p);
                int c = tableLayoutPanel1.GetColumn(p);
                int s = tableLayoutPanel1.GetColumnSpan(p);
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
                .Where(p => tableLayoutPanel1.GetRow(p) == row)
                .OrderBy(p => tableLayoutPanel1.GetColumn(p))
                .ToList();

            var layoutWidth = tableLayoutPanel1.ColumnCount;
            Dictionary<DayPanel, (int oldCol, int span)> shifts = new();
            int cursor = fromCol + requiredSpan;

            foreach (var panel in toShift)
            {
                int col = tableLayoutPanel1.GetColumn(panel);
                int span = tableLayoutPanel1.GetColumnSpan(panel);
                if (col >= fromCol)
                {
                    if (cursor + span > layoutWidth)
                    {
                        AppLogger.Error("Posun není možný, došlo by k přetečení dne.");
                        return false;
                    }
                    shifts[panel] = (col, span);
                    cursor += span;
                    tableLayoutPanel1.SetColumn(panel, cursor - span);
                }
            }

            foreach (var kvp in shifts)
            {
                var panel = kvp.Key;
                var (newCol, span) = (tableLayoutPanel1.GetColumn(panel), kvp.Value.span);
                var entry = await _timeEntryRepo.GetTimeEntryByIdAsync(panel.EntryId);
                if (entry == null) continue;
                entry.Timestamp = _selectedDate.AddDays(row).AddMinutes(newCol * TimeSlotLengthInMinutes);
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);
            }

            return true;
        }
    }
}
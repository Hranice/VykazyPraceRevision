using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private List<Project> _projects = new List<Project>();
        private List<TimeEntryType> _timeEntryTypes = new List<TimeEntryType>();
        private int _selectedTimeEntryId = 0;
        private readonly User _selectedUser = new User();
        private DateTime _selectedDate;
        private int arrivalColumn = 12;
        private int leaveColumn = 16;

        private List<DayPanel> panels = new List<DayPanel>();
        private DayPanel? activePanel = null;
        private DayPanel? lastPanel = null;
        private bool isResizing = false;
        private bool isMoving = false;
        private bool isResizingLeft = false;
        private int startMouseX, startPanelX;
        private int originalColumn, originalColumnSpan;
        private const int ResizeThreshold = 5;


        public CalendarV2(User currentUser)
        {
            InitializeComponent();
            DoubleBuffered = true;

            _selectedDate = DateTime.Today.AddDays(-(int)DateTime.Today.DayOfWeek + (int)DayOfWeek.Monday);
            _selectedUser = currentUser;
        }

        private async void CalendarV2_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            await LoadTimeEntryTypesAsync();
            await LoadProjectsAsync();
            await RenderCalendar();
        }

        private async Task LoadTimeEntryTypesAsync()
        {
            try
            {
                _timeEntryTypes = await _timeEntryRepo.GetAllTimeEntryTypesAsync();

                Invoke(() =>
                {
                    comboBoxEntryType.Items.Clear();
                    comboBoxEntryType.Items.AddRange(_timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());
                    if (comboBoxEntryType.Items.Count > 0) comboBoxEntryType.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání typů časových záznamů.", ex);
                }));
            }
        }

        private async Task LoadProjectsAsync()
        {
            try
            {
                _projects = await _projectRepo.GetAllProjectsAndContractsAsync();

                Invoke(new Action(() =>
                {
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání projektů.", ex);
                }));
            }
        }

        bool comboBoxProjectsLoading = false;

        private async Task LoadSidebar()
        {
            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);

            comboBoxProjectsLoading = true;

            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;

            BeginInvoke((Action)(() =>
            {
                comboBoxStart.SelectedIndex = 0;
                comboBoxEnd.SelectedIndex = 0;

                comboBoxStart.SelectedIndex = minutesStart / 30;
                comboBoxEnd.SelectedIndex = minutesEnd / 30;

                groupBox1.Text = $"{days[(int)timeStamp.DayOfWeek]} - {timeStamp:dd.MM.yyyy}";

                if (lastPanel?.EntryId != -1)
                {
                    textBoxDescription.Text = timeEntry.Description;
                    comboBoxProjects.Text = FormatHelper.FormatProjectToString(timeEntry.Project);
                    comboBoxEntryType.Text = FormatHelper.FormatTimeEntryTypeToString(_timeEntryTypes.First(x => x.Id == timeEntry.EntryTypeId));
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

            var newTimeEntry = new TimeEntry()
            {
                ProjectId = _projects[0].Id,
                EntryTypeId = _timeEntryTypes[0].Id,
                UserId = _selectedUser.Id,
                Timestamp = clickedDate,
                EntryMinutes = 30
            };

            var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
            _selectedTimeEntryId = addedTimeEntry.Id;
            await LoadSidebar();
            await RenderCalendar();
        }

        private async Task RenderCalendar()
        {
            tableLayoutPanel1.SuspendLayout();
            panelContainer.SuspendLayout();

            Point scrollPosition = panelContainer.AutoScrollPosition;
            panelContainer.AutoScroll = false;
            _loadingUC.BringToFront();

            tableLayoutPanel1.Controls.Clear();
            panels.Clear();

            var entries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);

            foreach (var entry in entries)
            {
                var newPanel = new DayPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = ColorTranslator.FromHtml(_timeEntryTypes.First(x => x.Id == entry.EntryTypeId).Color ?? "#ADD8E6"),
                    BorderStyle = BorderStyle.FixedSingle,
                    EntryId = entry.Id
                };

                newPanel.UpdateUi(entry.Description, entry.Project?.ProjectDescription);

                int column = GetColumnBasedOnTimeEntry(entry.Timestamp);
                int row = GetRowBasedOnTimeEntry(entry.Timestamp);
                int columnSpan = GetColumnSpanBasedOnTimeEntry(entry.EntryMinutes);

                newPanel.MouseMove += dayPanel_MouseMove;
                newPanel.MouseDown += dayPanel_MouseDown;
                newPanel.MouseUp += dayPanel_MouseUp;
                newPanel.MouseLeave += dayPanel_MouseLeave;
                newPanel.MouseClick += dayPanel_MouseClick;
                newPanel.MouseDoubleClick += dayPanel_MouseDoubleClick;

                tableLayoutPanel1.Controls.Add(newPanel, column, row);
                tableLayoutPanel1.SetColumnSpan(newPanel, columnSpan);
                panels.Add(newPanel);
            }

            // Odstranění starých indikátorů
            var oldIndicators = panelContainer.Controls.OfType<Panel>().Where(p => p.Name == "indicator").ToList();
            foreach (var ctrl in oldIndicators)
            {
                panelContainer.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            BeginInvoke((Action)(() =>
            {
                // Přidání indikátorů
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        var panelIndicator = new Panel
                        {
                            Name = "indicator",
                            Size = new Size(1, 69),
                            Location = new Point((30 * arrivalColumn) + 6 + (i * (30 * leaveColumn) - Math.Abs(scrollPosition.X)), j * 69 + 33 + j),
                            BackColor = Color.Red
                        };

                        panelContainer.Controls.Add(panelIndicator);
                        panelIndicator.BringToFront();
                    }
                }

                UpdateDateLabels();

                panelContainer.AutoScroll = true;
                panelContainer.AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), 0);

                tableLayoutPanel1.ResumeLayout(true);
                panelContainer.ResumeLayout(true);


                _loadingUC.Visible = false;
            }));
        }


        private async void dayPanel_MouseClick(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;


            foreach (var ctrl in tableLayoutPanel1.Controls)
            {
                if (ctrl is DayPanel pan)
                {
                    pan.Deactivate();
                }
            }
            panel.Activate();

            _selectedTimeEntryId = panel.EntryId;
            await LoadSidebar();
        }

        #region DayPanel events
        private async void dayPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            //var result = new TimeEntryV2Dialog(_selectedUser, _selectedDate, panel.EntryId).ShowDialog();

            //ChangesMade = true;
            //await RenderCalendar();

            //switch (result)
            //{
            //    case DialogResult.OK:
            //        ChangesMade = true;
            //        await RenderCalendar();
            //        break;
            //    default:

            //        break;
            //}
        }

        private void dayPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            int currentMouseX = Cursor.Position.X;
            int deltaX = currentMouseX - startMouseX;
            int columnWidth = tableLayoutPanel1.Width / tableLayoutPanel1.ColumnCount;

            if (isResizing && activePanel == panel)
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
            else if (isMoving && activePanel == panel)
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
            else
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
        }

        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (Cursor == Cursors.SizeWE)
            {
                isResizing = true;
            }
            else
            {
                isMoving = true;
            }

            activePanel = panel;
            startMouseX = Cursor.Position.X;
            originalColumn = tableLayoutPanel1.GetColumn(panel);
            originalColumnSpan = tableLayoutPanel1.GetColumnSpan(panel);

            panel.Capture = true;
            panel.BackColor = Color.LightCoral;
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
            panel.BackColor = ColorTranslator.FromHtml(_timeEntryTypes.First(x => x.Id == entry.EntryTypeId).Color ?? "#ADD8E6");

            if (entry is not null)
            {
                entry.Timestamp = _selectedDate.AddMinutes(tableLayoutPanel1.GetColumn(panel) * 30).AddDays(tableLayoutPanel1.GetRow(panel));
                entry.EntryMinutes = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanel1.GetColumnSpan(panel));
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);
            }

            await LoadSidebar();
        }

        private void dayPanel_MouseLeave(object? sender, EventArgs e)
        {
            if (!isResizing && !isMoving)
            {
                Cursor = Cursors.Default;
            }
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

        private int GetColumnBasedOnTimeEntry(DateTime? timeStamp)
        {
            var minutes = timeStamp.Value.Hour * 60 + timeStamp.Value.Minute;
            return minutes / 30;
        }

        private int GetColumnSpanBasedOnTimeEntry(int entryMinutes)
        {
            return entryMinutes / 30;
        }

        private int GetRowBasedOnTimeEntry(DateTime? timeStamp)
        {
            return ((int)timeStamp.Value.DayOfWeek + 6) % 7;
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

        private void UpdateDateLabels()
        {
            labelDate1.Text = _selectedDate.ToString("d.M.yyyy");
            labelDate2.Text = _selectedDate.AddDays(1).ToString("d.M.yyyy");
            labelDate3.Text = _selectedDate.AddDays(2).ToString("d.M.yyyy");
            labelDate4.Text = _selectedDate.AddDays(3).ToString("d.M.yyyy");
            labelDate5.Text = _selectedDate.AddDays(4).ToString("d.M.yyyy");
            labelDate6.Text = _selectedDate.AddDays(5).ToString("d.M.yyyy");
            labelDate7.Text = _selectedDate.AddDays(6).ToString("d.M.yyyy");
        }

        private (bool valid, object reason) CheckForEmptyOrIncorrectFields()
        {
            if (comboBoxProjects.SelectedItem is null) return (false, "Projekt");
            if (string.IsNullOrEmpty(comboBoxEntryType.Text)) return (false, "Typ zápisu");
            if (string.IsNullOrEmpty(textBoxDescription.Text)) return (false, "Popis činnosti");
            return (true, "");
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
        private bool isUpdating = false;

        private void comboBoxProjects_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (!comboBoxProjects.Enabled)
                return;

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
            finally
            {
                isUpdating = false;
            }
        }

        private void comboBoxProjects_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxProjectsLoading)
                return;

            if (isUpdating)
                return;

            if (!comboBoxProjects.Enabled)
                return;

            isUpdating = true;
            try
            {
                string query = FormatHelper.RemoveDiacritics(comboBoxProjects.Text);
                int selectionStart = comboBoxProjects.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBoxProjects.DroppedDown = false;
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
                    return;
                }

                // Filtrace bez diakritiky
                List<string> filteredItems = _projects
                    .Select(FormatHelper.FormatProjectToString)
                    .Where(x => FormatHelper.RemoveDiacritics(x).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    string originalText = comboBoxProjects.Text;
                    int originalSelectionStart = comboBoxProjects.SelectionStart;

                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(filteredItems.ToArray());

                    comboBoxProjects.Text = originalText;
                    comboBoxProjects.SelectionStart = originalSelectionStart;
                    comboBoxProjects.SelectionLength = 0;

                    // Otevření rozevíracího seznamu
                    if (!comboBoxProjects.DroppedDown)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            comboBoxProjects.DroppedDown = true;
                            Cursor = Cursors.Default;
                        }));
                    }
                }
                else
                {
                    comboBoxProjects.DroppedDown = false;
                }
            }
            finally
            {
                isUpdating = false;
            }
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

            var addedTimeEntryType = await _timeEntryRepo.CreateTimeEntryTypeAsync(
                new TimeEntryType()
                {
                    Title = comboBoxEntryType.Text
                });

            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);

            int startHours = int.Parse(comboBoxStart.Text.Split(':')[0]);
            int startMinutes = int.Parse(comboBoxStart.Text.Split(':')[1]);

            var start = new DateTime(timeEntry.Timestamp.Value.Year, timeEntry.Timestamp.Value.Month, timeEntry.Timestamp.Value.Day,
                                       startHours,
                                       startMinutes, 0);

            var end = new DateTime(timeEntry.Timestamp.Value.Year, timeEntry.Timestamp.Value.Month, timeEntry.Timestamp.Value.Day,
                                       int.Parse(comboBoxEnd.Text.Split(':')[0]),
                                       int.Parse(comboBoxEnd.Text.Split(':')[1]), 0);

            if (comboBoxProjects.SelectedIndex > -1)
            {
                timeEntry.ProjectId = _projects[comboBoxProjects.SelectedIndex].Id;
            }

            timeEntry.Description = textBoxDescription.Text;
            timeEntry.Timestamp = start;
            timeEntry.EntryMinutes = GetNumberOfMinutesFromDateSpan(start, end);
            timeEntry.EntryTypeId = addedTimeEntryType?.Id ?? 0;

            var success = await _timeEntryRepo.UpdateTimeEntryAsync(timeEntry);
            if (success)
            {
                AppLogger.Information($"Zápis hodin {FormatHelper.FormatTimeEntryToString(timeEntry)} byl úspěšně proveden.");
                await RenderCalendar();
            }
            else
            {
                AppLogger.Error($"Zápis {FormatHelper.FormatTimeEntryToString(timeEntry)} nebyl proveden.");
            }
        }

        private void buttonRemove_Click(object sender, EventArgs e)
        {
            DeleteRecord();
        }

        public async void DeleteRecord()
        {
            var timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(_selectedTimeEntryId);

            var dialogResult = MessageBox.Show($"Smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dialogResult == DialogResult.Yes)
            {
                if (await _timeEntryRepo.DeleteTimeEntryAsync(_selectedTimeEntryId))
                {
                    AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl smazán z databáze.");
                }

                else
                {
                    AppLogger.Error($"Nepodařilo se smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} z databáze.");
                }
            }

            await RenderCalendar();
        }
    }
}

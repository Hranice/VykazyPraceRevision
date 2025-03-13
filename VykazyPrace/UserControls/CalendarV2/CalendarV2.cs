using System.Diagnostics;
using System.Net.Http.Headers;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Dialogs;
using VykazyPrace.Logging;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        public bool ChangesMade { get; private set; }
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly User _selectedUser = new User();
        private DateTime _selectedDate;
        private int arrivalColumn = 12;
        private int leaveColumn = 16;

        private List<DayPanel> panels = new List<DayPanel>();
        private DayPanel? activePanel = null;
        private bool isResizing = false;
        private bool isMoving = false;
        private bool isResizingLeft = false;
        private int startMouseX, startPanelX;
        private int originalColumn, originalColumnSpan;
        private const int ResizeThreshold = 5;
        bool resetScrollPosition = true;


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

            await RenderCalendar();
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

            var dialog = new TimeEntryV2Dialog(_selectedUser, clickedDate, -1);
            var result = dialog.ShowDialog();


            switch (result)
            {
                case DialogResult.OK:
                    ChangesMade = true;
                    await RenderCalendar();
                    break;
                default:

                    break;
            }
        }

        private async Task RenderCalendar()
        {
            Point scrollPosition = panelContainer.AutoScrollPosition;
            _loadingUC.BringToFront();

            tableLayoutPanel1.Controls.Clear();
            panels.Clear();

            var entries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_selectedUser, _selectedDate);

            tableLayoutPanel1.SuspendLayout();

            foreach (var entry in entries)
            {
                var newPanel = new DayPanel
                {
                    Dock = DockStyle.Fill,
                    BackColor = Color.LightBlue,
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
                newPanel.MouseDoubleClick += dayPanel_MouseDoubleClick;

                tableLayoutPanel1.Controls.Add(newPanel, column, row);
                tableLayoutPanel1.SetColumnSpan(newPanel, columnSpan);
                panels.Add(newPanel);
            }

            tableLayoutPanel1.ResumeLayout();

            BeginInvoke((Action)(() =>
            {
                for (int i = 0; i < 2; i++)
                {
                    for (int j = 0; j < 7; j++)
                    {
                        var panelIndicator = new Panel()
                        {
                            Size = new Size(1, 69),
                            Location = new Point((30 * arrivalColumn) + 6 + (i * (30 * leaveColumn)), j * 69 + 33 + j),
                            BackColor = Color.Red
                        };

                        panelContainer.Controls.Add(panelIndicator);
                        panelIndicator.BringToFront();
                    }
                }


                if (resetScrollPosition)
                {
                    panelContainer.AutoScrollPosition = new Point(310, panelContainer.AutoScrollPosition.Y);
                    resetScrollPosition = false;
                }

                else
                {
                    panelContainer.AutoScrollPosition = new Point(Math.Abs(scrollPosition.X), 0);
                }

                UpdateDateLabels();
                _loadingUC.Visible = false;
            }));
        }

        #region DayPanel events
        private async void dayPanel_MouseDoubleClick(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            var result = new TimeEntryV2Dialog(_selectedUser, _selectedDate, panel.EntryId).ShowDialog();

            ChangesMade = true;
            await RenderCalendar();

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
            panel.BackColor = Color.LightBlue;

            var entry = await _timeEntryRepo.GetTimeEntryByIdAsync(panel.EntryId);

            if (entry is not null)
            {
                entry.Timestamp = _selectedDate.AddMinutes(tableLayoutPanel1.GetColumn(panel) * 30).AddDays(tableLayoutPanel1.GetRow(panel));
                entry.EntryMinutes = GetEntryMinutesBasedOnColumnSpan(tableLayoutPanel1.GetColumnSpan(panel));
                await _timeEntryRepo.UpdateTimeEntryAsync(entry);

                ChangesMade = true;
            }
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
            //return new DateTime(_selectedDate.Year, _selectedDate.Month, _selectedDate.Day, totalMinutes / 60, totalMinutes % 60, 0);
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
    }
}

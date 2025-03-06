﻿using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;


//public event EventHandler<DayCell>? CellPositionChanged;
//public event EventHandler<DayCell>? CellCreated;

//protected virtual void OnCellPositionChanged(DayCell cell)
//{
//    CellPositionChanged?.Invoke(this, cell);
//}
//protected virtual void OnCellCreated(DayCell cell)
//{
//    CellCreated?.Invoke(this, cell);
//}

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2 : UserControl
    {
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private List<Project> _projects = new List<Project>();
        private List<TimeEntry> _timeEntries = new List<TimeEntry>();
        private readonly User _currentUser;
        private readonly DateTime _currentDate = DateTime.Now;

        private List<DayPanel> panels = new List<DayPanel>();
        private DayPanel activePanel = null;
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
            _currentUser = currentUser;
        }

        public int GetControlIndex(Control control)
        {
            if (control == null || tableLayoutPanel1 == null)
                return -1;

            return tableLayoutPanel1.Controls.IndexOf(control);
        }

        private void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            TableLayoutPanelCellPosition cell = GetCellAt(tableLayoutPanel1, e.Location);

            if (IsOverlapping(cell.Column, 1, cell.Row, null))
                return;

            DayPanel newPanel = new DayPanel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.LightBlue,
                BorderStyle = BorderStyle.FixedSingle
            };

            tableLayoutPanel1.Controls.Add(newPanel, cell.Column, cell.Row);
            tableLayoutPanel1.SetColumnSpan(newPanel, 1);
            panels.Add(newPanel);

            newPanel.MouseMove += panel1_MouseMove;
            newPanel.MouseDown += panel1_MouseDown;
            newPanel.MouseUp += panel1_MouseUp;
            newPanel.MouseLeave += panel1_MouseLeave;
        }

        private TableLayoutPanelCellPosition GetCellAt(TableLayoutPanel panel, Point clickPosition)
        {
            int width = panel.Width / panel.ColumnCount;
            int height = panel.Height / panel.RowCount;

            int col = Math.Min(clickPosition.X / width, panel.ColumnCount - 1);
            int row = Math.Min(clickPosition.Y / height, panel.RowCount - 1);

            return new TableLayoutPanelCellPosition(col, row);
        }

        private void panel1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!(sender is DayPanel panel)) return;

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

        private void panel1_MouseDown(object sender, MouseEventArgs e)
        {
            if (!(sender is DayPanel panel)) return;

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

        private void panel1_MouseUp(object sender, MouseEventArgs e)
        {
            if (!(sender is DayPanel panel)) return;

            isResizing = false;
            isMoving = false;
            activePanel = null;
            Cursor = Cursors.Default;
            panel.BackColor = Color.LightBlue;

            var pos = tableLayoutPanel1.GetCellPosition(panel);
        }

        private void panel1_MouseLeave(object sender, EventArgs e)
        {
            if (!isResizing && !isMoving)
            {
                Cursor = Cursors.Default;
            }
        }

        private bool IsOverlapping(int newColumn, int newSpan, int row, DayPanel currentPanel)
        {
            foreach (DayPanel p in panels)
            {
                if (p == currentPanel) continue; //

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

        private void CalendarV2_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            Task.Run(LoadData);
        }

        private async Task LoadData()
        {
            Invoke(() => _loadingUC.BringToFront());

            var projectsTask = LoadProjectsContractsAsync();
            var timeEntriesTask = LoadTimeEntriesAsync();

            await Task.WhenAll(projectsTask, timeEntriesTask);

            Invoke(() => _loadingUC.Visible = false);
        }

        private async Task LoadTimeEntriesAsync()
        {
            try
            {
                _timeEntries = await _timeEntryRepo.GetTimeEntriesByUserAndCurrentWeekAsync(_currentUser, _currentDate);
            }
            catch (Exception ex)
            {
                Invoke(() => AppLogger.Error("Chyba při načítání seznamu zapsaných hodin.", ex));
            }
        }

        private async Task LoadProjectsContractsAsync()
        {
            try
            {
                _projects = await _projectRepo.GetAllProjectsAndContractsAsync();
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání projektů.", ex);
                }));
            }
        }
    }
}

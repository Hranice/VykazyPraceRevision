namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private void dayPanel_MouseMove(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (!CanEditOwner(panel.OwnerId)) return;

            if (panel.Tag as string == "locked") return;

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

        private void dayPanel_MouseDown(object? sender, MouseEventArgs e)
        {
            if (sender is not DayPanel panel) return;

            if (!CanEditOwner(panel.OwnerId)) return;


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
            if (panel.Tag as string == "snack") return;

            if (panel.Tag as string == "locked") return;

            if (!CanEditOwner(panel.OwnerId)) return;

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
            if (panel.Tag as string == "locked") return;

            if (!CanEditOwner(panel.OwnerId)) return;

            int originalRow = tableLayoutPanelCalendar.GetRow(panel);
            int targetColumn = originalColumn + deltaX / columnWidth;

            int targetRow;
            if (panel.Tag as string == "snack")
            {
                targetRow = originalRow;
            }
            else
            {
                int rowHeight = tableLayoutPanelCalendar.Height / tableLayoutPanelCalendar.RowCount;
                int currentMouseY = tableLayoutPanelCalendar.PointToClient(Cursor.Position).Y;
                targetRow = Math.Max(0, Math.Min(currentMouseY / rowHeight, tableLayoutPanelCalendar.RowCount - 1));
            }

            var targetDate = _selectedDate.AddDays(targetRow);
            if (_specialDays.Any(d => d.Date.Date == targetDate.Date && d.Locked))
                return;

            int span = originalColumnSpan;

            if (targetColumn < 0 || targetColumn + span > tableLayoutPanelCalendar.ColumnCount)
                return;

            if (!IsOverlapping(targetColumn, span, targetRow, panel))
            {
                bool hasMoved =
                    tableLayoutPanelCalendar.GetColumn(panel) != targetColumn ||
                    tableLayoutPanelCalendar.GetRow(panel) != targetRow;

                if (hasMoved)
                {
                    tableLayoutPanelCalendar.SuspendLayout();

                    tableLayoutPanelCalendar.SetColumn(panel, targetColumn);

                    // snack zůstává ve svém řádku
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

            int minutesStart = newTimestamp.Hour * 60 + newTimestamp.Minute;
            int minutesEnd = minutesStart + entry.EntryMinutes;

            comboBoxStart.SelectedIndex = minutesStart / 30;
            comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

            //if (_selectedTimeEntryId != previousTimeEntryId)
            //{
            //    await LoadSidebar();
            //}

            await LoadSidebar();
        }

        private bool IsOverlapping(int column, int span, int row, DayPanel currentPanel)
        {
            int start = column;
            int end = column + span - 1;

            foreach (DayPanel p in _activePanels)
            {
                if (p == currentPanel) continue;
                if (!p.Visible) continue;

                int pRow = tableLayoutPanelCalendar.GetRow(p);
                if (pRow != row) continue;

                int pStart = tableLayoutPanelCalendar.GetColumn(p);
                int pEnd = pStart + tableLayoutPanelCalendar.GetColumnSpan(p) - 1;

                // kontrola překryvu (jakékoliv překrytí mezi start–end a pStart–pEnd)
                if (start <= pEnd && end >= pStart)
                    return true;
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

        private int GetEntryMinutesBasedOnColumnSpan(int columnSpan)
        {
            return columnSpan * 30;
        }
    }
}

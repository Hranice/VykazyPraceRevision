using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Logging;
using VykazyPrace.Dialogs;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private TableLayoutPanelCellPosition GetCellAt(TableLayoutPanel panel, Point clickPosition)
        {
            int width = panel.Width / panel.ColumnCount;
            int height = panel.Height / panel.RowCount;

            int col = Math.Min(clickPosition.X / width, panel.ColumnCount - 1);
            int row = Math.Min(clickPosition.Y / height, panel.RowCount - 1);

            return new TableLayoutPanelCellPosition(col, row);
        }

        /// <summary>
        /// Dvojklikem vloží nový záznam mezi stávající, s Replace/Move dialogem.
        /// </summary>
        private async void TableLayoutPanel1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (_selectedUser.Id != _loggedUser.Id && _selectedUser.MasterUserId != _loggedUser.Id) return;

            var cell = GetCellAt(tableLayoutPanelCalendar, e.Location);
            if (_projects.Count == 0 || _timeEntryTypes.Count == 0) return;

            var targetDate = _selectedDate.AddDays(cell.Row);
            if (_specialDays.Any(d => d.Date.Date == targetDate.Date && d.Locked)) return;

            int column = cell.Column;
            int row = cell.Row;
            int span = 1;

            // Najdi volné místo, ale pak ošetři kolizi
            while (column + span <= tableLayoutPanelCalendar.ColumnCount)
            {
                bool ov = tableLayoutPanelCalendar.Controls
                           .OfType<DayPanel>()
                           .Any(p =>
                           {
                               int r = tableLayoutPanelCalendar.GetRow(p);
                               if (r != row) return false;
                               int c = tableLayoutPanelCalendar.GetColumn(p);
                               int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                               return !(column + span - 1 < c || column > c + s - 1);
                           });
                if (!ov) break;
                column++;
            }
            if (column + span > tableLayoutPanelCalendar.ColumnCount) return;

            if (!await HandleOverlapAsync(column, row, span))
                return;

            DateTime ts = _selectedDate.AddDays(row)
                                       .AddMinutes(column * TimeSlotLengthInMinutes);
            if (_specialDays.Any(d => d.Date.Date == ts.Date && d.Locked)) return;

            int idx = customComboBoxProjects.SelectedIndex >= 0
                ? customComboBoxProjects.SelectedIndex
                : 0;
            int projId = _projects[idx].Id;

            var newEntry = new TimeEntry
            {
                ProjectId = projId,
                EntryTypeId = _timeEntryTypes[0].Id,
                UserId = _selectedUser.Id,
                Timestamp = ts,
                EntryMinutes = 30,
                AfterCare = _projects.First(p => p.Id == projId).IsArchived,
                IsLocked = 0
            };

            await OnNewEntryCreated(newEntry);
            await LoadSidebar();
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

        private async Task AdjustIndicatorsAsync(Point scrollPosition, int userId, DateTime weekStart)
        {
            var entries = await _arrivalDepartureRepo.GetWeekEntriesForUserAsync(userId, weekStart);

            if (entries is null)
            {
                AppLogger.Information("Nepodařilo se upravit indikátory - záznamy příchodů a odchodů jsou null.");
                return;
            }

            // smazání starých indikátorů
            var oldIndicators = panelContainer.Controls
                .OfType<Panel>()
                .Where(p => p.Name == "indicator")
                .ToList();
            foreach (var ctrl in oldIndicators)
            {
                panelContainer.Controls.Remove(ctrl);
                ctrl.Dispose();
            }

            int[] rowHeights = tableLayoutPanelCalendar.GetRowHeights();
            int[] columnWidths = tableLayoutPanelCalendar.GetColumnWidths();
            int[] headerRowHeights = customTableLayoutPanel1.GetRowHeights();
            const int minutesPerColumn = 30;
            var toolTip = new ToolTip();

            foreach (var e in entries)
            {
                // den v týdnu (Po=0 .. Ne=6)
                int dayIndex = ((int)e.WorkDate.DayOfWeek - 1 + 7) % 7;
                int rowHeight = dayIndex < rowHeights.Length ? rowHeights[dayIndex] : 69;
                int yPos = rowHeights.Take(dayIndex).Sum() + headerRowHeights[0];

                // výchozí raw časy
                TimeSpan? rawArrival = e.ArrivalTimestamp?.TimeOfDay;
                TimeSpan? rawDeparture = e.DepartureTimestamp?.TimeOfDay;

                // nic k zobrazení?
                if (!rawArrival.HasValue && !rawDeparture.HasValue)
                    continue;

                var (roundedArrival, roundedDeparture) =
                    RoundWorkTimeToNearestHalfHour(rawArrival, rawDeparture);

                // přepočet na sloupcové indexy a X pozice
                int? arrivalCol = roundedArrival.HasValue ? GetColumnIndexFromTime(roundedArrival.Value, minutesPerColumn) : (int?)null;
                int? leaveCol = roundedDeparture.HasValue ? GetColumnIndexFromTime(roundedDeparture.Value, minutesPerColumn) : (int?)null;

                int arrivalX = arrivalCol.HasValue ? columnWidths[0] * arrivalCol.Value - Math.Abs(scrollPosition.X) : 0;
                int leaveX = leaveCol.HasValue ? columnWidths[0] * leaveCol.Value - Math.Abs(scrollPosition.X) : 0;

                // tooltip text podle dostupnosti časů
                string tt = (rawArrival, rawDeparture) switch
                {
                    (TimeSpan a, TimeSpan d) => $"{a:hh\\:mm} – {d:hh\\:mm}",
                    (TimeSpan a, null) => $"Příchod {a:hh\\:mm}",
                    (null, TimeSpan d) => $"Odchod {d:hh\\:mm}",
                    _ => string.Empty
                };

                // vykreslení příchodu (pokud je)
                if (arrivalCol.HasValue)
                {
                    var arrivalIndicator = new Panel
                    {
                        Name = "indicator",
                        Size = new Size(2, rowHeight),
                        Location = new Point(arrivalX, yPos),
                        BackColor = Color.Green
                    };
                    toolTip.SetToolTip(arrivalIndicator, tt);
                    panelContainer.Controls.Add(arrivalIndicator);
                    arrivalIndicator.BringToFront();
                }

                // vykreslení odchodu (pokud je)
                if (leaveCol.HasValue)
                {
                    var leaveIndicator = new Panel
                    {
                        Name = "indicator",
                        Size = new Size(2, rowHeight),
                        Location = new Point(leaveX, yPos),
                        BackColor = Color.Red
                    };
                    toolTip.SetToolTip(leaveIndicator, tt);
                    panelContainer.Controls.Add(leaveIndicator);
                    leaveIndicator.BringToFront();
                }
            }
        }

        private (TimeSpan? roundedArrival, TimeSpan? roundedDeparture)
            RoundWorkTimeToNearestHalfHour(TimeSpan? rawArrival, TimeSpan? rawDeparture)
        {
            // nic k výpočtu
            if (!rawArrival.HasValue && !rawDeparture.HasValue)
                return (null, null);

            // helper: půlhodina AwayFromZero + ořez do intervalu [00:00, 24:00)
            static TimeSpan RoundHalfHour(TimeSpan t)
            {
                var mins = Math.Round(t.TotalMinutes / 30.0, 0, MidpointRounding.AwayFromZero) * 30.0;
                long ticks = TimeSpan.FromMinutes(mins).Ticks;
                long dayTicks = TimeSpan.FromDays(1).Ticks;

                if (ticks < 0) ticks = 0;
                if (ticks >= dayTicks) ticks = dayTicks - 1; // vyhnout se přesahu do dalšího dne

                return new TimeSpan(ticks);
            }

            // jen příchod
            if (rawArrival.HasValue && !rawDeparture.HasValue)
            {
                var ra = RoundHalfHour(rawArrival.Value);
                return (ra, null);
            }

            // jen odchod
            if (!rawArrival.HasValue && rawDeparture.HasValue)
            {
                var rd = RoundHalfHour(rawDeparture.Value);
                return (null, rd);
            }

            // oba časy -> speciální logika
            var roundedArrival = RoundHalfHour(rawArrival!.Value);

            // reálná délka
            var realDuration = rawDeparture!.Value - rawArrival.Value;
            if (realDuration < TimeSpan.Zero)
                realDuration = TimeSpan.Zero;

            // kompenzace, pokud se příchod zaokrouhlil DOLŮ (tj. roundedArrival < rawArrival)
            var arrivalCompensation = rawArrival.Value - roundedArrival;
            if (arrivalCompensation < TimeSpan.Zero)
                arrivalCompensation = TimeSpan.Zero;

            var effectiveDuration = realDuration + arrivalCompensation;

            // +5 min a zaokrouhlení délky dolů na půlhodinu
            double durWithOffset = effectiveDuration.TotalMinutes + 5;        // +5 min
            double roundedDurMin = Math.Floor(durWithOffset / 30.0) * 30.0;   // dolů na půlhodinu

            var roundedDeparture = roundedArrival + TimeSpan.FromMinutes(roundedDurMin);

            long dayMaxTicks = TimeSpan.FromDays(1).Ticks - 1;
            if (roundedDeparture.Ticks < 0) roundedDeparture = TimeSpan.Zero;
            if (roundedDeparture.Ticks > dayMaxTicks) roundedDeparture = new TimeSpan(dayMaxTicks);
            if (roundedDeparture < roundedArrival) roundedDeparture = roundedArrival;

            return (roundedArrival, roundedDeparture);
        }

        private int GetColumnIndexFromTime(TimeSpan timeOfDay, int minutesPerColumn)
        {
            return (int)(timeOfDay.TotalMinutes / minutesPerColumn);
        }


        private void tableLayoutPanel1_MouseClick(object sender, MouseEventArgs e)
        {
            var cell = GetCellAt(tableLayoutPanelCalendar, e.Location);
            pasteTargetCell = cell;

            ClearSelection();

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

                    // vypočítáme datum buňky
                    var targetDate = _selectedDate.AddDays(pasteTargetCell.Value.Row);
                    bool isLockedDay = _specialDays.Any(d => d.Date.Date == targetDate.Date && d.Locked);

                    pasteItem.Enabled = copiedEntry != null && !isLockedDay;
                }

                tableLayoutMenu.Show(tableLayoutPanelCalendar, e.Location);
            }
        }

        private async Task<bool> HandleOverlapAsync(int column, int row, int span)
        {
            var overlaps = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Where(p =>
                {
                    int c = tableLayoutPanelCalendar.GetColumn(p);
                    int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                    return !(column + span - 1 < c || column > c + s - 1);
                })
                .ToList();

            if (!overlaps.Any())
                return true;

            var result = new ReplaceOrMoveDialog().ShowDialog();
            if (result == DialogResult.Cancel)
                return false;

            if (result == DialogResult.No)
            {
                // Posun ostatní doprava (DB + UI)
                return await ShiftRightFrom(column, row, span);
            }
            else // DialogResult.Yes = Replace
            {
                // Nejprve zkontroluj, jestli mezi overlaps není svačina
                bool hasSnack = overlaps.Any(p => p.Tag as string == "snack");
                if (hasSnack)
                {
                    MessageBox.Show(
                        "Nelze nahradit záznam svačiny.",
                        "Chyba",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                    return false;
                }

                await RemoveOverlappingPanels(column, span, row);
                return true;
            }
        }


        private async Task<bool> ShiftRightFrom(int fromCol, int row, int requiredSpan)
        {
            var panelsInRow = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Select(p => new
                {
                    Panel = p,
                    Start = tableLayoutPanelCalendar.GetColumn(p),
                    Span = tableLayoutPanelCalendar.GetColumnSpan(p)
                })
                .OrderBy(x => x.Start)
                .ToList();

            int layoutWidth = tableLayoutPanelCalendar.ColumnCount;
            int cursor = fromCol + requiredSpan;
            var updateTasks = new List<Task>();

            foreach (var item in panelsInRow)
            {
                int origStart = item.Start;
                int span = item.Span;
                int origEnd = origStart + span;

                if (origEnd <= fromCol)
                    continue;

                if (origStart < cursor)
                {
                    if (cursor + span > layoutWidth)
                    {
                        AppLogger.Error("Posun není možný, došlo by k přetečení dne.");
                        return false;
                    }

                    tableLayoutPanelCalendar.SuspendLayout();
                    tableLayoutPanelCalendar.SetColumn(item.Panel, cursor);
                    tableLayoutPanelCalendar.ResumeLayout();

                    var entry = _currentEntries.FirstOrDefault(e => e.Id == item.Panel.EntryId);
                    if (entry != null)
                    {
                        entry.Timestamp = _selectedDate
                            .AddDays(row)
                            .AddMinutes(cursor * TimeSlotLengthInMinutes);
                        updateTasks.Add(_timeEntryRepo.UpdateTimeEntryAsync(entry));
                    }

                    cursor += span;
                }
                else
                {
                    cursor = origStart + span;
                }
            }

            if (updateTasks.Any())
                await Task.WhenAll(updateTasks);

            return true;
        }


        private async Task RemoveOverlappingPanels(int fromCol, int span, int row)
        {
            var toRemove = tableLayoutPanelCalendar.Controls
                .OfType<DayPanel>()
                .Where(p => tableLayoutPanelCalendar.GetRow(p) == row)
                .Where(p =>
                {
                    int c = tableLayoutPanelCalendar.GetColumn(p);
                    int s = tableLayoutPanelCalendar.GetColumnSpan(p);
                    return !(fromCol + span - 1 < c || fromCol > c + s - 1);
                })
                .ToList();

            var deletes = toRemove.Select(p => _timeEntryRepo.DeleteTimeEntryAsync(p.EntryId)).ToList();
            await Task.WhenAll(deletes);

            foreach (var panel in toRemove)
            {
                _currentEntries.RemoveAll(e => e.Id == panel.EntryId);
                RemoveEntryPanel(panel.EntryId);
            }
        }

        /// <summary>
        /// Vloží (Ctrl+V) zkopírovaný záznam s kompletním ošetřením kolizí.
        /// </summary>
        private async void PasteCopiedPanel()
        {
            if (copiedEntry == null || pasteTargetCell == null) return;
            if (_selectedUser.Id != _loggedUser.Id && _selectedUser.MasterUserId != _loggedUser.Id) return;

            var targetDate = _selectedDate.AddDays(pasteTargetCell.Value.Row);
            if (_specialDays.Any(d => d.Date.Date == targetDate.Date && d.Locked)) return;

            int column = pasteTargetCell.Value.Column;
            int row = pasteTargetCell.Value.Row;
            int span = copiedEntry.EntryMinutes / TimeSlotLengthInMinutes;
            if (column + span > tableLayoutPanelCalendar.ColumnCount) return;

            if (!await HandleOverlapAsync(column, row, span))
                return;

            DateTime ts = _selectedDate.AddDays(row)
                                       .AddMinutes(column * TimeSlotLengthInMinutes);
            if (_specialDays.Any(d => d.Date.Date == ts.Date && d.Locked)) return;

            var newEntry = new TimeEntry
            {
                EntryTypeId = copiedEntry.EntryTypeId,
                ProjectId = copiedEntry.ProjectId,
                Description = copiedEntry.Description,
                Note = copiedEntry.Note,
                EntryMinutes = copiedEntry.EntryMinutes,
                AfterCare = copiedEntry.AfterCare,
                UserId = _selectedUser.Id,
                Timestamp = ts,
                IsValid = copiedEntry.IsValid,
                IsLocked = 0
            };

            await OnNewEntryCreated(newEntry);
            await LoadSidebar();
        }
    }
}

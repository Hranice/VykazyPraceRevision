using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using VykazyPrace.Core.Database.Models;
using System.Diagnostics;
using WorkLogWpf.Views.Dialogs;

namespace WorkLogWpf.Views.Controls.WeekCalendarVertical
{
    public partial class WeekCalendar
    {
        private void RegisterBlockEvents(CalendarBlock block)
        {
            RegisterDragEvents(block);
            RegisterResizeEvents(block);

            block.MouseRightButtonUp += (s, e) =>
            {
                if (!block.CanBeEdited)
                    return;

                var targetDate = block.Entry.Timestamp.Value.Date;

                var otherRanges = CalendarGrid.Children.OfType<CalendarBlock>()
        .Where(b => b != block && b.Entry.Timestamp.HasValue)
        .Select(b => (
            Start: b.Entry.Timestamp.Value,
            End: b.Entry.Timestamp.Value.AddMinutes(b.Entry.EntryMinutes)
        ))
        .ToList();




                var dialog = new EntryDialog(block.Entry, otherRanges)
                {
                    Owner = Application.Current.MainWindow,
                };

                if (dialog.ShowDialog() == true)
                {
                    block.Entry = dialog.UpdatedEntry;
                    TrackEntryChange(block);
                }
            };

        }

        private void RegisterDragEvents(CalendarBlock block)
        {
            Point _blockCursorOffset = new Point();

            // Začátek dragování – uchopíme blok
            block.MouseLeftButtonDown += (s, e) =>
            {
                _draggedBlock = block;

                // Získáme pozici myši vůči CELÉMU CalendarGrid
                _blockCursorOffset = e.GetPosition(CalendarGrid);

                _originalCol = Grid.GetColumn(block);
                _originalRow = Grid.GetRow(block);
                block.CaptureMouse();
            };

            // Uvolnění myši – potvrzení nové pozice
            block.MouseLeftButtonUp += (s, e) =>
            {
                if (_draggedBlock != null)
                {
                    TrackEntryChange(_draggedBlock);
                    _draggedBlock.ReleaseMouseCapture();
                    _draggedBlock = null;
                    ResetResizeState();
                }
            };

            // Během pohybu myší aktualizuj pozici bloku
            block.MouseMove += (s, e) =>
            {
                if (_draggedBlock == null || !_draggedBlock.IsMouseCaptured) return;

                // Aktuální pozice kurzoru v CalendarGrid
                var pos = e.GetPosition(CalendarGrid);

                // Vypočti relativní posun myši
                double deltaY = pos.Y - _blockCursorOffset.Y;

                // Výška jednoho řádku (30 minut) – můžeš přizpůsobit, nebo zjistit z layoutu
                double rowHeight = CalendarGrid.RowDefinitions[0].ActualHeight;

                // Vypočti nový řádek jako původní + kolik řádků se ušlo
                int newRow = _originalRow + (int)Math.Round(deltaY / rowHeight);
                int newCol = GetColumnAt(pos.X);

                if (newRow < 0 || newCol < 1) return;

                int span = Grid.GetRowSpan(_draggedBlock);

                if (newRow == _originalRow && newCol == _originalCol)
                    return;

                bool hasCollision = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => b != _draggedBlock)
                    .Any(b => Grid.GetColumn(b) == newCol &&
                              RangesOverlap(newRow, newRow + span - 1,
                                            Grid.GetRow(b),
                                            Grid.GetRow(b) + Grid.GetRowSpan(b) - 1));

                if (!hasCollision && newRow + span <= CalendarGrid.RowDefinitions.Count)
                {
                    Grid.SetRow(_draggedBlock, newRow);
                    Grid.SetColumn(_draggedBlock, newCol);
                    HighlightTimeHeadersForBlock(_draggedBlock);
                }
            };
        }

        private void RegisterResizeEvents(CalendarBlock block)
        {
            // Ulož výchozí stav pro případ vracení zpět
            block.ResizeStarted += (s, e) =>
            {
                _resizingOriginalInitialBlock = block;
                _resizingOriginalBlock = block;
                _resizingStartRow = Grid.GetRow(block);
                _resizingStartSpan = Grid.GetRowSpan(block);
            };

            // Při tahu resizovacím thumbem určujeme nový konec nebo začátek
            block.ResizeDelta += (s, deltaPoint) =>
            {
                if (_resizingOriginalBlock == null) return;

                var thumb = s as FrameworkElement;
                Point absolute = Mouse.GetPosition(CalendarGrid);
                int col = Grid.GetColumn(_resizingOriginalBlock);
                int cursorRow;

                // Detekuj, který okraj bloku se resizuje
                if (thumb?.Name == "TopThumb")
                    cursorRow = GetRowAtTopEdge(absolute.Y);
                else
                    cursorRow = GetRowAt(absolute.Y);

                if (cursorRow < 0 || cursorRow >= CalendarGrid.RowDefinitions.Count) return;

                // Směruj akci na konkrétní handler
                if (thumb?.Name == "BottomThumb") HandleResizeDown(col, cursorRow);
                else if (thumb?.Name == "TopThumb") HandleResizeUp(col, cursorRow);

                // Po každém delta kroku aktualizuj entry
                if (_resizingOriginalBlock?.Entry != null)
                    TrackEntryChange(_resizingOriginalBlock);
                if (_newBlock?.Entry != null)
                    TrackEntryChange(_newBlock);
                if (_newBlockLeft?.Entry != null)
                    TrackEntryChange(_newBlockLeft);
            };

            // Po ukončení resizování zapiš změny a resetuj stav
            block.ResizeCompleted += (s, e) =>
            {
                if (_resizingOriginalBlock?.Entry != null)
                    TrackEntryChange(_resizingOriginalBlock);
                if (_newBlock?.Entry != null)
                    TrackEntryChange(_newBlock);
                if (_newBlockLeft?.Entry != null)
                    TrackEntryChange(_newBlockLeft);

                ResetResizeState();
            };
        }

        private void HandleResizeDown(int col, int cursorRow)
        {
            var block = _resizingOriginalBlock;
            int originalStart = _resizingStartRow;

            // Pokud se kurzor vrátí zpět do původního rozsahu
            if (_newBlock != null && cursorRow <= _resizingStartRow + _resizingStartSpan - 1)
            {
                RemoveNewBlock(ref _newBlock);
                _resizingOriginalBlock = block;
                _resizingStartRow = GetStart(block);
                _resizingStartSpan = Grid.GetRowSpan(block);
                return;
            }

            // Pokud už jsme vytvořili nový blok a stále ho chceme upravovat
            if (_newBlock != null)
            {
                int addedStart = GetStart(_newBlock);
                int addedSpan = cursorRow - addedStart + 1;

                if (addedSpan <= 0 || IsCollision(addedStart, addedStart + addedSpan - 1, col, _newBlock, block))
                {
                    RemoveNewBlock(ref _newBlock);
                    _resizingOriginalBlock = block;
                    _resizingStartRow = GetStart(block);
                    _resizingStartSpan = Grid.GetRowSpan(block);
                    return;
                }

                Grid.SetRowSpan(_newBlock, addedSpan);
                TrackEntryChange(_newBlock);
                return;
            }

            // Hledání kolidujících bloků
            var blocks = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && IsInColumn(b, col))
                .OrderBy(b => GetStart(b)).ToList();

            int targetEnd = cursorRow;

            var colliding = blocks
                .Where(b => RangesOverlap(originalStart, targetEnd, GetStart(b), GetEnd(b)))
                .ToList();

            if (colliding.Count > 1) return;

            if (colliding.Count == 0)
            {
                // Bez kolize – jen upravíme velikost
                int span = cursorRow - originalStart + 1;
                if (span <= 0 || originalStart + span > CalendarGrid.RowDefinitions.Count) return;

                Grid.SetRowSpan(block, span);
                TrackEntryChange(block);
                return;
            }

            // Koliduje s jedním blokem – vytvoříme nový blok za kolizí
            var collided = colliding[0];
            int collidedEnd = GetEnd(collided);

            if (blocks.Any(b => GetStart(b) > collidedEnd && GetStart(b) <= cursorRow)) return;

            int allowedSpan = GetStart(collided) - originalStart;
            Grid.SetRowSpan(block, allowedSpan);
            TrackEntryChange(block);

            int newStart = collidedEnd + 1;
            int newSpan = cursorRow - newStart + 1;

            if (newSpan <= 0 || newStart + newSpan > CalendarGrid.RowDefinitions.Count) return;

            // Vytvoř nový samostatný blok (split)
            var newEntry = new TimeEntry
            {
                UserId = _currentUser.Id,
                Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(col - 1).AddMinutes(newStart * 30),
                EntryMinutes = newSpan * 30,
                ProjectId = null,
                Description = ""
            };

            _newEntries.Add(newEntry);

            _newBlock = new CalendarBlock { Entry = newEntry };
            RegisterBlockEvents(_newBlock);
            Grid.SetColumn(_newBlock, col);
            Grid.SetRow(_newBlock, newStart);
            Grid.SetRowSpan(_newBlock, newSpan);
            CalendarGrid.Children.Add(_newBlock);
        }


        private int GetRowAtTopEdge(double y)
        {
            double accumulatedHeight = 0;
            for (int i = 0; i < CalendarGrid.RowDefinitions.Count; i++)
            {
                double rowHeight = CalendarGrid.RowDefinitions[i].ActualHeight;
                if (y < accumulatedHeight + rowHeight / 2)
                    return i;
                accumulatedHeight += rowHeight;
            }
            return CalendarGrid.RowDefinitions.Count - 1;
        }

        private void HandleResizeUp(int col, int cursorRow)
        {
            var block = _resizingOriginalBlock;
            int originEnd = _resizingStartRow + _resizingStartSpan - 1;

            // Pokud už byl vytvořen nový horní blok (split), sleduj, zda se máme vrátit
            if (_newBlockLeft != null)
            {
                var blockE = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _newBlockLeft && b != _resizingOriginalInitialBlock && IsInColumn(b, col));

                // Pokud se vracíme zpět (kurzor nad existující blok), zruš split
                if (blockE != null && cursorRow >= GetStart(blockE))
                {
                    RemoveNewBlock(ref _newBlockLeft);
                    _resizingOriginalBlock = _resizingOriginalInitialBlock;
                    _resizingStartRow = GetStart(_resizingOriginalBlock);
                    _resizingStartSpan = Grid.GetRowSpan(_resizingOriginalBlock);
                    return;
                }

                // Jinak uprav nový blok nahoře
                int fixedEnd = GetEnd(_resizingOriginalBlock);
                int newStart = Math.Max(0, cursorRow);
                int newSpan = fixedEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.RowDefinitions.Count ||
                    IsCollision(newStart, newStart + newSpan - 1, col, _newBlockLeft, _resizingOriginalBlock))
                    return;

                Grid.SetRow(_newBlockLeft, newStart);
                Grid.SetRowSpan(_newBlockLeft, newSpan);
                TrackEntryChange(_newBlockLeft);
                return;
            }

            // Hledání kolidujícího bloku, který je mezi kurzorem a původním začátkem
            var blockEToSplit = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && IsInColumn(b, col))
                .FirstOrDefault(b =>
                    RangesOverlap(cursorRow, _resizingStartRow - 1, GetStart(b), GetEnd(b))
                );

            if (blockEToSplit != null)
            {
                // Kolidující blok existuje – provedeme rozdělení
                int eStart = GetStart(blockEToSplit);
                int eEnd = GetEnd(blockEToSplit);

                int newStartRow = eEnd + 1;
                int newSpan = originEnd - newStartRow + 1;

                if (newSpan < 1 || newStartRow >= CalendarGrid.RowDefinitions.Count) return;

                // Přesun původního bloku dolů
                Grid.SetRow(block, newStartRow);
                Grid.SetRowSpan(block, newSpan);
                TrackEntryChange(block);

                // Vytvoření nového bloku nahoře (split)
                int newTopStart = cursorRow;
                int newTopSpan = eStart - newTopStart;

                if (newTopSpan < 1 || newTopStart < 0 ||
                    IsCollision(newTopStart, newTopStart + newTopSpan - 1, col, blockEToSplit, block)) return;

                var newEntry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(col - 1).AddMinutes(newTopStart * 30),
                    EntryMinutes = newTopSpan * 30,
                    ProjectId = null,
                    Description = ""
                };

                _newEntries.Add(newEntry);

                _newBlockLeft = new CalendarBlock
                {
                    Entry = newEntry
                };

                RegisterBlockEvents(_newBlockLeft);
                Grid.SetColumn(_newBlockLeft, col);
                Grid.SetRow(_newBlockLeft, newTopStart);
                Grid.SetRowSpan(_newBlockLeft, newTopSpan);
                CalendarGrid.Children.Add(_newBlockLeft);

                // Nový blok se stává "resizovaným"
                _resizingOriginalBlock = _newBlockLeft;
                _resizingStartRow = newTopStart;
                _resizingStartSpan = newTopSpan;
            }
            else
            {
                // Žádná kolize – zvětšujeme blok směrem nahoru
                int newStart = Math.Max(0, cursorRow);
                int newSpan = originEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.RowDefinitions.Count ||
                    IsCollision(newStart, newStart + newSpan - 1, col, _resizingOriginalBlock))
                    return;

                Grid.SetRow(_resizingOriginalBlock, newStart);
                Grid.SetRowSpan(_resizingOriginalBlock, newSpan);
                TrackEntryChange(_resizingOriginalBlock);
            }
        }

        public void CopySelectedBlock()
        {
            if (_selectedBlock == null) return;

            _clipboardBlock = CloneBlock(_selectedBlock);
        }

        public void PasteClipboardBlock()
        {
            if (_clipboardBlock == null || _selectedPosition == null) return;

            int row = _selectedPosition.Value.row;
            int col = _selectedPosition.Value.col;
            int span = Grid.GetRowSpan(_clipboardBlock);

            if (row + span > CalendarGrid.RowDefinitions.Count) return;

            bool collision = IsCollision(row, row + span - 1, col);
            if (collision) return;

            var newEntry = new TimeEntry
            {
                UserId = _currentUser.Id,
                Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(col - 1).AddMinutes(row * 30),
                EntryMinutes = span * 30,
                ProjectId = _clipboardBlock?.Entry?.ProjectId,
                Description = _clipboardBlock?.Entry?.Description ?? ""
            };

            _newEntries.Add(newEntry);

            var newBlock = new CalendarBlock { Entry = newEntry };

            Grid.SetRow(newBlock, row);
            Grid.SetRowSpan(newBlock, span);
            Grid.SetColumn(newBlock, col);
            RegisterBlockEvents(newBlock);
            CalendarGrid.Children.Add(newBlock);
        }


        private void ResetResizeState()
        {
            _resizingOriginalBlock = null;
            _newBlock = null;
            _newBlockLeft = null;
            _resizingStartRow = -1;
            _resizingStartSpan = -1;
        }

        private bool IsCollision(int start, int end, int row, params CalendarBlock[] excluded)
        {
            return CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => !excluded.Contains(b) && IsInRow(b, row))
                .Any(b => RangesOverlap(start, end, GetStart(b), GetEnd(b)));
        }

        public void DeleteSelectedBlock()
        {
            if (_selectedBlock != null)
            {
                if (_selectedBlock.Entry != null)
                {
                    if (_newEntries.Contains(_selectedBlock.Entry))
                        _newEntries.Remove(_selectedBlock.Entry);
                    else
                        _deletedEntries.Add(_selectedBlock.Entry);
                }

                CalendarGrid.Children.Remove(_selectedBlock);
                ClearHighlight();
            }
        }

        private void RemoveNewBlock(ref CalendarBlock block)
        {
            if (block != null)
            {
                CalendarGrid.Children.Remove(block);
                block = null;
            }
        }

        private CalendarBlock GetBlockAt(int row, int col)
        {
            return CalendarGrid.Children.OfType<CalendarBlock>()
                .FirstOrDefault(b => IsInRow(b, row) && col >= GetStart(b) && col <= GetEnd(b));
        }

        private CalendarBlock CloneBlock(CalendarBlock original)
        {
            var clone = new CalendarBlock
            {
                Entry = original.Entry != null ? new TimeEntry
                {
                    UserId = original.Entry.UserId,
                    Timestamp = original.Entry.Timestamp,
                    EntryMinutes = original.Entry.EntryMinutes,
                    ProjectId = original.Entry.ProjectId,
                    Description = original.Entry.Description
                } : null
            };

            Grid.SetColumnSpan(clone, Grid.GetColumnSpan(original));
            RegisterBlockEvents(clone);
            return clone;
        }

        private void TrackEntryChange(CalendarBlock block)
        {
            if (block?.Entry is null) return;

            int row = Grid.GetRow(block);
            int col = Grid.GetColumn(block);
            int span = Grid.GetRowSpan(block);

            DateTime monday = GetStartOfWeek(_currentWeekReference);
            DateTime day = monday.AddDays(col - 1);
            TimeSpan start = TimeSpan.FromMinutes(row * 30);

            block.Entry.Timestamp = day.Date + start;
            block.Entry.EntryMinutes = span * 30;

            if (_newEntries.Contains(block.Entry))
            {
                return;
            }

            _modifiedEntries.Add(block.Entry);
        }
    }
}

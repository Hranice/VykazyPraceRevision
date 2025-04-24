using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar
    {
        private void RegisterBlockEvents(CalendarBlock block)
        {
            RegisterDragEvents(block);
            RegisterResizeEvents(block);
        }

        private void RegisterDragEvents(CalendarBlock block)
        {
            Point _blockCursorOffset = new Point();

            block.MouseLeftButtonDown += (s, e) =>
            {
                _draggedBlock = block;
                _blockCursorOffset = e.GetPosition(block);
                _originalCol = Grid.GetColumn(block);
                _originalRow = Grid.GetRow(block);
                block.CaptureMouse();
            };

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

            block.ResizeCompleted += (s, e) =>
            {
                TrackEntryChange(_resizingOriginalBlock);
                ResetResizeState();
            };

            block.MouseMove += (s, e) =>
            {
                if (_draggedBlock == null || !_draggedBlock.IsMouseCaptured) return;

                var pos = e.GetPosition(CalendarGrid);
                double adjustedX = pos.X - _blockCursorOffset.X;
                int col = GetColumnAt(adjustedX);
                int row = GetRowAt(pos.Y);

                if (col < 0 || row <= 0) return;

                int span = Grid.GetColumnSpan(_draggedBlock);

                if (col == _originalCol && row == _originalRow)
                {
                    Grid.SetColumn(_draggedBlock, col);
                    Grid.SetRow(_draggedBlock, row);
                    return;
                }

                bool hasCollision = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => b != _draggedBlock)
                    .Any(b => Grid.GetRow(b) == row &&
                              RangesOverlap(col, col + span - 1,
                                            Grid.GetColumn(b),
                                            Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (!hasCollision && col + span <= CalendarGrid.ColumnDefinitions.Count)
                {
                    Grid.SetColumn(_draggedBlock, col);
                    Grid.SetRow(_draggedBlock, row);
                }
            };
        }

        private void RegisterResizeEvents(CalendarBlock block)
        {
            block.ResizeStarted += (s, e) =>
            {
                _resizingOriginalInitialBlock = block;
                _resizingOriginalBlock = block;
                _resizingStartCol = Grid.GetColumn(block);
                _resizingStartSpan = Grid.GetColumnSpan(block);
            };

            block.ResizeDelta += (s, deltaPoint) =>
            {
                if (_resizingOriginalBlock == null) return;

                var thumb = s as FrameworkElement;
                Point absolute = Mouse.GetPosition(CalendarGrid);
                int row = Grid.GetRow(_resizingOriginalBlock);
                int cursorCol;

                if (thumb?.Name == "LeftThumb")
                    cursorCol = GetColumnAtLeftEdge(absolute.X);
                else
                    cursorCol = GetColumnAt(absolute.X);

                if (cursorCol < 0 || cursorCol >= CalendarGrid.ColumnDefinitions.Count) return;

                if (thumb?.Name == "RightThumb") HandleResizeRight(row, cursorCol);
                else if (thumb?.Name == "LeftThumb") HandleResizeLeft(row, cursorCol);

                if (_resizingOriginalBlock?.Entry != null)
                    TrackEntryChange(_resizingOriginalBlock);
                if (_newBlock?.Entry != null)
                    TrackEntryChange(_newBlock);
                if (_newBlockLeft?.Entry != null)
                    TrackEntryChange(_newBlockLeft);
            };

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

        private void HandleResizeRight(int row, int cursorCol)
        {
            var block = _resizingOriginalBlock;
            int originalStart = _resizingStartCol;

            // Vrácení k původnímu bloku
            if (_newBlock != null && cursorCol <= _resizingStartCol + _resizingStartSpan - 1)
            {
                RemoveNewBlock(ref _newBlock);
                _resizingOriginalBlock = block;
                _resizingStartCol = GetStart(block);
                _resizingStartSpan = Grid.GetColumnSpan(block);
                return;
            }

            // Roztažení nově vytvořeného bloku
            if (_newBlock != null)
            {
                int addedStart = GetStart(_newBlock);
                int addedSpan = cursorCol - addedStart + 1;

                if (addedSpan <= 0 || IsCollision(addedStart, addedStart + addedSpan - 1, row, _newBlock, block))
                {
                    RemoveNewBlock(ref _newBlock);
                    _resizingOriginalBlock = block;
                    _resizingStartCol = GetStart(block);
                    _resizingStartSpan = Grid.GetColumnSpan(block);
                    return;
                }

                Grid.SetColumnSpan(_newBlock, addedSpan);
                TrackEntryChange(_newBlock);
                return;
            }

            // Kontrola kolizí a vytvoření nového bloku za kolidujícím
            var blocks = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && IsInRow(b, row))
                .OrderBy(b => GetStart(b)).ToList();

            int targetEnd = cursorCol;

            var colliding = blocks
                .Where(b => RangesOverlap(originalStart, targetEnd, GetStart(b), GetEnd(b)))
                .ToList();

            if (colliding.Count > 1) return;

            if (colliding.Count == 0)
            {
                int span = cursorCol - originalStart + 1;
                if (span <= 0 || originalStart + span > CalendarGrid.ColumnDefinitions.Count) return;

                Grid.SetColumnSpan(block, span);
                TrackEntryChange(block);
                return;
            }

            var collided = colliding[0];
            int collidedEnd = GetEnd(collided);

            if (blocks.Any(b => GetStart(b) > collidedEnd && GetStart(b) <= cursorCol)) return;

            int allowedSpan = GetStart(collided) - originalStart;
            Grid.SetColumnSpan(block, allowedSpan);
            TrackEntryChange(block);

            int newStart = collidedEnd + 1;
            int newSpan = cursorCol - newStart + 1;

            if (newSpan <= 0 || newStart + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

            var newEntry = new TimeEntry
            {
                UserId = _currentUser.Id,
                Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(row - 1).AddMinutes(newStart * 30),
                EntryMinutes = newSpan * 30,
                ProjectId = null,
                Description = ""
            };

            _newEntries.Add(newEntry);

            _newBlock = new CalendarBlock
            {
                Entry = newEntry
            };

            RegisterBlockEvents(_newBlock);
            Grid.SetRow(_newBlock, row);
            Grid.SetColumn(_newBlock, newStart);
            Grid.SetColumnSpan(_newBlock, newSpan);
            CalendarGrid.Children.Add(_newBlock);
        }

        private int GetColumnAtLeftEdge(double x)
        {
            double accumulatedWidth = 0;
            for (int i = 0; i < CalendarGrid.ColumnDefinitions.Count; i++)
            {
                double colWidth = CalendarGrid.ColumnDefinitions[i].ActualWidth;
                if (x < accumulatedWidth + colWidth / 2)
                    return i;
                accumulatedWidth += colWidth;
            }
            return CalendarGrid.ColumnDefinitions.Count - 1;
        }

        private void HandleResizeLeft(int row, int cursorCol)
        {
            var block = _resizingOriginalBlock;
            int originEnd = _resizingStartCol + _resizingStartSpan - 1;

            if (_newBlockLeft != null)
            {
                var blockE = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _newBlockLeft && b != _resizingOriginalInitialBlock && IsInRow(b, row));

                if (blockE != null && cursorCol >= GetStart(blockE))
                {
                    RemoveNewBlock(ref _newBlockLeft);
                    _resizingOriginalBlock = _resizingOriginalInitialBlock;
                    _resizingStartCol = GetStart(_resizingOriginalBlock);
                    _resizingStartSpan = Grid.GetColumnSpan(_resizingOriginalBlock);
                    return;
                }

                int fixedEnd = GetEnd(_resizingOriginalBlock);
                int newStart = Math.Max(0, cursorCol);
                int newSpan = fixedEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.ColumnDefinitions.Count ||
                    IsCollision(newStart, newStart + newSpan - 1, row, _newBlockLeft, _resizingOriginalBlock))
                    return;

                Grid.SetColumn(_newBlockLeft, newStart);
                Grid.SetColumnSpan(_newBlockLeft, newSpan);
                TrackEntryChange(_newBlockLeft);
                return;
            }

            var blockEToSplit = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && IsInRow(b, row))
                .FirstOrDefault(b => _resizingStartCol > GetEnd(b) && cursorCol < GetStart(b));

            if (blockEToSplit != null)
            {
                int eStart = GetStart(blockEToSplit);
                int eEnd = GetEnd(blockEToSplit);

                int newStartCol = eEnd + 1;
                int newSpan = (_resizingStartCol + _resizingStartSpan) - newStartCol;

                if (newSpan < 1 || newStartCol >= CalendarGrid.ColumnDefinitions.Count) return;

                Grid.SetColumn(block, newStartCol);
                Grid.SetColumnSpan(block, newSpan);
                TrackEntryChange(block);

                int newLeftStart = cursorCol;
                int newLeftSpan = eStart - newLeftStart;

                if (newLeftSpan < 1 || newLeftStart < 0) return;

                var newEntry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(row - 1).AddMinutes(newLeftStart * 30),
                    EntryMinutes = newLeftSpan * 30,
                    ProjectId = null,
                    Description = ""
                };

                _newEntries.Add(newEntry);

                _newBlockLeft = new CalendarBlock
                {
                    Entry = newEntry
                };

                RegisterBlockEvents(_newBlockLeft);
                Grid.SetRow(_newBlockLeft, row);
                Grid.SetColumn(_newBlockLeft, newLeftStart);
                Grid.SetColumnSpan(_newBlockLeft, newLeftSpan);
                CalendarGrid.Children.Add(_newBlockLeft);

                _resizingOriginalBlock = _newBlockLeft;
                _resizingStartCol = newLeftStart;
                _resizingStartSpan = newLeftSpan;
            }
            else
            {
                int newStart = Math.Max(0, cursorCol);
                int newSpan = originEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.ColumnDefinitions.Count ||
                    IsCollision(newStart, newStart + newSpan - 1, row, _resizingOriginalBlock))
                    return;

                Grid.SetColumn(_resizingOriginalBlock, newStart);
                Grid.SetColumnSpan(_resizingOriginalBlock, newSpan);
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
            int span = Grid.GetColumnSpan(_clipboardBlock);

            if (col + span > CalendarGrid.ColumnDefinitions.Count) return;

            bool collision = IsCollision(col, col + span - 1, row);
            if (collision) return;

            var newEntry = new TimeEntry
            {
                UserId = _currentUser.Id,
                Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(row - 1).AddMinutes(col * 30),
                EntryMinutes = span * 30,
                ProjectId = _clipboardBlock?.Entry?.ProjectId,
                Description = _clipboardBlock?.Entry?.Description ?? ""
            };

            _newEntries.Add(newEntry);

            var newBlock = new CalendarBlock
            {
                Entry = newEntry
            };

            Grid.SetColumn(newBlock, col);
            Grid.SetColumnSpan(newBlock, span);
            Grid.SetRow(newBlock, row);
            RegisterBlockEvents(newBlock);
            CalendarGrid.Children.Add(newBlock);
        }

        private void ResetResizeState()
        {
            _resizingOriginalBlock = null;
            _newBlock = null;
            _newBlockLeft = null;
            _resizingStartCol = -1;
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
            int span = Grid.GetColumnSpan(block);

            DateTime monday = GetStartOfWeek(_currentWeekReference);
            DateTime day = monday.AddDays(row - 1);
            TimeSpan start = TimeSpan.FromMinutes(col * 30);

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

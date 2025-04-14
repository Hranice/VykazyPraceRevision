using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar : UserControl
    {
        private CalendarBlock _draggedBlock = null;
        private int _originalCol, _originalRow;

        private CalendarBlock _resizingOriginalBlock;
        private CalendarBlock _resizingOriginalInitialBlock;
        private CalendarBlock _newBlock;
        private CalendarBlock _newBlockLeft;
        private Border _selectedCellHighlight = null;
        private CalendarBlock _selectedBlock = null;
        private (int row, int col)? _selectedPosition = null;
        private CalendarBlock _clipboardBlock = null;

        private int _resizingStartCol;
        private int _resizingStartSpan;

        public WeekCalendar()
        {
            InitializeComponent();
            BuildColumns();
            AddTimeHeaders();
            DrawGridLines();
        }

        private void HighlightSelectedCell(int row, int col)
        {
            ClearHighlight();

            _selectedCellHighlight = new Border
            {
                Background = new SolidColorBrush(Color.FromArgb(80, 0, 120, 215)),
                BorderBrush = Brushes.DodgerBlue,
                BorderThickness = new Thickness(1),
                Margin = new Thickness(1),
                IsHitTestVisible = false
            };

            Grid.SetRow(_selectedCellHighlight, row);
            Grid.SetColumn(_selectedCellHighlight, col);
            CalendarGrid.Children.Add(_selectedCellHighlight);

            _selectedPosition = (row, col);
        }

        private void HighlightSelectedBlock(CalendarBlock block)
        {
            ClearHighlight();

            block.BlockBorder.BorderBrush = Brushes.Red;
            block.BlockBorder.BorderThickness = new Thickness(2);

            _selectedBlock = block;
            _selectedPosition = (Grid.GetRow(block), Grid.GetColumn(block));
        }

        private void ClearHighlight()
        {
            if (_selectedCellHighlight != null)
            {
                CalendarGrid.Children.Remove(_selectedCellHighlight);
                _selectedCellHighlight = null;
            }

            if (_selectedBlock != null)
            {
                _selectedBlock.BlockBorder.BorderBrush = Brushes.DarkBlue;
                _selectedBlock.BlockBorder.BorderThickness = new Thickness(1);
                _selectedBlock = null;
            }

            _selectedPosition = null;
        }

        private void CalendarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(CalendarGrid);
            int col = GetColumnAt(pos.X);
            int row = GetRowAt(pos.Y);

            if (row <= 0 || col < 0) return;

            var clickedBlock = GetBlockAt(row, col);

            if (clickedBlock != null)
                HighlightSelectedBlock(clickedBlock);
            else
                HighlightSelectedCell(row, col);

            if (e.ClickCount == 2 && clickedBlock == null)
            {
                int desiredSpan = 2;

                var obstacle = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => IsInRow(b, row) && GetStart(b) > col)
                    .OrderBy(b => GetStart(b))
                    .FirstOrDefault();

                if (obstacle != null)
                    desiredSpan = Math.Min(desiredSpan, GetStart(obstacle) - col);

                if (desiredSpan <= 0) return;

                var block = new CalendarBlock();
                Grid.SetColumn(block, col);
                Grid.SetColumnSpan(block, desiredSpan);
                Grid.SetRow(block, row);
                CalendarGrid.Children.Add(block);
                RegisterBlockEvents(block);
            }
        }

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

            block.MouseMove += (s, e) =>
            {
                if (_draggedBlock == null || !_draggedBlock.IsMouseCaptured) return;

                var pos = e.GetPosition(CalendarGrid);
                double adjustedX = pos.X - _blockCursorOffset.X;
                int col = GetColumnAt(adjustedX);
                int row = GetRowAt(pos.Y);

                if (col >= 0 && row > 0 && (col != _originalCol || row != _originalRow))
                {
                    int span = Grid.GetColumnSpan(_draggedBlock);
                    bool hasCollision = CalendarGrid.Children.OfType<CalendarBlock>()
                        .Any(b => b != _draggedBlock && Grid.GetRow(b) == row &&
                                  RangesOverlap(col, col + span - 1, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                    if (!hasCollision && col + span <= CalendarGrid.ColumnDefinitions.Count)
                    {
                        Grid.SetColumn(_draggedBlock, col);
                        Grid.SetRow(_draggedBlock, row);
                    }
                }
            };

            block.MouseLeftButtonUp += (s, e) =>
            {
                _draggedBlock?.ReleaseMouseCapture();
                _draggedBlock = null;
                ResetResizeState();
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

                if ((s as FrameworkElement)?.Name == "LeftThumb")
                    cursorCol = GetColumnAtLeftEdge(absolute.X);
                else
                    cursorCol = GetColumnAt(absolute.X);


                if (cursorCol < 0 || cursorCol >= CalendarGrid.ColumnDefinitions.Count) return;

                if (thumb?.Name == "RightThumb") HandleResizeRight(row, cursorCol);
                else if (thumb?.Name == "LeftThumb") HandleResizeLeft(row, cursorCol);
            };

            block.ResizeCompleted += (s, e) => ResetResizeState();
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
                return;
            }

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
                return;
            }

            var collided = colliding[0];
            int collidedEnd = GetEnd(collided);

            if (blocks.Any(b => GetStart(b) > collidedEnd && GetStart(b) <= cursorCol)) return;

            int allowedSpan = GetStart(collided) - originalStart;
            Grid.SetColumnSpan(block, allowedSpan);

            int newStart = collidedEnd + 1;
            int newSpan = cursorCol - newStart + 1;

            if (newSpan <= 0 || newStart + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

            _newBlock = new CalendarBlock();
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

                int newLeftStart = cursorCol;
                int newLeftSpan = eStart - newLeftStart;

                if (newLeftSpan < 1 || newLeftStart < 0) return;

                _newBlockLeft = new CalendarBlock();
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

            var newBlock = new CalendarBlock();
            Grid.SetColumn(newBlock, col);
            Grid.SetColumnSpan(newBlock, span);
            Grid.SetRow(newBlock, row);
            RegisterBlockEvents(newBlock);
            CalendarGrid.Children.Add(newBlock);
        }

        public void DeleteSelectedBlock()
        {
            if (_selectedBlock != null)
            {
                CalendarGrid.Children.Remove(_selectedBlock);
                ClearHighlight();
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    CopySelectedBlock();
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    PasteClipboardBlock();
                    e.Handled = true;
                }
            }

            else if (e.Key == Key.Delete)
            {
                DeleteSelectedBlock();
                e.Handled = true;
            }
        }

        private void ResetResizeState()
        {
            _resizingOriginalBlock = null;
            _newBlock = null;
            _newBlockLeft = null;
            _resizingStartCol = -1;
            _resizingStartSpan = -1;
        }

        private void BuildColumns()
        {
            CalendarGrid.ColumnDefinitions.Clear();
            for (int i = 0; i < 48; i++)
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(60) });
        }

        private void DrawGridLines()
        {
            for (int r = 1; r < CalendarGrid.RowDefinitions.Count; r++)
            {
                for (int c = 0; c < CalendarGrid.ColumnDefinitions.Count; c++)
                {
                    var cellBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                        BorderThickness = new Thickness(0, 0, 1, 1),
                        Background = Brushes.Transparent,
                        IsHitTestVisible = false
                    };
                    Grid.SetRow(cellBorder, r);
                    Grid.SetColumn(cellBorder, c);
                    CalendarGrid.Children.Add(cellBorder);
                }
            }
        }


        private void AddTimeHeaders()
        {
            for (int i = 0; i < 48; i++)
            {
                if (i % 2 == 0)
                {
                    var time = TimeSpan.FromMinutes(i * 30);
                    var textBlock = new TextBlock
                    {
                        Text = $"{(int)time.TotalHours}:{time.Minutes:D2}",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };
                    Grid.SetRow(textBlock, 0);
                    Grid.SetColumn(textBlock, i);
                    CalendarGrid.Children.Add(textBlock);
                }
            }
        }

        private int GetColumnAt(double x)
        {
            double accumulatedWidth = 0;
            for (int i = 0; i < CalendarGrid.ColumnDefinitions.Count; i++)
            {
                accumulatedWidth += CalendarGrid.ColumnDefinitions[i].ActualWidth;
                if (x < accumulatedWidth) return i;
            }
            return -1;
        }

        private int GetRowAt(double y)
        {
            double accumulatedHeight = 0;
            for (int i = 0; i < CalendarGrid.RowDefinitions.Count; i++)
            {
                accumulatedHeight += CalendarGrid.RowDefinitions[i].ActualHeight;
                if (y < accumulatedHeight) return i;
            }
            return -1;
        }

        private bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }

        private int GetStart(CalendarBlock block) => Grid.GetColumn(block);
        private int GetEnd(CalendarBlock block) => GetStart(block) + Grid.GetColumnSpan(block) - 1;
        private bool IsInRow(CalendarBlock block, int row) => Grid.GetRow(block) == row;

        private bool IsCollision(int start, int end, int row, params CalendarBlock[] excluded)
        {
            return CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => !excluded.Contains(b) && IsInRow(b, row))
                .Any(b => RangesOverlap(start, end, GetStart(b), GetEnd(b)));
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
            var clone = new CalendarBlock();
            Grid.SetColumnSpan(clone, Grid.GetColumnSpan(original));
            RegisterBlockEvents(clone);
            return clone;
        }


    }
}
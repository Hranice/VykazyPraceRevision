using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar : UserControl
    {
        private CalendarBlock _draggedBlock = null;
        private int _originalCol, _originalRow;

        private int _resizingOriginalStartCol;
        private CalendarBlock _resizingOriginalBlock;
        private CalendarBlock _newBlock;
        private CalendarBlock _newBlockLeft;

        private int _resizingStartCol;
        private int _resizingStartSpan;

        public WeekCalendar()
        {
            InitializeComponent();
            BuildColumns();
            AddTimeHeaders();
        }

        private void CalendarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount == 2)
            {
                var pos = e.GetPosition(CalendarGrid);
                int col = GetColumnAt(pos.X);
                int row = GetRowAt(pos.Y);

                if (row <= 0 || col < 0) return;

                var block = new CalendarBlock();
                Grid.SetColumn(block, col);
                Grid.SetColumnSpan(block, 2);
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
                _resizingOriginalStartCol = Grid.GetColumn(block);
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
                int cursorCol = GetColumnAt(absolute.X);

                if (cursorCol < 0 || cursorCol >= CalendarGrid.ColumnDefinitions.Count) return;

                if (thumb?.Name == "RightThumb") HandleResizeRight(row, cursorCol);
                else if (thumb?.Name == "LeftThumb") HandleResizeLeft(row, cursorCol);
            };

            block.ResizeCompleted += (s, e) => ResetResizeState();
        }

        private void HandleResizeRight(int row, int cursorCol)
        {
            var block = _resizingOriginalBlock;

            if (_newBlock != null && cursorCol <= _resizingStartCol + _resizingStartSpan - 1)
            {
                CalendarGrid.Children.Remove(_newBlock);
                _newBlock = null;
                _resizingOriginalBlock = block;
                _resizingStartCol = Grid.GetColumn(block);
                _resizingStartSpan = Grid.GetColumnSpan(block);
                return;
            }

            if (_newBlock != null)
            {
                int newStart = Grid.GetColumn(_newBlock);
                int newSpan = cursorCol - newStart + 1;

                if (newSpan <= 0)
                {
                    CalendarGrid.Children.Remove(_newBlock);
                    _newBlock = null;
                    _resizingOriginalBlock = block;
                    _resizingStartCol = Grid.GetColumn(block);
                    _resizingStartSpan = Grid.GetColumnSpan(block);
                    return;
                }

                var collision = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _newBlock && b != block && Grid.GetRow(b) == row &&
                        RangesOverlap(newStart, newStart + newSpan - 1, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));
                if (collision != null) return;

                Grid.SetColumnSpan(_newBlock, newSpan);
                return;
            }

            var overlapping = CalendarGrid.Children.OfType<CalendarBlock>()
                .FirstOrDefault(b => b != block && Grid.GetRow(b) == row &&
                    RangesOverlap(_resizingStartCol, cursorCol, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

            if (overlapping != null)
            {
                int maxSpan = Math.Max(1, Grid.GetColumn(overlapping) - _resizingStartCol);
                Grid.SetColumnSpan(block, maxSpan);

                int newStart = Grid.GetColumn(overlapping) + Grid.GetColumnSpan(overlapping);
                int newSpan = cursorCol - newStart + 1;

                if (newSpan <= 0 || newStart + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

                _newBlock = new CalendarBlock();
                RegisterBlockEvents(_newBlock);
                Grid.SetRow(_newBlock, row);
                Grid.SetColumn(_newBlock, newStart);
                Grid.SetColumnSpan(_newBlock, newSpan);
                CalendarGrid.Children.Add(_newBlock);
            }
            else
            {
                int newSpan = cursorCol - _resizingStartCol + 1;
                if (newSpan <= 0 || _resizingStartCol + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

                var check = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != block && Grid.GetRow(b) == row &&
                        RangesOverlap(_resizingStartCol, _resizingStartCol + newSpan - 1,
                                      Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (check != null) return;
                Grid.SetColumnSpan(block, newSpan);
            }
        }

        private void HandleResizeLeft(int row, int cursorCol)
        {
            var block = _resizingOriginalBlock;
            int originEnd = _resizingStartCol + _resizingStartSpan - 1;

            if (_newBlockLeft != null && cursorCol >= _resizingOriginalStartCol)
            {
                CalendarGrid.Children.Remove(_newBlockLeft);
                _newBlockLeft = null;
                _resizingOriginalBlock = block;
                _resizingStartCol = Grid.GetColumn(block);
                _resizingStartSpan = Grid.GetColumnSpan(block);
                return;
            }

            if (_newBlockLeft != null)
            {
                int newStart = Math.Max(0, cursorCol);
                int newEnd = Grid.GetColumn(_resizingOriginalBlock) - 1;
                int newSpan = newEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.ColumnDefinitions.Count) return;

                var collision = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _newBlockLeft && b != _resizingOriginalBlock && Grid.GetRow(b) == row &&
                        RangesOverlap(newStart, newStart + newSpan - 1,
                            Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (collision != null) return;

                Grid.SetColumn(_newBlockLeft, newStart);
                Grid.SetColumnSpan(_newBlockLeft, newSpan);
                return;
            }

            var leftOverlap = CalendarGrid.Children.OfType<CalendarBlock>()
                .FirstOrDefault(b => b != block && Grid.GetRow(b) == row &&
                    RangesOverlap(cursorCol, originEnd,
                        Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

            if (leftOverlap != null)
            {
                int eCol = Grid.GetColumn(leftOverlap);
                int eSpan = Grid.GetColumnSpan(leftOverlap);
                int eEnd = eCol + eSpan - 1;

                int newStartCol = eEnd + 1;
                int newSpan = (_resizingStartCol + _resizingStartSpan) - newStartCol;

                if (newSpan < 1 || newStartCol >= CalendarGrid.ColumnDefinitions.Count) return;

                Grid.SetColumn(block, newStartCol);
                Grid.SetColumnSpan(block, newSpan);

                int newLeftStart = Math.Min(cursorCol, eCol - 1);
                int newLeftEnd = eCol - 1;
                int newLeftSpan = newLeftEnd - newLeftStart + 1;

                if (newLeftSpan < 1 || newLeftStart < 0 || newLeftEnd < 0) return;

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

                if (newSpan < 1 || newStart >= CalendarGrid.ColumnDefinitions.Count) return;

                var check = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _resizingOriginalBlock && Grid.GetRow(b) == row &&
                        RangesOverlap(newStart, newStart + newSpan - 1,
                            Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (check != null) return;

                Grid.SetColumn(_resizingOriginalBlock, newStart);
                Grid.SetColumnSpan(_resizingOriginalBlock, newSpan);
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
    }
}
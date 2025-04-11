using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Shapes;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar : UserControl
    {
        private CalendarBlock _draggedBlock = null;
        private Point _dragStartPoint;
        private int _originalCol, _originalRow;

        private CalendarBlock _resizingOriginalBlock;
        private CalendarBlock _newBlock;
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

        private int GetColumnAt(double x)
        {
            double accumulatedWidth = 0;
            for (int i = 0; i < CalendarGrid.ColumnDefinitions.Count; i++)
            {
                accumulatedWidth += CalendarGrid.ColumnDefinitions[i].ActualWidth;
                if (x < accumulatedWidth)
                    return i;
            }
            return -1;
        }

        private int GetRowAt(double y)
        {
            double accumulatedHeight = 0;
            for (int i = 0; i < CalendarGrid.RowDefinitions.Count; i++)
            {
                accumulatedHeight += CalendarGrid.RowDefinitions[i].ActualHeight;
                if (y < accumulatedHeight)
                    return i;
            }
            return -1;
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

        private void RegisterBlockEvents(CalendarBlock block)
        {
            Point _blockCursorOffset = new Point();

            block.MouseLeftButtonDown += (s, e) =>
            {
                _draggedBlock = block;
                _dragStartPoint = e.GetPosition(CalendarGrid);
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

                    bool hasCollision = CalendarGrid.Children
                        .OfType<CalendarBlock>()
                        .Any(b =>
                            b != _draggedBlock &&
                            Grid.GetRow(b) == row &&
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
                if (_draggedBlock != null)
                {
                    _draggedBlock.ReleaseMouseCapture();
                    _draggedBlock = null;
                }

                _resizingOriginalBlock = null;
                _newBlock = null;
                _resizingStartCol = -1;
                _resizingStartSpan = -1;
            };

            block.ResizeStarted += (s, e) =>
            {
                _resizingOriginalBlock = block;
                _resizingStartCol = Grid.GetColumn(block);
                _resizingStartSpan = Grid.GetColumnSpan(block);
            };

            block.ResizeDelta += (s, deltaPoint) =>
            {
                if (_resizingOriginalBlock == null) return;

                Point absolute = Mouse.GetPosition(CalendarGrid);
                int col = GetColumnAt(absolute.X);
                int row = Grid.GetRow(_resizingOriginalBlock);
                if (col < 0 || col >= CalendarGrid.ColumnDefinitions.Count) return;

                if (_newBlock != null && col <= _resizingStartCol + _resizingStartSpan - 1)
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
                    int newSpan = col - newStart + 1;

                    if (newSpan <= 0)
                    {
                        CalendarGrid.Children.Remove(_newBlock);
                        _newBlock = null;

                        _resizingOriginalBlock = block;
                        _resizingStartCol = Grid.GetColumn(block);
                        _resizingStartSpan = Grid.GetColumnSpan(block);
                        return;
                    }

                    if (newStart + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

                    var collision = CalendarGrid.Children
                        .OfType<CalendarBlock>()
                        .FirstOrDefault(b =>
                            b != _newBlock &&
                            b != _resizingOriginalBlock &&
                            Grid.GetRow(b) == row &&
                            RangesOverlap(newStart, newStart + newSpan - 1,
                                          Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                    if (collision != null) return;

                    Grid.SetColumnSpan(_newBlock, newSpan);
                    return;
                }

                var overlapping = CalendarGrid.Children
                    .OfType<CalendarBlock>()
                    .FirstOrDefault(b =>
                        b != _resizingOriginalBlock &&
                        Grid.GetRow(b) == row &&
                        RangesOverlap(_resizingStartCol, col,
                                      Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (overlapping != null)
                {
                    int firstBlockCol = Grid.GetColumn(overlapping);
                    int maxSpan = Math.Max(1, firstBlockCol - _resizingStartCol);
                    Grid.SetColumnSpan(_resizingOriginalBlock, maxSpan);

                    int newStart = Grid.GetColumn(overlapping) + Grid.GetColumnSpan(overlapping);
                    int newSpan = col - newStart + 1;

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
                    int newSpan = col - _resizingStartCol + 1;
                    if (newSpan <= 0 || _resizingStartCol + newSpan > CalendarGrid.ColumnDefinitions.Count) return;

                    var checkCollision = CalendarGrid.Children
                        .OfType<CalendarBlock>()
                        .FirstOrDefault(b =>
                            b != _resizingOriginalBlock &&
                            Grid.GetRow(b) == row &&
                            RangesOverlap(_resizingStartCol, _resizingStartCol + newSpan - 1,
                                          Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                    if (checkCollision != null) return;

                    Grid.SetColumnSpan(_resizingOriginalBlock, newSpan);
                }
            };

            block.ResizeCompleted += (s, e) =>
            {
                _resizingOriginalBlock = null;
                _newBlock = null;
                _resizingStartCol = -1;
                _resizingStartSpan = -1;
            };
        }




        private bool RangesOverlap(int aStart, int aEnd, int bStart, int bEnd)
        {
            return aStart <= bEnd && bStart <= aEnd;
        }
    }
}

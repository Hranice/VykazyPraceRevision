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

        private int _resizingOriginalStartCol;
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

            var clickedBlock = CalendarGrid.Children.OfType<CalendarBlock>()
                .FirstOrDefault(b => Grid.GetRow(b) == row &&
                                     col >= Grid.GetColumn(b) &&
                                     col < Grid.GetColumn(b) + Grid.GetColumnSpan(b));

            if (clickedBlock != null)
            {
                HighlightSelectedBlock(clickedBlock);
            }
            else
            {
                HighlightSelectedCell(row, col);
            }

            if (e.ClickCount == 2)
            {
                if (clickedBlock != null)
                    return;

                int desiredSpan = 2;

                var obstacle = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => Grid.GetRow(b) == row && Grid.GetColumn(b) > col)
                    .OrderBy(b => Grid.GetColumn(b))
                    .FirstOrDefault();

                if (obstacle != null)
                {
                    int obstacleStart = Grid.GetColumn(obstacle);
                    desiredSpan = Math.Min(desiredSpan, obstacleStart - col);
                }

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
                int addedStart = Grid.GetColumn(_newBlock);
                int addedSpan = cursorCol - addedStart + 1;

                if (addedSpan <= 0)
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
                        RangesOverlap(addedStart, addedStart + addedSpan - 1, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));
                if (collision != null) return;

                Grid.SetColumnSpan(_newBlock, addedSpan);
                return;
            }

            var blocks = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && Grid.GetRow(b) == row)
                .OrderBy(b => Grid.GetColumn(b))
                .ToList();

            int originalStart = _resizingStartCol;
            int targetEnd = cursorCol;

            var collidingBlocks = blocks
                .Where(b =>
                {
                    int bStart = Grid.GetColumn(b);
                    int bEnd = bStart + Grid.GetColumnSpan(b) - 1;
                    return RangesOverlap(originalStart, targetEnd, bStart, bEnd);
                })
                .ToList();

            if (collidingBlocks.Count > 1)
                return;

            if (collidingBlocks.Count == 0)
            {
                int span = cursorCol - originalStart + 1;
                if (span <= 0 || originalStart + span > CalendarGrid.ColumnDefinitions.Count)
                    return;

                Grid.SetColumnSpan(block, span);
                return;
            }

            var collided = collidingBlocks[0];
            int collidedEnd = Grid.GetColumn(collided) + Grid.GetColumnSpan(collided) - 1;

            var nextBlock = blocks.FirstOrDefault(b =>
                Grid.GetColumn(b) > collidedEnd &&
                Grid.GetColumn(b) <= cursorCol);

            if (nextBlock != null)
            {
                return;
            }

            int allowedSpan = Grid.GetColumn(collided) - originalStart;
            Grid.SetColumnSpan(block, allowedSpan);

            int addedStartAfterCollision = collidedEnd + 1;
            int addedSpanAfterCollision = cursorCol - addedStartAfterCollision + 1;

            if (addedSpanAfterCollision <= 0 || addedStartAfterCollision + addedSpanAfterCollision > CalendarGrid.ColumnDefinitions.Count)
                return;

            _newBlock = new CalendarBlock();
            RegisterBlockEvents(_newBlock);
            Grid.SetRow(_newBlock, row);
            Grid.SetColumn(_newBlock, addedStartAfterCollision);
            Grid.SetColumnSpan(_newBlock, addedSpanAfterCollision);
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
                    .FirstOrDefault(b => b != _newBlockLeft && b != _resizingOriginalInitialBlock && Grid.GetRow(b) == row);

                if (blockE != null)
                {
                    int blockEStartCol = Grid.GetColumn(blockE);
                    Console.WriteLine("[ResizeLeft RETURN CHECK]");
                    Console.WriteLine($" - cursorCol: {cursorCol}");
                    Console.WriteLine($" - blockEStartCol: {blockEStartCol}");

                    if (cursorCol >= blockEStartCol)
                    {
                        CalendarGrid.Children.Remove(_newBlockLeft);
                        _newBlockLeft = null;

                        _resizingOriginalBlock = _resizingOriginalInitialBlock;
                        _resizingStartCol = Grid.GetColumn(_resizingOriginalBlock);
                        _resizingStartSpan = Grid.GetColumnSpan(_resizingOriginalBlock);

                        Console.WriteLine(" -> cursor reached blockE -> removing _newBlockLeft");
                        return;
                    }
                }
            }

            // Resizování nově vytvořeného bloku vlevo
            if (_newBlockLeft != null)
            {
                int fixedEnd = Grid.GetColumn(_resizingOriginalBlock) + Grid.GetColumnSpan(_resizingOriginalBlock) - 1;
                int newStart = Math.Max(0, cursorCol);
                int newSpan = fixedEnd - newStart + 1;

                if (newSpan < 1 || newStart >= CalendarGrid.ColumnDefinitions.Count)
                    return;

                var collision = CalendarGrid.Children.OfType<CalendarBlock>()
                    .FirstOrDefault(b => b != _newBlockLeft && b != _resizingOriginalBlock && Grid.GetRow(b) == row &&
                        RangesOverlap(newStart, newStart + newSpan - 1,
                            Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

                if (collision != null) return;

                Grid.SetColumn(_newBlockLeft, newStart);
                Grid.SetColumnSpan(_newBlockLeft, newSpan);
                return;
            }

            // Zjištění, zda máme vytvořit nový levý blok
            var blockEToSplit = CalendarGrid.Children.OfType<CalendarBlock>()
                .Where(b => b != block && Grid.GetRow(b) == row)
                .FirstOrDefault(b =>
                {
                    int eStart = Grid.GetColumn(b);
                    int eEnd = eStart + Grid.GetColumnSpan(b) - 1;
                    return _resizingStartCol > eEnd && cursorCol < eStart;
                });

            if (blockEToSplit != null)
            {
                int eCol = Grid.GetColumn(blockEToSplit);
                int eSpan = Grid.GetColumnSpan(blockEToSplit);
                int eEnd = eCol + eSpan - 1;

                int newStartCol = eEnd + 1;
                int newSpan = (_resizingStartCol + _resizingStartSpan) - newStartCol;

                if (newSpan < 1 || newStartCol >= CalendarGrid.ColumnDefinitions.Count)
                    return;

                Grid.SetColumn(block, newStartCol);
                Grid.SetColumnSpan(block, newSpan);

                int newLeftStart = cursorCol;
                int newLeftSpan = eCol - newLeftStart;

                if (newLeftSpan < 1 || newLeftStart < 0)
                    return;

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
                // Normální resizování doleva bez kolize
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

        public void CopySelectedBlock()
        {
            if (_selectedBlock == null) return;

            _clipboardBlock = new CalendarBlock();
            _clipboardBlock.Width = _selectedBlock.ActualWidth;
            _clipboardBlock.Height = _selectedBlock.ActualHeight;
            Grid.SetColumnSpan(_clipboardBlock, Grid.GetColumnSpan(_selectedBlock));

            RegisterBlockEvents(_clipboardBlock); // důležité pro interaktivitu
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
        }


        public void PasteClipboardBlock()
        {
            if (_clipboardBlock == null || _selectedPosition == null) return;

            int targetRow = _selectedPosition.Value.row;
            int targetCol = _selectedPosition.Value.col;
            int span = Grid.GetColumnSpan(_clipboardBlock);

            // Zkontroluj, zda je v cíli dost místa bez kolizí
            bool collision = CalendarGrid.Children.OfType<CalendarBlock>()
                .Any(b => Grid.GetRow(b) == targetRow &&
                          RangesOverlap(targetCol, targetCol + span - 1, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

            if (collision || targetCol + span > CalendarGrid.ColumnDefinitions.Count)
                return;

            var pastedBlock = new CalendarBlock();
            RegisterBlockEvents(pastedBlock);
            Grid.SetColumn(pastedBlock, targetCol);
            Grid.SetColumnSpan(pastedBlock, span);
            Grid.SetRow(pastedBlock, targetRow);

            CalendarGrid.Children.Add(pastedBlock);
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
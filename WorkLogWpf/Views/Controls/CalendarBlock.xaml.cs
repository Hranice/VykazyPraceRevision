using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace WorkLogWpf.Views.Controls
{
    public partial class CalendarBlock : UserControl
    {
        private double _cursorRelativeColumn = 0;
        private bool _isMouseDown = false;
        private bool _dragStarted = false;
        private Point _dragStartPoint;

        public CalendarBlock()
        {
            InitializeComponent();

            this.MouseLeftButtonDown += CalendarBlock_MouseLeftButtonDown;
            this.MouseMove += CalendarBlock_MouseMove;
            this.MouseLeftButtonUp += CalendarBlock_MouseLeftButtonUp;

            LeftThumb.DragDelta += ResizeLeft;
            RightThumb.DragDelta += ResizeRight;
        }

        private void CalendarBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = true;
            _dragStarted = false;

            var parent = Parent as UIElement;
            var mousePos = e.GetPosition(this); // POZOR – relativní vůči bloku
            _cursorRelativeColumn = mousePos.X / ActualWidth; // v procentech

            _dragStartPoint = e.GetPosition(parent);
            var blockPos = TranslatePoint(new Point(0, 0), parent);
            _dragOffset = _dragStartPoint - blockPos;

            CaptureMouse();
            e.Handled = true;
        }

        private void CalendarBlock_MouseMove(object sender, MouseEventArgs e)
        {
            if (!_isMouseDown) return;

            var parent = Parent as Grid;
            if (parent == null) return;

            var currentPos = e.GetPosition(parent);
            var delta = currentPos - _dragStartPoint;

            if (!_dragStarted && delta.Length > 5)
                _dragStarted = true;

            if (_dragStarted)
            {
                double targetX = currentPos.X - (_cursorRelativeColumn * ActualWidth);
                int newCol = GetExactColumnAt(parent, targetX);
                int span = Grid.GetColumnSpan(this);

                if (newCol >= 0 && newCol + span <= parent.ColumnDefinitions.Count)
                {
                    {
                        Grid.SetColumn(this, newCol);

                        int newRow = GetRowAt(parent, currentPos.Y);
                        if (newRow > 0)
                            Grid.SetRow(this, newRow);
                    }
                }

            }
        }

        private int GetExactColumnAt(Grid grid, double x)
        {
            if (x < 0) return 0;

            double accWidth = 0;

            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                double colWidth = grid.ColumnDefinitions[i].ActualWidth;
                if (x >= accWidth && x < accWidth + colWidth)
                    return i;

                accWidth += colWidth;
            }

            return grid.ColumnDefinitions.Count - 1;
        }

        private void CalendarBlock_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isMouseDown = false;
            _dragStarted = false;
            ReleaseMouseCapture();
        }

        private void ResizeLeft(object sender, DragDeltaEventArgs e)
        {
            int currentCol = Grid.GetColumn(this);
            int span = Grid.GetColumnSpan(this);

            var parent = Parent as Grid;
            if (parent == null) return;

            double x = TranslatePoint(new Point(0, 0), parent).X + e.HorizontalChange;
            int newCol = GetExactColumnAt(parent, x);
            int newSpan = currentCol + span - newCol;

            if (newCol >= 0 && newSpan > 0 && newCol + newSpan <= parent.ColumnDefinitions.Count)
            {
                Grid.SetColumn(this, newCol);
                Grid.SetColumnSpan(this, newSpan);
            }
        }

        private void ResizeRight(object sender, DragDeltaEventArgs e)
        {
            int currentCol = Grid.GetColumn(this);
            int span = Grid.GetColumnSpan(this);

            var parent = Parent as Grid;
            if (parent == null) return;

            double x = TranslatePoint(new Point(ActualWidth, 0), parent).X + e.HorizontalChange;
            int newEndCol = GetExactColumnAt(parent, x);
            int newSpan = newEndCol - currentCol + 1;

            if (newSpan > 0 && currentCol + newSpan <= parent.ColumnDefinitions.Count)
            {
                Grid.SetColumnSpan(this, newSpan);
            }
        }

        private int GetColumnAt(Grid grid, double x)
        {
            double acc = 0;
            for (int i = 0; i < grid.ColumnDefinitions.Count; i++)
            {
                acc += grid.ColumnDefinitions[i].ActualWidth;
                if (x < acc)
                    return i;
            }
            return -1;
        }

        private int GetRowAt(Grid grid, double y)
        {
            double acc = 0;
            for (int i = 0; i < grid.RowDefinitions.Count; i++)
            {
                acc += grid.RowDefinitions[i].ActualHeight;
                if (y < acc)
                    return i;
            }
            return -1;
        }
    }
}

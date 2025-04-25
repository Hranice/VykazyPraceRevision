using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Diagnostics;
using System.Windows.Documents;

namespace WorkLogWpf.Views.Controls.WeekCalendarVertical
{
    public partial class WeekCalendar
    {
        private readonly List<UIElement> _highlightedTimeHeaders = new();
        private readonly List<Border> _timeHeaderBorders = new();


        private (int row, int column, int span)? GetGridPlacement(DateTime timestamp, int durationMinutes)
        {
            DayOfWeek day = timestamp.DayOfWeek;
            int column = day switch
            {
                DayOfWeek.Monday => 1,
                DayOfWeek.Tuesday => 2,
                DayOfWeek.Wednesday => 3,
                DayOfWeek.Thursday => 4,
                DayOfWeek.Friday => 5,
                DayOfWeek.Saturday => 6,
                DayOfWeek.Sunday => 7,
                _ => -1
            };

            if (column < 1 || column > 7)
                return null;

            TimeSpan time = timestamp.TimeOfDay;
            int row = (int)(time.TotalMinutes / 30); // každý řádek = 30 minut
            int span = Math.Max(1, (int)Math.Ceiling(durationMinutes / 30.0));

            if (row < 0 || row + span > CalendarGrid.RowDefinitions.Count)
                return null;

            return (row, column, span);
        }

        private void UpdateWeekLabel(DateTime monday)
        {
            var sunday = monday.AddDays(6);
            WeekLabel.Text = $"Týden: {monday:dd.MM.yyyy} – {sunday:dd.MM.yyyy}";

            var today = DateTime.Today;
            if (today >= monday && today <= sunday)
            {
                WeekLabel.FontWeight = FontWeights.Bold;
            }
            else
            {
                WeekLabel.FontWeight = FontWeights.Normal;
            }
        }

        private void HighlightTimeHeadersForBlock(CalendarBlock block)
        {
            ClearTimeHeaderHighlights();

            int startRow = Grid.GetRow(block);
            int span = Grid.GetRowSpan(block);

            for (int r = startRow; r < startRow + span; r++)
            {
                var border = _timeHeaderBorders.FirstOrDefault(b => Grid.GetRow(b) == r);
                if (border != null)
                {
                    border.Background = new SolidColorBrush(Color.FromRgb(230, 240, 255));
                    _highlightedTimeHeaders.Add(border);
                }
            }
        }

        private void ClearTimeHeaderHighlights()
        {
            foreach (var border in _highlightedTimeHeaders)
            {
                (border as Border).Background = Brushes.Transparent;
            }
            _highlightedTimeHeaders.Clear();
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
            block.Highlight();

            _selectedBlock = block;
            _selectedPosition = (Grid.GetRow(block), Grid.GetColumn(block));

            HighlightTimeHeadersForBlock(block);
        }

        private void ClearHighlight()
        {
            if (_selectedCellHighlight != null)
            {
                CalendarGrid.Children.Remove(_selectedCellHighlight);
                _selectedCellHighlight = null;
            }

            if (_selectedBlock is not null)
            {
                _selectedBlock.ClearHighlight();
                _selectedPosition = null;
            }
        }

        private void BuildRows()
        {
            CalendarGrid.RowDefinitions.Clear();
            for (int i = 0; i < 48; i++)
                CalendarGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(54) });
        }


        private void UpdateDayLabels(DateTime monday)
        {
            var today = DateTime.Today;

            string[] dayNames = { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle" };

            for (int i = 0; i < 7; i++)
            {
                DateTime date = monday.AddDays(i);
                string label = $"{dayNames[i]}\n{date:dd.MM.yyyy}";

                if (DayLabelGrid.Children[i + 1] is TextBlock textBlock)
                {
                    textBlock.Text = label;

                    if (date.Date == today)
                    {
                        textBlock.FontWeight = FontWeights.Bold;
                    }
                    else
                    {
                        textBlock.FontWeight = FontWeights.Normal;
                    }
                }
            }

            HighlightTodayColumn(monday);
        }

        private void HighlightTodayColumn(DateTime monday)
        {
            var today = DateTime.Today;
            int dayIndex = (int)today.DayOfWeek;
            if (dayIndex == 0) dayIndex = 7; // Neděle

            int column = dayIndex >= 1 && dayIndex <= 7 ? dayIndex : -1;

            if (_todayHighlight != null)
            {
                CalendarGrid.Children.Remove(_todayHighlight);
                _todayHighlight = null;
            }

            if (today >= monday && today <= monday.AddDays(6) && column > 0)
            {
                _todayHighlight = new Border
                {
                    Background = new SolidColorBrush(Color.FromRgb(230, 240, 255)),
                    IsHitTestVisible = false
                };

                Grid.SetColumn(_todayHighlight, column);
                Grid.SetRowSpan(_todayHighlight, CalendarGrid.RowDefinitions.Count);
                Panel.SetZIndex(_todayHighlight, -1);

                CalendarGrid.Children.Insert(0, _todayHighlight);
            }
        }
        private void DrawGridLines()
        {
            // Linky v hlavním gridu
            for (int r = 0; r < CalendarGrid.RowDefinitions.Count; r++)
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

            // Linky v horním gridu pro názvy dnů
            for (int c = 0; c < DayLabelGrid.ColumnDefinitions.Count; c++)
            {
                var cellBorder = new Border
                {
                    BorderBrush = new SolidColorBrush(Color.FromRgb(220, 220, 220)),
                    BorderThickness = new Thickness(0, 0, 1, 1),
                    Background = Brushes.Transparent,
                    IsHitTestVisible = false
                };

                Grid.SetColumn(cellBorder, c);
                Grid.SetRow(cellBorder, 0);
                Panel.SetZIndex(cellBorder, -100);
                DayLabelGrid.Children.Add(cellBorder);
            }

            AddScrollbarPlaceholderColumn();
        }

        private void AddScrollbarPlaceholderColumn()
        {
            var placeholder = new Border
            {
                Background = Brushes.Transparent,
                Width = SystemParameters.VerticalScrollBarWidth, // dynamická šířka
                IsHitTestVisible = false
            };

            Grid.SetColumn(placeholder, 8); // poslední sloupec (dynamicky doplněný)
            Grid.SetRow(placeholder, 0);
            DayLabelGrid.Children.Add(placeholder);
        }


        private void AddTimeHeaders()
        {
            for (int i = 0; i < 48; i++)
            {
                var time = TimeSpan.FromMinutes(i * 30);

                var textBlock = new TextBlock
                {
                    Text = $"{(int)time.TotalHours}:{time.Minutes:D2}",
                    HorizontalAlignment = HorizontalAlignment.Right,
                    Margin = new Thickness(2, 0, 5, 0),
                    VerticalAlignment = VerticalAlignment.Center
                };

                var border = new Border
                {
                    Background = Brushes.Transparent,
                    Child = textBlock
                };

                Grid.SetRow(border, i);
                Grid.SetColumn(border, 0);

                CalendarGrid.Children.Add(border);
                _timeHeaderBorders.Add(border);
            }
        }

    }
}

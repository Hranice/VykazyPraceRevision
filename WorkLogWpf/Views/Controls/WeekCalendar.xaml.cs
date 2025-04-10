using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WorkLogWpf.Views.Controls
{
    /// <summary>
    /// Interakční logika pro WeekCalendar.xaml
    /// </summary>
    public partial class WeekCalendar : UserControl
    {
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
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition
                {
                    Width = new GridLength(60)
                });
            }
        }



        private void AddTimeHeaders()
        {
            for (int i = 0; i < 48; i++)
            {
                if (i % 2 == 0) // každou hodinu
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
    }
}

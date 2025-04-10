using System;
using System.Collections.Generic;
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

        private void BuildColumns()
        {
            for (int i = 0; i < 48; i++)
            {
                CalendarGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(60) });
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

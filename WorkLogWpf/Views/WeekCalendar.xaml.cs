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

namespace WorkLogWpf.Views
{
    /// <summary>
    /// Interakční logika pro WeekCalendar.xaml
    /// </summary>
    public partial class WeekCalendar : UserControl
    {
        public WeekCalendar()
        {
            InitializeComponent();
            GenerateTimeHeaders();
            GenerateDayLabels();
        }

        private void GenerateTimeHeaders()
        {
            for (int i = 0; i < 48; i++)
            {
                if (i % 2 == 0) // každou celou hodinu
                {
                    var timeLabel = new TextBlock
                    {
                        Text = TimeSpan.FromMinutes(i * 30).ToString(@"hh\:mm"),
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        FontWeight = FontWeights.Bold
                    };

                    Grid.SetRow(timeLabel, 0);
                    Grid.SetColumn(timeLabel, i + 1); // +1 kvůli prvním názvům dnů
                    CalendarGrid.Children.Add(timeLabel);
                }
            }
        }

        private void GenerateDayLabels()
        {
            string[] days = { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota", "Neděle" };

            for (int i = 0; i < 7; i++)
            {
                var dayLabel = new TextBlock
                {
                    Text = days[i],
                    Margin = new Thickness(4),
                    VerticalAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    FontWeight = FontWeights.Bold
                };

                Grid.SetRow(dayLabel, i + 1); // +1 kvůli headeru
                Grid.SetColumn(dayLabel, 0);
                CalendarGrid.Children.Add(dayLabel);
            }
        }
    }
}

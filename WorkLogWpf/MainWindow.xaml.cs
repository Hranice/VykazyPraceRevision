using System.Windows;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using WorkLogWpf.Views.Controls.WeekCalendarVertical;

namespace WorkLogWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly TimeEntryRepository _entryRepository;
        private readonly UserRepository _userRepository;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            _entryRepository = new TimeEntryRepository();
            _userRepository = new UserRepository();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var calendar = new WeekCalendar(_entryRepository);
            MainContentGrid.Children.Add(calendar);

            var currentUser = await _userRepository.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();
            await calendar.LoadEntriesAsync(currentUser, DateTime.Today);
        }
    }
}
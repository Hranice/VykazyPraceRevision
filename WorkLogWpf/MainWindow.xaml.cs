using System.Windows;
using System.Windows.Input;
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
        private readonly ProjectRepository _projectRepository;
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepository;
        private readonly TimeEntryTypeRepository _timeEntryTypeRepository;
        private WeekCalendar weekCalendar;

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            PreviewKeyDown += GlobalKeyHandler;

            _entryRepository = new TimeEntryRepository();
            _userRepository = new UserRepository();
            _projectRepository = new ProjectRepository();
            _timeEntrySubTypeRepository = new TimeEntrySubTypeRepository();
            _timeEntryTypeRepository = new TimeEntryTypeRepository();

            weekCalendar = new WeekCalendar(_entryRepository,
                _userRepository,
                _projectRepository,
                _timeEntrySubTypeRepository,
                _timeEntryTypeRepository);
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            MainContentGrid.Children.Add(weekCalendar);

            var currentUser = await _userRepository.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();
            await weekCalendar.LoadEntriesAsync(currentUser, DateTime.Today);
        }

        private async void GlobalKeyHandler(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    weekCalendar?.CopySelectedBlock();
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    weekCalendar?.PasteClipboardBlock();
                    e.Handled = true;
                }
                else if (e.Key == Key.S)
                {
                    await weekCalendar?.SaveModifiedEntriesAsync();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete)
            {
                weekCalendar?.DeleteSelectedBlock();
                e.Handled = true;
            }
        }

    }
}
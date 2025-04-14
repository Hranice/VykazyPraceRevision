using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using WorkLogWpf.Views.Controls;

namespace WorkLogWpf
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly UserRepository _userRepo = new();

        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var currentUser = await _userRepo.GetUserByWindowsUsernameAsync(Environment.UserName) ?? new User();
            await WeekCalendar.LoadEntriesAsync(currentUser);
        }
    }
}
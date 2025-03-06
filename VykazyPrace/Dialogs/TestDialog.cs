using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class TestDialog : Form
    {
        private readonly User _currentUser;

        public TestDialog(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private void TestDialog_Load(object sender, EventArgs e)
        {
            var calendarV21 = new UserControls.CalendarV2.CalendarV2(_currentUser);
            calendarV21.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            calendarV21.Location = new Point(0, 0);
            calendarV21.Name = "calendarV21";
            calendarV21.Size = new Size(1520, 537);
            calendarV21.TabIndex = 0;

            this.Controls.Add(calendarV21);

            //panel1.Controls.Add(calendarV21);

            //List<string> days = new List<string>() { "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek"};

            //for (int i = 0; i < 7;)
            //{
            //    var panel = new Panel()
            //    {
            //        Location = new Point(3, 53),
            //        Size = new Size(67, 56)
            //    };

            //    panel.Controls.Add(new Label()
            //    {
            //        Text = 
            //    })
            //}
        }

        private void TestDialog_Shown(object sender, EventArgs e)
        {
            //BeginInvoke(new Action(() => panel1.AutoScrollPosition = new Point(370, panel1.AutoScrollPosition.Y)));
        }
    }
}

using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Logging;
using VykazyPrace.UserControls.CalendarV2;

namespace VykazyPrace.Dialogs
{
    public partial class TestDialog : Form
    {
        private readonly User _currentUser;
        private CalendarV2 _calendar;

        public TestDialog(User currentUser)
        {
            InitializeComponent();
            DoubleBuffered = true;
            _currentUser = currentUser;
            KeyPreview = true;
            this.KeyDown += Form1_KeyDown;
        }

        private async void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                await _calendar.DeleteRecord();
            }
        }

        private void TestDialog_Load(object sender, EventArgs e)
        {
            //_calendar = new UserControls.CalendarV2.CalendarV2(_currentUser);
            //_calendar.Dock = DockStyle.Fill;
            //_calendar.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            //_calendar.Location = new Point(0, 0);
            //_calendar.Name = "calendarV21";
            //_calendar.Size = new Size(1126, 620);
            //_calendar.TabIndex = 0;

            //panel3.Controls.Add(_calendar);
        }
    }
}

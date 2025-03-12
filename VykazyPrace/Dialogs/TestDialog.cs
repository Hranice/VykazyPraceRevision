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
            _currentUser = currentUser;
        }

        private void TestDialog_Load(object sender, EventArgs e)
        {
            _calendar = new UserControls.CalendarV2.CalendarV2(_currentUser);
            _calendar.Font = new Font("Reddit Sans", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 238);
            _calendar.Location = new Point(0, 0);
            _calendar.Name = "calendarV21";
            _calendar.Size = new Size(1520, 580);
            _calendar.TabIndex = 0;

            this.Controls.Add(_calendar);

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

        private void TestDialog_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (_calendar.ChangesMade)
            {
                var result = MessageBox.Show("Některé změny nebyly uloženy, chcete je uložit?", "Uložit změny", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);

                switch (result)
                {
                    case DialogResult.Yes:
                        e.Cancel = true;
                        SaveChangesAndClose();
                        e.Cancel = false;
                        break;
                    case DialogResult.No:
                        e.Cancel = false;
                        break;
                    default:
                        e.Cancel = true;
                        break;
                }
            }
        }

        private async void SaveChangesAndClose()
        {
            await _calendar.SaveChanges();
            this.Close();
        }
    }
}

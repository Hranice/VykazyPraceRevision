using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.UserControls.CalendarV2;
using static System.Windows.Forms.DataFormats;

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

            panel1.Controls.Add(calendarV21);
        }

        private void TestDialog_Shown(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => panel1.AutoScrollPosition = new Point(370, panel1.AutoScrollPosition.Y)));
        }
    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.Dialogs
{
    public partial class AboutDialog : Form
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void AboutDialog_Load(object sender, EventArgs e)
        {
            Version currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = $"Verze {currentVersion}";
        }
    }
}

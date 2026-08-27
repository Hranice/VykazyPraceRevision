using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Diagnostics;
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
            Version? currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
            labelVersion.Text = $"Verze {currentVersion}";

            buttonShowChangelog.Enabled = File.Exists(GetChangelogPath());
        }

        private void buttonShowChangelog_Click(object sender, EventArgs e)
        {
            string changelogPath = GetChangelogPath();

            if (!File.Exists(changelogPath))
            {
                MessageBox.Show("Seznam změn nebyl nalezen.", "WorkLog",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = changelogPath,
                UseShellExecute = true
            });
        }

        private static string GetChangelogPath() =>
            Path.Combine(AppContext.BaseDirectory, "Changelog.docx");
    }
}

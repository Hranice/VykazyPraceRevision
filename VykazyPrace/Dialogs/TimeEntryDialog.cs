using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntryDialog : Form
    {
        private List<string> allItems = new List<string>();

        public TimeEntryDialog()
        {
            InitializeComponent();
        }

        private void TimeEntryDialog_Load(object sender, EventArgs e)
        {
            allItems = new List<string> { "Option 1", "Option 2", "Choice 3", "Selection 4", "Alternative 5" };
            comboBox1.Items.AddRange(allItems.ToArray());

        }

        private bool isUpdating = false;

        private void comboBox1_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            isUpdating = true;
            try
            {
                string query = comboBox1.Text;
                int selectionStart = comboBox1.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBox1.DroppedDown = false;
                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(allItems.ToArray());
                    return;
                }

                // Filtrace podle libovolné části textu
                List<string> filteredItems = allItems
                    .Where(x => x.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    string originalText = comboBox1.Text;
                    int originalSelectionStart = comboBox1.SelectionStart;

                    comboBox1.Items.Clear();
                    comboBox1.Items.AddRange(filteredItems.ToArray());

                    comboBox1.Text = originalText;
                    comboBox1.SelectionStart = originalSelectionStart;
                    comboBox1.SelectionLength = 0;

                    // Otevření rozevíracího seznamu
                    if (!comboBox1.DroppedDown)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            comboBox1.DroppedDown = true;
                            Cursor = Cursors.Default;
                        }));
                    }
                }
                else
                {
                    comboBox1.DroppedDown = false;
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void comboBox1_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            isUpdating = true;
            try
            {
                if (comboBox1.SelectedItem != null)
                {
                    comboBox1.Text = comboBox1.SelectedItem.ToString();
                    comboBox1.SelectionStart = comboBox1.Text.Length;
                    comboBox1.SelectionLength = 0;
                    comboBox1.DroppedDown = false;
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label4_Click(object sender, EventArgs e)
        {

        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {

        }
    }
}

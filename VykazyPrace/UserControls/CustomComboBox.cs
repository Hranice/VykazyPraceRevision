using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace VykazyPrace.UserControls
{
    public partial class CustomComboBox : UserControl
    {
        private readonly TextBox textBox;
        private readonly ListBox listBox;
        private List<string> originalItems = new();

        public event EventHandler? ItemSelected;

        public string SelectedItem => textBox.Text;

        public CustomComboBox()
        {
            textBox = new TextBox { Dock = DockStyle.Top };
            listBox = new ListBox
            {
                Dock = DockStyle.Bottom,
                Visible = false,
                Height = 100,
                IntegralHeight = false
            };

            Controls.Add(listBox);
            Controls.Add(textBox);
            Height = textBox.Height;

            textBox.TextChanged += TextBox_TextChanged;
            textBox.KeyDown += TextBox_KeyDown;
            listBox.Click += ListBox_Click;
            listBox.MouseMove += ListBox_MouseMove;
            LostFocus += CustomComboBox_LostFocus;
        }

        public void SetItems(IEnumerable<string> items)
        {
            originalItems = items.ToList();
            listBox.Items.Clear();
            listBox.Items.AddRange(originalItems.ToArray());
        }

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            string query = textBox.Text.ToLowerInvariant();

            var filtered = originalItems
                .Where(item => item.ToLowerInvariant().Contains(query))
                .ToList();

            listBox.Items.Clear();
            listBox.Items.AddRange(filtered.ToArray());
            listBox.Visible = filtered.Count > 0;
            Height = textBox.Height + (listBox.Visible ? listBox.Height : 0);
        }

        private void ListBox_Click(object? sender, EventArgs e)
        {
            if (listBox.SelectedItem != null)
            {
                textBox.Text = listBox.SelectedItem.ToString();
                listBox.Visible = false;
                Height = textBox.Height;
                ItemSelected?.Invoke(this, EventArgs.Empty);
            }
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listBox.Visible)
            {
                listBox.Focus();
                if (listBox.Items.Count > 0)
                    listBox.SelectedIndex = 0;
            }
        }

        private void ListBox_MouseMove(object? sender, MouseEventArgs e)
        {
            int index = listBox.IndexFromPoint(e.Location);
            if (index >= 0 && index != listBox.SelectedIndex)
            {
                listBox.SelectedIndex = index;
            }
        }

        private void CustomComboBox_LostFocus(object? sender, EventArgs e)
        {
            if (!textBox.Focused && !listBox.Focused)
            {
                listBox.Visible = false;
                Height = textBox.Height;
            }
        }
    }
}

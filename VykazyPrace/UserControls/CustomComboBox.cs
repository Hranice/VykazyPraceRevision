using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

using System.Security.Permissions;

namespace VykazyPrace.UserControls
{
    public partial class CustomComboBox : UserControl
    {
        private readonly TextBox textBox;
        private readonly ListBox listBox;
        private readonly ToolTip listBoxToolTip = new();
        private List<string> originalItems = new();

        private bool suppressTextChanged = false;

        public event EventHandler? ItemSelected;

        public string SelectedItem => textBox.Text;

        private int _selectedIndex = -1;

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                if (value >= 0 && value < listBox.Items.Count)
                {
                    _selectedIndex = value;
                    string selected = listBox.Items[value].ToString()!;
                    suppressTextChanged = true;
                    textBox.Text = selected;
                    suppressTextChanged = false;
                    listBox.SelectedIndex = value;
                }
                else
                {
                    _selectedIndex = -1;
                    suppressTextChanged = true;
                    textBox.Text = string.Empty;
                    suppressTextChanged = false;
                    listBox.SelectedIndex = -1;
                }
            }
        }


        public CustomComboBox()
        {
            textBox = new TextBox { Dock = DockStyle.Fill };
            listBox = new ListBox
            {
                Visible = false,
                IntegralHeight = true,
                Height = 250
            };

            Controls.Add(textBox);
            AutoSize = false;
            Height = textBox.Height;

            textBox.TextChanged += TextBox_TextChanged;
            textBox.KeyDown += TextBox_KeyDown;
            Application.AddMessageFilter(new ClickOutsideMessageFilter(this, listBox, HideDropDown));
            textBox.LostFocus += TextBox_LostFocus;
            textBox.MouseDown += TextBox_MouseDown;
            listBox.Click += ListBox_Click;
            listBox.MouseMove += ListBox_MouseMove;
            listBox.KeyDown += ListBox_KeyDown;
        }

        public void SetItems(IEnumerable<string> items)
        {
            originalItems = items.ToList();
            listBox.Items.Clear();
            listBox.Items.AddRange(originalItems.ToArray());
        }

        public void SetText(string text)
        {
            suppressTextChanged = true;
            textBox.Text = text;
            suppressTextChanged = false;
        }

        public string GetText() => textBox.Text;

        private void TextBox_TextChanged(object? sender, EventArgs e)
        {
            if (suppressTextChanged)
                return;

            string query = textBox.Text.ToLowerInvariant();
            var filtered = originalItems.Where(item => item.ToLowerInvariant().Contains(query)).ToList();

            listBox.Items.Clear();
            listBox.Items.AddRange(filtered.ToArray());

            if (filtered.Count > 0)
            {
                ShowDropDown();
            }
            else
            {
                HideDropDown();
            }
        }

        private void ShowDropDown()
        {
            if (!listBox.Visible)
            {
                if (TopLevelControl is null) return;

                var textBoxScreenLocation = textBox.PointToScreen(Point.Empty);
                var listBoxLocation = TopLevelControl.PointToClient(new Point(textBoxScreenLocation.X, textBoxScreenLocation.Y + textBox.Height));

                int maxWidth = listBox.Items.Count > 0
                    ? listBox.Items.Cast<string>().Select(item => TextRenderer.MeasureText(item, listBox.Font).Width).Max() + SystemInformation.VerticalScrollBarWidth
                    : textBox.Width;

                listBox.Width = Math.Max(textBox.Width, maxWidth);
                listBox.Location = listBoxLocation;

                TopLevelControl.Controls.Add(listBox);
                listBox.BringToFront();
                listBox.Visible = true;
            }
        }

        private void HideDropDown()
        {
            if (listBox.Visible)
            {
                listBox.BeginInvoke(new Action(() =>
                {
                    listBox.Visible = false;
                    if (TopLevelControl?.Controls.Contains(listBox) == true)
                        TopLevelControl.Controls.Remove(listBox);
                }));
            }
        }

        private void ListBox_Click(object? sender, EventArgs e)
        {
            ConfirmSelection();
        }

        private void TextBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down && listBox.Items.Count > 0)
            {
                if (!listBox.Visible) ShowDropDown();
                listBox.Focus();
                listBox.SelectedIndex = 0;
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Up && listBox.Visible)
            {
                if (listBox.SelectedIndex > 0)
                {
                    listBox.SelectedIndex--;
                }
                e.Handled = true;
            }
            else if ((e.KeyCode == Keys.Enter || e.KeyCode == Keys.Tab) && listBox.Visible)
            {
                ConfirmSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideDropDown();
                e.Handled = true;
            }
        }

        private void ListBox_KeyDown(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                ConfirmSelection();
                e.Handled = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                HideDropDown();
                textBox.Focus();
                e.Handled = true;
            }
        }
        private void TextBox_MouseDown(object? sender, MouseEventArgs e)
        {
            if (!listBox.Visible && listBox.Items.Count > 0)
            {
                ShowDropDown();
            }
        }

        private void ConfirmSelection()
        {
            if (listBox.SelectedItem is string selected)
            {
                suppressTextChanged = true;
                textBox.Text = selected;
                suppressTextChanged = false;

                ItemSelected?.Invoke(this, EventArgs.Empty);

                textBox.Focus();
                textBox.SelectionStart = textBox.Text.Length;

                HideDropDown();
            }
        }

        private void ListBox_MouseMove(object? sender, MouseEventArgs e)
        {
            int index = listBox.IndexFromPoint(e.Location);
            if (index >= 0 && index != listBox.SelectedIndex)
            {
                listBox.SelectedIndex = index;
            }

            if (index >= 0 && index < listBox.Items.Count)
            {
                string itemText = listBox.Items[index].ToString() ?? string.Empty;
                var itemSize = TextRenderer.MeasureText(itemText, listBox.Font);

                if (itemSize.Width > listBox.ClientSize.Width)
                {
                    if (listBoxToolTip.GetToolTip(listBox) != itemText)
                    {
                        listBoxToolTip.SetToolTip(listBox, itemText);
                    }
                }
                else
                {
                    listBoxToolTip.SetToolTip(listBox, string.Empty);
                }
            }
        }

        private void TextBox_LostFocus(object? sender, EventArgs e)
        {
            BeginInvoke(new Action(() =>
            {
                if (!textBox.Focused && !listBox.Focused)
                {
                    HideDropDown();
                }
            }));
        }

        private class ClickOutsideMessageFilter : IMessageFilter
        {
            private readonly Control owner;
            private readonly Control dropdown;
            private readonly Action hideAction;

            public ClickOutsideMessageFilter(Control owner, Control dropdown, Action hideAction)
            {
                this.owner = owner;
                this.dropdown = dropdown;
                this.hideAction = hideAction;
            }

            public bool PreFilterMessage(ref Message m)
            {
                if (m.Msg == 0x201) // WM_LBUTTONDOWN
                {
                    Point mousePos = Control.MousePosition;
                    if (!owner.Bounds.Contains(owner.Parent?.PointToClient(mousePos) ?? Point.Empty) &&
                        !dropdown.Bounds.Contains(dropdown.TopLevelControl?.PointToClient(mousePos) ?? Point.Empty))
                    {
                        hideAction();
                    }
                }
                return false;
            }
        }
    }
}

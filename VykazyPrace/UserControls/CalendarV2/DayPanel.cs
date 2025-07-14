using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        public int EntryId { get; set; }
        private List<string> _lines = new();
        private Color _assignedColor;


        public DayPanel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }
        public void UpdateUi(string? title, string? subtitle)
        {
            _lines = WrapTextIntoLines($"{title}\n{subtitle}", this.Font, this.Width - 6, maxLines: 4);
            this.Invalidate();
        }

        public void SetAssignedColor(Color color)
        {
            _assignedColor = color;
            this.BackColor = color;
        }

        public void Activate()
        {
            this.BackColor = ControlPaint.Light(_assignedColor, 0.4f);
            this.Refresh();
        }

        public void Deactivate()
        {
            this.BackColor = _assignedColor; // vrať přesně tu barvu, která tam byla původně
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            g.Clear(this.BackColor);

            int padding = 3;
            float availableWidth = this.Width - padding * 2;
            float availableHeight = this.Height - padding * 2;


            if (_lines.Count == 0) return;

            SizeF oneLineSize = g.MeasureString("A", this.Font);
            float lineHeight = oneLineSize.Height;
            float y = padding;

            using Brush textBrush = new SolidBrush(this.ForeColor);
            foreach (var line in _lines)
            {
                g.DrawString(line, this.Font, textBrush,
                    new RectangleF(padding, y, availableWidth, lineHeight));
                y += lineHeight;
            }
        }
        private List<string> WrapTextIntoLines(string text, Font font, int maxWidth, int maxLines)
        {
            List<string> result = new();
            if (string.IsNullOrWhiteSpace(text)) return result;

            using Graphics g = this.CreateGraphics();

            string[] tokens = text.Replace("\r", "").Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
            List<string> words = new();

            foreach (string token in tokens)
                words.AddRange(token.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries));

            string currentLine = "";

            foreach (string word in words)
            {
                string testLine = string.IsNullOrEmpty(currentLine) ? word : currentLine + " " + word;
                SizeF testSize = g.MeasureString(testLine, font);

                if (testSize.Width > maxWidth)
                {
                    if (!string.IsNullOrEmpty(currentLine))
                    {
                        result.Add(currentLine);
                        if (result.Count >= maxLines)
                            break;
                        currentLine = "";
                    }

                    // Rozdělení dlouhého slova na více řádků
                    string remaining = word;
                    while (remaining.Length > 0)
                    {
                        int len = 1;
                        while (len <= remaining.Length)
                        {
                            string part = remaining.Substring(0, len);
                            if (g.MeasureString(part, font).Width > maxWidth)
                            {
                                len--;
                                break;
                            }
                            len++;
                        }

                        // bezpečnostní korekce
                        if (len <= 0) len = 1;
                        if (len > remaining.Length) len = remaining.Length;

                        string line = remaining.Substring(0, Math.Min(len, remaining.Length));
                        result.Add(line);
                        remaining = remaining.Substring(Math.Min(len, remaining.Length));

                        if (result.Count >= maxLines)
                            break;
                    }


                    if (result.Count >= maxLines)
                        break;
                }
                else
                {
                    currentLine = testLine;
                }
            }

            if (!string.IsNullOrEmpty(currentLine) && result.Count < maxLines)
                result.Add(currentLine);

            // Ořezání s „…“ pokud je stále text navíc
            if (result.Count > maxLines)
                result = result.GetRange(0, maxLines);

            if (result.Count == maxLines)
            {
                string last = result[^1];
                if (!last.EndsWith("…"))
                {
                    while (g.MeasureString(last + "…", font).Width > maxWidth && last.Length > 0)
                    {
                        last = last.Substring(0, last.Length - 1);
                    }
                    result[^1] = last + "…";
                }
            }
            return result;
        }
    }
}

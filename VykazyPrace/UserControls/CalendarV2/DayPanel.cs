using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        public int EntryId { get; set; }
        public int OwnerId { get; set; }

        // Flag označení (pro multiselect)
        private bool _selected;
        public bool Selected
        {
            get => _selected;
            set
            {
                _selected = value;
                Invalidate();
            }
        }

        private List<string> _titleLines = new();
        private List<string> _subtitleLines = new();
        private Color _assignedColor;

        // nově ukládáme title/subtitle, abychom je mohli přepočítat při resize
        private string? _lastTitle;
        private string? _lastSubtitle;

        public DayPanel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;

            // kdykoliv se změní velikost, přepočteme řádky
            this.SizeChanged += (s, e) =>
            {
                UpdateUi(_lastTitle, _lastSubtitle);
            };
        }

        /// <summary>
        /// Přepočte a vykreslí nové linky textu podle aktuální šířky.
        /// </summary>
        public void UpdateUi(string? title, string? subtitle)
        {
            // uložím si nové originální hodnoty
            _lastTitle = title;
            _lastSubtitle = subtitle;

            // zabalíme text do řádků podle aktuální šířky
            _titleLines = WrapTextIntoLines(title ?? "", this.Font, this.Width - 6, maxLines: 2);
            _subtitleLines = WrapTextIntoLines(subtitle ?? "", this.Font, this.Width - 6, maxLines: 2);

            // přerender
            this.Invalidate();
        }

        public void SetAssignedColor(Color color)
        {
            _assignedColor = color;
            this.BackColor = color;
        }

        public void Activate()
        {
            this.BackColor = Selected ? ControlPaint.Light(_assignedColor, 1f) : ControlPaint.Light(_assignedColor, 0.7f);
            this.Refresh();
        }

        public void Deactivate()
        {
            this.BackColor = Selected ? ControlPaint.Light(_assignedColor, 1f) : _assignedColor;
            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            var g = e.Graphics;
            g.Clear(this.BackColor);

            int padding = 3;
            float availW = this.Width - padding * 2;
            float lineH = g.MeasureString("A", this.Font).Height;
            using var b = new SolidBrush(Color.FromArgb(240, this.ForeColor));

            // vykreslíme title odshora
            float y = padding;
            foreach (var line in _titleLines)
            {
                g.DrawString(line, this.Font, b, new RectangleF(padding, y, availW, lineH));
                y += lineH;
            }

            // spočítáme, kde začíná subtitle (odspodu)
            float subtitleHeight = _subtitleLines.Count * lineH;
            float ySub = this.Height - padding - subtitleHeight - 2;
            foreach (var line in _subtitleLines)
            {
                g.DrawString(line, this.Font, b, new RectangleF(padding, ySub, availW, lineH));
                ySub += lineH;
            }


            if (Selected)
            {
                var rect = ClientRectangle;
                using var pen = new Pen(ControlPaint.Dark(_assignedColor, 0.4f), 2);
                e.Graphics.DrawRectangle(pen, rect);
            }
        }
        private List<string> WrapTextIntoLines(string text, Font font, int maxWidth, int maxLines)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(text)) return result;

            using var g = this.CreateGraphics();
            var tokens = text.Replace("\r", "").Split('\n');
            var words = tokens.SelectMany(t => t.Split(' ')).Where(w => w.Length > 0);

            string current = "";
            foreach (var w in words)
            {
                var test = string.IsNullOrEmpty(current) ? w : current + " " + w;
                if (g.MeasureString(test, font).Width > maxWidth)
                {
                    if (!string.IsNullOrEmpty(current))
                    {
                        result.Add(current);
                        if (result.Count >= maxLines) break;
                        current = "";
                    }
                    // zbytek slova
                    var rem = w;
                    while (rem.Length > 0 && result.Count < maxLines)
                    {
                        int len = 1;
                        while (len <= rem.Length && g.MeasureString(rem.Substring(0, len), font).Width <= maxWidth)
                            len++;
                        len = Math.Max(1, len - 1);
                        result.Add(rem.Substring(0, len));
                        rem = rem.Substring(len);
                    }
                    if (result.Count >= maxLines) break;
                }
                else
                {
                    current = test;
                }
            }
            if (!string.IsNullOrEmpty(current) && result.Count < maxLines)
                result.Add(current);

            // ořez a "…"
            if (result.Count > maxLines) result = result.Take(maxLines).ToList();
            if (result.Count == maxLines)
            {
                var last = result[^1];
                if (!last.EndsWith("…"))
                {
                    while (g.MeasureString(last + "…", font).Width > maxWidth && last.Length > 0)
                        last = last.Substring(0, last.Length - 1);
                    result[^1] = last + "…";
                }
            }
            return result;
        }
    }

}

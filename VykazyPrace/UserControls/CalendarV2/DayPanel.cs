using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Linq;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        public int EntryId { get; set; }
        private int _borderThickness = 0;

        private string? _title;
        private string? _subtitle;

        public DayPanel()
        {
            InitializeComponent();
            this.DoubleBuffered = true;
        }

        public void UpdateUi(string? title, string? subtitle)
        {
            _title = PrepareTrimmedText(title, this.Font, this.Width, 2);
            _subtitle = PrepareTrimmedText(subtitle, this.Font, this.Width, 2);
            Invalidate(); // Překreslí panel
        }

        private string PrepareTrimmedText(string? text, Font font, int maxWidth, int maxLines)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            string current = "";
            List<string> lines = new List<string>();

            using var g = this.CreateGraphics();
            foreach (char c in text)
            {
                string test = current + c;

                // 🟡 Použij MeasureString místo MeasureText pro přesnější šířku
                var size = g.MeasureString(test, font);

                if (size.Width > maxWidth - 8) // posun o něco víc než 6
                {
                    lines.Add(current);
                    current = c.ToString();

                    if (lines.Count == maxLines)
                        break;
                }
                else
                {
                    current = test;
                }
            }

            if (lines.Count < maxLines && !string.IsNullOrEmpty(current))
                lines.Add(current);

            int totalLength = lines.Sum(l => l.Length);
            if (totalLength < text.Length)
            {
                if (lines.Count == maxLines)
                    lines[maxLines - 1] = lines[maxLines - 1].TrimEnd() + "…";
                else
                    lines[^1] += "…";
            }

            return string.Join("\n", lines);
        }


        public void Activate()
        {
            this.Font = new Font(this.Font, FontStyle.Bold);
            _borderThickness = 1;
            Invalidate();
        }

        public void Deactivate()
        {
            this.Font = new Font(this.Font, FontStyle.Regular);
            _borderThickness = 0;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            e.Graphics.Clear(this.BackColor);

            // Vykreslení rámečku
            if (_borderThickness > 0)
            {
                using (Pen pen = new Pen(Color.Black, _borderThickness))
                {
                    e.Graphics.DrawRectangle(pen, _borderThickness / 2, _borderThickness / 2,
                        this.Width - _borderThickness * 3, this.Height - _borderThickness * 3);
                }
            }

            // Vykreslení textů
            Rectangle textArea = new Rectangle(3, 3, this.Width - 6, this.Height - 6);
            using var brush = new SolidBrush(this.ForeColor);

            string fullText = (_title ?? "") + "\n" + (_subtitle ?? "");
            TextRenderer.DrawText(e.Graphics, fullText, this.Font, textArea, this.ForeColor,
                TextFormatFlags.Top | TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
        }
    }
}

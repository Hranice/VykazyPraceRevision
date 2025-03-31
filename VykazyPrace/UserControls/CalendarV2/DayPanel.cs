namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        public int EntryId { get; set; }
        private int _borderThickness = 0;

        public DayPanel()
        {
            InitializeComponent();
        }

        public void UpdateUi(string? title, string? subtitle)
        {
            SetLabelHeightForLines(label1, 2);
            SetLabelHeightForLines(label2, 2);

            label1.Text = TrimTextToFitTwoLines(label1, title);
            label2.Text = TrimTextToFitTwoLines(label2, subtitle);
        }


        private string TrimTextToFitTwoLines(Label label, string? text)
        {
            if (string.IsNullOrEmpty(text))
                return "";

            int maxWidth = label.Width - label.Padding.Horizontal - 4;
            using var g = label.CreateGraphics();
            string current = "";
            List<string> lines = new List<string>();

            foreach (char c in text)
            {
                string test = current + c;
                Size size = TextRenderer.MeasureText(g, test, label.Font);

                if (size.Width > maxWidth)
                {
                    lines.Add(current);
                    current = c.ToString();

                    if (lines.Count == 2)
                        break;
                }
                else
                {
                    current = test;
                }
            }

            if (lines.Count < 2 && !string.IsNullOrEmpty(current))
                lines.Add(current);

            // Přidáme … pokud jsme text ořízli
            int totalLength = lines.Sum(l => l.Length);
            if (totalLength < text.Length)
            {
                if (lines.Count == 2)
                    lines[1] = lines[1].TrimEnd() + "…";
                else
                    lines[^1] += "…";
            }

            return string.Join(Environment.NewLine, lines);
        }


        private void SetLabelHeightForLines(Label label, int lineCount)
        {
            using Graphics g = label.CreateGraphics();
            Size oneLine = TextRenderer.MeasureText(g, "A", label.Font);

            int totalHeight = oneLine.Height * lineCount + label.Padding.Vertical + 2;
            label.Height = totalHeight;
        }


        public void Activate()
        {
            this.Font = new Font(this.Font, FontStyle.Bold);
            _borderThickness = 1;
        }

        public void Deactivate()
        {
            this.Font = new Font(this.Font, FontStyle.Regular);
            _borderThickness = 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (_borderThickness > 0)
            {
                using (Pen pen = new Pen(Color.Black, _borderThickness))
                {
                    e.Graphics.DrawRectangle(pen, _borderThickness / 2, _borderThickness / 2,
                        this.Width - _borderThickness * 3, this.Height - _borderThickness * 3);
                }
            }
        }
    }

}

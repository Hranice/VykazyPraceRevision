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
            label1.Text = title;
            label2.Text = subtitle;
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

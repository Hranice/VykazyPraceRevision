using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        public int EntryId { get; set; }

        public DayPanel()
        {
            InitializeComponent();
        }

        public void UpdateUi(string? title, string? subtitle)
        {
            label1.Text = title;
            label2.Text = subtitle;
        }
    }
}

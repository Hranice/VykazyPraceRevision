using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class DayPanel : UserControl
    {
        private TimeEntry _timeEntry = new TimeEntry();

        public TimeEntry TimeEntry
        {
            get
            {
                return _timeEntry;
            }
            set
            {
                _timeEntry = value;
                UpdateUi();

            }
        }

        public DayPanel()
        {
            InitializeComponent();
        }

        private void UpdateUi()
        {
            if (TimeEntry != null)
            {
                label1.Text = TimeEntry.Description;
                label2.Text = TimeEntry.Project?.ProjectDescription;
            }
        }
    }
}
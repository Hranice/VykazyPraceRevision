using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntryV2Dialog : Form
    {
        public TimeEntry TimeEntry;
        public TimeEntryV2Dialog(TimeEntry timeEntry)
        {
            InitializeComponent();
            TimeEntry = timeEntry;
        }

        private void LoadUi()
        {
            int minutesStart = TimeEntry.Timestamp.Value.Hour * 60 + TimeEntry.Timestamp.Value.Minute;
            int minutesEnd = minutesStart + TimeEntry.EntryMinutes;

            int meow = minutesStart / 30;
            comboBoxStart.SelectedIndex = minutesStart / 30;
            comboBoxEnd.SelectedIndex = minutesEnd / 30;
            textBox1.Text = TimeEntry.Description;
        }

        private void TimeEntryV2Dialog_Load(object sender, EventArgs e)
        {
            LoadUi();
        }

        private void buttonConfirm_Click(object sender, EventArgs e)
        {
            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                int.Parse(comboBoxStart.Text.Split(':')[0]),
                int.Parse(comboBoxStart.Text.Split(':')[1]), 0);

            var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                int.Parse(comboBoxEnd.Text.Split(':')[0]),
                int.Parse(comboBoxEnd.Text.Split(':')[1]), 0);

            TimeEntry = new TimeEntry()
            {
                Timestamp = start,
                EntryMinutes = GetNumberOfMinutesFromDateSpan(start, end)
            };
            // TODO: stejně jako u timeentrydialogu, jebnout sem repozitář a napojit všecko
        }

        private int GetNumberOfMinutesFromDateSpan(DateTime start, DateTime end)
        {
            return (int)(end - start).TotalMinutes;
        }

        private void comboBoxEnd_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (comboBoxStart.SelectedIndex > 0)
            {
                if (TimeDifferenceOutOfRange())
                {
                    comboBoxEnd.SelectedIndex = comboBoxStart.SelectedIndex + 1;
                }
            }
        }

        private void comboBoxStart_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBoxEnd.SelectedIndex > 0)
            {
                if (TimeDifferenceOutOfRange())
                {
                    comboBoxStart.SelectedIndex = comboBoxEnd.SelectedIndex - 1;
                }
            }
        }

        private bool TimeDifferenceOutOfRange()
        {
            int minutesStart = comboBoxStart.SelectedIndex * 30;
            int minutesEnd = comboBoxEnd.SelectedIndex * 30;

            return minutesEnd - minutesStart <= 0;
        }
    }
}

using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.Views.Dialogs
{
    public partial class EntryDialog : Window
    {
        public List<(DateTime Start, DateTime End)> _otherEntries;

        private TimeEntry _entry;
        public TimeEntry UpdatedEntry { get; private set; }


        public EntryDialog(TimeEntry entry, List<(DateTime Start, DateTime End)> otherEntries)
        {
            InitializeComponent();
            _entry = entry;
            _otherEntries = otherEntries;

            foreach (var o in _otherEntries)
            {
                Debug.WriteLine($"BLOCK: {o.Start:HH:mm:ss.fff} - {o.End:HH:mm:ss.fff}");
            }



            FillTimeCombos();
            LoadEntry();
        }

        private void FillTimeCombos()
        {
            Debug.WriteLine("!!! FillTimeCombos called");

            StartTimeCombo.SelectionChanged -= StartTimeCombo_SelectionChanged;

            var availableStarts = GetAvailableStartTimes();

            var originalStart = _entry.Timestamp.Value.TimeOfDay;

            availableStarts = availableStarts.OrderBy(t => t).ToList();
            StartTimeCombo.ItemsSource = availableStarts;

            // Pokud je původní čas v dostupných, vyber ho. Jinak nevybírej nic.
            if (availableStarts.Contains(originalStart))
                StartTimeCombo.SelectedItem = originalStart;
            else
                StartTimeCombo.SelectedIndex = -1; // nebo 0, jak chceš

            StartTimeCombo.SelectionChanged += StartTimeCombo_SelectionChanged;
            StartTimeCombo_SelectionChanged(null, null); // vygeneruj rovnou i konce
        }



        private void StartTimeCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StartTimeCombo.SelectedItem is not TimeSpan selectedStart) return;

            var availableEnds = GetAvailableEndTimes(selectedStart);
            var originalEnd = _entry.Timestamp?.TimeOfDay + TimeSpan.FromMinutes(_entry.EntryMinutes);


            if (!availableEnds.Contains(originalEnd))
                availableEnds.Add(originalEnd);

            availableEnds = availableEnds.OrderBy(t => t).ToList();
            EndTimeCombo.ItemsSource = availableEnds;
            EndTimeCombo.SelectedItem = originalEnd;
        }



        private List<TimeSpan?> GetAvailableEndTimes(TimeSpan? selectedStart)
        {
            if (_entry.Timestamp == null || selectedStart == null)
                return new List<TimeSpan?>();

            var result = new List<TimeSpan?>();
            var baseDate = _entry.Timestamp.Value.Date;
            var step = TimeSpan.FromMinutes(30);
            var startDateTime = baseDate + selectedStart.Value;

            for (var i = 1; i < 48; i++)
            {
                var end = startDateTime + TimeSpan.FromMinutes(i * 30);

                bool hasConflict = _otherEntries.Any(o =>
                    RangesOverlap(startDateTime, end, o.Start, o.End));

                if (hasConflict)
                    break;

                result.Add(end.TimeOfDay);
            }

            return result;
        }


        private List<TimeSpan?> GetAvailableStartTimes()
        {
            var result = new List<TimeSpan?>();
            if (_entry.Timestamp == null)
            {
                Debug.WriteLine("⛔ _entry.Timestamp je null – přerušuji výpočet start časů");
                return result;
            }

            var baseDate = _entry.Timestamp.Value.Date;
            var step = TimeSpan.FromMinutes(30);

            Debug.WriteLine("=== KONFLIKTNÍ BLOKY ===");
            foreach (var o in _otherEntries)
            {
                Debug.WriteLine($"BLOCK: {o.Start:HH:mm} - {o.End:HH:mm}");
            }

            Debug.WriteLine("=== TESTOVANÉ STARTY ===");

            for (int i = 0; i < 48; i++)
            {
                var start = baseDate.AddMinutes(i * 30);
                var end = start.AddMinutes(_entry.EntryMinutes);

                bool conflict = _otherEntries.Any(o =>
                {
                    bool overlap = RangesOverlap(start, end, o.Start, o.End);
                    if (overlap)
                    {
                        Debug.WriteLine($"❌ {start:HH:mm} - {end:HH:mm} koliduje s {o.Start:HH:mm} - {o.End:HH:mm}");
                    }
                    return overlap;
                });

                if (!conflict)
                {
                    Debug.WriteLine($"✅ {start:HH:mm} - {end:HH:mm} je volné");
                    result.Add(start.TimeOfDay);
                }
            }

            return result;
        }






        private void LoadEntry()
        {
            // Příklad: naplnění dat, později můžeš napojit na binding
            NoteBox.Text = _entry.Description;

            // CostCenterCombo.ItemsSource = ...
            // IndexCombo.ItemsSource = ...
        }

        private void Confirm_Click(object sender, RoutedEventArgs e)
        {
            if (StartTimeCombo.SelectedItem is not TimeSpan startTime ||
                EndTimeCombo.SelectedItem is not TimeSpan endTime)
                return;

            if (endTime <= startTime)
            {
                MessageBox.Show("Čas ukončení musí být po začátku.", "Neplatný čas", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            DateTime start = _entry.Timestamp.Value.Date + startTime;
            DateTime end = _entry.Timestamp.Value.Date + endTime;

            // Zkontroluj konflikty s jinými bloky (kromě sebe)
            bool conflict = _otherEntries.Any(o =>
                o.Start != _entry.Timestamp && RangesOverlap(start, end, o.Start, o.End));

            if (conflict)
            {
                MessageBox.Show("Zvolený čas koliduje s jiným záznamem.", "Kolize", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            _entry.Timestamp = start;
            _entry.EntryMinutes = (int)(end - start).TotalMinutes;
            _entry.Description = NoteBox.Text;

            UpdatedEntry = _entry;
            DialogResult = true;
            Close();
        }

        private static bool RangesOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
        {
            return !(aEnd <= bStart || aStart >= bEnd);
        }





        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            // Můžeš např. vracet bool nebo nastavit jinou vlastnost
            this.DialogResult = false;
            this.Close();
        }
    }
}

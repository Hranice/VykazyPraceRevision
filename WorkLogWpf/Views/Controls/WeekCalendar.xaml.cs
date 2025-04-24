using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;

namespace WorkLogWpf.Views.Controls
{
    public partial class WeekCalendar : UserControl
    {
        private CalendarBlock _draggedBlock = null;
        private int _originalCol, _originalRow;

        private CalendarBlock _resizingOriginalBlock;
        private CalendarBlock _resizingOriginalInitialBlock;
        private CalendarBlock _newBlock;
        private CalendarBlock _newBlockLeft;
        private Border _selectedCellHighlight = null;
        private Border _todayHighlight;
        private CalendarBlock _selectedBlock = null;
        private (int row, int col)? _selectedPosition = null;
        private CalendarBlock _clipboardBlock = null;

        private readonly TimeEntryRepository _timeEntryRepository;

        private DateTime _currentWeekReference = DateTime.Now;
        private User _currentUser;

        private int _resizingStartCol;
        private int _resizingStartSpan;

        private readonly HashSet<TimeEntry> _modifiedEntries = new();
        private readonly List<TimeEntry> _newEntries = new();
        private readonly List<TimeEntry> _deletedEntries = new();

        public WeekCalendar(TimeEntryRepository timeEntryRepository)
        {
            InitializeComponent();

            _timeEntryRepository = timeEntryRepository;

            BuildColumns();
            AddTimeHeaders();
            DrawGridLines();
        }

        public async Task SaveModifiedEntriesAsync()
        {
            foreach (var entry in _modifiedEntries)
            {
                await _timeEntryRepository.UpdateTimeEntryAsync(entry);
            }

            foreach (var entry in _newEntries)
            {
                await _timeEntryRepository.CreateTimeEntryAsync(entry);
            }

            foreach (var entry in _deletedEntries)
            {
                await _timeEntryRepository.DeleteTimeEntryAsync(entry.Id);
            }

            _modifiedEntries.Clear();
            _newEntries.Clear();
            _deletedEntries.Clear();
        }

        public async Task LoadEntriesAsync(User user, DateTime weekReference)
        {
            _currentUser = user;
            _currentWeekReference = weekReference;

            var (startOfWeek, endOfWeek) = GetCurrentWeekBounds(weekReference);

            var entries = await _timeEntryRepository.GetAllTimeEntriesByUserAsync(user);

            var weeklyEntries = entries
                .Where(e => e.Timestamp.HasValue &&
                            e.Timestamp.Value >= startOfWeek &&
                            e.Timestamp.Value <= endOfWeek)
                .ToList();

            // vymazat staré bloky
            var toRemove = CalendarGrid.Children.OfType<CalendarBlock>().ToList();
            foreach (var block in toRemove)
                CalendarGrid.Children.Remove(block);

            foreach (var entry in weeklyEntries)
                AddBlockFromTimeEntry(entry);

            UpdateDayLabels(startOfWeek);
            UpdateWeekLabel(startOfWeek);
        }

        private async void PreviousWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekReference = _currentWeekReference.AddDays(-7);
            await LoadEntriesAsync(_currentUser, _currentWeekReference);
        }

        private async void NextWeek_Click(object sender, RoutedEventArgs e)
        {
            _currentWeekReference = _currentWeekReference.AddDays(7);
            await LoadEntriesAsync(_currentUser, _currentWeekReference);
        }

        private void CalendarGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var pos = e.GetPosition(CalendarGrid);
            int col = GetColumnAt(pos.X);
            int row = GetRowAt(pos.Y);

            if (row <= 0 || col < 0) return;

            var clickedBlock = GetBlockAt(row, col);

            if (clickedBlock != null)
                HighlightSelectedBlock(clickedBlock);
            else
                HighlightSelectedCell(row, col);

            if (e.ClickCount == 2 && clickedBlock == null)
            {
                int desiredSpan = 2;

                var obstacle = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => IsInRow(b, row) && GetStart(b) > col)
                    .OrderBy(b => GetStart(b))
                    .FirstOrDefault();

                if (obstacle != null)
                    desiredSpan = Math.Min(desiredSpan, GetStart(obstacle) - col);

                if (desiredSpan <= 0) return;

                var entry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(row - 1).AddMinutes(col * 30),
                    EntryMinutes = desiredSpan * 30,
                    ProjectId = null,
                    Description = ""
                };

                _newEntries.Add(entry);

                var block = new CalendarBlock
                {
                    Entry = entry
                };

                Grid.SetColumn(block, col);
                Grid.SetColumnSpan(block, desiredSpan);
                Grid.SetRow(block, row);
                CalendarGrid.Children.Add(block);
                RegisterBlockEvents(block);
            }
        }

        private void AddBlockFromTimeEntry(TimeEntry entry)
        {
            if (entry.Timestamp == null) return;

            var placement = GetGridPlacement(entry.Timestamp.Value, entry.EntryMinutes);
            if (placement == null) return;

            var (row, col, span) = placement.Value;

            // Zkontroluj kolize
            bool collision = CalendarGrid.Children.OfType<CalendarBlock>()
                .Any(b => Grid.GetRow(b) == row &&
                          RangesOverlap(col, col + span - 1, Grid.GetColumn(b), Grid.GetColumn(b) + Grid.GetColumnSpan(b) - 1));

            if (collision) return;

            var block = new CalendarBlock
            {
                Entry = entry,
                ToolTip = $"{entry.Project?.ProjectTitle ?? "Neznámý projekt"}\n{entry.Description}"
            };

            Grid.SetRow(block, row);
            Grid.SetColumn(block, col);
            Grid.SetColumnSpan(block, span);
            CalendarGrid.Children.Add(block);
            RegisterBlockEvents(block);
        }

        protected override async void OnPreviewKeyDown(KeyEventArgs e)
        {
            base.OnPreviewKeyDown(e);

            if (Keyboard.Modifiers == ModifierKeys.Control)
            {
                if (e.Key == Key.C)
                {
                    CopySelectedBlock();
                    e.Handled = true;
                }
                else if (e.Key == Key.V)
                {
                    PasteClipboardBlock();
                    e.Handled = true;
                }
                else if (e.Key == Key.S)
                {
                    await SaveModifiedEntriesAsync();
                    e.Handled = true;
                }
            }
            else if (e.Key == Key.Delete)
            {
                DeleteSelectedBlock();
                e.Handled = true;
            }
        }
    }
}
﻿using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;

namespace WorkLogWpf.Views.Controls.WeekCalendarVertical
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

        private List<Project> _projects = new List<Project>();
        private List<TimeEntrySubType> _timeEntrySubTypes = new List<TimeEntrySubType>();
        private List<TimeEntryType> _timeEntryTypes = new List<TimeEntryType>();
        private readonly TimeEntryRepository _timeEntryRepository;
        private readonly ProjectRepository _projectRepository = new ProjectRepository();
        private readonly TimeEntrySubTypeRepository _timeEntrySubTypeRepository = new TimeEntrySubTypeRepository();
        private readonly TimeEntryTypeRepository _timeEntryTypeRepository = new TimeEntryTypeRepository();
        private readonly UserRepository _userRepository = new UserRepository();

        private DateTime _currentWeekReference = DateTime.Now;
        private User _currentUser;

        private int _resizingStartRow;
        private int _resizingStartSpan;

        private readonly HashSet<TimeEntry> _modifiedEntries = new();
        private readonly List<TimeEntry> _newEntries = new();
        private readonly List<TimeEntry> _deletedEntries = new();

        public WeekCalendar(TimeEntryRepository timeEntryRepository,
     UserRepository userRepository,
     ProjectRepository projectRepository,
     TimeEntrySubTypeRepository timeEntrySubTypeRepository,
     TimeEntryTypeRepository timeEntryTypeRepository)
        {
            InitializeComponent();

            _timeEntryRepository = timeEntryRepository;
            _userRepository = userRepository;
            _projectRepository = projectRepository;
            _timeEntrySubTypeRepository = timeEntrySubTypeRepository;
            _timeEntryTypeRepository = timeEntryTypeRepository;

            BuildRows();
            AddTimeHeaders();
            DrawGridLines();

            this.Loaded += WeekCalendar_Loaded;
        }

        private async void WeekCalendar_Loaded(object sender, RoutedEventArgs e)
        {
            await LoadDataAsync();
        }

        public async Task SaveModifiedEntriesAsync()
        {
            foreach (var entry in _modifiedEntries)
            {
                Debug.WriteLine($"Saving {entry.Timestamp} / {entry.EntryMinutes}");
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

            var entries = await _timeEntryRepository.GetAllTimeEntriesByUserAsync(user, true);

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

        private async Task LoadDataAsync()
        {
            _projects = await _projectRepository.GetAllFullProjectsAndPreProjectsAsync();
            _timeEntrySubTypes = await _timeEntrySubTypeRepository.GetAllTimeEntrySubTypesByUserIdAsync(_currentUser.Id);
            _timeEntryTypes = await _timeEntryTypeRepository.GetAllTimeEntryTypesAsync();
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
            int row = GetRowAt(pos.Y);
            int col = GetColumnAt(pos.X);

            if (row < 0 || col < 1) return;

            var element = e.OriginalSource as FrameworkElement;
            var blockUnderCursor = element?.DataContext as CalendarBlock
                ?? FindParent<CalendarBlock>(element);

            if (blockUnderCursor != null)
            {
                HighlightSelectedBlock(blockUnderCursor);
                return;
            }

            var clickedBlock = GetBlockAt(row, col);

            if (clickedBlock != null)
            {
                HighlightSelectedBlock(clickedBlock);
            }
            else
            {
                HighlightSelectedCell(row, col);
            }


            if (e.ClickCount == 2 && clickedBlock == null)
            {
                int desiredSpan = 2;

                var obstacle = CalendarGrid.Children.OfType<CalendarBlock>()
                    .Where(b => IsInColumn(b, col) && GetStart(b) > row)
                    .OrderBy(b => GetStart(b))
                    .FirstOrDefault();

                if (obstacle != null)
                    desiredSpan = Math.Min(desiredSpan, GetStart(obstacle) - row);

                if (desiredSpan <= 0) return;

                var entry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    Timestamp = GetStartOfWeek(_currentWeekReference).AddDays(col - 1).AddMinutes(row * 30),
                    EntryMinutes = desiredSpan * 30,
                    ProjectId = null,
                    Description = ""
                };

                _newEntries.Add(entry);

                var block = new CalendarBlock
                {
                    Entry = entry
                };

                Grid.SetRow(block, row);
                Grid.SetRowSpan(block, desiredSpan);
                Grid.SetColumn(block, col);
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
                .Any(b => Grid.GetColumn(b) == col &&
                          RangesOverlap(row, row + span - 1, Grid.GetRow(b), Grid.GetRow(b) + Grid.GetRowSpan(b) - 1));

            if (collision) return;

            var block = new CalendarBlock
            {
                Entry = entry,
                ToolTip = $"{entry.Project?.ProjectTitle ?? "Neznámý projekt"}\n{entry.Description}"
            };

            Grid.SetRow(block, row);
            Grid.SetRowSpan(block, span);
            Grid.SetColumn(block, col);
            CalendarGrid.Children.Add(block);
            RegisterBlockEvents(block);
        }
    }
}
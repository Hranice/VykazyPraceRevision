using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.ViewModels
{
    public class EntryDialogViewModel : INotifyPropertyChanged
    {
        private readonly TimeEntry _originalEntry;
        private readonly List<(DateTime Start, DateTime End)> _otherEntries;

        public event PropertyChangedEventHandler? PropertyChanged;
        public event EventHandler<bool?>? RequestClose;

        public TimeEntry UpdatedEntry { get; private set; }

        public ObservableCollection<TimeSpan> AvailableStartTimes { get; } = new();
        public ObservableCollection<TimeSpan> AvailableEndTimes { get; } = new();

        private TimeSpan? _selectedStartTime;
        public TimeSpan? SelectedStartTime
        {
            get => _selectedStartTime;
            set
            {
                if (_selectedStartTime != value)
                {
                    _selectedStartTime = value;
                    OnPropertyChanged();
                    LoadEndTimes();
                }
            }
        }

        private TimeSpan? _selectedEndTime;
        public TimeSpan? SelectedEndTime
        {
            get => _selectedEndTime;
            set
            {
                _selectedEndTime = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Project> AvailableProjects { get; }
        private Project? _selectedProject;
        public Project? SelectedProject
        {
            get => _selectedProject;
            set { _selectedProject = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TimeEntrySubType> AvailableSubTypes { get; }
        private TimeEntrySubType? _selectedSubType;
        public TimeEntrySubType? SelectedSubType
        {
            get => _selectedSubType;
            set { _selectedSubType = value; OnPropertyChanged(); }
        }

        public ObservableCollection<TimeEntryType> AvailableEntryTypes { get; }
        private TimeEntryType? _selectedEntryType;
        public TimeEntryType? SelectedEntryType
        {
            get => _selectedEntryType;
            set { _selectedEntryType = value; OnPropertyChanged(); }
        }

        public string? Note { get; set; }

        public bool IsCategoryProvoz { get; set; }
        public bool IsCategoryProjekt { get; set; }
        public bool IsCategorySkoleni { get; set; }
        public bool IsCategoryNepritomnost { get; set; }
        public bool IsCategoryOstatni { get; set; }

        public bool IsEntryTypePrace { get; set; }
        public bool IsEntryTypeAdministrativa { get; set; }
        public bool IsEntryTypeMeeting { get; set; }

        public ICommand ConfirmCommand { get; }
        public ICommand DeleteCommand { get; }

        public EntryDialogViewModel(
            TimeEntry entry,
            List<(DateTime Start, DateTime End)> otherEntries,
            List<Project> projects,
            List<TimeEntrySubType> subTypes,
            List<TimeEntryType> entryTypes)
        {
            _originalEntry = entry;
            _otherEntries = otherEntries;
            UpdatedEntry = new TimeEntry
            {
                Id = entry.Id,
                Timestamp = entry.Timestamp,
                EntryMinutes = entry.EntryMinutes,
                Description = entry.Description,
                UserId = entry.UserId,
                ProjectId = entry.ProjectId,
                EntryTypeId = entry.EntryTypeId,
                Note = entry.Note,
                IsLocked = entry.IsLocked,
                IsValid = entry.IsValid
            };

            AvailableProjects = new ObservableCollection<Project>(projects);
            SelectedProject = projects.FirstOrDefault(p => p.Id == entry.ProjectId);

            AvailableSubTypes = new ObservableCollection<TimeEntrySubType>(subTypes);
            SelectedSubType = subTypes.FirstOrDefault();

            AvailableEntryTypes = new ObservableCollection<TimeEntryType>(entryTypes);
            SelectedEntryType = entryTypes.FirstOrDefault(t => t.Id == entry.EntryTypeId);

            Note = entry.Description;

            ConfirmCommand = new RelayCommand(_ => Confirm());
            DeleteCommand = new RelayCommand(_ => RequestClose?.Invoke(this, false));

            LoadStartTimes();
        }

        private void LoadStartTimes()
        {
            AvailableStartTimes.Clear();
            var list = GetAvailableStartTimes();
            foreach (var t in list) AvailableStartTimes.Add(t);

            var originalStart = _originalEntry.Timestamp?.TimeOfDay;
            if (originalStart.HasValue && list.Contains(originalStart.Value))
                SelectedStartTime = originalStart.Value;
        }

        private void LoadEndTimes()
        {
            AvailableEndTimes.Clear();
            if (SelectedStartTime == null) return;

            var originalEnd = _originalEntry.Timestamp?.TimeOfDay + TimeSpan.FromMinutes(_originalEntry.EntryMinutes);
            var ends = GetAvailableEndTimes(SelectedStartTime.Value);
            if (originalEnd.HasValue && !ends.Contains(originalEnd.Value))
                ends.Add(originalEnd.Value);

            foreach (var t in ends.OrderBy(t => t)) AvailableEndTimes.Add(t);
            SelectedEndTime = originalEnd;
        }

        private List<TimeSpan> GetAvailableStartTimes()
        {
            var result = new List<TimeSpan>();
            var baseDate = _originalEntry.Timestamp.Value.Date;

            for (int i = 0; i < 48; i++)
            {
                var start = baseDate.AddMinutes(i * 30);
                var end = start.AddMinutes(_originalEntry.EntryMinutes);
                if (!_otherEntries.Any(o => RangesOverlap(start, end, o.Start, o.End)))
                    result.Add(start.TimeOfDay);
            }

            return result;
        }

        private List<TimeSpan> GetAvailableEndTimes(TimeSpan selectedStart)
        {
            var result = new List<TimeSpan>();
            var baseDate = _originalEntry.Timestamp.Value.Date;
            var startDateTime = baseDate + selectedStart;

            for (var i = 1; i < 48; i++)
            {
                var end = startDateTime.AddMinutes(i * 30);
                if (_otherEntries.Any(o => RangesOverlap(startDateTime, end, o.Start, o.End)))
                    break;

                result.Add(end.TimeOfDay);
            }

            return result;
        }

        private static bool RangesOverlap(DateTime aStart, DateTime aEnd, DateTime bStart, DateTime bEnd)
            => !(aEnd <= bStart || aStart >= bEnd);

        private void Confirm()
        {
            if (!SelectedStartTime.HasValue || !SelectedEndTime.HasValue)
                return;

            var start = _originalEntry.Timestamp.Value.Date + SelectedStartTime.Value;
            var end = _originalEntry.Timestamp.Value.Date + SelectedEndTime.Value;

            if (end <= start || _otherEntries.Any(o => o.Start != _originalEntry.Timestamp && RangesOverlap(start, end, o.Start, o.End)))
                return;

            UpdatedEntry.Timestamp = start;
            UpdatedEntry.EntryMinutes = (int)(end - start).TotalMinutes;
            UpdatedEntry.Description = Note;
            UpdatedEntry.Project = SelectedProject;
            UpdatedEntry.EntryType = SelectedEntryType;

            RequestClose?.Invoke(this, true);
        }

        private void OnPropertyChanged([CallerMemberName] string? name = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }

    public class RelayCommand : ICommand
    {
        private readonly Action<object?> _execute;
        private readonly Predicate<object?>? _canExecute;

        public RelayCommand(Action<object?> execute, Predicate<object?>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute?.Invoke(parameter) ?? true;
        public void Execute(object? parameter) => _execute(parameter);
        public event EventHandler? CanExecuteChanged;
        public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}

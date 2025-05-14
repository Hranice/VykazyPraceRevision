using System;
using System.Collections.Generic;
using System.Windows;
using WorkLogWpf.ViewModels;
using VykazyPrace.Core.Database.Models;

namespace WorkLogWpf.Views.Dialogs
{
    public partial class EntryDialog : Window
    {
        public TimeEntry UpdatedEntry => _viewModel.UpdatedEntry;

        private readonly EntryDialogViewModel _viewModel;

        public EntryDialog(
            TimeEntry entry,
            List<(DateTime Start, DateTime End)> otherEntries,
            List<Project> availableProjects,
            List<TimeEntrySubType> availableSubTypes,
            List<TimeEntryType> availableEntryTypes)
        {
            InitializeComponent();

            _viewModel = new EntryDialogViewModel(entry, otherEntries, availableProjects, availableSubTypes, availableEntryTypes);
            _viewModel.RequestClose += ViewModel_RequestClose;

            DataContext = _viewModel;
        }

        private void ViewModel_RequestClose(object? sender, bool? result)
        {
            DialogResult = result;
            Close();
        }
    }
}

using System.Data;
using System.Diagnostics;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntryDialog : Form
    {
        private int _projectType = 0;
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private List<string> _projectsFormattedToString = new List<string>();
        private readonly User _currentUser;
        private DateTime _currentDate;
        private int _minutesCount = 0;

        public TimeEntryDialog(User currentUser, DateTime currentDate)
        {
            InitializeComponent();

            _currentUser = currentUser;
            _currentDate = currentDate;
        }

        private void TimeEntryDialog_Load(object sender, EventArgs e)
        {
            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);
            
            Task.Run(LoadData);
        }

        private async Task LoadData()
        {
            Invoke(() => _loadingUC.BringToFront());

            var projectsTask = LoadProjectsContractsAsync();
            var timeEntriesTask = LoadTimeEntriesAsync();

            await Task.WhenAll(projectsTask, timeEntriesTask);

            Invoke(() => _loadingUC.Visible = false);
        }

        private async Task LoadTimeEntriesAsync()
        {
            try
            {
                var timeEntries = await _timeEntryRepo.GetAllTimeEntriesByUserAsync(_currentUser, _projectType);

                Invoke(() =>
                {
                    listBoxTimeEntries.Items.Clear();
                    listBoxTimeEntries.Items.AddRange(timeEntries.Select(FormatTimeEntryToString).ToArray());
                });
            }
            catch (Exception ex)
            {
                Invoke(() => AppLogger.Error("Chyba při načítání seznamu zapsaných hodin.", ex));
            }
        }


        private async Task LoadProjectsContractsAsync()
        {
            try
            {
                List<Project> projects = await _projectRepo.GetAllProjectsAndContractsAsync(_projectType);

                Invoke(new Action(() =>
                {
                    comboBoxProjectsContracts.Items.Clear();
                    _projectsFormattedToString.Clear();
                    foreach (var project in projects)
                    {
                        _projectsFormattedToString.Add(FormatProjectToString(project));
                    }

                    comboBoxProjectsContracts.Items.AddRange(_projectsFormattedToString.ToArray());
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání projektů.", ex);
                }));
            }
        }

        private async void VisualiseSelectedProjectOrContract()
        {
            switch (_projectType)
            {
                case 0:
                    buttonProject.BackColor = Color.White;
                    buttonProject.Font = new Font(buttonProject.Font, FontStyle.Bold);
                    buttonContract.BackColor = Color.FromKnownColor(KnownColor.AppWorkspace);
                    buttonContract.Font = new Font(buttonContract.Font, FontStyle.Regular);
                    label8.Text = "Projekt*";
                    break;
                case 1:
                    buttonContract.BackColor = Color.White;
                    buttonContract.Font = new Font(buttonContract.Font, FontStyle.Bold);
                    buttonProject.BackColor = Color.FromKnownColor(KnownColor.AppWorkspace);
                    buttonProject.Font = new Font(buttonProject.Font, FontStyle.Regular);
                    label8.Text = "Zakázka*";
                    break;
            }

            await LoadTimeEntriesAsync();
            await LoadProjectsContractsAsync();
        }

        private void buttonProject_Click(object sender, EventArgs e)
        {
            _projectType = 0;
            VisualiseSelectedProjectOrContract();
        }

        private void buttonContract_Click(object sender, EventArgs e)
        {
            _projectType = 1;
            VisualiseSelectedProjectOrContract();
        }

        private string FormatProjectToString(Project project)
        {
            return $"{project.Id} ({project.ProjectDescription}): {project.ProjectTitle}";
        }

        private string FormatTimeEntryToString(TimeEntry timeEntry)
        {
            return $"{timeEntry.Id} ({timeEntry?.Project?.ProjectDescription}): {timeEntry?.EntryMinutes / 60.0} h - {timeEntry?.Description}";
        }

        private bool isUpdating = false;

        private void comboBoxProjectsContracts_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (!comboBoxProjectsContracts.Enabled)
                return;

            isUpdating = true;
            try
            {
                if (comboBoxProjectsContracts.SelectedItem != null)
                {
                    comboBoxProjectsContracts.Text = comboBoxProjectsContracts.SelectedItem.ToString();
                    comboBoxProjectsContracts.SelectionStart = comboBoxProjectsContracts.Text.Length;
                    comboBoxProjectsContracts.SelectionLength = 0;
                    comboBoxProjectsContracts.DroppedDown = false;
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void comboBoxProjectsContracts_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (!comboBoxProjectsContracts.Enabled)
                return;

            isUpdating = true;
            try
            {
                string query = comboBoxProjectsContracts.Text;
                int selectionStart = comboBoxProjectsContracts.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBoxProjectsContracts.DroppedDown = false;
                    comboBoxProjectsContracts.Items.Clear();
                    comboBoxProjectsContracts.Items.AddRange(_projectsFormattedToString.ToArray());
                    return;
                }

                // Filtrace podle libovolné části textu
                List<string> filteredItems = _projectsFormattedToString
                    .Where(x => x.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    string originalText = comboBoxProjectsContracts.Text;
                    int originalSelectionStart = comboBoxProjectsContracts.SelectionStart;

                    comboBoxProjectsContracts.Items.Clear();
                    comboBoxProjectsContracts.Items.AddRange(filteredItems.ToArray());

                    comboBoxProjectsContracts.Text = originalText;
                    comboBoxProjectsContracts.SelectionStart = originalSelectionStart;
                    comboBoxProjectsContracts.SelectionLength = 0;

                    // Otevření rozevíracího seznamu
                    if (!comboBoxProjectsContracts.DroppedDown)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            comboBoxProjectsContracts.DroppedDown = true;
                            Cursor = Cursors.Default;
                        }));
                    }
                }
                else
                {
                    comboBoxProjectsContracts.DroppedDown = false;
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private async void buttonWrite_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                AppLogger.Error("Nepodařilo se načíst aktuálního uživatele, nelze přidat zápis hodin do databáze.");
                return;
            }

            if (buttonWrite.Text == "Konec prohlížení")
            {
                ClearFields();

                buttonProject.Enabled = true;
                buttonContract.Enabled = true;
                textBoxDescription.Enabled = true;
                comboBoxProjectsContracts.Enabled = true;
                buttonAddHalfHour.Enabled = true;
                buttonAddHour.Enabled = true;
                buttonSubtractHalfHour.Enabled = true;
                buttonSubtractHour.Enabled = true;

                buttonWrite.Text = "Zapsat";

                await LoadData();
            }
            else
            {
                var (valid, reason) = CheckForEmptyOrIncorrectFields();
                if (!valid)
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                    return;
                }

                var newTimeEntry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    ProjectId = int.Parse(comboBoxProjectsContracts.SelectedItem?.ToString()?.Split(' ')[0] ?? "0"),
                    Description = textBoxDescription.Text,
                    EntryMinutes = _minutesCount
                };

                var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
                if (addedTimeEntry is not null)
                {
                    AppLogger.Information($"Zápis hodin {FormatTimeEntryToString(addedTimeEntry)} byl úspěšně proveden.", true);
                    ClearFields();
                }
                else
                {
                    AppLogger.Error($"Zápis {FormatTimeEntryToString(newTimeEntry)} nebyl proveden.");
                }

                await LoadTimeEntriesAsync();
            }
        }

        private (bool valid, object reason) CheckForEmptyOrIncorrectFields()
        {
            if (comboBoxProjectsContracts.SelectedItem is null) return (false, _projectType == 0 ? "Projekt" : "Zakázka");
            return (true, "");
        }

        private void ClearFields()
        {
            listBoxTimeEntries.Items.Clear();
            comboBoxProjectsContracts.SelectedText = string.Empty;
            comboBoxProjectsContracts.Text = string.Empty;
            textBoxDescription.Text = string.Empty;
            _minutesCount = 0;
            UpdateHoursLabel();
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var timeEntry = await GetTimeEntryBySelectedItem();

            if (timeEntry != null)
            {
                var dialogResult = MessageBox.Show($"Smazat záznam {FormatTimeEntryToString(timeEntry)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _timeEntryRepo.DeleteTimeEntryAsync(timeEntry.Id))
                    {
                        AppLogger.Information($"Záznam {FormatTimeEntryToString(timeEntry)} byl smazán z databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat záznam {FormatTimeEntryToString(timeEntry)} z databáze.");
                    }

                    await LoadTimeEntriesAsync();
                }
            }
        }

        private async void listBoxTimeEntries_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxTimeEntries.SelectedItem is not null)
            {
                var timeEntry = await GetTimeEntryBySelectedItem();

                if (timeEntry != null)
                {
                    buttonProject.Enabled = false;
                    buttonContract.Enabled = false;
                    buttonAddHalfHour.Enabled = false;
                    buttonAddHour.Enabled = false;
                    buttonSubtractHalfHour.Enabled = false;
                    buttonSubtractHour.Enabled = false;
                    textBoxDescription.Enabled = false;
                    textBoxDescription.Text = timeEntry.Description;
                    comboBoxProjectsContracts.Enabled = false;
                    comboBoxProjectsContracts.Text = FormatProjectToString(timeEntry.Project);
                    labelHours.Enabled = false;
                    _minutesCount = timeEntry.EntryMinutes;
                    UpdateHoursLabel();

                    buttonWrite.Text = "Konec prohlížení";
                }
            }
        }

        private async Task<TimeEntry?> GetTimeEntryBySelectedItem()
        {
            TimeEntry? timeEntry = new TimeEntry();

            if (listBoxTimeEntries.SelectedItem != null)
            {
                string? selectedItem = listBoxTimeEntries.SelectedItem?.ToString();
                string? idString = selectedItem?.Split(' ')[0];

                if (int.TryParse(idString, out int timeEntryId))
                {
                    timeEntry = await _timeEntryRepo.GetTimeEntryByIdAsync(timeEntryId);
                }

                else
                {
                    AppLogger.Error($"Nepodařilo se získat zápis hodin {selectedItem} z databáze, id '{idString}' je neplatné.");
                }

                if (timeEntry == null)
                {
                    AppLogger.Error($"Nepodařilo se získat zápis hodin {selectedItem} z databáze.");
                }
            }

            return timeEntry;
        }

        private void buttonSubtractHour_Click(object sender, EventArgs e)
        {
            _minutesCount -= 60;
            UpdateHoursLabel();
        }

        private void buttonSubtractHalfHour_Click(object sender, EventArgs e)
        {
            _minutesCount -= 30;
            UpdateHoursLabel();
        }

        private void buttonAddHalfHour_Click(object sender, EventArgs e)
        {
            _minutesCount += 30;
            UpdateHoursLabel();
        }

        private void buttonAddHour_Click(object sender, EventArgs e)
        {
            _minutesCount += 60;
            UpdateHoursLabel();
        }

        private void UpdateHoursLabel()
        {
            labelHours.Text = $"{_minutesCount / 60.0} h";
        }
    }
}

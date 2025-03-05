using System.Data;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntryDialog : Form
    {
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

            UpdateLabelDates();

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
                var timeEntryTypes = await _timeEntryRepo.GetAllTimeEntryTypesAsync();
                var timeEntries = await _timeEntryRepo.GetTimeEntriesByUserAndDateAsync(_currentUser, _currentDate);

                Invoke(() =>
                {
                    comboBoxEntryType.Items.Clear();
                    comboBoxEntryType.Items.AddRange(timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());
                    if (comboBoxEntryType.Items.Count > 0) comboBoxEntryType.SelectedIndex = 0;
                    listBoxTimeEntries.Items.Clear();
                    listBoxTimeEntries.Items.AddRange(timeEntries.Select(FormatHelper.FormatTimeEntryToString).ToArray());
                    UpdateLabelFinishedHours();
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
                List<Project> projects = await _projectRepo.GetAllProjectsAndContractsAsync();

                Invoke(new Action(() =>
                {
                    comboBoxProjects.Items.Clear();
                    _projectsFormattedToString.Clear();
                    foreach (var project in projects)
                    {
                        _projectsFormattedToString.Add(FormatHelper.FormatProjectToString(project));
                    }

                    comboBoxProjects.Items.AddRange(_projectsFormattedToString.ToArray());
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

        private bool isUpdating = false;

        private void comboBoxProjectsContracts_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (!comboBoxProjects.Enabled)
                return;

            isUpdating = true;
            try
            {
                if (comboBoxProjects.SelectedItem != null)
                {
                    comboBoxProjects.Text = comboBoxProjects.SelectedItem.ToString();
                    comboBoxProjects.SelectionStart = comboBoxProjects.Text.Length;
                    comboBoxProjects.SelectionLength = 0;
                    comboBoxProjects.DroppedDown = false;
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

            if (!comboBoxProjects.Enabled)
                return;

            isUpdating = true;
            try
            {
                string query = comboBoxProjects.Text;
                int selectionStart = comboBoxProjects.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBoxProjects.DroppedDown = false;
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_projectsFormattedToString.ToArray());
                    return;
                }

                // Filtrace podle libovolné části textu
                List<string> filteredItems = _projectsFormattedToString
                    .Where(x => x.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
                    .ToList();

                if (filteredItems.Count > 0)
                {
                    string originalText = comboBoxProjects.Text;
                    int originalSelectionStart = comboBoxProjects.SelectionStart;

                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(filteredItems.ToArray());

                    comboBoxProjects.Text = originalText;
                    comboBoxProjects.SelectionStart = originalSelectionStart;
                    comboBoxProjects.SelectionLength = 0;

                    // Otevření rozevíracího seznamu
                    if (!comboBoxProjects.DroppedDown)
                    {
                        BeginInvoke(new Action(() =>
                        {
                            comboBoxProjects.DroppedDown = true;
                            Cursor = Cursors.Default;
                        }));
                    }
                }
                else
                {
                    comboBoxProjects.DroppedDown = false;
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

                comboBoxEntryType.Enabled = true;
                comboBoxProjects.Enabled = true;
                textBoxDescription.Enabled = true;
                maskedTextBoxNumberOfHours.Enabled = true;

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

                var newTimeEntryType = new TimeEntryType
                {
                    Title = comboBoxEntryType.Text
                };

                var addedTimeEntryType = await _timeEntryRepo.CreateTimeEntryTypeAsync(newTimeEntryType);

                var newTimeEntry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    ProjectId = int.Parse(comboBoxProjects.SelectedItem?.ToString()?.Split(' ')[0] ?? "0"),
                    Description = textBoxDescription.Text,
                    EntryMinutes = _minutesCount,
                    Timestamp = _currentDate,
                    EntryTypeId = addedTimeEntryType?.Id ?? 0
                };

                var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
                if (addedTimeEntry is not null)
                {
                    AppLogger.Information($"Zápis hodin {FormatHelper.FormatTimeEntryToString(addedTimeEntry)} byl úspěšně proveden.");
                    ClearFields();
                }
                else
                {
                    AppLogger.Error($"Zápis {FormatHelper.FormatTimeEntryToString(newTimeEntry)} nebyl proveden.");
                }

                await LoadTimeEntriesAsync();
            }
        }

        private (bool valid, object reason) CheckForEmptyOrIncorrectFields()
        {
            if (comboBoxProjects.SelectedItem is null) return (false, "Projekt");
            if (string.IsNullOrEmpty(comboBoxEntryType.Text)) return (false, "Typ zápisu");
            if (string.IsNullOrEmpty(textBoxDescription.Text)) return (false, "Popis činnosti");
            if (!maskedTextBoxNumberOfHours.MaskFull) return (false, "Počet hodin");
            return (true, "");
        }

        private void ClearFields()
        {
            listBoxTimeEntries.Items.Clear();
            textBoxDescription.Text = string.Empty;
            _minutesCount = 0;
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var timeEntry = await GetTimeEntryBySelectedItem();

            if (timeEntry != null)
            {
                var dialogResult = MessageBox.Show($"Smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _timeEntryRepo.DeleteTimeEntryAsync(timeEntry.Id))
                    {
                        AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} byl smazán z databáze.");
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat záznam {FormatHelper.FormatTimeEntryToString(timeEntry)} z databáze.");
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
                    comboBoxEntryType.Enabled = false;
                    comboBoxEntryType.Text = timeEntry?.EntryType?.Title ?? "";
                    comboBoxProjects.Enabled = false;
                    comboBoxProjects.Text = FormatHelper.FormatProjectToString(timeEntry?.Project);
                    textBoxDescription.Enabled = false;
                    textBoxDescription.Text = timeEntry?.Description;
                    maskedTextBoxNumberOfHours.Enabled = false;
                    _minutesCount = timeEntry?.EntryMinutes ?? 0;
                    UpdateHoursMaskedTextBox();

                    buttonWrite.Text = "Konec prohlížení";
                }
            }
        }

        private void UpdateHoursMaskedTextBox()
        {
            maskedTextBoxNumberOfHours.Text = $"{_minutesCount / 60.0} h";
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

        private void labelNextDate_Click(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(1);
            UpdateLabelDates();
        }

        private void labelPreviousDate_Click(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(-1);
            UpdateLabelDates();
        }

        private void labelCurrentDate_Click(object sender, EventArgs e)
        {
            _currentDate = _currentDate.AddDays(1);
            UpdateLabelDates();
        }

        private async void UpdateLabelDates()
        {
            labelCurrentDate.Text = _currentDate.ToShortDateString();
            labelNextDate.Text = _currentDate.AddDays(1).ToShortDateString();
            labelPreviousDate.Text = _currentDate.AddDays(-1).ToShortDateString();

            await LoadTimeEntriesAsync();
        }

        private async void UpdateLabelFinishedHours()
        {
            int projectMinutes = (await _timeEntryRepo.GetTimeEntriesByUserAndDateAsync(_currentUser, _currentDate))
                .Sum(te => te.EntryMinutes);

            if (projectMinutes > 450)
            {
                //labelFinishedHours.ForeColor = Color.DarkSlateBlue;
            }

            else if (projectMinutes == 450)
            {
                //labelFinishedHours.ForeColor = Color.Green;
            }

            else
            {
                //labelFinishedHours.ForeColor = Color.Tomato;
            }

            //labelFinishedHours.Text = $"{totalMinutes / 60.0} / 7,5 h    ({projectMinutes / 60.0}+{contractMinutes / 60.0})";

            groupBox1.Text = $"Zápis hodin (zbývá zapsat {(450 - projectMinutes) / 60.0} h)";
        }

        private void maskedTextBoxNumberOfHours_TextChanged(object sender, EventArgs e)
        {
            if (maskedTextBoxNumberOfHours.MaskFull)
            {
                _minutesCount = (int)(double.Parse(maskedTextBoxNumberOfHours.Text.Split(' ')[0])*60);
            }
        }
    }
}

using System.Globalization;
using System.Text;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class TimeEntryV2Dialog : Form
    {
        private readonly LoadingUC _loadingUC = new LoadingUC();
        public TimeEntry TimeEntry { get; set; }

        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private List<Project> _projects = new List<Project>();
        private readonly User _currentUser;

        public TimeEntryV2Dialog(User currentUser, TimeEntry timeEntry)
        {
            InitializeComponent();
            TimeEntry = timeEntry;
            _currentUser = currentUser;
        }

        #region Loading
        private void LoadUi()
        {
            int minutesStart = TimeEntry.Timestamp.Value.Hour * 60 + TimeEntry.Timestamp.Value.Minute;
            int minutesEnd = minutesStart + TimeEntry.EntryMinutes;

            comboBoxStart.SelectedIndex = minutesStart / 30 - 1;
            comboBoxEnd.SelectedIndex = minutesEnd / 30 - 1;
            comboBoxProjects.SelectedText = FormatHelper.FormatProjectToString(TimeEntry.Project);
            comboBoxEntryType.SelectedText = FormatHelper.FormatTimeEntryTypeToString(TimeEntry.EntryType);
            textBoxDescription.Text = TimeEntry.Description;
        }

        private void TimeEntryV2Dialog_Load(object sender, EventArgs e)
        {
            LoadUi();

            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            Task.Run(LoadData);
        }

        private async Task LoadData()
        {
            Invoke(() => _loadingUC.BringToFront());

            var projectsTask = LoadProjectsContractsAsync();
            var timeEntryTypesTask = LoadTimeEntryTypesAsync();

            await Task.WhenAll(projectsTask, timeEntryTypesTask);

            Invoke(() => _loadingUC.Visible = false);
        }

        private async Task LoadTimeEntryTypesAsync()
        {
            try
            {
                var timeEntryTypes = await _timeEntryRepo.GetAllTimeEntryTypesAsync();

                Invoke(() =>
                {
                    comboBoxEntryType.Items.Clear();
                    comboBoxEntryType.Items.AddRange(timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());
                    if (comboBoxEntryType.Items.Count > 0) comboBoxEntryType.SelectedIndex = 0;
                });
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání typů časových záznamů.", ex);
                }));
            }
        }

        private async Task LoadProjectsContractsAsync()
        {
            try
            {
                _projects = await _projectRepo.GetAllProjectsAndContractsAsync();

                Invoke(new Action(() =>
                {
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
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
        #endregion

        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            // TODO: pokud je timeentry id = -1, přidat .. jinak update

            if (_currentUser == null)
            {
                AppLogger.Error("Nepodařilo se načíst aktuálního uživatele, nelze přidat zápis hodin do databáze.");
                return;
            }

            var (valid, reason) = CheckForEmptyOrIncorrectFields();
            if (!valid)
            {
                AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                return;
            }

            var start = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                       int.Parse(comboBoxStart.Text.Split(':')[0]),
                                       int.Parse(comboBoxStart.Text.Split(':')[1]), 0);

            var end = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day,
                                       int.Parse(comboBoxEnd.Text.Split(':')[0]),
                                       int.Parse(comboBoxEnd.Text.Split(':')[1]), 0);

            var newTimeEntryType = new TimeEntryType() { Title = comboBoxEntryType.Text };
            var addedTimeEntryType = await _timeEntryRepo.CreateTimeEntryTypeAsync(newTimeEntryType);

            // nový záznam
            if (TimeEntry.Id == -1)
            {
                var newTimeEntry = new TimeEntry
                {
                    UserId = _currentUser.Id,
                    ProjectId = _projects[comboBoxProjects.SelectedIndex].Id,
                    Description = textBoxDescription.Text,
                    Timestamp = start,
                    EntryMinutes = GetNumberOfMinutesFromDateSpan(start, end),
                    EntryTypeId = addedTimeEntryType?.Id ?? 0
                };

                TimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);

                //var addedTimeEntry = await _timeEntryRepo.CreateTimeEntryAsync(newTimeEntry);
                //if (addedTimeEntry is not null)
                //{
                //    AppLogger.Information($"Zápis hodin {FormatHelper.FormatTimeEntryToString(addedTimeEntry)} byl úspěšně proveden.");
                //    DialogResult = DialogResult.OK;
                //}
                //else
                //{
                //    AppLogger.Error($"Zápis {FormatHelper.FormatTimeEntryToString(newTimeEntry)} nebyl proveden.");
                //}
            }

            // existující záznam
            else
            {
                if (comboBoxProjects.SelectedIndex > -1) TimeEntry.ProjectId = _projects[comboBoxProjects.SelectedIndex].Id;
                TimeEntry.Description = textBoxDescription.Text;
                TimeEntry.Timestamp = start;
                TimeEntry.EntryMinutes = GetNumberOfMinutesFromDateSpan(start, end);
                TimeEntry.EntryTypeId = addedTimeEntryType?.Id ?? 0;

                //var success = await _timeEntryRepo.UpdateTimeEntryAsync(TimeEntry);
                //if (success)
                //{
                //    AppLogger.Information($"Zápis hodin {FormatHelper.FormatTimeEntryToString(TimeEntry)} byl úspěšně proveden.");
                //    DialogResult = DialogResult.OK;
                //}
                //else
                //{
                //    AppLogger.Error($"Zápis {FormatHelper.FormatTimeEntryToString(TimeEntry)} nebyl proveden.");
                //}
            }
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var dialogResult = MessageBox.Show($"Smazat záznam {FormatHelper.FormatTimeEntryToString(TimeEntry)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

            if (dialogResult == DialogResult.Yes)
            {
                if (await _timeEntryRepo.DeleteTimeEntryAsync(TimeEntry.Id))
                {
                    AppLogger.Information($"Záznam {FormatHelper.FormatTimeEntryToString(TimeEntry)} byl smazán z databáze.");
                    Close();
                }

                else
                {
                    AppLogger.Error($"Nepodařilo se smazat záznam {FormatHelper.FormatTimeEntryToString(TimeEntry)} z databáze.");
                }
            }
        }

        private (bool valid, object reason) CheckForEmptyOrIncorrectFields()
        {
            if (comboBoxProjects.SelectedItem is null && TimeEntry.Id == -1) return (false, "Projekt");
            if (string.IsNullOrEmpty(comboBoxEntryType.Text)) return (false, "Typ zápisu");
            if (string.IsNullOrEmpty(textBoxDescription.Text)) return (false, "Popis činnosti");
            return (true, "");
        }

        #region Timespans
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
        #endregion

        #region Projects combobox
        private bool isUpdating = false;

        private void comboBoxProjects_SelectionChangeCommitted(object sender, EventArgs e)
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

        private void comboBoxProjects_TextChanged(object sender, EventArgs e)
        {
            if (isUpdating)
                return;

            if (!comboBoxProjects.Enabled)
                return;

            isUpdating = true;
            try
            {
                string query = RemoveDiacritics(comboBoxProjects.Text);
                int selectionStart = comboBoxProjects.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBoxProjects.DroppedDown = false;
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
                    return;
                }

                // Filtrace bez diakritiky
                List<string> filteredItems = _projects
                    .Select(FormatHelper.FormatProjectToString)
                    .Where(x => RemoveDiacritics(x).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
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

        // Metoda pro odstranění diakritiky
        private string RemoveDiacritics(string text)
        {
            if (string.IsNullOrEmpty(text))
                return text;

            string normalized = text.Normalize(NormalizationForm.FormD);
            StringBuilder sb = new StringBuilder();

            foreach (char c in normalized)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                {
                    sb.Append(c);
                }
            }

            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
        #endregion

        private void button1_Click(object sender, EventArgs e)
        {
            TimeEntry.Id = 69;
        }
    }
}
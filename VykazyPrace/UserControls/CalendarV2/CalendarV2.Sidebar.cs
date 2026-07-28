using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using VykazyPrace.Core.Services.TimeEntry;

namespace VykazyPrace.UserControls.CalendarV2
{
    public partial class CalendarV2
    {
        private async Task LoadSidebar()
        {
            string[] days = { "Neděle", "Pondělí", "Úterý", "Středa", "Čtvrtek", "Pátek", "Sobota" };
            flowLayoutPanel2.Visible = _selectedTimeEntryId > -1;

            var timeEntry = _currentEntries.FirstOrDefault(e => e.Id == _selectedTimeEntryId);
            if (timeEntry == null) return;

            // pokud je svačina, schovej sidebar
            if (timeEntry.ProjectId == WorkLogIds.Projects.Snack && timeEntry.EntryTypeId == WorkLogIds.EntryTypes.Snack)
            {
                flowLayoutPanel2.Visible = false;
                return;
            }

            DateTime timeStamp = timeEntry.Timestamp ?? _selectedDate;
            int minutesStart = timeStamp.Hour * 60 + timeStamp.Minute;
            int minutesEnd = minutesStart + timeEntry.EntryMinutes;

            bool canEdit = timeEntry.IsLocked == 0 && CanEditSelectedUser();
            flowLayoutPanel2.Enabled = canEdit;


            if (timeEntry.IsValid != 1)
            {
                // NEvalidní záznam – jen základní ovládací prvky
                BeginInvoke((Action)(() =>
                {
                    comboBoxStart.SelectedIndex = minutesStart / 30;
                    comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);
                    customComboBoxSubTypes.SetText(string.Empty);
                    textBoxNote.Text = string.Empty;
                    comboBoxEntryType.Text = string.Empty;

                    foreach (var radio in flowLayoutPanel2.Controls.OfType<RadioButton>())
                        radio.Checked = false;

                    tableLayoutPanel4.Visible = false;
                    tableLayoutPanel6.Visible = false;
                    tableLayoutPanelProject.Visible = false;
                    tableLayoutPanelEntryType.Visible = false;
                    tableLayoutPanelEntrySubType.Visible = false;
                    panel4.Visible = false;
                }));
                return;
            }

            Project proj = timeEntry.Project
                           ?? await _projectRepo.GetProjectByIdAsync(timeEntry.ProjectId ?? 0);
            if (proj == null) return;
            timeEntry.Project = proj;

            checkBoxArchivedProjects.Checked = proj.IsArchived == 1;

            await LoadTimeEntryTypesAsync(proj.ProjectType);

            switch (proj.Id)
            {
                case WorkLogIds.Projects.Other: SelectRadioButtonByText("OSTATNÍ"); break;
                case WorkLogIds.Projects.Absence: SelectRadioButtonByText("NEPŘÍTOMNOST"); break;
                case WorkLogIds.Projects.Training: SelectRadioButtonByText("ŠKOLENÍ"); break;
                case WorkLogIds.Projects.CustomerService: SelectRadioButtonByText("ZÁKAZNICKÝ SERVIS"); break;
                default:
                    int idx = proj.ProjectType + 1;
                    if (idx == 2 || idx == 3) idx = 2;
                    if (flowLayoutPanel2.Controls.Find($"radioButton{idx}", false).FirstOrDefault() is RadioButton rb)
                        rb.Checked = true;
                    break;
            }

            BeginInvoke((Action)(() =>
            {
                comboBoxStart.SelectedIndex = minutesStart / 30;
                comboBoxEnd.SelectedIndex = Math.Min(minutesEnd / 30, comboBoxEnd.Items.Count - 1);

                customComboBoxSubTypes.SetText(timeEntry.Description);
                customComboBoxProjects.SetText(FormatHelper.FormatProjectToString(proj));
                textBoxNote.Text = timeEntry.Note;

                if (proj.ProjectType is 0 or 1 or 2)
                {
                    int baseId = proj.ProjectType switch
                    {
                        0 => 1,
                        1 => 10,
                        2 => 13,
                        _ => 0
                    };
                    int radioIndex = (int)(timeEntry.EntryTypeId - baseId);
                    var radioPanel = tableLayoutPanelEntryType
                                     .Controls
                                     .OfType<TableLayoutPanel>()
                                     .FirstOrDefault();
                    var radios = radioPanel?.Controls.OfType<RadioButton>().ToList();
                    if (radios != null && radioIndex >= 0 && radioIndex < radios.Count)
                        radios[radioIndex].Checked = true;
                }
                else
                {
                    var selectedType = _timeEntryTypes.FirstOrDefault(x => x.Id == timeEntry.EntryTypeId);
                    comboBoxEntryType.Text = timeEntry.AfterCare == 1
                        ? FormatHelper.FormatTimeEntryTypeWithAfterCareToString(selectedType)
                        : FormatHelper.FormatTimeEntryTypeToString(selectedType);
                }
            }));
        }
        private void SelectRadioButtonByText(string text)
        {
            var rb = flowLayoutPanel2.Controls
                .OfType<RadioButton>()
                .FirstOrDefault(r => r.Text.Equals(text, StringComparison.InvariantCultureIgnoreCase));

            if (rb != null) rb.Checked = true;
        }


        private async void radioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (sender is RadioButton rb && rb.Checked)
            {
                int index = 0;
                label4.Text = "Poznámka";

                tableLayoutPanel4.Visible = true;
                tableLayoutPanel6.Visible = true;
                tableLayoutPanelProject.Visible = true;
                tableLayoutPanelEntryType.Visible = true;
                tableLayoutPanelEntrySubType.Visible = true;
                customComboBoxSubTypes.SetText(string.Empty);
                customComboBoxProjects.SetText(string.Empty);
                panel4.Visible = true;

                switch (rb.Text)
                {
                    case "PROVOZ":
                        index = 0;
                        labelProject.Text = "Nákladové středisko*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        checkBoxArchivedProjects.Checked = false;
                        break;
                    case "PROJEKT":
                        index = 1;
                        labelProject.Text = "Projekt*";
                        labelType.Text = "Typ záznamu*";
                        tableLayoutPanelProject.Visible = true;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = true;
                        break;
                    case "ŠKOLENÍ":
                        index = 3;
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = false;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        checkBoxArchivedProjects.Checked = false;
                        label4.Text = "Poznámka*";
                        break;
                    case "NEPŘÍTOMNOST":
                        labelType.Text = "Důvod*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = false;
                        checkBoxArchivedProjects.Visible = false;
                        checkBoxArchivedProjects.Checked = false;
                        index = 4;
                        break;
                    case "OSTATNÍ":
                        index = 5;
                        labelType.Text = "Činnost*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        checkBoxArchivedProjects.Checked = false;
                        break;
                    case "ZÁKAZNICKÝ SERVIS":
                        index = 7;
                        labelType.Text = "Nákladové středisko*";
                        tableLayoutPanelProject.Visible = false;
                        tableLayoutPanelEntryType.Visible = true;
                        tableLayoutPanelEntrySubType.Visible = true;
                        checkBoxArchivedProjects.Visible = false;
                        checkBoxArchivedProjects.Checked = false;
                        break;
                    default:
                        AppLogger.Error("Chyba výběru kategorie.", new NotImplementedException("Neznamá kategorie pro sidebar: " + rb.Text));
                        break;
                }

                await LoadProjectsAsync(index);
                await LoadTimeEntryTypesAsync(index);
            }
        }

        /// <summary>
        /// Událost přepnutí zobrazení archivovaných / klasických projektů.
        /// </summary>
        private async void checkBoxArchivedProjects_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(1);
            await LoadTimeEntryTypesAsync(1);
        }

        /// <summary>
        /// Událost kliknutí potvrzení změn v Sidebaru.
        /// </summary>
        private async void buttonConfirm_Click(object sender, EventArgs e)
        {
            if (!CanEditSelectedUser()) return;

            // Uložení pozice scrollu (kvůli přerenderování)
            userHasScrolled = true;
            int scrollX = panelContainer.HorizontalScroll.Value;
            int scrollY = panelContainer.VerticalScroll.Value;

            var (valid, reason) = CheckForEmptyOrIncorrectFields();
            if (!valid)
            {
                AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                return;
            }

            int selectedEntryTypeId = GetSelectedEntryTypeId();
            if (selectedEntryTypeId == 0)
            {
                AppLogger.Error("[EntryUpdate]: Nepodařilo se zjistit SelectedEntryTypeId.");
                return;
            }

            // Zjištění ProjectId pro PROVOZ/PROJEKT/PŘEDPROJEKT (0/1/2)
            int? selectedProjectId = null;
            if (_currentProjectType is 0 or 1 or 2)
            {
                Project? selectedProject = _projects.FirstOrDefault(p =>
                    FormatHelper.FormatProjectToString(p)
                        .Equals(customComboBoxProjects.SelectedItem,
                                StringComparison.InvariantCultureIgnoreCase));

                if (selectedProject == null)
                {
                    AppLogger.Error("[EntryUpdate]: Vybraný projekt neodpovídá žádné možnosti v seznamu.");
                    return;
                }

                selectedProjectId = selectedProject.Id;
            }

            var request = new TimeEntryUpdateRequest
            {
                CurrentProjectType = _currentProjectType,
                SelectedEntryTypeId = selectedEntryTypeId,
                SelectedUserId = _selectedUser.Id,
                Note = textBoxNote.Text,
                SubTypeTitle = customComboBoxSubTypes.GetText(),
                SelectedProjectId = selectedProjectId,
                SelectedEntryIds = _selectedEntryIds.ToList(),
                SelectedTimeEntryId = _selectedTimeEntryId,
                CurrentEntries = _currentEntries,
                Projects = _projects
            };

            var result = await _timeEntryUpdateService.UpdateEntriesAsync(request);

            if (!result.HasAnyUpdate)
            {
                AppLogger.Information("[EntryUpdate]: Nebyl aktualizován žádný záznam.");
            }

            _selectedEntryIds.Clear();
            DeselectAllPanels();
            _selectedTimeEntryId = -1;
            DeactivateAllPanels();
            UpdateBulkEditIndicator();
            await LoadTimeEntrySubTypesAsync();
            await RenderCalendar();
            UpdateHourLabels();
            await LoadSidebar();

            // Návrat na původní scroll pozici
            BeginInvoke((Action)(() =>
            {
                panelContainer.HorizontalScroll.Value =
                    Math.Max(0, Math.Min(scrollX, panelContainer.HorizontalScroll.Maximum));
                panelContainer.VerticalScroll.Value =
                    Math.Max(0, Math.Min(scrollY, panelContainer.VerticalScroll.Maximum));
            }));
        }

        private async void buttonRemove_Click(object sender, EventArgs e) => await DeleteRecord();

        /// <summary>
        /// Zjistí EntryType podle zvoleného RadioButtonu (provoz, projekt, ...)
        /// Vrátí korespondující číslo.
        /// </summary>
        private int GetSelectedEntryTypeId()
        {
            // Školení
            if (_currentProjectType == 3)
                return 16;

            // Určení EntryTypeId podle zvolené kategorie
            int selectedEntryTypeId = 0;
            if (_currentProjectType is 0 or 1 or 2)
            {
                var radioButtons = tableLayoutPanelEntryType.Controls
                    .OfType<TableLayoutPanel>()
                    .SelectMany(panel => panel.Controls.OfType<RadioButton>())
                    .ToList();

                for (int i = 0; i < radioButtons.Count; i++)
                {
                    if (radioButtons[i].Checked)
                    {
                        selectedEntryTypeId = _currentProjectType switch
                        {
                            0 => 1 + i,
                            1 => 10 + i,
                            2 => 13 + i,
                            _ => 0
                        };
                        break;
                    }
                }
            }
            else if (comboBoxEntryType.SelectedIndex > -1)
            {
                selectedEntryTypeId = _timeEntryTypes[comboBoxEntryType.SelectedIndex].Id;
            }

            return selectedEntryTypeId;
        }

        /// <summary>
        /// Událost při změně velikosti - Layoutová úprava prvků v Sidebaru
        /// </summary>
        private void flowLayoutPanel2_SizeChanged(object sender, EventArgs e)
        {
            int newWidth = flowLayoutPanel2.ClientSize.Width - 10;
            tableLayoutPanelProject.Width = newWidth;
            tableLayoutPanelEntryType.Width = newWidth;
            tableLayoutPanelEntrySubType.Width = newWidth;
            tableLayoutPanel6.Width = newWidth;

            ClearComboBoxSelections(flowLayoutPanel2);
        }

        /// <summary>
        /// Aktualizuje pozadí Sidebaru - modrá značí více než jednu vybranou položku.
        /// </summary>
        private void UpdateBulkEditIndicator() => flowLayoutPanel2.BackColor = _selectedEntryIds.Count > 1 ? Color.FromArgb(227, 255, 250) : Color.White;

        /// <summary>
        /// Provede validační kontrolu vstupních polí v Sidebaru
        /// podle toho, jaký typ záznamu je aktuálně zvolen.
        /// </summary>
        private (bool valid, string reason) CheckForEmptyOrIncorrectFields()
        {
            var rb = flowLayoutPanel2.Controls
               .OfType<RadioButton>()
               .FirstOrDefault(r => r.Checked);

            bool ProjectTextMatches = _projects.Any(p =>
                FormatHelper.FormatProjectToString(p).Equals(customComboBoxProjects.SelectedItem, StringComparison.InvariantCultureIgnoreCase));

            bool EntryTypeMatches = _timeEntryTypes.Any(t =>
                FormatHelper.FormatTimeEntryTypeToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase) ||
                FormatHelper.FormatTimeEntryTypeWithAfterCareToString(t).Equals(comboBoxEntryType.Text, StringComparison.InvariantCultureIgnoreCase));

            switch (rb?.Text)
            {
                case "PROVOZ":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Nákladové středisko neodpovídá žádné možnosti");
                    break;
                case "PROJEKT":
                case "PŘEDPROJEKT":
                    if (string.IsNullOrWhiteSpace(customComboBoxProjects.SelectedItem) || !ProjectTextMatches)
                        return (false, "Projekt neodpovídá žádné možnosti");
                    break;
                case "ŠKOLENÍ":
                    if (string.IsNullOrWhiteSpace(textBoxNote.Text))
                        return (false, "Poznámka");
                    break;
                case "NEPŘÍTOMNOST":
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Důvod neodpovídá žádné možnosti");
                    break;
                default:
                    if (string.IsNullOrWhiteSpace(comboBoxEntryType.Text) || !EntryTypeMatches)
                        return (false, "Činnost neodpovídá žádné možnosti");
                    break;
            }

            return (true, "");
        }

        /// <summary>
        /// Rekurzivně projde všechny potomky a u ComboBoxů zruší označený text
        /// (nastaví caret na konec a výběr na 0).
        /// </summary>
        private void ClearComboBoxSelections(Control parent)
        {
            foreach (Control control in parent.Controls)
            {
                if (control is ComboBox cb)
                {
                    cb.SelectionStart = cb.Text.Length;
                    cb.SelectionLength = 0;
                }
                else
                    ClearComboBoxSelections(control);
            }
        }
    }
}

using System.Text.RegularExpressions;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Helpers;
using VykazyPrace.Core.Logging;
using static VykazyPrace.Core.Helpers.Enums;

namespace VykazyPrace.Dialogs
{
    public partial class ProjectManagementDialog : Form
    {
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo = new TimeEntryTypeRepository();
        private readonly TimeEntryRepository _timeEntryRepo = new TimeEntryRepository();
        private readonly User _currentUser;
        private List<Project> _filteredProjects = new List<Project>();
        private List<TimeEntryType> _timeEntryTypes = new List<TimeEntryType>();
        private bool waitForProjectSelection = false;

        public ProjectManagementDialog(User currentUser)
        {
            InitializeComponent();

            KeyPreview = true;
            _currentUser = currentUser;
        }

        private async void ProjectManagementDialog_Load(object sender, EventArgs e)
        {
            await LoadProjectsAsync(0);
        }

        private async Task LoadTimeEntryTypesAsync(int projectType)
        {
            _timeEntryTypes = await _timeEntryTypeRepo
                .GetAllTimeEntryTypesByProjectTypeAsync(projectType);
        }

        private bool comboBoxProjectsLoading = false;

        private async Task LoadProjectsAsync(int projectType)
        {
            try
            {
                // Načtení všech položek
                var allProjects = await _projectRepo.GetAllProjectsAsync(true);
                // Filtrování podle zadaného typu
                var filtered = FilterProjects(allProjects, projectType);

                // Pro typy 4 a 5 navíc načteme time entry types
                if (projectType == (int)ProjectType.Absence || projectType == (int)ProjectType.Other)
                    await LoadTimeEntryTypesAsync(projectType);

                // Naplníme ovládací prvky
                PopulateControls(projectType, filtered);

                _filteredProjects = filtered.ToList();

                ClearSelection();
                GenerateNextDescription();
            }
            catch (Exception ex)
            {
                AppLogger.Error($"Chyba při načítání typu {projectType}.", ex);
            }
        }

        private IEnumerable<Project> FilterProjects(IEnumerable<Project> projects, int projectType)
        {
            return projects.Where(p =>
                projectType switch
                {
                    1 or 2 => (p.ProjectType == 1 || p.ProjectType == 2),
                    _ => p.ProjectType == projectType
                }
            )
            .Where(p =>
                projectType == 1 || projectType == 2
                    ? p.IsArchived == (checkBoxArchived.Checked ? 1 : 0)
                    : true
            )
            .Where(p =>
                projectType == 1 || projectType == 2
                    ? (!checkBoxProposed.Checked
                       || Regex.IsMatch(p.ProjectDescription ?? "", @"^000\dN\d\d$"))
                    : true
            );
        }

        private void PopulateControls(int projectType, IEnumerable<Project> projects)
        {
            // Pro projekty (typy 1 a 2) vytvoříme wrappery
            if (projectType == 1 || projectType == 2)
            {
                var items = projects
                    .Select(p => new ProjectItem(p))
                    .Cast<object>()
                    .ToArray();

                FillListBox(listBoxProject, items);
                FillComboBox(comboBoxProjects, items);
            }
            else if (projectType == 0)
            {
                var items = projects
                    .Select(p => new ProjectItem(p))
                    .Cast<object>()
                    .ToArray();

                FillListBox(listBoxOperation, items);
            }
            else if (projectType == 4)
            {
                var items = _timeEntryTypes
                    .Select(t => FormatHelper.FormatTimeEntryTypeToString(t))
                    .ToArray();
                FillListBox(listBoxAbsence, items.Cast<object>().ToArray());
            }
            else if (projectType == 5)
            {
                var items = _timeEntryTypes
                    .Select(t => FormatHelper.FormatTimeEntryTypeToString(t))
                    .ToArray();
                FillListBox(listBoxOther, items.Cast<object>().ToArray());
            }
        }

        private void FillListBox(ListBox lb, object[] items)
        {
            lb.BeginUpdate();
            lb.Items.Clear();
            lb.Items.AddRange(items);
            lb.EndUpdate();
        }

        private void FillComboBox(ComboBox cb, object[] items)
        {
            comboBoxProjectsLoading = true;
            cb.BeginUpdate();
            cb.Items.Clear();
            cb.Items.AddRange(items);
            if (cb.Items.Count > 0)
                cb.SelectedIndex = 0;
            else
                cb.Text = "";
            cb.EndUpdate();
            comboBoxProjectsLoading = false;
        }

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            int type = tabControl1.SelectedTab.Text switch
            {
                "PROVOZ" => 0,
                "PROJEKT" => 1,
                "NEPŘÍTOMNOST" => 4,
                "OSTATNÍ" => 5,
                _ => 0
            };
            await LoadProjectsAsync(type);
        }


        private async void checkBoxArchive_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(1);
            buttonArchiveProject.Text = checkBoxArchived.Checked ? "Obnovit" : "Archivovat";
        }

        private async void buttonAddOperation_Click(object sender, EventArgs e)
        {
            var project = new Project()
            {
                ProjectType = 0,
                CreatedBy = _currentUser.Id,
                ProjectTitle = textBoxOperation.Text,
                ProjectDescription = "",
                IsArchived = 0
            };

            if (buttonAddOperation.Text == "Přidat")
            {
                await _projectRepo.CreateProjectAsync(project);
            }

            else
            {
                project.Id = _filteredProjects.Find(x => x.ProjectDescription == textBoxProjectDescription.Text).Id;
                await _projectRepo.UpdateProjectAsync(project);
            }

            await LoadProjectsAsync(project.ProjectType);
            ClearSelection();
        }

        private async void buttonAddProject_Click(object sender, EventArgs e)
        {
            var project = new Project()
            {
                CreatedBy = _currentUser.Id,
                ProjectTitle = textBoxProjectTitle.Text,
                ProjectDescription = textBoxProjectDescription.Text,
                ProjectType = FormatHelper.IsPreProject(textBoxProjectDescription.Text) ? 2 : 1,
                IsArchived = 0
            };

            if (buttonAddProject.Text == "Přidat")
            {
                await _projectRepo.CreateProjectAsync(project);
            }

            else if (buttonAddProject.Text == "Schválit")
            {
                project.Id = int.Parse(labelProjectId.Text);
                GenerateNextDescription();
                project.ProjectDescription = textBoxProjectDescription.Text;
                await _projectRepo.UpdateProjectAsync(project);
            }

            // ULOŽIT
            else
            {
                project.Id = int.Parse(labelProjectId.Text);
                await _projectRepo.UpdateProjectAsync(project);
            }

            await LoadProjectsAsync(project.ProjectType);
            ClearSelection();
        }

        private async void buttonArchiveProject_Click(object sender, EventArgs e)
        {
            int isArchived = buttonArchiveProject.Text == "Archivovat" ? 1 : 0;

            string textBoxDescription = textBoxProjectDescription.Text;

            var project = _filteredProjects.Find(x => x.ProjectDescription == textBoxProjectDescription.Text);

            if (project == null)
            {
                AppLogger.Error("Nepodařilo se najít zvolený záznam.");
                return;
            }

            project.IsArchived = isArchived;

            await _projectRepo.UpdateProjectAsync(project);
            await LoadProjectsAsync(project.ProjectType);
            ClearSelection();
        }

        private async void buttonAddAbsence_Click(object sender, EventArgs e)
        {
            var timeEntryType = new TimeEntryType()
            {
                Title = textBoxAbsence.Text,
                Color = "#ADD8E6",
                ForProjectType = 4
            };

            if (buttonAddAbsence.Text == "Přidat")
            {
                await _timeEntryTypeRepo.CreateTimeEntryTypeAsync(timeEntryType);
            }

            else
            {
                timeEntryType.Id = _timeEntryTypes[listBoxAbsence.SelectedIndex].Id;
                await _timeEntryTypeRepo.UpdateTimeEntryTypeAsync(timeEntryType);
            }

            await LoadProjectsAsync((int)timeEntryType.ForProjectType);
            ClearSelection();
        }

        private async void buttonAddOther_Click(object sender, EventArgs e)
        {
            var timeEntryType = new TimeEntryType()
            {
                Title = textBoxOther.Text,
                Color = "#ADD8E6",
                ForProjectType = 5
            };

            if (buttonAddOther.Text == "Přidat")
            {
                await _timeEntryTypeRepo.CreateTimeEntryTypeAsync(timeEntryType);
            }

            else
            {
                timeEntryType.Id = _timeEntryTypes[listBoxOther.SelectedIndex].Id;
                await _timeEntryTypeRepo.UpdateTimeEntryTypeAsync(timeEntryType);
            }

            await LoadProjectsAsync((int)timeEntryType.ForProjectType);
            ClearSelection();
        }

        private bool isUpdating;

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

                    listBoxProject.SelectedIndex = listBoxProject.FindString(comboBoxProjects.Text);
                }
            }
            finally
            {
                isUpdating = false;
            }
        }

        private void comboBoxProjects_TextChanged(object sender, EventArgs e)
        {
            if (comboBoxProjectsLoading)
                return;

            if (isUpdating)
                return;

            if (!comboBoxProjects.Enabled)
                return;

            isUpdating = true;
            try
            {
                string query = FormatHelper.RemoveDiacritics(comboBoxProjects.Text);
                int selectionStart = comboBoxProjects.SelectionStart;

                if (string.IsNullOrWhiteSpace(query))
                {
                    comboBoxProjects.DroppedDown = false;
                    comboBoxProjects.Items.Clear();
                    comboBoxProjects.Items.AddRange(_filteredProjects.Select(FormatHelper.FormatProjectToString).ToArray());
                    return;
                }

                // Filtrace bez diakritiky
                List<string> filteredItems = _filteredProjects
                    .Select(FormatHelper.FormatProjectToString)
                    .Where(x => FormatHelper.RemoveDiacritics(x).IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0)
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

        private void ProjectManagementDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                ClearSelection();
                GenerateNextDescription();
            }
        }

        private void GenerateNextDescription()
        {

            if (_filteredProjects == null || !_filteredProjects.Any())
            {
                textBoxProjectDescription.Text = "0001";
                return;
            }

            var maxNumber = _filteredProjects
                .Select(p => GetLeadingNumber(p.ProjectDescription))
                .Where(n => n.HasValue)
                .Select(n => n.Value)
                .DefaultIfEmpty(0)
                .Max();

            var nextNumber = maxNumber + 1;
            textBoxProjectDescription.Text = nextNumber.ToString("D4");
        }

        private int? GetLeadingNumber(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return null;

            // Vezme všechny počáteční číslice (např. z "0255E24" udělá "0255")
            var match = Regex.Match(input, @"^\d+");
            if (match.Success && int.TryParse(match.Value, out int result))
                return result;

            return null;
        }

        private void ClearSelection()
        {
            listBoxOperation.ClearSelected();
            listBoxProject.ClearSelected();
            listBoxAbsence.ClearSelected();
            listBoxOther.ClearSelected();

            textBoxOperation.Clear();
            textBoxProjectDescription.Clear();
            textBoxProjectTitle.Clear();
            textBoxAbsence.Clear();
            textBoxOther.Clear();

            buttonAddOperation.Text = "Přidat";
            buttonAddProject.Text = "Přidat";
            buttonAddAbsence.Text = "Přidat";
            buttonAddOther.Text = "Přidat";
            groupBox2.Text = "Nový projekt";

            buttonArchiveProject.Visible = false;
            buttonDeclineAndReplace.Visible = false;
        }

        private void listBoxOperation_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxOperation.Text = listBoxOperation.SelectedItem?.ToString();
            buttonAddOperation.Text = "Uložit";
        }

        private void listBoxAbsence_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxAbsence.Text = listBoxAbsence.SelectedItem?.ToString();
            buttonAddAbsence.Text = "Uložit";
        }

        private void listBoxOther_SelectedIndexChanged(object sender, EventArgs e)
        {
            textBoxOther.Text = listBoxOther.SelectedItem?.ToString();
            buttonAddOther.Text = "Uložit";
        }

        private async void listBoxProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxProject.SelectedItem is ProjectItem pi)
            {
                var active = pi.Project;
                buttonArchiveProject.Visible = true;

                // Synchronizace comboBoxu
                comboBoxProjectsLoading = true;
                int idx = comboBoxProjects.Items
                    .OfType<ProjectItem>()
                    .ToList()
                    .FindIndex(x => x.Project.Id == active.Id);
                if (idx >= 0) comboBoxProjects.SelectedIndex = idx;
                comboBoxProjectsLoading = false;

                // Naplnění detailů
                labelProjectId.Text = active.Id.ToString();
                textBoxProjectTitle.Text = active.ProjectTitle;
                textBoxProjectDescription.Text = active.ProjectDescription;

                bool proposed = Regex.IsMatch(active.ProjectDescription ?? "", @"^000\dN\d\d$");
                buttonAddProject.Text = proposed ? "Schválit" : "Uložit";
                buttonDeclineAndReplace.Visible = proposed;
                buttonArchiveProject.Text = checkBoxArchived.Checked ? "Obnovit" : "Archivovat";
                groupBox2.Text = "Úprava projektu";

                // Logika pro replace-and-delete
                if (waitForProjectSelection)
                {
                    var originalId = int.TryParse(labelProjectId.Text, out var oid) ? oid : 0;
                    var result = MessageBox.Show(
                        $"Akce je nevratná.\n\nPřejete si vykázané hodiny převést na projekt {pi}?",
                        "Nahradit a smazat",
                        MessageBoxButtons.YesNoCancel
                    );

                    if (result == DialogResult.Yes)
                    {
                        waitForProjectSelection = false;
                        groupBox1.Enabled = groupBox2.Enabled = true;
                        listBoxProject.BackColor = SystemColors.Window;

                        var affected = await _timeEntryRepo.UpdateProjectIdForEntriesAsync(originalId, active.Id);
                        AppLogger.Information($"Přepsáno {affected} záznamů.", true);

                        await _projectRepo.DeleteProjectAsync(originalId);
                        AppLogger.Information($"Projekt s ID {originalId} byl smazán.");

                        await LoadProjectsAsync(1);
                    }
                    else if (result == DialogResult.Cancel)
                    {
                        waitForProjectSelection = false;
                        groupBox1.Enabled = groupBox2.Enabled = true;
                        listBoxProject.BackColor = SystemColors.Window;
                    }
                }
            }
        }

        private void buttonDeclineAndReplace_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Návrh bude smazán.\n\nVyberte prosím ze seznamu projekt, na který budou připsané již vykázané hodiny.", "Zamítnout a nahradit", MessageBoxButtons.OKCancel);
            if (result == DialogResult.OK)
            {
                waitForProjectSelection = true;
                groupBox1.Enabled = false;
                groupBox2.Enabled = false;
                listBoxProject.BackColor = Color.FromArgb(255, 255, 227, 95);
            }
        }

        public class ProjectItem
        {
            public Project Project { get; }
            public ProjectItem(Project project) => Project = project;
            public override string ToString() =>
                FormatHelper.FormatProjectToString(Project);
        }
    }
}

using System.Reflection.Metadata.Ecma335;
using System.Text.RegularExpressions;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class ProjectManagementDialog : Form
    {
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly TimeEntryTypeRepository _timeEntryTypeRepo = new TimeEntryTypeRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly User _currentUser;
        private List<Project> _filteredProjects = new List<Project>();
        private List<TimeEntryType> _timeEntryTypes = new List<TimeEntryType>();

        public ProjectManagementDialog(User currentUser)
        {
            InitializeComponent();

            KeyPreview = true;
            _currentUser = currentUser;
        }

        private async void ProjectManagementDialog_Load(object sender, EventArgs e)
        {
            listBoxProject.Items.Clear();
            listBoxProject.Items.Add("Načítání...");

            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            await LoadProjectsAsync(0);
        }

        private async Task LoadTimeEntryTypes(int projectType)
        {
            _timeEntryTypes = await _timeEntryTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);
        }

        private bool comboBoxProjectsLoading = false;

        private async Task LoadProjectsAsync(int projectType)
        {
            try
            {
                var _projects = await _projectRepo.GetAllProjectsAsync(true);

                Invoke(new Action(async () =>
                {
                    var filteredProjects = _projects
                        .Where(p => (projectType == 1 || projectType == 2)
                            ? (p.ProjectType == 1 || p.ProjectType == 2)
                            : p.ProjectType == projectType)
                        .ToArray();

                    switch (projectType)
                    {
                        case 0:
                            // PROVOZ
                            listBoxOperation.Items.Clear();
                            listBoxOperation.Items.AddRange(
                                filteredProjects.Select(FormatHelper.FormatProjectToString).ToArray()
                            );
                            break;

                        case 1:
                        case 2:
                            // PROJEKT nebo PŘEDPROJEKT
                            comboBoxProjectsLoading = true;
                            listBoxProject.Items.Clear();
                            comboBoxProjects.Items.Clear();

                            var projectItems = filteredProjects
                                .Where(p => p.IsArchived == (checkBoxArchive.Checked ? 1 : 0))
                                .Select(FormatHelper.FormatProjectToString)
                                .ToArray();

                            listBoxProject.Items.AddRange(projectItems);
                            comboBoxProjects.Items.AddRange(projectItems);

                            if (comboBoxProjects.Items.Count > 0)
                                comboBoxProjects.SelectedIndex = 0;
                            else
                                comboBoxProjects.Text = "";

                            comboBoxProjectsLoading = false;
                            break;

                        case 3:
                            // ŠKOLENÍ
                            break;

                        case 4:
                            // NEPŘÍTOMNOST
                            await LoadTimeEntryTypes(projectType);
                            listBoxAbsence.Items.Clear();
                            listBoxAbsence.Items.AddRange(
                                _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray()
                            );
                            break;

                        case 5:
                            // OSTATNÍ
                            await LoadTimeEntryTypes(projectType);
                            listBoxOther.Items.Clear();
                            listBoxOther.Items.AddRange(
                                _timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray()
                            );
                            break;
                    }

                    _filteredProjects = filteredProjects.ToList();
                    ClearSelection();
                    GenerateNextDescription();
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

        private async void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (tabControl1.SelectedTab.Text)
            {
                case "PROVOZ":
                    await LoadProjectsAsync(0);
                    break;
                case "PROJEKT":
                    await LoadProjectsAsync(1);
                    break;
                case "NEPŘÍTOMNOST":
                    await LoadProjectsAsync(4);
                    break;
                case "OSTATNÍ":
                    await LoadProjectsAsync(5);
                    break;
            }
        }

        private async void checkBoxArchive_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(1);
            buttonArchiveProject.Text = checkBoxArchive.Checked ? "Obnovit" : "Archivovat";
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

            else
            {
                project.Id = _filteredProjects.Find(x => x.ProjectDescription == textBoxProjectDescription.Text).Id;
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

        private void listBoxProject_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxProject.SelectedItem is not null)
            {
                buttonArchiveProject.Visible = true;

                comboBoxProjectsLoading = true;
                comboBoxProjects.SelectedIndex = comboBoxProjects.FindString(listBoxProject.SelectedItem.ToString());
                comboBoxProjectsLoading = false;

                textBoxProjectTitle.Text = listBoxProject.SelectedItem.ToString().Split(':')[1].TrimStart(' ');
                textBoxProjectDescription.Text = _filteredProjects.Find(x => x.ProjectTitle == textBoxProjectTitle.Text).ProjectDescription;

                buttonAddProject.Text = "Uložit";
                buttonArchiveProject.Text = checkBoxArchive.Checked ? "Obnovit" : "Archivovat";
                groupBox2.Text = "Úprava projektu";
            }
        }
    }
}

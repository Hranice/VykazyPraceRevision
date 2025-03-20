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
        private readonly TimeEntryTypeRepository _timeEntrTypeRepo = new TimeEntryTypeRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly User _currentUser;
        //private List<Project> _projects = new List<Project>();
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
            _timeEntryTypes = await _timeEntrTypeRepo.GetAllTimeEntryTypesByProjectTypeAsync(projectType);
        }

        private bool comboBoxProjectsLoading = false;

        private async Task LoadProjectsAsync(int projectType)
        {
            try
            {
                var _projects = await _projectRepo.GetAllProjectsAndContractsAsync(true);

                Invoke(new Action(async () =>
                {
                    var filteredProjects = _projects
                             .Where(p => p.ProjectType == projectType)
                             .ToArray();

                    switch (projectType)
                    {
                        case 0:
                            // PROVOZ
                            listBoxOperation.Items.Clear();
                            listBoxOperation.Items.AddRange(filteredProjects.Select(FormatHelper.FormatProjectToString).ToArray());
                            break;
                        case 1:
                            // PROJEKT
                            comboBoxProjectsLoading = true;
                            listBoxProject.Items.Clear();
                            comboBoxProjects.Items.Clear();

                            if (checkBoxArchive.Checked)
                            {
                                listBoxProject.Items.AddRange(filteredProjects
                                    .Where(p => p.IsArchived == 1)
                                    .Select(FormatHelper.FormatProjectToString)
                                    .ToArray());

                                comboBoxProjects.Items.AddRange(filteredProjects
                                    .Where(p => p.IsArchived == 1)
                                    .Select(FormatHelper.FormatProjectToString)
                                    .ToArray());
                            }
                            else
                            {
                                listBoxProject.Items.AddRange(filteredProjects
                                     .Where(p => p.IsArchived == 0)
                                     .Select(FormatHelper.FormatProjectToString)
                                     .ToArray());

                                comboBoxProjects.Items.AddRange(filteredProjects
                                     .Where(p => p.IsArchived == 0)
                                     .Select(FormatHelper.FormatProjectToString)
                                     .ToArray());
                            }

                            if (comboBoxProjects.Items.Count > 0)
                            {
                                comboBoxProjects.SelectedIndex = 0;
                            }

                            else
                            {
                                comboBoxProjects.Text = "";
                            }

                            comboBoxProjectsLoading = false;
                            break;
                        case 2:
                            // PŘEDPROJEKT
                            comboBoxProjectsLoading = true;
                            listBoxProject.Items.Clear();
                            comboBoxProjects.Items.Clear();

                            if (checkBoxArchive.Checked)
                            {
                                listBoxProject.Items.AddRange(filteredProjects
                                    .Where(p => p.IsArchived == 1)
                                    .Select(FormatHelper.FormatProjectToString)
                                    .ToArray());

                                comboBoxProjects.Items.AddRange(filteredProjects
                                     .Where(p => p.IsArchived == 1)
                                     .Select(FormatHelper.FormatProjectToString)
                                     .ToArray());
                            }
                            else
                            {
                                listBoxProject.Items.AddRange(filteredProjects
                                    .Where(p => p.IsArchived == 0)
                                    .Select(FormatHelper.FormatProjectToString)
                                    .ToArray());

                                comboBoxProjects.Items.AddRange(filteredProjects
                                     .Where(p => p.IsArchived == 0)
                                     .Select(FormatHelper.FormatProjectToString)
                                     .ToArray());
                            }

                            comboBoxProjectsLoading = false;
                            break;
                        case 3:
                            // ŠKOLENÍ
                            break;
                        case 4:
                            // NEPŘÍTOMNOST
                            await LoadTimeEntryTypes(projectType);

                            listBoxAbsence.Items.Clear();
                            listBoxAbsence.Items.AddRange(_timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());
                            break;
                        case 5:
                            // OSTATNÍ
                            await LoadTimeEntryTypes(projectType);

                            listBoxOther.Items.Clear();
                            listBoxOther.Items.AddRange(_timeEntryTypes.Select(FormatHelper.FormatTimeEntryTypeToString).ToArray());
                            break;
                    }

                    _filteredProjects = filteredProjects.ToList();
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


        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                AppLogger.Error("Nepodařilo se načíst aktuálního uživatele, nelze přidat projekt do databáze.");
                return;
            }

            //if (buttonAdd.Text == "Konec prohlížení")
            //{
            //    groupBox1.Text = "Přidání projektu";
            //    textBoxProjectContractDescription.Enabled = true;
            //    textBoxProjectContractNote.Enabled = true;
            //    textBoxProjectContractTitle.Enabled = true;
            //    ClearFields();
            //    buttonAdd.Text = "Přidat";
            //}
            //else
            //{
            //    var (valid, reason) = CheckForEmptyOrIncorrectFields();
            //    if (!valid)
            //    {
            //        AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
            //        return;
            //    }

            //    var newProject = new Project
            //    {
            //        //ProjectDescription = textBoxProjectContractDescription.Text,
            //        //ProjectTitle = textBoxProjectContractTitle.Text,
            //        //Note = textBoxProjectContractNote.Text,
            //        CreatedBy = _currentUser.Id
            //    };

            //    var addedProject = await _projectRepo.CreateProjectAsync(newProject);
            //    if (addedProject is not null)
            //    {
            //        AppLogger.Information($"Projekt {FormatHelper.FormatProjectToString(addedProject)} byl úspěšně přidán.", true);
            //        ClearFields();
            //    }
            //    else
            //    {
            //        AppLogger.Error($"Projekt {FormatHelper.FormatProjectToString(newProject)} nebyl přidán.");
            //    }

            //    //await LoadProjectsContractsAsync();
            //}
        }

        private void ClearFields()
        {
            //textBoxProjectContractTitle.Text = "";
            //textBoxProjectContractDescription.Text = "";
            //textBoxProjectContractNote.Text = "";
        }

        private (bool, string) CheckForEmptyOrIncorrectFields()
        {
            //if (string.IsNullOrEmpty(textBoxProjectContractDescription.Text)) return (false, "Popis");
            //if (string.IsNullOrEmpty(textBoxProjectContractTitle.Text)) return (false, "Název");

            return (true, "");
        }

        private async void buttonArchive_Click(object sender, EventArgs e)
        {
            var project = _filteredProjects[listBoxProject.SelectedIndex];
            project.IsArchived = 1;

            if (project != null)
            {
                var dialogResult = MessageBox.Show($"Archivovat projekt {FormatHelper.FormatProjectToString(project)}?", "Archivovat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _projectRepo.UpdateProjectAsync(project))
                    {
                        AppLogger.Information($"Projekt {FormatHelper.FormatProjectToString(project)} byl archivován.");
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat projekt {FormatHelper.FormatProjectToString(project)} z databáze.");
                    }

                    //await LoadProjectsContractsAsync();
                }
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
                    await LoadProjectsAsync(checkBoxPreProject.Checked ? 2 : 1);
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
            await LoadProjectsAsync(checkBoxPreProject.Checked ? 2 : 1);
        }

        private async void checkBoxPreProject_CheckedChanged(object sender, EventArgs e)
        {
            await LoadProjectsAsync(checkBoxPreProject.Checked ? 2 : 1);
        }

        private void buttonAddOperation_Click(object sender, EventArgs e)
        {

        }

        private void buttonAddProject_Click(object sender, EventArgs e)
        {

        }

        private void buttonArchiveProject_Click(object sender, EventArgs e)
        {

        }

        private void buttonAddAbsence_Click(object sender, EventArgs e)
        {

        }

        private void buttonAddOther_Click(object sender, EventArgs e)
        {

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
            }
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
                comboBoxProjectsLoading = true;
                comboBoxProjects.SelectedIndex = comboBoxProjects.FindString(listBoxProject.SelectedItem.ToString());
                comboBoxProjectsLoading = false;

                textBoxProjectDescription.Text = listBoxProject.SelectedItem.ToString().Split(':')[0];
                textBoxProjectTitle.Text = listBoxProject.SelectedItem.ToString().Split(':')[1].TrimStart(' ');
                buttonAddProject.Text = "Uložit";

                var project = _filteredProjects[listBoxProject.SelectedIndex];

                if (project != null)
                {
                    //groupBox1.Text = $"Zobrazení projektu ({project.Id})";
                    //textBoxProjectContractDescription.Enabled = false;
                    //textBoxProjectContractDescription.Text = project.ProjectDescription;
                    //textBoxProjectContractTitle.Enabled = false;
                    //textBoxProjectContractTitle.Text = project.ProjectTitle;
                    //textBoxProjectContractNote.Enabled = false;
                    //textBoxProjectContractNote.Text = project.Note;

                    //buttonAdd.Text = "Konec prohlížení";
                }
            }
        }
    }
}

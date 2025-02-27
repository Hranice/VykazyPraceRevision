using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class ProjectManagementDialog : Form
    {
        private int _projectType = 0;
        private readonly ProjectRepository _projectRepo = new ProjectRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();

        public ProjectManagementDialog()
        {
            InitializeComponent();
        }

        private void ProjectManagementDialog_Load(object sender, EventArgs e)
        {
            listBoxProjectContract.Items.Clear();
            listBoxProjectContract.Items.Add("Načítání...");

            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);
            _loadingUC.BringToFront();

            Task.Run(() => LoadProjectsContractsAsync());
        }

        private async Task LoadProjectsContractsAsync()
        {
            List<Project> projects = new List<Project>();

            try
            {
                switch (_projectType)
                {
                    case 0:
                        projects = await _projectRepo.GetAllProjectsAsync();
                        break;
                    case 1:
                        projects = await _projectRepo.GetAllContractsAsync();
                        break;
                    default:
                        projects = await _projectRepo.GetAllProjectsAndContractsAsync();
                        break;
                }

                Invoke(new Action(() =>
                {
                    listBoxProjectContract.Items.Clear();
                    foreach (var project in projects)
                    {
                        listBoxProjectContract.Items.Add(FormatProjectToString(project));
                    }
                    _loadingUC.Visible = false;
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error($"Chyba při načítání projektů.", ex);
                    _loadingUC.Visible = false;
                }));
            }
        }

        private string FormatProjectToString(Project project)
        {
            return $"{project.Id} ({project.ProjectDescription}): {project.ProjectTitle}";
        }

        private async void buttonProject_Click(object sender, EventArgs e)
        {
            _projectType = 0;
            await LoadProjectsContractsAsync();
        }

        private async void buttonContract_Click(object sender, EventArgs e)
        {
            _projectType = 1;
            await LoadProjectsContractsAsync();
        }

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (buttonAdd.Text == "Konec prohlížení")
            {
                groupBox1.Text = $"Přidání projektu";
                textBoxProjectContractDescription.Enabled = true;
                textBoxProjectContractNote.Enabled = true;
                textBoxProjectContractTitle.Enabled = true;
                ClearFields();
                buttonAdd.Text = "Přidat";
            }

            else
            {
                var dataCheck = CheckForEmptyOrIncorrectFields();

                if (dataCheck.Item1)
                {
                    var newProject = new Project
                    {
                        ProjectDescription = textBoxProjectContractDescription.Text,
                        ProjectTitle = textBoxProjectContractTitle.Text,
                        Note = textBoxProjectContractNote.Text
                    };

                    var addedProject = await _projectRepo.CreateProjectAsync(newProject);
                    if (addedProject is not null)
                    {
                        AppLogger.Information($"Uživatel {FormatProjectToString(addedProject)} byl přidán do databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Uživatel {FormatProjectToString(newProject)} nebyl přidán do databáze.");
                    }

                    await LoadProjectsContractsAsync();
                }

                else
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {dataCheck.Item2}");
                }
            }
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var project = await GetProjectBySelectedItem();

            if (project != null)
            {
                var dialogResult = MessageBox.Show($"Smazat projekt {FormatProjectToString(project)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _projectRepo.DeleteProjectAsync(project.Id))
                    {
                        AppLogger.Information($"Projekt {FormatProjectToString(project)} byl smazán z databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat projekt {FormatProjectToString(project)} z databáze.");
                    }

                    await LoadProjectsContractsAsync();
                }
            }
        }

        private void ClearFields()
        {
            textBoxProjectContractTitle.Text = "";
            textBoxProjectContractDescription.Text = "";
            textBoxProjectContractNote.Text = "";
        }

        private (bool, string) CheckForEmptyOrIncorrectFields()
        {
            bool pass = true;
            string? reason = "";
            if (string.IsNullOrEmpty(textBoxProjectContractDescription.Text)) pass = false; reason = "Popis";
            if (string.IsNullOrEmpty(textBoxProjectContractTitle.Text)) pass = false; reason = "Název";

            return (pass, reason);
        }

        private async Task<Project?> GetProjectBySelectedItem()
        {
            Project? project = new Project();

            if (listBoxProjectContract.SelectedItem != null)
            {
                string? selectedItem = listBoxProjectContract.SelectedItem?.ToString();
                string? idString = selectedItem?.Split(' ')[0];

                if (int.TryParse(idString, out int userId))
                {
                    project = await _projectRepo.GetProjectByIdAsync(userId);
                }

                else
                {
                    AppLogger.Error($"Nepodařilo se získat projekt {selectedItem} z databáze, id '{idString}' je neplatné.");
                }

                if (project == null)
                {
                    AppLogger.Error($"Nepodařilo se získat projekt {selectedItem} z databáze.");
                }
            }

            return project;
        }

        private async void listBoxProjectContract_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxProjectContract.SelectedIndex >= listBoxProjectContract.Items.Count - 1)
            {
                var project = await GetProjectBySelectedItem();

                if (project != null)
                {
                    groupBox1.Text = $"Zobrazení projektu ({project.Id})";
                    textBoxProjectContractDescription.Enabled = false;
                    textBoxProjectContractDescription.Text = project.ProjectDescription;
                    textBoxProjectContractTitle.Enabled = false;
                    textBoxProjectContractTitle.Text = project.ProjectTitle;
                    textBoxProjectContractNote.Enabled = false;
                    textBoxProjectContractNote.Text = project.Note;

                    buttonAdd.Text = "Konec prohlížení";
                }
            }
        }
    }
}

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
        private readonly UserRepository _userRepo = new UserRepository();
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly User _currentUser;

        public ProjectManagementDialog(User currentUser)
        {
            InitializeComponent();

            _currentUser = currentUser;
        }

        private void ProjectManagementDialog_Load(object sender, EventArgs e)
        {
            listBoxProjectContract.Items.Clear();
            listBoxProjectContract.Items.Add("Načítání...");

            _loadingUC.Size = this.Size;
            this.Controls.Add(_loadingUC);

            Task.Run(() => LoadProjectsContractsAsync());
        }

        private async Task LoadProjectsContractsAsync()
        {
            Invoke(() => _loadingUC.BringToFront());

            try
            {
                List<Project> projects = await _projectRepo.GetAllProjectsAndContractsAsync();

                Invoke(new Action(() =>
                {
                    listBoxProjectContract.Items.Clear();
                    foreach (var project in projects)
                    {
                        listBoxProjectContract.Items.Add(FormatHelper.FormatProjectToString(project));
                    }
                    _loadingUC.Visible = false;
                }));
            }
            catch (Exception ex)
            {
                Invoke(new Action(() =>
                {
                    AppLogger.Error("Chyba při načítání projektů.", ex);
                    _loadingUC.Visible = false;
                }));
            }

            Invoke(() => _loadingUC.Visible = false);
        }


   

        private async void buttonAdd_Click(object sender, EventArgs e)
        {
            if (_currentUser == null)
            {
                AppLogger.Error("Nepodařilo se načíst aktuálního uživatele, nelze přidat projekt do databáze.");
                return;
            }

            if (buttonAdd.Text == "Konec prohlížení")
            {
                groupBox1.Text = "Přidání projektu";
                textBoxProjectContractDescription.Enabled = true;
                textBoxProjectContractNote.Enabled = true;
                textBoxProjectContractTitle.Enabled = true;
                ClearFields();
                buttonAdd.Text = "Přidat";
            }
            else
            {
                var (valid, reason) = CheckForEmptyOrIncorrectFields();
                if (!valid)
                {
                    AppLogger.Error($"Je třeba správně vyplnit všechna potřebná data! Chybný parametr: {reason}");
                    return;
                }

                var newProject = new Project
                {
                    ProjectDescription = textBoxProjectContractDescription.Text,
                    ProjectTitle = textBoxProjectContractTitle.Text,
                    Note = textBoxProjectContractNote.Text,
                    CreatedBy = _currentUser.Id
                };

                var addedProject = await _projectRepo.CreateProjectAsync(newProject);
                if (addedProject is not null)
                {
                    AppLogger.Information($"Projekt {FormatHelper.FormatProjectToString(addedProject)} byl úspěšně přidán.", true);
                    ClearFields();
                }
                else
                {
                    AppLogger.Error($"Projekt {FormatHelper.FormatProjectToString(newProject)} nebyl přidán.");
                }

                await LoadProjectsContractsAsync();
            }
        }

        private async void buttonRemove_Click(object sender, EventArgs e)
        {
            var project = await GetProjectBySelectedItem();

            if (project != null)
            {
                var dialogResult = MessageBox.Show($"Smazat projekt {FormatHelper.FormatProjectToString(project)}?", "Smazat?", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);

                if (dialogResult == DialogResult.Yes)
                {
                    if (await _projectRepo.DeleteProjectAsync(project.Id))
                    {
                        AppLogger.Information($"Projekt {FormatHelper.FormatProjectToString(project)} byl smazán z databáze.", true);
                        ClearFields();
                    }

                    else
                    {
                        AppLogger.Error($"Nepodařilo se smazat projekt {FormatHelper.FormatProjectToString(project)} z databáze.");
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
            if (string.IsNullOrEmpty(textBoxProjectContractDescription.Text)) return (false, "Popis");
            if (string.IsNullOrEmpty(textBoxProjectContractTitle.Text)) return (false, "Název");

            return (true, "");
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
            if (listBoxProjectContract.SelectedItem is not null)
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

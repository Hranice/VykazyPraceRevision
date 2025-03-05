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
        private readonly LoadingUC _loadingUC = new LoadingUC();
        private readonly User _currentUser;
        private List<Project> _projects = new List<Project>();

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
                _projects = await _projectRepo.GetAllProjectsAndContractsAsync();

                Invoke(new Action(() =>
                {
                    listBoxProjectContract.Items.Clear();
                    listBoxProjectContract.Items.AddRange(_projects.Select(FormatHelper.FormatProjectToString).ToArray());
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

        private void listBoxProjectContract_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listBoxProjectContract.SelectedItem is not null)
            {
                var project = _projects[listBoxProjectContract.SelectedIndex];

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

        private async void buttonArchive_Click(object sender, EventArgs e)
        {
            var project = _projects[listBoxProjectContract.SelectedIndex];
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

                    await LoadProjectsContractsAsync();
                }
            }
        }
    }
}

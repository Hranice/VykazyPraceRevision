using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
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
    }
}

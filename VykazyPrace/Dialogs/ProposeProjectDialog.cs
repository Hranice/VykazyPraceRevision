using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Logging.VykazyPrace.Logging;
using VykazyPrace.Helpers;
using VykazyPrace.Logging;
using VykazyPrace.UserControls;

namespace VykazyPrace.Dialogs
{
    public partial class ProposeProjectDialog : Form
    {
        private readonly User _currentUser;
        private readonly ProjectRepository _projectRepo = new ProjectRepository();

        public ProposeProjectDialog(User currentUser)
        {
            InitializeComponent();
            _currentUser = currentUser;
        }

        private async Task GenerateNextProjectDescriptionAsync()
        {
            try
            {
                var allProjects = await _projectRepo.GetAllProjectsAsync(false);

                var currentYear = DateTime.Now.Year % 100;

                // Najdi všechny projekty odpovídající formátu "0000N25"
                var regex = new Regex(@"^(?<number>\d{4})N(?<year>\d{2})$");

                var matchingProjects = allProjects
                    .Select(p => p.ProjectDescription)
                    .Where(desc => desc != null && regex.IsMatch(desc))
                    .Select(desc => regex.Match(desc))
                    .Where(m => m.Success && m.Groups["year"].Value == currentYear.ToString("D2"))
                    .ToList();

                int nextNumber;
                if (matchingProjects.Any())
                {
                    var maxNumber = matchingProjects
                        .Select(m => int.Parse(m.Groups["number"].Value))
                        .Max();

                    nextNumber = maxNumber + 1;
                }
                else
                {
                    nextNumber = 1; // nový rok, začínáme od 1
                }

                var newDescription = $"{nextNumber:D4}N{currentYear:D2}";
                textBoxProjectDescription.Text = newDescription;
            }
            catch (Exception ex)
            {
                AppLogger.Error("Chyba při generování nového ProjectDescription.", ex);
            }
        }

        private async void buttonSend_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxProjectDescription.Text))
            {
                AppLogger.Error("Pole názvu projektu nesmí být prázdné.");
            }

            var project = new Project()
            {
                CreatedBy = _currentUser.Id,
                ProjectTitle = textBoxProjectTitle.Text,
                ProjectDescription = textBoxProjectDescription.Text,
                ProjectType = FormatHelper.IsPreProject(textBoxProjectDescription.Text) ? 2 : 1,
                IsArchived = 0
            };

            await _projectRepo.CreateProjectAsync(project);
            await GenerateNextProjectDescriptionAsync();
            this.Close();
        }

        private async void ProposeProjectDialog_Load(object sender, EventArgs e)
        {
            await GenerateNextProjectDescriptionAsync();
        }
    }
}

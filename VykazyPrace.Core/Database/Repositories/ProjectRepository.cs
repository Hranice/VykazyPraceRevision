using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ProjectRepository
    {
        private readonly VykazyPraceContext _context;

        public ProjectRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Přidání nového projektu do databáze.
        /// </summary>
        public async Task<Project> CreateProjectAsync(Project project)
        {
            var user = await _context.Users.FindAsync(project.CreatedBy);
            if (user == null)
            {
                throw new Exception($"Uživatel s ID {project.CreatedBy} neexistuje.");
            }

            project.CreatedByNavigation = user;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        /// <summary>
        /// Získání všech projektů i zakázek.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAndContractsAsync()
        {
            return await _context.Projects.Include(p => p.CreatedByNavigation).ToListAsync();
        }

        /// <summary>
        /// Získání všech projektů i zakázek.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAndContractsAsync(int projectType)
        {
            return await _context.Projects
                          .Where(p => p.ProjectType == projectType)
                          .Include(p => p.CreatedByNavigation)
                          .ToListAsync();
        }

        /// <summary>
        /// Získání projektu podle ID.
        /// </summary>
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            return await _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Aktualizace projektu v databázi.
        /// </summary>
        public async Task<bool> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null)
                return false;

            existingProject.ProjectTitle = project.ProjectTitle;
            existingProject.ProjectDescription = project.ProjectDescription;
            existingProject.ProjectType = project.ProjectType;
            existingProject.Note = project.Note;

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Smazání projektu podle ID.
        /// </summary>
        public async Task<bool> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return false;

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
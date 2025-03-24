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
        /// Získání všech projektů i zakázek, seřazených podle interního/externího označení,
        /// roku sestupně a pořadového čísla sestupně. Chybné záznamy jsou umístěny na konec.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAndContractsAsync(bool includeArchived = false)
        {
            IQueryable<Project> projectsQuery = _context.Projects
                .Include(p => p.CreatedByNavigation)
                .AsQueryable();

            if (!includeArchived)
            {
                projectsQuery = projectsQuery.Where(p => p.IsArchived == 0);
            }

            var projects = await projectsQuery.ToListAsync(); // Asynchronní načtení dat do paměti

            return projects
                .OrderBy(p => IsValidProjectDescription(p.ProjectDescription) ? 1 : 0) // Ostatní první, seřazené na konec
                .ThenBy(p => GetProjectType(p.ProjectDescription)) // I/E
                .ThenByDescending(p => GetProjectYear(p.ProjectDescription)) // Rok sestupně
                .ThenByDescending(p => GetProjectNumber(p.ProjectDescription)) // Pořadové číslo sestupně
                .ToList();
        }



        /// <summary>
        /// Získání všech projektů i zakázek, seřazených podle interního/externího označení,
        /// roku sestupně a pořadového čísla sestupně. Filtrováno podle typu projektu.
        /// Chybné záznamy jsou umístěny na konec.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAndContractsAsyncByProjectType(int projectType, bool onlyArchived = false)
        {
            var projects = await _context.Projects
                .Include(p => p.CreatedByNavigation)
                .Where(p => p.IsArchived == (onlyArchived ? 1 : 0) && p.ProjectType == projectType)
                .ToListAsync(); // Asynchronní načtení dat do paměti

            return projects
                .OrderBy(p => IsValidProjectDescription(p.ProjectDescription) ? 1 : 0) // Ostatní první, seřazené na konec
                .ThenBy(p => GetProjectType(p.ProjectDescription)) // I/E
                .ThenByDescending(p => GetProjectYear(p.ProjectDescription)) // Rok sestupně
                .ThenByDescending(p => GetProjectNumber(p.ProjectDescription)) // Pořadové číslo sestupně
                .ToList();
        }

        /// <summary>
        /// Ověří, zda je ProjectDescription platný (má správnou délku a formát).
        /// </summary>
        private bool IsValidProjectDescription(string description)
        {
            return !string.IsNullOrEmpty(description) &&
                   description.Length >= 7 &&
                   (description[4] == 'I' || description[4] == 'E') &&
                   int.TryParse(description.Substring(5, 2), out _) &&
                   int.TryParse(description.Substring(0, 4), out _);
        }

        /// <summary>
        /// Vrací typ projektu (I/E) nebo prázdný string pro neplatné záznamy.
        /// </summary>
        private string GetProjectType(string description)
        {
            return IsValidProjectDescription(description) ? description.Substring(4, 1) : "";
        }

        /// <summary>
        /// Vrací rok jako číslo, nebo nejnižší možnou hodnotu pro neplatné záznamy.
        /// </summary>
        private int GetProjectYear(string description)
        {
            return IsValidProjectDescription(description) && int.TryParse(description.Substring(5, 2), out int year) ? year : int.MinValue;
        }

        /// <summary>
        /// Vrací pořadové číslo jako číslo, nebo nejnižší možnou hodnotu pro neplatné záznamy.
        /// </summary>
        private int GetProjectNumber(string description)
        {
            return IsValidProjectDescription(description) && int.TryParse(description.Substring(0, 4), out int number) ? number : int.MinValue;
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
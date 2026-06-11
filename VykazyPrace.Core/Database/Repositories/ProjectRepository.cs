using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class ProjectRepository
    {
        private readonly IDbContextFactory<VykazyPraceContext> _contextFactory;

        public ProjectRepository(IDbContextFactory<VykazyPraceContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        /// <summary>
        /// Přidání nového projektu do databáze.
        /// </summary>
        public async Task<Project> CreateProjectAsync(Project project)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var user = await context.Users.FindAsync(project.CreatedBy);

            if (user == null)
            {
                throw new Exception($"Uživatel s ID {project.CreatedBy} neexistuje.");
            }

            project.CreatedByNavigation = user;

            context.Projects.Add(project);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return project;
        }

        /// <summary>
        /// Získání všech projektů.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAsync(bool includeArchived = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            IQueryable<Project> projectsQuery = context.Projects
                .AsNoTracking()
                .Include(p => p.CreatedByNavigation);

            if (!includeArchived)
            {
                projectsQuery = projectsQuery.Where(p => p.IsArchived == 0);
            }

            var projects = await projectsQuery.ToListAsync();

            return projects
                .OrderBy(p => GetProjectNumber(p.ProjectDescription))
                .ToList();
        }

        /// <summary>
        /// Filtrováno podle typu projektu.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAsyncByProjectType(
            int projectType,
            bool onlyArchived = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var projects = await context.Projects
                .AsNoTracking()
                .Include(p => p.CreatedByNavigation)
                .Where(p =>
                    p.IsArchived == (onlyArchived ? 1 : 0) &&
                    p.ProjectType == projectType)
                .ToListAsync();

            return projects
                .OrderBy(p => GetProjectNumber(p.ProjectDescription))
                .ToList();
        }

        /// <summary>
        /// Získání všech zakázek a předprojektů.
        /// </summary>
        public async Task<List<Project>> GetAllFullProjectsAndPreProjectsAsync(bool archived = false)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var projects = await context.Projects
                .AsNoTracking()
                .Include(p => p.CreatedByNavigation)
                .Where(p =>
                    p.IsArchived == (archived ? 1 : 0) &&
                    (p.ProjectType == 1 || p.ProjectType == 2))
                .ToListAsync();

            return projects
                .OrderBy(p => GetProjectNumber(p.ProjectDescription))
                .ToList();
        }

        /// <summary>
        /// Získání projektu podle ID.
        /// </summary>
        public async Task<Project?> GetProjectByIdAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            return await context.Projects
                .AsNoTracking()
                .Include(p => p.CreatedByNavigation)
                .Include(p => p.TimeEntries)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Aktualizace projektu v databázi.
        /// </summary>
        public async Task<bool> UpdateProjectAsync(Project project)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var existingProject = await context.Projects.FindAsync(project.Id);

            if (existingProject == null)
                return false;

            existingProject.ProjectTitle = project.ProjectTitle;
            existingProject.ProjectDescription = project.ProjectDescription;
            existingProject.ProjectType = project.ProjectType;
            existingProject.Note = project.Note;
            existingProject.DateFullFilled = project.DateFullFilled;
            existingProject.IsArchived = project.IsArchived;

            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Smazání projektu podle ID.
        /// </summary>
        public async Task<bool> DeleteProjectAsync(int id)
        {
            await using var context = await _contextFactory.CreateDbContextAsync();

            var project = await context.Projects.FindAsync(id);

            if (project == null)
                return false;

            context.Projects.Remove(project);
            await VykazyPraceContextExtensions.SafeSaveAsync(context);

            return true;
        }

        /// <summary>
        /// Vrací pořadové číslo jako číslo, nebo nejvyšší možnou hodnotu pro neplatné záznamy.
        /// </summary>
        private static int GetProjectNumber(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return int.MaxValue;

            var match = Regex.Match(description, @"^\d+");

            return match.Success && int.TryParse(match.Value, out int number)
                ? number
                : int.MaxValue;
        }

        /// <summary>
        /// Ověří, zda je ProjectDescription platný.
        /// </summary>
        private static bool IsValidProjectDescription(string? description)
        {
            return !string.IsNullOrEmpty(description) &&
                   description.Length >= 7 &&
                   (description[4] == 'I' || description[4] == 'E') &&
                   int.TryParse(description.Substring(5, 2), out _) &&
                   int.TryParse(description.Substring(0, 4), out _);
        }

        /// <summary>
        /// Vrací typ projektu I/E nebo prázdný string pro neplatné záznamy.
        /// </summary>
        private static string GetProjectType(string? description)
        {
            return IsValidProjectDescription(description)
                ? description!.Substring(4, 1)
                : string.Empty;
        }

        /// <summary>
        /// Vrací rok jako číslo, nebo nejnižší možnou hodnotu pro neplatné záznamy.
        /// </summary>
        private static int GetProjectYear(string? description)
        {
            return IsValidProjectDescription(description) &&
                   int.TryParse(description!.Substring(5, 2), out int year)
                ? year
                : int.MinValue;
        }
    }
}
using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
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
        /// Vytvoří nový projekt v databázi.
        /// </summary>
        /// <param name="project">Nový projekt k vytvoření.</param>
        /// <returns>Tuple s výsledkem operace, vytvořeným projektem (nebo null) a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, Project? Projects, string? Error)> CreateProjectAsync(Project project)
        {
            try
            {
                var user = await _context.Users.FindAsync(project.CreatedBy);
                if (user == null)
                    return (false, null, $"Uživatel s ID {project.CreatedBy} neexistuje.");

                project.CreatedByNavigation = user;
                _context.Projects.Add(project);

                var saveResult = await VykazyPraceContextExtensions.SafeSaveAsync(_context);
                return saveResult.Success
                    ? (true, project, null)
                    : (false, null, saveResult.ErrorMessage);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Načte všechny projekty z databáze, s volitelným zahrnutím archivovaných.
        /// </summary>
        /// <param name="includeArchived">Zda zahrnout archivované projekty.</param>
        /// <returns>Tuple s výsledkem operace, seznamem projektů (nebo null) a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, List<Project>? Projects, string? Error)> GetAllProjectsAsync(bool includeArchived = false)
        {
            return await GetSortedProjectsAsync(
                _context.Projects.Include(p => p.CreatedByNavigation),
                p => includeArchived || p.IsArchived == 0
            );
        }

        /// <summary>
        /// Načte všechny projekty podle typu projektu a archivace.
        /// </summary>
        /// <param name="projectType">Typ projektu.</param>
        /// <param name="onlyArchived">Zda vrátit pouze archivované projekty.</param>
        /// <returns>Tuple s výsledkem operace, seznamem projektů (nebo null) a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, List<Project>? Projects, string? Error)> GetAllProjectsAsyncByProjectType(int projectType, bool onlyArchived = false)
        {
            return await GetSortedProjectsAsync(
                _context.Projects.Include(p => p.CreatedByNavigation),
                p => p.ProjectType == projectType && p.IsArchived == (onlyArchived ? 1 : 0)
            );
        }

        /// <summary>
        /// Načte všechny projekty typu "Plný projekt" a "Předprojekt", s volitelným filtrem na archivaci.
        /// </summary>
        /// <param name="archived">Zda vrátit archivované projekty.</param>
        /// <returns>Tuple s výsledkem operace, seznamem projektů (nebo null) a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, List<Project>? Projects, string? Error)> GetAllFullProjectsAndPreProjectsAsync(bool archived = false)
        {
            return await GetSortedProjectsAsync(
                _context.Projects.Include(p => p.CreatedByNavigation),
                p => (p.ProjectType == 1 || p.ProjectType == 2) && p.IsArchived == (archived ? 1 : 0)
            );
        }

        /// <summary>
        /// Načte detail konkrétního projektu podle ID.
        /// </summary>
        /// <param name="id">ID projektu.</param>
        /// <returns>Tuple s výsledkem operace, nalezeným projektem (nebo null) a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, Project? Project, string? Error)> GetProjectByIdAsync(int id)
        {
            try
            {
                var project = await _context.Projects
                    .Include(p => p.CreatedByNavigation)
                    .Include(p => p.TimeEntries)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return (true, project, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Aktualizuje existující projekt v databázi.
        /// </summary>
        /// <param name="project">Projekt s novými daty.</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> UpdateProjectAsync(Project project)
        {
            var existingProject = await _context.Projects.FindAsync(project.Id);
            if (existingProject == null)
                return new SaveResult(true, null, 0); // Nic se nezměnilo

            existingProject.ProjectTitle = project.ProjectTitle;
            existingProject.ProjectDescription = project.ProjectDescription;
            existingProject.ProjectType = project.ProjectType;
            existingProject.Note = project.Note;

            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Smaže projekt podle ID.
        /// </summary>
        /// <param name="id">ID projektu k odstranění.</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> DeleteProjectAsync(int id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
                return new SaveResult(true, null, 0); // Už neexistuje

            _context.Projects.Remove(project);
            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Načte a seřadí projekty dle číselného prefixu v popisu, s použitím filtru.
        /// </summary>
        /// <param name="query">Výchozí dotaz (např. včetně Include).</param>
        /// <param name="predicate">Filtr aplikovaný na projekty po načtení.</param>
        /// <returns>Tuple s výsledkem operace, filtrovaným a seřazeným seznamem projektů (nebo null) a případnou chybovou zprávou.</returns>
        private async Task<(bool Success, List<Project>? Result, string? Error)> GetSortedProjectsAsync(
            IQueryable<Project> query,
            Func<Project, bool> predicate)
        {
            try
            {
                var projects = await query.ToListAsync();
                var filtered = projects.Where(predicate)
                                       .OrderBy(p => GetProjectNumber(p.ProjectDescription))
                                       .ToList();

                return (true, filtered, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Extrahuje číselný prefix z popisu projektu, který slouží k řazení.
        /// </summary>
        /// <param name="description">Popis projektu.</param>
        /// <returns>Číselná hodnota prefixu nebo int.MaxValue pokud není číslo přítomno.</returns>
        private int GetProjectNumber(string description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return int.MaxValue;

            var match = Regex.Match(description, @"^\d+");
            return match.Success && int.TryParse(match.Value, out int number)
                ? number
                : int.MaxValue;
        }
    }
}

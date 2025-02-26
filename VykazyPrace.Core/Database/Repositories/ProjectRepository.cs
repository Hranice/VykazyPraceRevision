﻿using Microsoft.EntityFrameworkCore;
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

        public ProjectRepository(VykazyPraceContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Přidání nového projektu do databáze.
        /// </summary>
        public async Task<Project> CreateProjectAsync(Project project)
        {
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            return project;
        }

        /// <summary>
        /// Získání všech projektů.
        /// </summary>
        public async Task<List<Project>> GetAllProjectsAsync()
        {
            return await _context.Projects.Include(p => p.CreatedByNavigation).ToListAsync();
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
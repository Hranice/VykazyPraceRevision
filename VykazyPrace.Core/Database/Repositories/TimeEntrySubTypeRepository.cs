using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntrySubTypeRepository
    {
        private readonly VykazyPraceContext _context;

        public TimeEntrySubTypeRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Vytvoří nový záznam <see cref="TimeEntrySubType"/> nebo obnoví archivovaný se stejným názvem a uživatelem.
        /// </summary>
        /// <param name="subType">Podtyp záznamu času k vytvoření.</param>
        /// <returns>Tuple s výsledkem, záznamem (novým nebo obnoveným), a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, TimeEntrySubType? TimeEntrySubTypes, string? Error)> CreateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            try
            {
                var existingEntry = await _context.TimeEntrySubTypes
                    .FirstOrDefaultAsync(t => t.Title == subType.Title && t.UserId == subType.UserId);

                if (existingEntry != null)
                {
                    if (existingEntry.IsArchived == 1)
                    {
                        existingEntry.IsArchived = 0;
                        var saveResult = await _context.SafeSaveAsync();
                        if (!saveResult.Success)
                            return (false, null, saveResult.ErrorMessage);
                    }

                    return (true, existingEntry, null);
                }

                _context.TimeEntrySubTypes.Add(subType);
                var result = await _context.SafeSaveAsync();
                return result.Success
                    ? (true, subType, null)
                    : (false, null, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Načte všechny podtypy záznamů času včetně uživatelů.
        /// </summary>
        /// <returns>Tuple s výsledkem, seznamem podtypů a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, List<TimeEntrySubType>? TimeEntrySubTypes, string? Error)> GetAllTimeEntrySubTypesAsync()
        {
            try
            {
                var result = await _context.TimeEntrySubTypes.Include(t => t.User).ToListAsync();
                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Načte všechny nearchivované podtypy záznamů času podle ID uživatele.
        /// </summary>
        /// <param name="userId">ID uživatele.</param>
        /// <returns>Tuple s výsledkem, seznamem podtypů a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, List<TimeEntrySubType>? TimeEntrySubTypes, string? Error)> GetAllTimeEntrySubTypesByUserIdAsync(int userId)
        {
            try
            {
                var result = await _context.TimeEntrySubTypes
                    .Where(t => t.UserId == userId && t.IsArchived == 0)
                    .Include(t => t.User)
                    .ToListAsync();

                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Načte podtyp záznamu času podle ID.
        /// </summary>
        /// <param name="id">ID podtypu.</param>
        /// <returns>Tuple s výsledkem, nalezeným podtypem nebo null a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, TimeEntrySubType? TimeEntrySubType, string? Error)> GetTimeEntrySubTypeByIdAsync(int id)
        {
            try
            {
                var result = await _context.TimeEntrySubTypes
                    .Include(t => t.User)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return (true, result, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Aktualizuje existující podtyp záznamu času.
        /// </summary>
        /// <param name="subType">Upravený podtyp.</param>
        /// <returns>Tuple s výsledkem, true při úspěchu, a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, bool Result, string? Error)> UpdateTimeEntrySubTypeAsync(TimeEntrySubType subType)
        {
            throw new NotImplementedException("Tu má být saveresult");
            try
            {
                var existingSubType = await _context.TimeEntrySubTypes.FindAsync(subType.Id);
                if (existingSubType == null)
                    return (true, false, null); // Neexistuje

                existingSubType.Title = subType.Title;
                existingSubType.UserId = subType.UserId;

                var result = await _context.SafeSaveAsync();
                return (result.Success, result.Success, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return (false, false, ex.Message);
            }
        }

        /// <summary>
        /// Smaže podtyp záznamu času podle ID.
        /// </summary>
        /// <param name="id">ID podtypu k odstranění.</param>
        /// <returns>Tuple s výsledkem, true pokud byl smazán, a případnou chybovou zprávou.</returns>
        public async Task<(bool Success, bool Result, string? Error)> DeleteTimeEntrySubTypeAsync(int id)
        {
            throw new NotImplementedException("Tu má být saveresult");
            try
            {
                var subType = await _context.TimeEntrySubTypes.FindAsync(id);
                if (subType == null)
                    return (true, false, null); // Neexistuje

                _context.TimeEntrySubTypes.Remove(subType);
                var result = await _context.SafeSaveAsync();
                return (result.Success, result.Success, result.ErrorMessage);
            }
            catch (Exception ex)
            {
                return (false, false, ex.Message);
            }
        }
    }
}

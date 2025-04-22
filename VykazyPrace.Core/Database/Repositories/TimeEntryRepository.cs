using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class TimeEntryRepository
    {
        private readonly VykazyPraceContext _context;

        public TimeEntryRepository()
        {
            _context = new VykazyPraceContext();
        }

        /// <summary>
        /// Přidání nového časového záznamu do databáze.
        /// </summary>
        /// <param name="timeEntry">Objekt záznamu k vytvoření.</param>
        /// <returns>Tuple s úspěchem operace, nově vytvořeným záznamem a případnou chybovou hláškou.</returns>
        public async Task<(bool Success, TimeEntry? Entry, string? ErrorMessage)> CreateTimeEntryAsync(TimeEntry timeEntry)
        {
            _context.TimeEntries.Add(timeEntry);
            var result = await _context.SafeSaveAsync();

            if (!result.Success)
                return (false, null, result.ErrorMessage);

            return (true, timeEntry, null);
        }


        /// <summary>
        /// Získání všech časových záznamů včetně vazeb.
        /// </summary>
        /// <returns>Tuple s úspěchem, seznamem záznamů a případnou chybovou hláškou.</returns>
        public async Task<(bool Success, List<TimeEntry>? Entries, string? ErrorMessage)> GetAllTimeEntriesAsync()
        {
            try
            {
                var entries = await _context.TimeEntries
                .Include(t => t.User)
                .ThenInclude(t => t.UserGroup)
                .Include(t => t.EntryType)
                .Include(t => t.Project)
                .ToListAsync();

                return (true, entries, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Získání všech časových záznamů pro uživatele.
        /// </summary>
        /// <param name="user">Uživatel, pro kterého se záznamy získávají.</param>
        /// <param name="includeSnacks">Zda zahrnout i „snack“ záznamy.</param>
        /// <returns>Tuple s úspěchem, seznamem záznamů a chybovou hláškou.</returns>
        public async Task<(bool Success, List<TimeEntry>? Entries, string? ErrorMessage)> GetAllTimeEntriesByUserAsync(User user, bool includeSnacks = false)
        {
            List<TimeEntry> entries = new List<TimeEntry>();

            try
            {
                if (includeSnacks)
                {
                    entries = await _context.TimeEntries
                        .AsNoTracking()
                        .Where(te => te.UserId == user.Id)
                        .Include(te => te.Project)
                        .ToListAsync();
                }

                else
                {
                    entries = await _context.TimeEntries
                        .AsNoTracking()
                        .Where(te => te.UserId == user.Id &&
                                    !(te.ProjectId == 132 && te.EntryTypeId == 24))
                        .Include(te => te.Project)
                        .ToListAsync();
                }

                return (true, entries, null);
            }

            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele podle zakázky/projektu.
        /// </summary>)
        public async Task<List<TimeEntry>> GetAllTimeEntriesByUserAsync(User user, int projectType)
        {
            throw new NotImplementedException("Je třeba převést na safe metodu.");

            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id && te.Project != null && te.Project.ProjectType == projectType)
                .Include(te => te.Project)
                .ToListAsync();

        }

        /// <summary>
        /// Získání všech časových záznamů pro konkrétního uživatele, typ projektu a konkrétní den.
        /// </summary>
        public async Task<List<TimeEntry>> GetTimeEntriesByProjectTypeAndDateAsync(User user, int projectType, DateTime date)
        {
            throw new NotImplementedException("Je třeba převést na safe metodu.");

            return await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Project.ProjectType == projectType
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();
        }

        /// <summary>
        /// Získání záznamů uživatele pro konkrétní den.
        /// </summary>
        /// <param name="user">Uživatel.</param>
        /// <param name="date">Datum.</param>
        /// <returns>Tuple s úspěchem, seznamem záznamů a chybovou hláškou.</returns>
        public async Task<(bool Success, List<TimeEntry>? Entries, string? ErrorMessage)> GetTimeEntriesByUserAndDateAsync(User user, DateTime date)
        {
            try
            {
                var entries = await _context.TimeEntries
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date == date.Date)
                .Include(te => te.Project)
                .ToListAsync();

                return (true, entries, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Získání záznamů uživatele pro celý týden podle zadaného dne.
        /// </summary>
        /// <param name="user">Uživatel.</param>
        /// <param name="date">Den, podle kterého se určí týden.</param>
        /// <returns>Tuple s úspěchem, seznamem záznamů a chybovou hláškou.</returns>
        public async Task<(bool Success, List<TimeEntry>? Entries, string? ErrorMessage)> GetTimeEntriesByUserAndCurrentWeekAsync(User user, DateTime date)
        {
            var startOfWeek = date.Date.AddDays(-(int)date.DayOfWeek + (date.DayOfWeek == DayOfWeek.Sunday ? -6 : 1));
            var endOfWeek = startOfWeek.AddDays(6);

            try
            {
                var entries = await _context.TimeEntries
                .AsNoTracking()
                .Where(te => te.UserId == user.Id
                             && te.Project != null
                             && te.Timestamp.HasValue
                             && te.Timestamp.Value.Date >= startOfWeek
                             && te.Timestamp.Value.Date <= endOfWeek)
                .Include(te => te.Project)
                .ToListAsync();

                return (true, entries, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }


        /// <summary>
        /// Získání celkového počtu odpracovaných minut pro daný den.
        /// </summary>
        /// <param name="user">Uživatel.</param>
        /// <param name="date">Datum.</param>
        /// <returns>Tuple s úspěchem, počtem minut a chybovou hláškou.</returns>
        public async Task<(bool Success, int TotalMinutes, string? ErrorMessage)> GetTotalMinutesForUserByDayAsync(User user, DateTime date)
        {
            DateTime today = DateTime.Today;

            try
            {
                int totalMinutes = await _context.TimeEntries
             .Where(te => te.UserId == user.Id
                          && te.Timestamp.HasValue
                          && te.Timestamp.Value.Date == date.Date)
             .SumAsync(te => te.EntryMinutes);

                return (true, totalMinutes, null);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Získání celkového počtu odpracovaných minut pro daný den a typ projektu.
        /// </summary>
        /// <param name="user">Uživatel.</param>
        /// <param name="date">Datum.</param>
        /// <param name="projectType">Typ projektu.</param>
        public async Task<(bool Success, int TotalMinutes, string? ErrorMessage)> GetTotalMinutesForUserByDayAsync(User user, DateTime date, int projectType)
        {
            DateTime today = DateTime.Today;
            try
            {
                var totalMinutes = await _context.TimeEntries
              .Where(te => te.UserId == user.Id
                           && te.Project != null
                           && te.Project.ProjectType == projectType
                           && te.Timestamp.HasValue
                           && te.Timestamp.Value.Date == date.Date)
              .SumAsync(te => te.EntryMinutes);

                return (true, totalMinutes, null);
            }
            catch (Exception ex)
            {
                return (false, 0, ex.Message);
            }
        }

        /// <summary>
        /// Získání jednoho záznamu podle ID, včetně uživatele a projektu.
        /// </summary>
        /// <param name="id">ID záznamu.</param>
        /// <returns>Tuple s úspěchem, nalezeným záznamem a chybovou hláškou.</returns>
        public async Task<(bool Success, TimeEntry? Entry, string? Error)> TryGetTimeEntryByIdAsync(int id)
        {
            try
            {
                var entry = await _context.TimeEntries
                    .AsNoTracking()
                    .Include(t => t.User)
                    .Include(t => t.Project)
                    .FirstOrDefaultAsync(t => t.Id == id);

                return (true, entry, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Aktualizace existujícího záznamu podle ID.
        /// </summary>
        /// <param name="timeEntry">Změněný záznam.</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> UpdateTimeEntryAsync(TimeEntry? timeEntry)
        {
            var existingEntry = await _context.TimeEntries.FindAsync(timeEntry.Id);
            if (existingEntry == null)
                return new SaveResult(true, null, 0);

            existingEntry.EntryTypeId = timeEntry.EntryTypeId;
            existingEntry.Description = timeEntry.Description;
            existingEntry.EntryMinutes = timeEntry.EntryMinutes;
            existingEntry.Timestamp = timeEntry.Timestamp;
            existingEntry.UserId = timeEntry.UserId;
            existingEntry.ProjectId = timeEntry.ProjectId;
            existingEntry.AfterCare = timeEntry.AfterCare;
            existingEntry.IsValid = timeEntry.IsValid;
            existingEntry.Note = timeEntry.Note;

            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Nahrazení textu v popisu všech záznamů daného uživatele.
        /// </summary>
        /// <param name="userId">ID uživatele.</param>
        /// <param name="originalText">Text, který se má nahradit.</param>
        /// <param name="newText">Text, který se má vložit.</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> ReplaceDescriptionForUserAsync(int userId, string originalText, string newText)
        {
            int affected = await _context.TimeEntries
                .Where(e => e.UserId == userId && e.Description.Contains(originalText))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.Description, newText));

            return new SaveResult(true, null, affected);
        }

        /// <summary>
        /// Smazání časového záznamu podle ID.
        /// </summary>
        /// <param name="id">ID záznamu.</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> DeleteTimeEntryAsync(int id)
        {
            var entry = await _context.TimeEntries.FindAsync(id);
            if (entry == null)
                return new SaveResult(true, null, 0);

            _context.TimeEntries.Remove(entry);
            return await VykazyPraceContextExtensions.SafeSaveAsync(_context);
        }

        /// <summary>
        /// Sumarizace odpracovaných hodin dle uživatele a projektu za dané období.
        /// </summary>
        /// <param name="fromDate">Počáteční datum.</param>
        /// <param name="toDate">Koncové datum.</param>
        /// <returns>Tuple s úspěchem, seznamem souhrnů a chybovou hláškou.</returns>
        public async Task<(bool Success, List<TimeEntrySummary>? Summaries, string? Error)> GetTimeEntriesSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var entriesSummaries = await _context.TimeEntries
               .Where(te => te.Timestamp.HasValue
                            && te.Timestamp.Value.Date >= fromDate.Date
                            && te.Timestamp.Value.Date <= toDate.Date
                            && !(te.ProjectId == 132 && te.EntryTypeId == 24))
               .GroupBy(te => new { te.UserId, te.ProjectId })
               .Select(g => new TimeEntrySummary
               {
                   UserId = g.Key.UserId,
                   ProjectId = g.Key.ProjectId,
                   TotalHours = g.Sum(te => te.EntryMinutes) / 60.0
               })
               .ToListAsync();

                return (true, entriesSummaries, null);
            }
            catch (Exception ex)
            {
                return (false, null, ex.Message);
            }
        }

        /// <summary>
        /// Zamknutí všech záznamů v daném měsíci.
        /// </summary>
        /// <param name="month">Název měsíce v češtině (např. "Leden").</param>
        /// <returns>Výsledek uložení databáze ve formátu <see cref="SaveResult"/>.</returns>
        public async Task<SaveResult> LockAllEntriesInMonth(string month)
        {
            var months = new Dictionary<string, int>(StringComparer.InvariantCultureIgnoreCase)
            {
                ["Leden"] = 1,
                ["Únor"] = 2,
                ["Březen"] = 3,
                ["Duben"] = 4,
                ["Květen"] = 5,
                ["Červen"] = 6,
                ["Červenec"] = 7,
                ["Srpen"] = 8,
                ["Září"] = 9,
                ["Říjen"] = 10,
                ["Listopad"] = 11,
                ["Prosinec"] = 12
            };

            if (!months.TryGetValue(month, out var monthNumber))
                throw new ArgumentException($"Neplatný měsíc: {month}");

            int affected = await _context.TimeEntries
                .Where(e => e.Timestamp.HasValue && e.Timestamp.Value.Month == monthNumber)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(e => e.IsLocked, 1));

            return new SaveResult(true, null, affected);
        }


        /// <summary>
        /// Pomocná třída obsahující souhrnné informace o odpracovaném čase.
        /// </summary>
        public class TimeEntrySummary
        {
            public int? UserId { get; set; }
            public int? ProjectId { get; set; }
            public double TotalHours { get; set; }
        }
    }
}

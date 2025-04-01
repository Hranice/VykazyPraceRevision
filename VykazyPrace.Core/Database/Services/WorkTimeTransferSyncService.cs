using VykazyPrace.Core.Database.Models;
using VykazyPrace.Core.Database.Repositories;
using VykazyPrace.Core.Database.MsSql;

namespace VykazyPrace.Core.Database.Services
{
    public class WorkTimeTransferSyncService
    {
        private readonly WorkTimeTransferRepository _sqliteRepository;
        private readonly WorkTimeTransferViewManager _viewManager;

        public WorkTimeTransferSyncService(
            WorkTimeTransferRepository sqliteRepository,
            WorkTimeTransferViewManager viewManager)
        {
            _sqliteRepository = sqliteRepository;
            _viewManager = viewManager;
        }

        /// <summary>
        /// Vrátí záznamy z SQLite nebo je dotáhne z MSSQL view a uloží
        /// </summary>
        public async Task<List<WorkTimeTransfer>> GetOrSyncByDateAsync(DateTime date, int personalNumber)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");

            // 1. Zkontroluj, jestli už máme data v SQLite
            bool exists = await _sqliteRepository.HasRecordsForDateAsync(date);
            if (exists)
            {
                return (await _sqliteRepository.GetByPersonalNumberAsync(personalNumber))
                    .Where(r => r.WorkDate == formattedDate)
                    .ToList();
            }

            // 2. Dotáhni všechna data z MSSQL pro daného uživatele (view obsahuje celý měsíc)
            var allFromView = _viewManager.LoadByPersonalNumber(personalNumber);

            var filtered = allFromView
                .Where(x => x.WorkDate.Date == date.Date)
                .ToList();

            if (filtered.Count == 0)
                return new List<WorkTimeTransfer>();

            // 3. Převod na SQLite entity
            var toSave = filtered.Select(r => new WorkTimeTransfer
            {
                PersonId = r.PersonId,
                PersonalNumber = r.PersonalNumber,
                Arrival = r.Arrival?.ToString("s"),
                Departure = r.Departure?.ToString("s"),
                DepartureReason = r.DepartureReason,
                StandardHours = r.StandardHours,
                OvertimeHours = r.OvertimeHours,
                WorkDate = r.WorkDate.ToString("yyyy-MM-dd"),
                ApprovalState = r.ApprovalState
            }).ToList();

            // 4. Ulož do SQLite
            await _sqliteRepository.SaveRangeAsync(toSave);

            return toSave;
        }
    }
}

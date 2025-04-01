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

        public async Task SyncAsync(int personalNumber)
        {
            var records = _viewManager.LoadByPersonalNumber(personalNumber);

            await _sqliteRepository.ClearAllAsync();

            var entities = records.Select(r => new WorkTimeTransfer
            {
                PersonId = r.PersonId,
                PersonalNumber = r.PersonalNumber,
                Arrival = r.Arrival,
                Departure = r.Departure,
                DepartureReason = r.DepartureReason,
                StandardHours = r.StandardHours,
                OvertimeHours = r.OvertimeHours,
                WorkDate = r.WorkDate,
                ApprovalState = r.ApprovalState
            }).ToList();

            await _sqliteRepository.SaveRangeAsync(entities);
        }
    }
}

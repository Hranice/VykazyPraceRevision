using Microsoft.EntityFrameworkCore;
using VykazyPrace.Core.Database.Models;

namespace VykazyPrace.Core.Database.Repositories
{
    public class WorkTimeTransferRepository
    {
        private readonly VykazyPraceContext _context;

        public WorkTimeTransferRepository(VykazyPraceContext context)
        {
            _context = context;
        }

        public async Task<List<WorkTimeTransfer>> GetAllAsync()
        {
            return await _context.WorkTimeTransfers.ToListAsync();
        }

        public async Task ClearAllAsync()
        {
            var all = await _context.WorkTimeTransfers.ToListAsync();
            _context.WorkTimeTransfers.RemoveRange(all);
            await _context.SaveChangesAsync();
        }

        public async Task SaveRangeAsync(IEnumerable<WorkTimeTransfer> records)
        {
            _context.WorkTimeTransfers.AddRange(records);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkTimeTransfer>> GetByPersonalNumberAsync(int personalNumber)
        {
            return await _context.WorkTimeTransfers
                .Where(w => w.PersonalNumber == personalNumber)
                .ToListAsync();
        }

        public async Task<bool> HasRecordsForDateAsync(DateTime date)
        {
            string formattedDate = date.ToString("yyyy-MM-dd");
            return await _context.WorkTimeTransfers.AnyAsync(x => x.WorkDate == formattedDate);
        }

    }
}

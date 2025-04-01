using VykazyPrace.Core.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace VykazyPrace.Core.Database.Repositories
{
    public class WorkTimeTransferRepository
    {
        private readonly VykazyPraceContext _context;

        public WorkTimeTransferRepository(VykazyPraceContext context)
        {
            _context = context;
        }

        public async Task SaveRangeAsync(IEnumerable<WorkTimeTransfer> transfers)
        {
            _context.WorkTimeTransfers.AddRange(transfers);
            await _context.SaveChangesAsync();
        }

        public async Task ClearAllAsync()
        {
            var all = await _context.WorkTimeTransfers.ToListAsync();
            _context.WorkTimeTransfers.RemoveRange(all);
            await _context.SaveChangesAsync();
        }

        public async Task<List<WorkTimeTransfer>> GetAllAsync()
        {
            return await _context.WorkTimeTransfers.ToListAsync();
        }
    }
}

using Microsoft.EntityFrameworkCore;

namespace VykazyPrace.Core.Database.Models
{
    public static class VykazyPraceContextExtensions
    {
        public static async Task<bool> SafeSaveAsync(this DbContext context, int maxRetries = 3, int delayMs = 200)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    await context.SaveChangesAsync();
                    return true;
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("database is locked") == true)
                {
                    await Task.Delay(delayMs);
                }
            }

            return false;
        }
    }
}

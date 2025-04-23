using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace VykazyPrace.Core.Database.Models
{
    public record SaveResult(bool Success, string? ErrorMessage, int AffectedRecords = 0);

    public static class VykazyPraceContextExtensions
    {
        public static async Task<SaveResult> SafeSaveAsync(this DbContext context, int maxRetries = 3, int delayMs = 200)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    int affected = await context.SaveChangesAsync();
                    return new SaveResult(true, null, affected);
                }
                catch (DbUpdateException ex) when (ex.InnerException?.Message.Contains("database is locked") == true)
                {
                    await Task.Delay(delayMs);
                }
                catch (DbUpdateException ex) when (
                    ex.InnerException?.Message.Contains("unable to open database file") == true ||
                    ex.InnerException?.Message.Contains("disk I/O error") == true)
                {
                    return new SaveResult(false, "Nelze uložit změny – síťový disk je pravděpodobně odpojen.");
                }
                catch (IOException ioEx)
                {
                    return new SaveResult(false, $"Chyba vstupu/výstupu: {ioEx.Message}");
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.InnerException);
                    return new SaveResult(false, $"Neznámá chyba: {ex.Message}\n\n{ex.InnerException}");
                }
            }

            return new SaveResult(false, "Nepodařilo se uložit změny po několika pokusech.");
        }
    }



}

using Microsoft.Data.Sqlite;
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
                catch (DbUpdateException ex) when (
                    ex.InnerException?.Message.Contains("database is locked") == true)
                {
                    // tranzientní zámek → zkusíme znovu
                    await Task.Delay(delayMs);
                }
                catch (DbUpdateException ex) when (
                    ex.InnerException?.Message.Contains("I/O error") == true
                 || ex.InnerException?.Message.Contains("unable to open database file") == true)
                {
                    // nelze otevřít soubor (např. odpojená Wi-Fi) → okamžitě false
                    return false;
                }
                catch (IOException)
                {
                    // obecná IO výjimka
                    return false;
                }
            }

            // vyčerpali jsme retry u zamčené DB
            return false;
        }

    }
}

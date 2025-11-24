using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Linq;
using VykazyPrace.Core.Logging;

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
                    // nelze otevřít soubor (např. odpojená Wi-Fi) => okamžitě false
                    return false;
                }
                catch (IOException ex)
                {
                    // obecná IO výjimka
                    AppLogger.Error("Nastala chyba IO operace do databáze.", ex);
                    return false;
                }

                catch(Exception ex)
                {
                    AppLogger.Error("Nastala chyba zápisu do databáze.", ex);
                }
            }

            // vyčerpali jsme retry u zamčené DB
            return false;
        }

        /// <summary>
        /// Bezpečný FindAsync s retry na BUSY/LOCKED. 
        /// Vrací default (null), pokud nastane IO/CantOpen nebo po vyčerpání retry.
        /// </summary>
        public static async Task<TEntity?> SafeFindAsync<TEntity>(
            this DbSet<TEntity> set,
            object?[] keyValues,
            int maxRetries = 3,
            int delayMs = 200,
            CancellationToken ct = default) where TEntity : class
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await set.FindAsync(keyValues, ct);
                }
                catch (SqliteException ex) when (IsBusyOrLocked(ex))
                {
                    await Task.Delay(delayMs, ct);
                }
                catch (SqliteException ex) when (IsCantOpenOrIo(ex))
                {
                    return default;
                }
                catch (IOException)
                {
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// Obecný „safe“ obal okolo libovolné async čtecí operace nad DbContextem.
        /// Při SQLITE_BUSY/LOCKED retry; při IO/CANTOPEN vrací default(T).
        /// </summary>
        public static async Task<T?> SafeGetAsync<T>(
            this DbContext _,
            Func<Task<T>> action,
            int maxRetries = 3,
            int delayMs = 200,
            CancellationToken ct = default)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await action();
                }
                catch (SqliteException ex) when (IsBusyOrLocked(ex))
                {
                    await Task.Delay(delayMs, ct);
                }
                catch (SqliteException ex) when (IsCantOpenOrIo(ex))
                {
                    return default;
                }
                catch (IOException)
                {
                    return default;
                }
            }

            return default;
        }

        /// <summary>
        /// Pohodlný „safe“ ToListAsync nad IQueryable (retry na busy/locked).
        /// </summary>
        public static Task<List<T>?> SafeToListAsync<T>(
            this IQueryable<T> query,
            int maxRetries = 3,
            int delayMs = 200,
            CancellationToken ct = default)
            => ExecuteWithRetry(() => EntityFrameworkQueryableExtensions.ToListAsync(query, ct),
                                maxRetries, delayMs, ct);

        /// <summary>
        /// Pohodlný „safe“ FirstOrDefaultAsync (s volitelným predikátem).
        /// </summary>
        public static Task<T?> SafeFirstOrDefaultAsync<T>(
            this IQueryable<T> query,
            Expression<Func<T, bool>>? predicate = null,
            int maxRetries = 3,
            int delayMs = 200,
            CancellationToken ct = default)
        {
            var q = predicate is null ? query : query.Where(predicate);
            return ExecuteWithRetry(() => EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(q, ct),
                                    maxRetries, delayMs, ct);
        }

        // --- PRIVATE HELPERS ---

        private static async Task<T?> ExecuteWithRetry<T>(
            Func<Task<T>> taskFactory,
            int maxRetries,
            int delayMs,
            CancellationToken ct)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    return await taskFactory();
                }
                catch (SqliteException ex) when (IsBusyOrLocked(ex))
                {
                    await Task.Delay(delayMs, ct);
                }
                catch (SqliteException ex) when (IsCantOpenOrIo(ex))
                {
                    return default;
                }
                catch (IOException)
                {
                    return default;
                }
            }

            return default;
        }

        private static bool IsBusyOrLocked(SqliteException? ex)
            => ex?.SqliteErrorCode is 5 or 6; // SQLITE_BUSY (5) / SQLITE_LOCKED (6)

        private static bool IsCantOpenOrIo(SqliteException? ex)
            => ex?.SqliteErrorCode is 10 or 14; // SQLITE_IOERR (10) / SQLITE_CANTOPEN (14)
    }
}
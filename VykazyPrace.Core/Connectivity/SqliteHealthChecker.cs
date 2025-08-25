using Microsoft.Data.Sqlite;

namespace VykazyPrace.Core.Connectivity
{
    public sealed class SqliteHealthChecker : IDatabaseHealthChecker
    {
        private readonly string _sqliteConnectionString;
        private readonly string _sqliteFilePath;
        private readonly TimeSpan _openTimeout;

        public SqliteHealthChecker(string sqliteConnectionString, string sqliteFilePath, TimeSpan? openTimeout = null)
        {
            _sqliteConnectionString = sqliteConnectionString ?? throw new ArgumentNullException(nameof(sqliteConnectionString));
            _sqliteFilePath = sqliteFilePath ?? throw new ArgumentNullException(nameof(sqliteFilePath));
            _openTimeout = openTimeout ?? TimeSpan.FromSeconds(2);
        }

        public async Task<ConnectionStatus> CheckAsync(CancellationToken ct)
        {
            try
            {
                // 1) Ověř, že soubor je dostupný na síti a dá se otevřít (rychlá IO kontrola).
                if (!File.Exists(_sqliteFilePath)) return ConnectionStatus.Unavailable;

                // Lehké otevření pro čtení – chytí i dočasnou ztrátu přístupu/práv.
                using (var fs = new FileStream(_sqliteFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, useAsync: true))
                {
                    var buffer = new byte[1];
                    await fs.ReadAsync(buffer, 0, 0, ct).ConfigureAwait(false); // no-op read to touch
                }

                // 2) Skutečné DB otevření
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(_openTimeout);

                using var conn = new SqliteConnection(_sqliteConnectionString);
                await conn.OpenAsync(cts.Token).ConfigureAwait(false);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA schema_version;";
                var _ = await cmd.ExecuteScalarAsync(cts.Token).ConfigureAwait(false);

                return ConnectionStatus.Available;
            }
            catch
            {
                return ConnectionStatus.Unavailable;
            }
        }
    }
}

using Microsoft.Data.Sqlite;

namespace VykazyPrace.Core.Connectivity
{
    public sealed class SqliteHealthChecker : IDatabaseHealthChecker
    {
        private readonly string _sqliteConnectionString;
        private readonly string _sqliteFilePath;
        private readonly TimeSpan _openTimeout;

        /// <summary>
        /// Nejjednodušší použití: stačí jen cesta k souboru.
        /// readOnly = true použij, pokud chceš jen ověřovat čtení (a nechceš, aby se soubor případně vytvářel).
        /// </summary>
        public SqliteHealthChecker(string sqliteFilePath, bool readOnly = false, TimeSpan? openTimeout = null)
            : this(BuildConnectionString(sqliteFilePath, readOnly), sqliteFilePath, openTimeout)
        {
        }

        /// <summary>
        /// Původní konstruktor — zůstává kvůli zpětné kompatibilitě.
        /// </summary>
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
                // 1) Lehká IO kontrola – soubor existuje a jde otevřít (síť/ACL).
                if (!File.Exists(_sqliteFilePath)) return ConnectionStatus.Unavailable;

                using (var fs = new FileStream(_sqliteFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 1, useAsync: true))
                {
                    var buffer = new byte[0];
                    await fs.ReadAsync(buffer, 0, 0, ct).ConfigureAwait(false); // no-op, jen "ťuk"
                }

                // 2) Otevření DB + drobný dotaz
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(_openTimeout);

                using var conn = new SqliteConnection(_sqliteConnectionString);
                await conn.OpenAsync(cts.Token).ConfigureAwait(false);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "PRAGMA schema_version;";
                _ = await cmd.ExecuteScalarAsync(cts.Token).ConfigureAwait(false);

                return ConnectionStatus.Available;
            }
            catch
            {
                return ConnectionStatus.Unavailable;
            }
        }

        private static string BuildConnectionString(string path, bool readOnly)
        {
            // Builder se postará o správné escapování a uvozovky, když je v cestě mezera/; apod.
            var builder = new SqliteConnectionStringBuilder
            {
                DataSource = path,
                // ReadOnly => rychlejší fail, když nejsou práva; ReadWrite když chceš i zapisovat.
                Mode = readOnly ? SqliteOpenMode.ReadOnly : SqliteOpenMode.ReadWrite,
                Cache = SqliteCacheMode.Shared,
                Pooling = false
            };
            return builder.ToString();
        }
    }
}
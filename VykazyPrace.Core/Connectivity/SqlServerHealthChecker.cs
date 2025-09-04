using Microsoft.Data.SqlClient;

namespace VykazyPrace.Core.Connectivity
{
    public sealed class SqlServerHealthChecker : IDatabaseHealthChecker
    {
        private readonly string _connectionString;
        private readonly TimeSpan _openTimeout;

        public SqlServerHealthChecker(string connectionString, TimeSpan? openTimeout = null)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            _openTimeout = openTimeout ?? TimeSpan.FromSeconds(3);
        }

        public async Task<ConnectionStatus> CheckAsync(CancellationToken ct)
        {
            try
            {
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
                cts.CancelAfter(_openTimeout);

                using var conn = new SqlConnection(_connectionString);
                await conn.OpenAsync(cts.Token).ConfigureAwait(false);

                using var cmd = conn.CreateCommand();
                cmd.CommandText = "SELECT 1";
                cmd.CommandTimeout = (int)_openTimeout.TotalSeconds;
                var result = await cmd.ExecuteScalarAsync(cts.Token).ConfigureAwait(false);
                return (result is int i && i == 1) ? ConnectionStatus.Available : ConnectionStatus.Unavailable;
            }
            catch
            {
                return ConnectionStatus.Unavailable;
            }
        }
    }
}

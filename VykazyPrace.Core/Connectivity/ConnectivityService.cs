namespace VykazyPrace.Core.Connectivity
{
    public sealed class ConnectivityService
    {
        private readonly IDatabaseHealthChecker _sqlServerChecker;
        private readonly IDatabaseHealthChecker _sqliteChecker;

        public event EventHandler<HealthSnapshot>? StatusChanged;

        public ConnectivityService(IDatabaseHealthChecker sqlServerChecker, IDatabaseHealthChecker sqliteChecker)
        {
            _sqlServerChecker = sqlServerChecker;
            _sqliteChecker = sqliteChecker;
        }

        public async Task<HealthSnapshot> CheckAllAsync(CancellationToken ct)
        {
            var sqlTask = _sqlServerChecker.CheckAsync(ct);
            var sqliteTask = _sqliteChecker.CheckAsync(ct);

            await Task.WhenAll(sqlTask, sqliteTask).ConfigureAwait(false);
            var snapshot = new HealthSnapshot(sqlTask.Result, sqliteTask.Result);

            StatusChanged?.Invoke(this, snapshot);
            return snapshot;
        }
    }
}

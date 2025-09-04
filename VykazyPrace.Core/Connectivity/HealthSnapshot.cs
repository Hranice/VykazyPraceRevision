namespace VykazyPrace.Core.Connectivity
{
    public sealed class HealthSnapshot
    {
        public ConnectionStatus SqlServer { get; }
        public ConnectionStatus Sqlite { get; }

        public bool IsReadOnlyMode => SqlServer != ConnectionStatus.Available || Sqlite != ConnectionStatus.Available;

        public HealthSnapshot(ConnectionStatus sqlServer, ConnectionStatus sqlite)
        {
            SqlServer = sqlServer;
            Sqlite = sqlite;
        }

        public override string ToString() => $"SQL: {SqlServer}, SQLite: {Sqlite}";
    }
}

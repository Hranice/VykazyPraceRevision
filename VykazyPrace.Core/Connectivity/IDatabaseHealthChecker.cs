
namespace VykazyPrace.Core.Connectivity
{
    public interface IDatabaseHealthChecker
    {
        Task<ConnectionStatus> CheckAsync(CancellationToken ct);
    }
}

using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public interface ITcpConnectionProvider
{
    IReadOnlyList<TcpConnectionInfo> GetConnections();
}

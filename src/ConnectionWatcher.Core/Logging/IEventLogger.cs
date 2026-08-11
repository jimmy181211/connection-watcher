using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Logging;

public interface IEventLogger
{
    Task AppendAsync(ConnectionEvent connectionEvent, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ConnectionEvent>> ReadRecentAsync(
        int maximumEntries = 2000,
        CancellationToken cancellationToken = default);
}

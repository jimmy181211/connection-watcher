using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Monitoring;

public interface IProcessContextProvider
{
    ProcessContext GetContext(
        int processId,
        string fallbackName,
        string? fallbackPath);
}

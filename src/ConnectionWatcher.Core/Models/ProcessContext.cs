namespace ConnectionWatcher.Core.Models;

public sealed record ProcessSnapshot(
    int ProcessId,
    string ProcessName,
    string? ProcessPath,
    string? ProductName,
    string? CompanyName,
    string? FileDescription);

public sealed record WindowsServiceSnapshot(
    int ProcessId,
    string ServiceName,
    string DisplayName);

public sealed record ProcessContext(
    ProcessSnapshot Owner,
    IReadOnlyList<ProcessSnapshot> ParentProcesses,
    IReadOnlyList<WindowsServiceSnapshot> RelatedServices);

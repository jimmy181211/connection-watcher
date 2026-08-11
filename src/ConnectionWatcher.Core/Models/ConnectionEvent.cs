using System.Net.NetworkInformation;

namespace ConnectionWatcher.Core.Models;

public sealed class ConnectionEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();
    public DateTimeOffset DetectedAt { get; init; }
    public DateTimeOffset LastSeenAt { get; internal set; }
    public DateTimeOffset? EndedAt { get; internal set; }
    public bool DurationKnown { get; internal set; } = true;
    public IReadOnlyList<Guid> RuleIds { get; init; } = Array.Empty<Guid>();
    public IReadOnlyList<string> RuleNames { get; init; } = Array.Empty<string>();
    public MatchAction Action { get; init; }
    public int RepeatAlertMinutes { get; init; } = 5;
    public string LocalAddress { get; init; } = string.Empty;
    public int LocalPort { get; init; }
    public string RemoteAddress { get; init; } = string.Empty;
    public int RemotePort { get; init; }
    public TcpState State { get; init; }
    public int ProcessId { get; init; }
    public string ProcessName { get; init; } = string.Empty;
    public string? ProcessPath { get; init; }

    public bool IsActive => EndedAt is null;

    public TimeSpan? GetObservedDuration(DateTimeOffset now)
    {
        if (!DurationKnown)
        {
            return null;
        }

        DateTimeOffset end = EndedAt ?? now;
        TimeSpan duration = end - DetectedAt;
        return duration < TimeSpan.Zero ? TimeSpan.Zero : duration;
    }

    public void MarkHistoricalInactive()
    {
        if (!IsActive)
        {
            return;
        }

        if (LastSeenAt == default)
        {
            LastSeenAt = DetectedAt;
        }

        EndedAt = LastSeenAt;
        DurationKnown = false;
    }

    internal void MarkSeen(DateTimeOffset seenAt)
    {
        if (IsActive && seenAt > LastSeenAt)
        {
            LastSeenAt = seenAt;
        }
    }

    internal void Complete()
    {
        if (!IsActive)
        {
            return;
        }

        if (LastSeenAt == default || LastSeenAt < DetectedAt)
        {
            LastSeenAt = DetectedAt;
        }

        EndedAt = LastSeenAt;
    }

    public static ConnectionEvent Create(
        DateTimeOffset detectedAt,
        TcpConnectionInfo connection,
        IReadOnlyList<MonitoringRule> rules)
    {
        MatchAction action = rules.Max(rule => rule.Action);
        int repeatMinutes = action == MatchAction.PopupAlert
            ? rules.Where(rule => rule.Action == MatchAction.PopupAlert)
                .Min(rule => rule.RepeatAlertMinutes)
            : 5;

        return new ConnectionEvent
        {
            DetectedAt = detectedAt,
            LastSeenAt = detectedAt,
            RuleIds = rules.Select(rule => rule.Id).ToArray(),
            RuleNames = rules.Select(rule => rule.Name).ToArray(),
            Action = action,
            RepeatAlertMinutes = repeatMinutes,
            LocalAddress = connection.LocalAddress.ToString(),
            LocalPort = connection.LocalPort,
            RemoteAddress = connection.RemoteAddress.ToString(),
            RemotePort = connection.RemotePort,
            State = connection.State,
            ProcessId = connection.ProcessId,
            ProcessName = connection.ProcessName,
            ProcessPath = connection.ProcessPath
        };
    }
}

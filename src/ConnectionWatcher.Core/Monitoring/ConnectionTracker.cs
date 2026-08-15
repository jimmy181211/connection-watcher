using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Rules;

namespace ConnectionWatcher.Core.Monitoring;

public sealed record ConnectionTrackingResult(
    IReadOnlyList<ConnectionEvent> DetectedEvents,
    IReadOnlyList<ConnectionEvent> CompletedEvents);

public sealed class ConnectionTracker
{
    public static readonly TimeSpan DisappearanceGracePeriod =
        TimeSpan.FromSeconds(2);

    private readonly Dictionary<ConnectionKey, TrackedConnection> _tracked = [];

    public ConnectionTrackingResult Process(
        IEnumerable<TcpConnectionInfo> connections,
        IReadOnlyList<MonitoringRule> rules,
        DateTimeOffset detectedAt)
    {
        HashSet<ConnectionKey> seen = [];
        List<ConnectionEvent> detectedEvents = [];
        List<ConnectionEvent> completedEvents = [];
        MonitoringRule[] enabledRules = rules.Where(rule => rule.Enabled).ToArray();

        foreach (TcpConnectionInfo connection in connections)
        {
            MonitoringRule[] matches = enabledRules
                .Where(rule => RuleMatcher.Matches(rule, connection))
                .ToArray();

            if (matches.Length == 0)
            {
                continue;
            }

            ConnectionKey key = connection.Key;
            seen.Add(key);
            if (!_tracked.TryGetValue(key, out TrackedConnection? tracked))
            {
                tracked = new TrackedConnection();
                _tracked[key] = tracked;
            }

            foreach (ConnectionEvent trackedEvent in tracked.Events)
            {
                trackedEvent.MarkSeen(detectedAt);
            }

            MonitoringRule[] newlyMatched = matches
                .Where(rule => tracked.MatchedRuleIds.Add(rule.Id))
                .ToArray();
            if (newlyMatched.Length > 0)
            {
                ConnectionEvent connectionEvent = ConnectionEvent.Create(
                    detectedAt,
                    connection,
                    newlyMatched);
                tracked.Events.Add(connectionEvent);
                detectedEvents.Add(connectionEvent);
            }
        }

        foreach ((ConnectionKey key, TrackedConnection tracked) in _tracked.ToArray())
        {
            if (seen.Contains(key))
            {
                continue;
            }

            DateTimeOffset lastSeenAt = tracked.Events.Count == 0
                ? detectedAt
                : tracked.Events.Max(connectionEvent => connectionEvent.LastSeenAt);
            if (detectedAt - lastSeenAt >= DisappearanceGracePeriod)
            {
                completedEvents.AddRange(Complete(tracked));
                _tracked.Remove(key);
            }
        }

        return new ConnectionTrackingResult(detectedEvents, completedEvents);
    }

    public IReadOnlyList<ConnectionEvent> CompleteAll()
    {
        List<ConnectionEvent> completedEvents = [];
        foreach (TrackedConnection tracked in _tracked.Values)
        {
            completedEvents.AddRange(Complete(tracked));
        }

        _tracked.Clear();
        return completedEvents;
    }

    public void Reset()
    {
        _tracked.Clear();
    }

    private static IReadOnlyList<ConnectionEvent> Complete(TrackedConnection tracked)
    {
        foreach (ConnectionEvent connectionEvent in tracked.Events)
        {
            connectionEvent.Complete();
        }

        return tracked.Events.ToArray();
    }

    private sealed class TrackedConnection
    {
        public HashSet<Guid> MatchedRuleIds { get; } = [];
        public List<ConnectionEvent> Events { get; } = [];
    }
}

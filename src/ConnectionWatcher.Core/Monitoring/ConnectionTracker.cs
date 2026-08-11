using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Rules;

namespace ConnectionWatcher.Core.Monitoring;

public sealed class ConnectionTracker
{
    private readonly Dictionary<ConnectionKey, TrackedConnection> _tracked = [];

    public IReadOnlyList<ConnectionEvent> Process(
        IEnumerable<TcpConnectionInfo> connections,
        IReadOnlyList<MonitoringRule> rules,
        DateTimeOffset detectedAt)
    {
        HashSet<ConnectionKey> seen = [];
        List<ConnectionEvent> events = [];
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

            tracked.MissedPolls = 0;
            MonitoringRule[] newlyMatched = matches
                .Where(rule => tracked.MatchedRuleIds.Add(rule.Id))
                .ToArray();
            if (newlyMatched.Length > 0)
            {
                events.Add(ConnectionEvent.Create(detectedAt, connection, newlyMatched));
            }
        }

        foreach ((ConnectionKey key, TrackedConnection tracked) in _tracked.ToArray())
        {
            if (seen.Contains(key))
            {
                continue;
            }

            tracked.MissedPolls++;
            if (tracked.MissedPolls >= 2)
            {
                _tracked.Remove(key);
            }
        }

        return events;
    }

    public void Reset()
    {
        _tracked.Clear();
    }

    private sealed class TrackedConnection
    {
        public HashSet<Guid> MatchedRuleIds { get; } = [];
        public int MissedPolls { get; set; }
    }
}

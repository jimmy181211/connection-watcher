namespace ConnectionWatcher.Core.Models;

public enum MonitoringRuleType
{
    TcpConnection,
    LocalListener
}

public enum MatchAction
{
    SilentLog,
    TrayNotice,
    PopupAlert
}

public sealed class MonitoringRule
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public MonitoringRuleType Type { get; set; } = MonitoringRuleType.TcpConnection;
    public string? RemoteIp { get; set; }
    public PortRange RemotePort { get; set; } = PortRange.Any;
    public PortRange LocalPort { get; set; } = PortRange.Any;
    public MatchAction Action { get; set; } = MatchAction.SilentLog;
    public int RepeatAlertMinutes { get; set; } = 5;
    public bool Enabled { get; set; } = true;

    public MonitoringRule Copy()
    {
        return new MonitoringRule
        {
            Id = Id,
            Name = Name,
            Type = Type,
            RemoteIp = RemoteIp,
            RemotePort = RemotePort with { },
            LocalPort = LocalPort with { },
            Action = Action,
            RepeatAlertMinutes = RepeatAlertMinutes,
            Enabled = Enabled
        };
    }
}

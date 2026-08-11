using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Configuration;

public sealed class AppSettings
{
    public string Language { get; set; } = string.Empty;
    public bool StartWithWindows { get; set; }
    public bool ResumeMonitoring { get; set; }
    public bool AlertSound { get; set; }
    public int LogLimitMb { get; set; } = 25;
    public List<MonitoringRule> Rules { get; set; } = [];
}

using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.Core.Configuration;

public sealed class AppSettings
{
    public const int DefaultLogLimitMb = 25;
    public const int MinimumLogLimitMb = 5;
    public const int MaximumLogLimitMb = 500;
    public const int DefaultAlertVolumePercent = 40;
    public const int MinimumAlertVolumePercent = 10;
    public const int MaximumAlertVolumePercent = 100;

    public string Language { get; set; } = string.Empty;
    public bool StartWithWindows { get; set; }
    public bool ResumeMonitoring { get; set; }
    public bool AlertSound { get; set; }
    public int AlertVolumePercent { get; set; } = DefaultAlertVolumePercent;
    public int LogLimitMb { get; set; } = DefaultLogLimitMb;
    public List<MonitoringRule> Rules { get; set; } = [];
}

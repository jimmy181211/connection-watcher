using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.App.Localization;

public static class UiText
{
    private static readonly IReadOnlyDictionary<string, string> Chinese =
        new Dictionary<string, string>
        {
            ["AppTitle"] = "TCP连接监视器",
            ["Home"] = "首页",
            ["Rules"] = "监视规则",
            ["Events"] = "事件记录",
            ["Settings"] = "设置",
            ["MonitoringStopped"] = "监控已停止",
            ["MonitoringRunning"] = "正在监控",
            ["MonitoringInterrupted"] = "监控出现错误",
            ["MonitoringStatus"] = "监控状态",
            ["StatusHint"] = "只有点击“开始监控”后才会检查连接。",
            ["StartMonitoring"] = "开始监控",
            ["StopMonitoring"] = "停止监控",
            ["EnabledRules"] = "启用规则",
            ["CheckInterval"] = "检查间隔",
            ["OneSecond"] = "1秒",
            ["ShortConnectionNote"] = "每1秒读取一次TCP连接表；极短连接可能无法记录。",
            ["RulesDescription"] = "规则决定监视什么；“匹配后操作”决定发现后怎样处理。",
            ["NewRule"] = "新建规则",
            ["Edit"] = "编辑",
            ["Delete"] = "删除",
            ["Name"] = "规则名称",
            ["Condition"] = "监视条件",
            ["MatchAction"] = "匹配后操作",
            ["Enabled"] = "启用",
            ["NoRules"] = "还没有规则。请先新建并启用一条规则。",
            ["DeleteRuleTitle"] = "删除规则",
            ["DeleteRuleQuestion"] = "确定删除规则“{0}”吗？过去的事件记录不会被删除。",
            ["SilentLog"] = "静默记录",
            ["TrayNotice"] = "托盘提示并记录",
            ["PopupAlert"] = "弹窗警报并记录",
            ["TcpConnection"] = "TCP连接",
            ["LocalListener"] = "本地监听端口",
            ["AnyRemoteIp"] = "任意远程IP",
            ["RemoteIp"] = "远程IP",
            ["RemotePort"] = "远程端口",
            ["LocalPort"] = "本地端口",
            ["AnyPort"] = "任意端口",
            ["ListenerOn"] = "监听本地端口 {0}",
            ["ConnectionCondition"] = "远程IP：{0} · 远程端口：{1} · 本地端口：{2}",
            ["Any"] = "任意",
            ["SearchEvents"] = "搜索规则、IP、端口或程序",
            ["EventsDescription"] = "每个新匹配连接记录一次；持续连接不会每秒重复写入。",
            ["ExportCsv"] = "导出CSV",
            ["OpenLogFolder"] = "打开日志文件夹",
            ["Time"] = "时间",
            ["MatchedRules"] = "匹配规则",
            ["RemoteEndpoint"] = "远程目标",
            ["LocalEndpoint"] = "本地端点",
            ["TcpState"] = "TCP状态",
            ["Program"] = "程序",
            ["NoEvents"] = "还没有符合规则的连接记录。",
            ["ExportComplete"] = "事件记录已导出。",
            ["Language"] = "界面语言",
            ["Chinese"] = "中文",
            ["English"] = "English",
            ["StartWithWindows"] = "登录Windows后启动程序",
            ["StartWithWindowsHint"] = "只启动程序；是否自动监控由下面的选项决定。",
            ["ResumeMonitoring"] = "启动程序后恢复监控",
            ["ResumeMonitoringHint"] = "默认关闭；只使用已启用的规则。",
            ["AlertSound"] = "紧急提醒声音",
            ["AlertSoundHint"] = "默认关闭，避免打扰工作。",
            ["LogLimit"] = "事件日志上限",
            ["LogLimitValue"] = "25MB（5个文件，每个最多5MB）",
            ["Privacy"] = "本工具不联网、不上传日志、不读取数据包内容，也不会自动封锁连接。",
            ["CreateRule"] = "新建监视规则",
            ["EditRule"] = "编辑监视规则",
            ["RuleType"] = "规则类型",
            ["AnyRemoteIpCheck"] = "任意远程IP",
            ["AnyRemotePortCheck"] = "任意远程端口",
            ["AnyLocalPortCheck"] = "任意本地端口",
            ["PortFormat"] = "可填写单个端口（1433）或范围（1400-1500）",
            ["RepeatInterval"] = "关闭弹窗后的重复提醒间隔",
            ["EveryTime"] = "每次",
            ["OneMinute"] = "1分钟",
            ["FiveMinutes"] = "5分钟",
            ["FifteenMinutes"] = "15分钟",
            ["EnableAfterSave"] = "保存后启用这条规则",
            ["RulePreview"] = "规则预览",
            ["Save"] = "保存",
            ["Cancel"] = "取消",
            ["PreviewConnection"] = "当程序的TCP连接符合以下条件：{0}，则{1}。",
            ["PreviewListener"] = "当程序监听本地端口 {0} 时，{1}。",
            ["NameRequired"] = "请填写规则名称。",
            ["InvalidRemoteIp"] = "远程IP格式不正确。",
            ["InvalidRemotePort"] = "远程端口必须是1–65535之间的单个端口或范围。",
            ["InvalidLocalPort"] = "本地端口必须是1–65535之间的单个端口或范围。",
            ["ConditionRequired"] = "请至少限制一个IP或端口。",
            ["ListenerPortRequired"] = "监听规则必须指定本地端口。",
            ["InvalidRepeatInterval"] = "重复提醒间隔无效。",
            ["NeedEnabledRule"] = "开始监控前，请先创建并启用至少一条规则。",
            ["UrgentTitle"] = "监视规则已匹配",
            ["FirstSeen"] = "首次发生",
            ["LatestSeen"] = "最近发生",
            ["Occurrences"] = "本次提醒期间",
            ["Times"] = "次",
            ["NotMalwareVerdict"] = "此连接匹配了您设置的规则，但这本身不代表电脑已经感染病毒。",
            ["ViewDetails"] = "查看详情",
            ["Close"] = "关闭",
            ["Open"] = "打开",
            ["Exit"] = "退出程序",
            ["CloseWhileRunning"] = "监控正在运行。选择“是”将最小化到托盘；选择“否”将停止监控并退出。",
            ["CloseWhileRunningTitle"] = "监控仍在运行",
            ["TrayNormal"] = "TCP连接监视器：正在监控",
            ["TrayNotices"] = "TCP连接监视器：{0}条普通提醒",
            ["Error"] = "错误",
            ["UnexpectedError"] = "意外错误",
            ["StartupSettingError"] = "无法修改Windows启动设置：{0}",
            ["ProcessPathUnavailable"] = "无法读取（权限限制）"
        };

    private static readonly IReadOnlyDictionary<string, string> English =
        new Dictionary<string, string>
        {
            ["AppTitle"] = "TCP Connection Watcher",
            ["Home"] = "Home",
            ["Rules"] = "Monitoring rules",
            ["Events"] = "Event log",
            ["Settings"] = "Settings",
            ["MonitoringStopped"] = "Monitoring stopped",
            ["MonitoringRunning"] = "Monitoring",
            ["MonitoringInterrupted"] = "Monitoring error",
            ["MonitoringStatus"] = "Monitoring status",
            ["StatusHint"] = "Connections are checked only after you select Start monitoring.",
            ["StartMonitoring"] = "Start monitoring",
            ["StopMonitoring"] = "Stop monitoring",
            ["EnabledRules"] = "Enabled rules",
            ["CheckInterval"] = "Check interval",
            ["OneSecond"] = "1 second",
            ["ShortConnectionNote"] = "The TCP connection table is read once per second; very short connections may be missed.",
            ["RulesDescription"] = "Rules define what to monitor; “Action on match” defines what happens next.",
            ["NewRule"] = "New rule",
            ["Edit"] = "Edit",
            ["Delete"] = "Delete",
            ["Name"] = "Rule name",
            ["Condition"] = "Monitoring condition",
            ["MatchAction"] = "Action on match",
            ["Enabled"] = "Enabled",
            ["NoRules"] = "No rules yet. Create and enable a rule first.",
            ["DeleteRuleTitle"] = "Delete rule",
            ["DeleteRuleQuestion"] = "Delete rule “{0}”? Existing event records will not be deleted.",
            ["SilentLog"] = "Log silently",
            ["TrayNotice"] = "Tray notice and log",
            ["PopupAlert"] = "Pop up alert and log",
            ["TcpConnection"] = "TCP connection",
            ["LocalListener"] = "Local listening port",
            ["AnyRemoteIp"] = "Any remote IP",
            ["RemoteIp"] = "Remote IP",
            ["RemotePort"] = "Remote port",
            ["LocalPort"] = "Local port",
            ["AnyPort"] = "Any port",
            ["ListenerOn"] = "Listen on local port {0}",
            ["ConnectionCondition"] = "Remote IP: {0} · remote port: {1} · local port: {2}",
            ["Any"] = "Any",
            ["SearchEvents"] = "Search rule, IP, port, or program",
            ["EventsDescription"] = "Each newly matched connection is logged once; an ongoing connection is not written every second.",
            ["ExportCsv"] = "Export CSV",
            ["OpenLogFolder"] = "Open log folder",
            ["Time"] = "Time",
            ["MatchedRules"] = "Matched rules",
            ["RemoteEndpoint"] = "Remote endpoint",
            ["LocalEndpoint"] = "Local endpoint",
            ["TcpState"] = "TCP state",
            ["Program"] = "Program",
            ["NoEvents"] = "No connections have matched a rule yet.",
            ["ExportComplete"] = "Event log exported.",
            ["Language"] = "Interface language",
            ["Chinese"] = "中文",
            ["English"] = "English",
            ["StartWithWindows"] = "Start after Windows sign-in",
            ["StartWithWindowsHint"] = "Starts the app only; the option below controls automatic monitoring.",
            ["ResumeMonitoring"] = "Resume monitoring after launch",
            ["ResumeMonitoringHint"] = "Off by default; only enabled rules are used.",
            ["AlertSound"] = "Urgent alert sound",
            ["AlertSoundHint"] = "Off by default to avoid interrupting work.",
            ["LogLimit"] = "Event log limit",
            ["LogLimitValue"] = "25 MB (5 files, up to 5 MB each)",
            ["Privacy"] = "This tool does not connect to the internet, upload logs, read packet contents, or block connections automatically.",
            ["CreateRule"] = "Create monitoring rule",
            ["EditRule"] = "Edit monitoring rule",
            ["RuleType"] = "Rule type",
            ["AnyRemoteIpCheck"] = "Any remote IP",
            ["AnyRemotePortCheck"] = "Any remote port",
            ["AnyLocalPortCheck"] = "Any local port",
            ["PortFormat"] = "Enter one port (1433) or a range (1400-1500)",
            ["RepeatInterval"] = "Pop-up repeat interval after closing",
            ["EveryTime"] = "Every time",
            ["OneMinute"] = "1 minute",
            ["FiveMinutes"] = "5 minutes",
            ["FifteenMinutes"] = "15 minutes",
            ["EnableAfterSave"] = "Enable this rule after saving",
            ["RulePreview"] = "Rule preview",
            ["Save"] = "Save",
            ["Cancel"] = "Cancel",
            ["PreviewConnection"] = "When a program’s TCP connection matches: {0}, {1}.",
            ["PreviewListener"] = "When a program listens on local port {0}, {1}.",
            ["NameRequired"] = "Enter a rule name.",
            ["InvalidRemoteIp"] = "The remote IP address is invalid.",
            ["InvalidRemotePort"] = "Remote port must be one port or a range from 1 to 65535.",
            ["InvalidLocalPort"] = "Local port must be one port or a range from 1 to 65535.",
            ["ConditionRequired"] = "Restrict at least one IP address or port.",
            ["ListenerPortRequired"] = "A listener rule requires a local port.",
            ["InvalidRepeatInterval"] = "The repeat interval is invalid.",
            ["NeedEnabledRule"] = "Create and enable at least one rule before starting monitoring.",
            ["UrgentTitle"] = "Monitoring rule matched",
            ["FirstSeen"] = "First seen",
            ["LatestSeen"] = "Latest seen",
            ["Occurrences"] = "During this alert",
            ["Times"] = "times",
            ["NotMalwareVerdict"] = "This connection matched a rule you created, but that alone does not mean the computer is infected.",
            ["ViewDetails"] = "View details",
            ["Close"] = "Close",
            ["Open"] = "Open",
            ["Exit"] = "Exit",
            ["CloseWhileRunning"] = "Monitoring is running. Choose Yes to minimize to the tray, or No to stop monitoring and exit.",
            ["CloseWhileRunningTitle"] = "Monitoring is still running",
            ["TrayNormal"] = "TCP Connection Watcher: monitoring",
            ["TrayNotices"] = "TCP Connection Watcher: {0} tray notices",
            ["Error"] = "Error",
            ["UnexpectedError"] = "Unexpected error",
            ["StartupSettingError"] = "Windows startup setting could not be changed: {0}",
            ["ProcessPathUnavailable"] = "Unavailable (permission restricted)"
        };

    public static string Language { get; private set; } = "en";

    public static bool IsChinese => Language.StartsWith("zh", StringComparison.OrdinalIgnoreCase);

    public static void SetLanguage(string language)
    {
        Language = language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? "zh-CN"
            : "en";
    }

    public static string Get(string key)
    {
        IReadOnlyDictionary<string, string> source = IsChinese ? Chinese : English;
        return source.TryGetValue(key, out string? value) ? value : key;
    }

    public static string Action(MatchAction action)
    {
        return Get(action switch
        {
            MatchAction.SilentLog => "SilentLog",
            MatchAction.TrayNotice => "TrayNotice",
            _ => "PopupAlert"
        });
    }

    public static string RuleType(MonitoringRuleType type)
    {
        return Get(type == MonitoringRuleType.LocalListener
            ? "LocalListener"
            : "TcpConnection");
    }

    public static string FormatRuleCondition(MonitoringRule rule)
    {
        if (rule.Type == MonitoringRuleType.LocalListener)
        {
            return string.Format(Get("ListenerOn"), FormatPort(rule.LocalPort));
        }

        return string.Format(
            Get("ConnectionCondition"),
            string.IsNullOrWhiteSpace(rule.RemoteIp) ? Get("Any") : rule.RemoteIp,
            FormatPort(rule.RemotePort),
            FormatPort(rule.LocalPort));
    }

    public static string FormatPort(PortRange range)
    {
        return range.IsAny ? Get("AnyPort") : range.ToString();
    }
}

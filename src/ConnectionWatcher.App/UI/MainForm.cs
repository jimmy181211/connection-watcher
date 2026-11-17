using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ConnectionWatcher.App.Localization;
using ConnectionWatcher.App.Services;
using ConnectionWatcher.Core.Configuration;
using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Monitoring;

namespace ConnectionWatcher.App.UI;

/// <summary>
/// Own shared window state and initialize controls in their original order.
/// </summary>
public sealed partial class MainForm : Form
{
    private readonly AppSettings _settings;
    private readonly SettingsStore _settingsStore;
    private readonly CsvEventLogger _logger;
    private readonly MonitoringEngine _engine;
    private readonly UpdateCheckService _updateCheckService = new();
    private readonly object _settingsGate = new();
    private readonly List<ConnectionEvent> _events = [];
    private readonly Dictionary<string, Panel> _pages = [];
    private readonly Dictionary<string, Button> _navButtons = [];
    private readonly Dictionary<Guid, DateTimeOffset> _lastPopupClosed = [];
    private readonly NotifyIcon _notifyIcon;
    private readonly ContextMenuStrip _trayMenu = new();
    private readonly ToolStripMenuItem _trayOpen = new();
    private readonly ToolStripMenuItem _trayToggle = new();
    private readonly ToolStripMenuItem _trayExit = new();
    private readonly Icon _applicationIcon = AppIconProvider.Load();

    private readonly Label _sidebarStatus = new();
    private readonly Label _sidebarHint = new();
    private readonly Label _homeTitle = TitleLabel();
    private readonly Label _homeSubtitle = SubtitleLabel();
    private readonly Label _homeStatusCaption = new();
    private readonly Label _homeStatus = ValueLabel();
    private readonly Label _homeRulesCaption = new();
    private readonly Label _homeRules = ValueLabel();
    private readonly Label _homeIntervalCaption = new();
    private readonly NumericUpDown _checkInterval = new()
    {
        Name = "CheckIntervalInput",
        Minimum = AppSettings.MinimumCheckIntervalSeconds,
        Maximum = AppSettings.MaximumCheckIntervalSeconds,
        Increment = AppSettings.CheckIntervalStepSeconds,
        DecimalPlaces = 1,
        Width = 86,
        TextAlign = HorizontalAlignment.Right,
    };
    private readonly Label _checkIntervalUnit = new() { AutoSize = true };
    private readonly Label _actionLegendTitle = new();
    private readonly Label _silentActionLegend = new()
    {
        Name = "SilentActionLegend",
    };
    private readonly Label _trayActionLegend = new()
    {
        Name = "TrayActionLegend",
    };
    private readonly Label _popupActionLegend = new()
    {
        Name = "PopupActionLegend",
    };
    private readonly Button _monitorButton = new();

    private readonly Label _rulesTitle = TitleLabel();
    private readonly Label _rulesSubtitle = SubtitleLabel();
    private readonly Button _newRuleButton = new();
    private readonly Button _editRuleButton = new();
    private readonly Button _deleteRuleButton = new();
    private readonly DataGridView _rulesGrid = NewGrid();

    private readonly Label _eventsTitle = TitleLabel();
    private readonly Label _eventsSubtitle = SubtitleLabel();
    private readonly TextBox _eventSearch = new();
    private readonly Button _exportButton = new();
    private readonly Button _openLogsButton = new();
    private readonly Button _clearEventsButton = new()
    {
        Name = "ClearEventsButton",
        AutoSize = true,
        MinimumSize = new Size(118, 34),
    };
    private readonly DataGridView _eventsGrid = NewGrid();
    private readonly Label _eventDetailsHint = SubtitleLabel();
    private Font? _eventStatusFont;
    private readonly System.Windows.Forms.Timer _eventDurationTimer = new()
    {
        Interval = 1000,
    };

    private readonly Label _settingsTitle = TitleLabel();
    private readonly Label _settingsSubtitle = SubtitleLabel();
    private readonly ComboBox _language = new()
    {
        Name = "LanguageComboBox",
        DropDownStyle = ComboBoxStyle.DropDownList,
        DrawMode = DrawMode.OwnerDrawFixed,
        Width = 165,
    };
    private readonly Label _startWithWindowsCaption = new();
    private readonly CheckBox _startWithWindows = new()
    {
        Name = "StartWithWindowsCheckBox",
    };
    private readonly Label _startWithWindowsHint = SubtitleLabel();
    private readonly Label _resumeMonitoringCaption = new();
    private readonly CheckBox _resumeMonitoring = new()
    {
        Name = "ResumeMonitoringCheckBox",
    };
    private readonly Label _resumeMonitoringHint = SubtitleLabel();
    private readonly Label _alertSoundCaption = new();
    private readonly CheckBox _alertSound = new()
    {
        Name = "AlertSoundCheckBox",
    };
    private readonly Label _alertSoundHint = SubtitleLabel();
    private readonly Button _testAlertSoundButton = new()
    {
        Name = "TestAlertSoundButton",
        AutoSize = true,
        MinimumSize = new Size(105, 34),
    };
    private readonly FlowLayoutPanel _alertVolumeControls = new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false,
    };
    private readonly Label _alertVolumeCaption = new();
    private readonly Label _alertVolumeHint = SubtitleLabel();
    private readonly NumericUpDown _alertVolume = new()
    {
        Name = "AlertVolumeInput",
        Minimum = AppSettings.MinimumAlertVolumePercent,
        Maximum = AppSettings.MaximumAlertVolumePercent,
        Increment = 5,
        TextAlign = HorizontalAlignment.Right,
        Width = 110,
    };
    private readonly Label _logLimitCaption = new();
    private readonly Label _logLimitHint = SubtitleLabel();
    private readonly NumericUpDown _logLimit = new()
    {
        Minimum = AppSettings.MinimumLogLimitMb,
        Maximum = AppSettings.MaximumLogLimitMb,
        Increment = 5,
        ThousandsSeparator = true,
        TextAlign = HorizontalAlignment.Right,
        Width = 110,
    };
    private readonly Label _helpCenterCaption = new();
    private readonly Label _helpCenterHint = SubtitleLabel();
    private readonly Button _helpCenterButton = new()
    {
        Name = "HelpCenterButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36),
    };
    private readonly Label _feedbackCaption = new();
    private readonly Label _feedbackHint = SubtitleLabel();
    private readonly Button _feedbackButton = new()
    {
        Name = "FeedbackButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36),
    };
    private readonly Label _updateCaption = new()
    {
        Name = "SoftwareUpdatesCaption",
    };
    private readonly Label _updateHint = new()
    {
        Name = "SoftwareUpdatesHint",
        AutoSize = true,
        ForeColor = SystemColors.GrayText,
        MaximumSize = new Size(780, 0),
    };
    private readonly Button _checkUpdatesButton = new()
    {
        Name = "CheckUpdatesButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36),
    };

    private bool _refreshingRules;
    private bool _refreshingSettings;
    private bool _exitRequested;
    private bool _monitoringError;
    private int _trayNoticeCount;
    private UrgentAlertForm? _urgentAlert;

    public MainForm(
        AppSettings settings,
        SettingsStore settingsStore,
        CsvEventLogger logger
    )
    {
        _settings = settings;
        _settingsStore = settingsStore;
        _logger = logger;
        _engine = new MonitoringEngine(
            new WindowsTcpConnectionProvider(),
            logger,
            GetRulesSnapshot,
            new WindowsProcessContextProvider(),
            TimeSpan.FromSeconds((double)_settings.CheckIntervalSeconds)
        );
        _engine.EventDetected += EngineEventDetected;
        _engine.EventCompleted += EngineEventCompleted;
        _engine.MonitoringError += EngineMonitoringError;
        _engine.MonitoringRecovered += EngineMonitoringRecovered;

        _notifyIcon = new NotifyIcon
        {
            Icon = _applicationIcon,
            Visible = true,
            ContextMenuStrip = _trayMenu,
        };
        _notifyIcon.DoubleClick += (_, _) => ShowMainWindow();

        BuildInterface();
        BuildTrayMenu();
        ApplyLanguage();
        RefreshRules();
        RefreshSettings();
        UpdateMonitoringStatus();

        Shown += MainFormShown;
        FormClosing += MainFormClosing;
        _eventDurationTimer.Tick += (_, _) => UpdateVisibleEventDurations();
        _eventDurationTimer.Start();
        Resize += (_, _) =>
        {
            if (WindowState == FormWindowState.Minimized)
            {
                Hide();
            }
        };
    }

    private MonitoringRule? SelectedRule()
    {
        if (
            _rulesGrid.SelectedRows.Count == 0
            || _rulesGrid.SelectedRows[0].Tag is not Guid id
        )
        {
            return null;
        }

        return _settings.Rules.FirstOrDefault(rule => rule.Id == id);
    }

    private IEnumerable<ConnectionEvent> FilteredEvents()
    {
        IEnumerable<ConnectionEvent> visible = _events.Where(
            IsVisibleAfterDisplayCutoff
        );
        string query = _eventSearch.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return visible;
        }

        return visible.Where(entry =>
            string.Join(" ", entry.RuleNames)
                .Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.RemoteAddress.Contains(
                query,
                StringComparison.OrdinalIgnoreCase
            )
            || entry
                .RemotePort.ToString(CultureInfo.InvariantCulture)
                .Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.LocalAddress.Contains(
                query,
                StringComparison.OrdinalIgnoreCase
            )
            || entry
                .LocalPort.ToString(CultureInfo.InvariantCulture)
                .Contains(query, StringComparison.OrdinalIgnoreCase)
            || entry.ProcessName.Contains(
                query,
                StringComparison.OrdinalIgnoreCase
            )
            || (
                entry.ProcessProductName?.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                ) ?? false
            )
            || (
                entry.ProcessCompanyName?.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                ) ?? false
            )
            || entry.ParentProcesses.Any(parent =>
                parent.ProcessName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                )
                || (
                    parent.ProductName?.Contains(
                        query,
                        StringComparison.OrdinalIgnoreCase
                    ) ?? false
                )
            )
            || entry.RelatedServices.Any(service =>
                service.ServiceName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                )
                || service.DisplayName.Contains(
                    query,
                    StringComparison.OrdinalIgnoreCase
                )
            )
        );
    }

    private static Version CurrentVersion() =>
        typeof(MainForm).Assembly.GetName().Version ?? new Version(1, 0, 0);

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _eventDurationTimer.Stop();
            _eventDurationTimer.Dispose();
            _eventStatusFont?.Dispose();
            _notifyIcon.Dispose();
            _applicationIcon.Dispose();
            _trayMenu.Dispose();
            _engine.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }

        base.Dispose(disposing);
    }

    private sealed record LanguageOption(string Code, string Label)
    {
        public override string ToString() => Label;
    }
}

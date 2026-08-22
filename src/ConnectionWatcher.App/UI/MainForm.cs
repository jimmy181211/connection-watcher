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

public sealed class MainForm : Form
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
        TextAlign = HorizontalAlignment.Right
    };
    private readonly Label _checkIntervalUnit = new() { AutoSize = true };
    private readonly Label _actionLegendTitle = new();
    private readonly Label _silentActionLegend = new() { Name = "SilentActionLegend" };
    private readonly Label _trayActionLegend = new() { Name = "TrayActionLegend" };
    private readonly Label _popupActionLegend = new() { Name = "PopupActionLegend" };
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
        MinimumSize = new Size(118, 34)
    };
    private readonly DataGridView _eventsGrid = NewGrid();
    private readonly Label _eventDetailsHint = SubtitleLabel();
    private Font? _eventStatusFont;
    private readonly System.Windows.Forms.Timer _eventDurationTimer = new()
    {
        Interval = 1000
    };

    private readonly Label _settingsTitle = TitleLabel();
    private readonly Label _settingsSubtitle = SubtitleLabel();
    private readonly ComboBox _language = new()
    {
        Name = "LanguageComboBox",
        DropDownStyle = ComboBoxStyle.DropDownList,
        DrawMode = DrawMode.OwnerDrawFixed,
        Width = 165
    };
    private readonly Label _startWithWindowsCaption = new();
    private readonly CheckBox _startWithWindows = new() { Name = "StartWithWindowsCheckBox" };
    private readonly Label _startWithWindowsHint = SubtitleLabel();
    private readonly Label _resumeMonitoringCaption = new();
    private readonly CheckBox _resumeMonitoring = new() { Name = "ResumeMonitoringCheckBox" };
    private readonly Label _resumeMonitoringHint = SubtitleLabel();
    private readonly Label _alertSoundCaption = new();
    private readonly CheckBox _alertSound = new() { Name = "AlertSoundCheckBox" };
    private readonly Label _alertSoundHint = SubtitleLabel();
    private readonly Button _testAlertSoundButton = new()
    {
        Name = "TestAlertSoundButton",
        AutoSize = true,
        MinimumSize = new Size(105, 34)
    };
    private readonly FlowLayoutPanel _alertVolumeControls = new()
    {
        AutoSize = true,
        AutoSizeMode = AutoSizeMode.GrowAndShrink,
        FlowDirection = FlowDirection.LeftToRight,
        WrapContents = false
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
        Width = 110
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
        Width = 110
    };
    private readonly Label _helpCenterCaption = new();
    private readonly Label _helpCenterHint = SubtitleLabel();
    private readonly Button _helpCenterButton = new()
    {
        Name = "HelpCenterButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36)
    };
    private readonly Label _feedbackCaption = new();
    private readonly Label _feedbackHint = SubtitleLabel();
    private readonly Button _feedbackButton = new()
    {
        Name = "FeedbackButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36)
    };
    private readonly Label _updateCaption = new()
    {
        Name = "SoftwareUpdatesCaption"
    };
    private readonly Label _updateHint = new()
    {
        Name = "SoftwareUpdatesHint",
        AutoSize = true,
        ForeColor = SystemColors.GrayText,
        MaximumSize = new Size(780, 0)
    };
    private readonly Button _checkUpdatesButton = new()
    {
        Name = "CheckUpdatesButton",
        AutoSize = true,
        MinimumSize = new Size(120, 36)
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
        CsvEventLogger logger)
    {
        _settings = settings;
        _settingsStore = settingsStore;
        _logger = logger;
        _engine = new MonitoringEngine(
            new WindowsTcpConnectionProvider(),
            logger,
            GetRulesSnapshot,
            new WindowsProcessContextProvider(),
            TimeSpan.FromSeconds((double)_settings.CheckIntervalSeconds));
        _engine.EventDetected += EngineEventDetected;
        _engine.EventCompleted += EngineEventCompleted;
        _engine.MonitoringError += EngineMonitoringError;
        _engine.MonitoringRecovered += EngineMonitoringRecovered;

        _notifyIcon = new NotifyIcon
        {
            Icon = _applicationIcon,
            Visible = true,
            ContextMenuStrip = _trayMenu
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

    private static Label TitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 4)
        };
    }

    private static Label SubtitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            MaximumSize = new Size(780, 0)
        };
    }

    private static Label ValueLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold)
        };
    }

    private static DataGridView NewGrid()
    {
        return new DataGridView
        {
            Dock = DockStyle.Fill,
            AllowUserToAddRows = false,
            AllowUserToDeleteRows = false,
            AllowUserToResizeRows = false,
            AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
            BackgroundColor = SystemColors.Window,
            BorderStyle = BorderStyle.Fixed3D,
            MultiSelect = false,
            ReadOnly = true,
            RowHeadersVisible = false,
            SelectionMode = DataGridViewSelectionMode.FullRowSelect
        };
    }

    private void BuildInterface()
    {
        Text = UiText.Get("AppTitle");
        Icon = (Icon)_applicationIcon.Clone();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 620);
        ClientSize = new Size(1100, 720);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Panel sidebar = BuildSidebar();
        Panel content = new() { Dock = DockStyle.Fill, Padding = new Padding(24) };
        _pages["Home"] = BuildHomePage();
        _pages["Rules"] = BuildRulesPage();
        _pages["Events"] = BuildEventsPage();
        _pages["Settings"] = BuildSettingsPage();
        foreach (Panel page in _pages.Values)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            content.Controls.Add(page);
        }

        root.Controls.Add(sidebar, 0, 0);
        root.Controls.Add(content, 1, 0);
        Controls.Add(root);
        ShowPage("Rules");
    }

    private Panel BuildSidebar()
    {
        Panel sidebar = new()
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.ControlLight,
            Padding = new Padding(10)
        };
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 6,
            ColumnCount = 1
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        int row = 0;
        foreach (string key in new[] { "Home", "Rules", "Events", "Settings" })
        {
            Button button = new()
            {
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                Height = 44,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 5)
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => ShowPage(key);
            _navButtons[key] = button;
            layout.Controls.Add(button, 0, row++);
        }

        FlowLayoutPanel status = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Bottom,
            Padding = new Padding(8, 12, 8, 4)
        };
        _sidebarStatus.AutoSize = true;
        _sidebarStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _sidebarHint.AutoSize = true;
        _sidebarHint.MaximumSize = new Size(170, 0);
        _sidebarHint.ForeColor = SystemColors.GrayText;
        status.Controls.Add(_sidebarStatus);
        status.Controls.Add(_sidebarHint);
        layout.Controls.Add(status, 0, 5);
        sidebar.Controls.Add(layout);
        return sidebar;
    }

    private Panel BuildHomePage()
    {
        Panel page = new();
        _homeTitle.Name = "HomeTitle";
        _homeSubtitle.Name = "HomeSubtitle";
        _monitorButton.Name = "MonitorButton";
        _monitorButton.AutoSize = true;
        _monitorButton.MinimumSize = new Size(180, 38);
        _monitorButton.Padding = new Padding(12, 2, 12, 2);
        _monitorButton.Click += async (_, _) => await ToggleMonitoringAsync();
        TableLayoutPanel header = Header(
            _homeTitle,
            _homeSubtitle,
            _monitorButton);

        TableLayoutPanel summary = new()
        {
            Name = "HomeSummary",
            AutoSize = false,
            ColumnCount = 3,
            RowCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 24, 0, 0)
        };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.Controls.Add(SummaryBox(_homeStatusCaption, _homeStatus), 0, 0);
        summary.Controls.Add(SummaryBox(_homeRulesCaption, _homeRules), 1, 0);
        FlowLayoutPanel intervalValue = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        _checkInterval.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _checkIntervalUnit.Font = new Font("Segoe UI", 10F);
        _checkIntervalUnit.Margin = new Padding(4, 7, 0, 0);
        intervalValue.Controls.AddRange([_checkInterval, _checkIntervalUnit]);
        summary.Controls.Add(
            SummaryBox(
                _homeIntervalCaption,
                intervalValue),
            2,
            0);
        _checkInterval.ValueChanged += (_, _) => CheckIntervalChanged();

        TableLayoutPanel legend = BuildActionLegend();

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 142));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        header.Dock = DockStyle.Fill;
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(summary, 0, 1);
        layout.Controls.Add(legend, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private TableLayoutPanel BuildActionLegend()
    {
        TableLayoutPanel legend = new()
        {
            Name = "ActionLegend",
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 12, 12, 0)
        };
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        legend.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        legend.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _actionLegendTitle.AutoSize = true;
        _actionLegendTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _actionLegendTitle.Margin = new Padding(0, 0, 0, 5);
        legend.Controls.Add(_actionLegendTitle, 0, 0);
        legend.SetColumnSpan(_actionLegendTitle, 3);

        ConfigureLegendItem(_silentActionLegend, MatchAction.SilentLog);
        ConfigureLegendItem(_trayActionLegend, MatchAction.TrayNotice);
        ConfigureLegendItem(_popupActionLegend, MatchAction.PopupAlert);
        legend.Controls.Add(_silentActionLegend, 0, 1);
        legend.Controls.Add(_trayActionLegend, 1, 1);
        legend.Controls.Add(_popupActionLegend, 2, 1);
        return legend;
    }

    private static void ConfigureLegendItem(Label label, MatchAction action)
    {
        label.Dock = DockStyle.Fill;
        label.AutoEllipsis = true;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.ForeColor = ActionColor(action);
        label.Margin = Padding.Empty;
    }

    private static Panel SummaryBox(
        Label caption,
        Control value,
        Label? hint = null)
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 12, 0),
            Height = 105
        };
        caption.AutoSize = true;
        caption.ForeColor = SystemColors.GrayText;
        caption.Location = new Point(16, 16);
        value.Location = new Point(16, 46);
        panel.Controls.Add(caption);
        panel.Controls.Add(value);
        if (hint is not null)
        {
            hint.AutoSize = false;
            hint.Location = new Point(16, 84);
            hint.Size = new Size(225, 34);
            hint.AutoEllipsis = true;
            panel.Controls.Add(hint);
        }
        return panel;
    }

    private Panel BuildRulesPage()
    {
        Panel page = new();
        _rulesTitle.Name = "RulesTitle";
        _newRuleButton.AutoSize = true;
        _newRuleButton.MinimumSize = new Size(110, 36);
        _newRuleButton.Click += (_, _) => AddRule();
        TableLayoutPanel header = Header(
            _rulesTitle,
            _rulesSubtitle,
            _newRuleButton);

        _rulesGrid.Name = "RulesGrid";
        _rulesGrid.ColumnHeadersDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _rulesGrid.RowsDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 20,
            MinimumWidth = 120
        });
        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 42,
            MinimumWidth = 235
        });
        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 24,
            MinimumWidth = 160
        });
        _rulesGrid.Columns.Add(new DataGridViewCheckBoxColumn
        {
            FillWeight = 14,
            MinimumWidth = 100
        });
        _rulesGrid.ReadOnly = false;
        foreach (DataGridViewColumn column in _rulesGrid.Columns)
        {
            column.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }
        foreach (DataGridViewColumn column in _rulesGrid.Columns.Cast<DataGridViewColumn>().Take(3))
        {
            column.ReadOnly = true;
        }
        _rulesGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_rulesGrid.IsCurrentCellDirty)
            {
                _rulesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        _rulesGrid.CellValueChanged += RuleEnabledChanged;
        _rulesGrid.SelectionChanged += (_, _) => UpdateRuleButtons();
        _rulesGrid.CellDoubleClick += (_, args) =>
        {
            if (args.RowIndex >= 0)
            {
                EditSelectedRule();
            }
        };

        FlowLayoutPanel actions = new()
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0)
        };
        _editRuleButton.AutoSize = true;
        _deleteRuleButton.AutoSize = true;
        _editRuleButton.Click += (_, _) => EditSelectedRule();
        _deleteRuleButton.Click += (_, _) => DeleteSelectedRule();
        actions.Controls.AddRange([_editRuleButton, _deleteRuleButton]);

        Panel body = new() { Dock = DockStyle.Fill };
        body.Controls.Add(_rulesGrid);
        body.Controls.Add(actions);
        body.Controls.Add(header);
        _rulesGrid.BringToFront();
        _rulesGrid.Margin = new Padding(0, header.Height + 20, 0, actions.Height);
        page.Controls.Add(body);

        header.Dock = DockStyle.Top;
        actions.Dock = DockStyle.Bottom;
        _rulesGrid.Dock = DockStyle.Fill;
        return page;
    }

    private Panel BuildEventsPage()
    {
        Panel page = new();
        _eventsTitle.Name = "EventsTitle";
        TableLayoutPanel header = Header(_eventsTitle, _eventsSubtitle);

        FlowLayoutPanel tools = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 5),
            WrapContents = true
        };
        _eventSearch.Width = 250;
        _eventSearch.Margin = new Padding(0, 3, 8, 3);
        _eventSearch.TextChanged += (_, _) => RefreshEvents();
        _exportButton.AutoSize = true;
        _openLogsButton.AutoSize = true;
        _exportButton.MinimumSize = new Size(96, 34);
        _openLogsButton.MinimumSize = new Size(118, 34);
        _exportButton.Click += async (_, _) => await ExportEventsAsync();
        _openLogsButton.Click += (_, _) => OpenLogFolder();
        _clearEventsButton.Click += (_, _) => ClearEventDisplay();
        tools.Controls.AddRange(
            [_eventSearch, _exportButton, _openLogsButton, _clearEventsButton]);

        _eventsGrid.Name = "EventsGrid";
        _eventsGrid.ColumnHeadersDefaultCellStyle.WrapMode = DataGridViewTriState.True;
        _eventsGrid.ColumnHeadersDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _eventsGrid.RowsDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleLeft;
        _eventsGrid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 18,
            MinimumWidth = 120
        });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 13,
            MinimumWidth = 92
        });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 14,
            MinimumWidth = 100
        });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 24,
            MinimumWidth = 140
        });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 20,
            MinimumWidth = 110
        });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            FillWeight = 11,
            MinimumWidth = 68
        });
        _eventsGrid.CellDoubleClick += (_, args) =>
        {
            if (args.RowIndex >= 0 &&
                _eventsGrid.Rows[args.RowIndex].Tag is ConnectionEvent entry)
            {
                ShowEventDetails(entry);
            }
        };

        _eventDetailsHint.Name = "EventDetailsHint";
        _eventDetailsHint.Dock = DockStyle.Fill;
        _eventDetailsHint.TextAlign = ContentAlignment.MiddleLeft;

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        header.Dock = DockStyle.Fill;
        tools.Dock = DockStyle.Fill;
        _eventsGrid.Dock = DockStyle.Fill;
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(tools, 0, 1);
        layout.Controls.Add(_eventsGrid, 0, 2);
        layout.Controls.Add(_eventDetailsHint, 0, 3);
        page.Controls.Add(layout);
        return page;
    }

    private Panel BuildSettingsPage()
    {
        Panel page = new();
        _settingsTitle.Name = "SettingsTitle";
        TableLayoutPanel header = Header(_settingsTitle, _settingsSubtitle);
        TableLayoutPanel settings = new()
        {
            Name = "SettingsTable",
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 20, 12, 0)
        };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        AddSetting(settings, "LanguageLabel", null, _language, 0);
        AddSetting(settings, string.Empty, _startWithWindowsHint, _startWithWindows, 1, _startWithWindowsCaption);
        AddSetting(settings, string.Empty, _resumeMonitoringHint, _resumeMonitoring, 2, _resumeMonitoringCaption);
        AddSetting(settings, string.Empty, _alertSoundHint, _alertSound, 3, _alertSoundCaption);
        _alertVolumeControls.Controls.AddRange([_testAlertSoundButton, _alertVolume]);
        AddSetting(settings, string.Empty, _alertVolumeHint, _alertVolumeControls, 4, _alertVolumeCaption);
        AddSetting(settings, string.Empty, _logLimitHint, _logLimit, 5, _logLimitCaption);
        AddSetting(settings, string.Empty, _helpCenterHint, _helpCenterButton, 6, _helpCenterCaption);
        AddSetting(settings, string.Empty, _feedbackHint, _feedbackButton, 7, _feedbackCaption);
        AddSetting(
            settings,
            string.Empty,
            _updateHint,
            _checkUpdatesButton,
            8,
            _updateCaption);

        _language.DrawItem += DrawLanguageItem;
        _language.SelectedIndexChanged += LanguageChanged;
        _startWithWindows.CheckedChanged += StartWithWindowsChanged;
        _resumeMonitoring.CheckedChanged += (_, _) =>
        {
            if (_refreshingSettings) return;
            _settings.ResumeMonitoring = _resumeMonitoring.Checked;
            SaveSettings();
        };
        _alertSound.CheckedChanged += (_, _) =>
        {
            if (_refreshingSettings) return;
            _settings.AlertSound = _alertSound.Checked;
            SaveSettings();
        };
        _alertVolume.ValueChanged += (_, _) =>
        {
            if (_refreshingSettings) return;
            _settings.AlertVolumePercent = decimal.ToInt32(_alertVolume.Value);
            SaveSettings();
        };
        _testAlertSoundButton.Click += (_, _) =>
            AlertSoundPlayer.Play(decimal.ToInt32(_alertVolume.Value));
        _logLimit.Validated += async (_, _) => await LogLimitChangedAsync();
        _helpCenterButton.Click += (_, _) =>
        {
            using HelpCenterForm helpCenter = new();
            helpCenter.ShowDialog(this);
        };
        _feedbackButton.Click += (_, _) =>
        {
            using FeedbackForm feedback = new();
            feedback.ShowDialog(this);
        };
        _checkUpdatesButton.Click += async (_, _) => await CheckForUpdatesAsync();

        Panel body = new() { Dock = DockStyle.Fill, AutoScroll = true };
        body.Controls.Add(settings);
        body.Controls.Add(header);
        header.Dock = DockStyle.Top;
        settings.Dock = DockStyle.Top;
        page.Controls.Add(body);
        return page;
    }

    private static TableLayoutPanel Header(
        Label title,
        Label subtitle,
        Control? action = null)
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Top,
            Height = 82,
            ColumnCount = action is null ? 1 : 2,
            RowCount = 1,
            Margin = Padding.Empty
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (action is not null)
        {
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        TableLayoutPanel text = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty
        };
        text.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        text.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        text.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        title.Dock = DockStyle.Fill;
        subtitle.AutoSize = false;
        subtitle.Dock = DockStyle.Fill;
        subtitle.AutoEllipsis = true;
        subtitle.Margin = Padding.Empty;
        text.Controls.Add(title, 0, 0);
        text.Controls.Add(subtitle, 0, 1);
        header.Controls.Add(text, 0, 0);
        if (action is not null)
        {
            action.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            action.Margin = new Padding(12, 0, 0, 0);
            header.Controls.Add(action, 1, 0);
        }

        return header;
    }

    private static void AddSetting(
        TableLayoutPanel layout,
        string labelName,
        Label? hint,
        Control value,
        int row,
        Label? explicitLabel = null)
    {
        FlowLayoutPanel text = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            // Keep each setting visually separated without making the page feel sparse.
            Margin = new Padding(0, 7, 12, 14)
        };
        Label label = explicitLabel ?? new Label();
        label.Name = string.IsNullOrEmpty(labelName) ? label.Name : labelName;
        label.AutoSize = true;
        label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        text.Controls.Add(label);
        if (hint is not null)
        {
            hint.Margin = new Padding(0, 2, 0, 0);
            text.Controls.Add(hint);
        }

        value.Anchor = AnchorStyles.Right;
        value.Margin = new Padding(8, 11, 0, 0);
        layout.Controls.Add(text, 0, row);
        layout.Controls.Add(value, 1, row);
    }

    private void BuildTrayMenu()
    {
        _trayMenu.Items.AddRange([_trayOpen, _trayToggle, new ToolStripSeparator(), _trayExit]);
        _trayOpen.Click += (_, _) => ShowMainWindow();
        _trayToggle.Click += async (_, _) => await ToggleMonitoringAsync();
        _trayExit.Click += async (_, _) => await ExitApplicationAsync();
    }

    private async void MainFormShown(object? sender, EventArgs e)
    {
        IReadOnlyList<ConnectionEvent> existing = await _logger.ReadRecentAsync();
        foreach (ConnectionEvent entry in existing.Where(entry => entry.IsActive))
        {
            entry.MarkHistoricalInactive();
        }
        _events.Clear();
        _events.AddRange(existing.Where(IsVisibleAfterDisplayCutoff));
        RefreshEvents();

        if (_settings.ResumeMonitoring && _settings.Rules.Any(rule => rule.Enabled))
        {
            StartMonitoring();
        }
    }

    private void ShowPage(string key)
    {
        foreach ((string pageKey, Panel page) in _pages)
        {
            page.Visible = pageKey == key;
        }

        foreach ((string buttonKey, Button button) in _navButtons)
        {
            button.BackColor = buttonKey == key
                ? SystemColors.GradientActiveCaption
                : SystemColors.ControlLight;
        }

        if (key == "Events")
        {
            _trayNoticeCount = 0;
            UpdateTrayIcon();
            RefreshEvents();
        }
    }

    private void ApplyLanguage()
    {
        UiFont.Apply(this);
        _eventStatusFont?.Dispose();
        _eventStatusFont = new Font(_eventsGrid.Font, FontStyle.Bold);
        Text = UiText.Get("AppTitle");
        foreach (string key in _navButtons.Keys)
        {
            _navButtons[key].Text = UiText.Get(key);
        }

        _homeTitle.Text = UiText.Get("Home");
        _homeSubtitle.Text = UiText.Get("StatusHint");
        _homeStatusCaption.Text = UiText.Get("MonitoringStatus");
        _homeRulesCaption.Text = UiText.Get("EnabledRules");
        _homeIntervalCaption.Text = UiText.Get("CheckInterval");
        _checkIntervalUnit.Text = UiText.Get("SecondsUnit");
        _actionLegendTitle.Text = UiText.Get("ActionLegend");
        _silentActionLegend.Text = ActionLegendText(MatchAction.SilentLog);
        _trayActionLegend.Text = ActionLegendText(MatchAction.TrayNotice);
        _popupActionLegend.Text = ActionLegendText(MatchAction.PopupAlert);

        _rulesTitle.Text = UiText.Get("Rules");
        _rulesSubtitle.Text = UiText.Get("RulesDescription");
        _newRuleButton.Text = UiText.Get("NewRule");
        _editRuleButton.Text = UiText.Get("Edit");
        _deleteRuleButton.Text = UiText.Get("Delete");
        string[] ruleHeaders = ["Name", "Condition", "MatchAction", "Enabled"];
        for (int index = 0; index < ruleHeaders.Length; index++)
        {
            _rulesGrid.Columns[index].HeaderText = UiText.Get(ruleHeaders[index]);
        }

        _eventsTitle.Text = UiText.Get("Events");
        _eventsSubtitle.Text = UiText.Get("EventsDescription");
        _eventSearch.PlaceholderText = UiText.Get("SearchEvents");
        _exportButton.Text = UiText.Get("ExportCsv");
        _openLogsButton.Text = UiText.Get("OpenLogFolder");
        _clearEventsButton.Text = UiText.Get("ClearDisplay");
        string[] eventHeaders =
        [
            "Time", "ConnectionStatus", "ObservedDuration",
            "RemoteEndpoint", "Application", "ActionColumn"
        ];
        for (int index = 0; index < eventHeaders.Length; index++)
        {
            _eventsGrid.Columns[index].HeaderText = UiText.Get(eventHeaders[index]);
        }
        _eventDetailsHint.Text = UiText.Get("EventDoubleClickHint");

        _settingsTitle.Text = UiText.Get("Settings");
        _settingsSubtitle.Text = UiText.Get("Privacy");
        Controls.Find("LanguageLabel", true).Single().Text = UiText.Get("Language");
        _startWithWindowsCaption.Text = UiText.Get("StartWithWindows");
        _startWithWindows.Text = string.Empty;
        _startWithWindowsHint.Text = UiText.Get("StartWithWindowsHint");
        _resumeMonitoringCaption.Text = UiText.Get("ResumeMonitoring");
        _resumeMonitoring.Text = string.Empty;
        _resumeMonitoringHint.Text = UiText.Get("ResumeMonitoringHint");
        _alertSoundCaption.Text = UiText.Get("AlertSound");
        _alertSound.Text = string.Empty;
        _alertSoundHint.Text = UiText.Get("AlertSoundHint");
        _testAlertSoundButton.Text = UiText.Get("TestSound");
        _alertVolumeCaption.Text = UiText.Get("AlertVolume");
        _alertVolumeHint.Text = UiText.Get("AlertVolumeHint");
        _logLimitCaption.Text = UiText.Get("LogLimit");
        _logLimitHint.Text = UiText.Get("LogLimitHint");
        _helpCenterCaption.Text = UiText.Get("HelpCenter");
        _helpCenterHint.Text = UiText.Get("HelpCenterHint");
        _helpCenterButton.Text = UiText.Get("OpenHelpCenter");
        _feedbackCaption.Text = UiText.Get("Feedback");
        _feedbackHint.Text = UiText.Get("FeedbackHint");
        _feedbackButton.Text = UiText.Get("OpenFeedback");
        _updateCaption.Text = string.Format(
            UiText.Get("SoftwareUpdatesWithVersion"),
            CurrentVersion().ToString(3));
        _updateHint.Text = UiText.Get("SoftwareUpdatesHint");
        _checkUpdatesButton.Text = UiText.Get("CheckNow");

        _trayOpen.Text = UiText.Get("Open");
        _trayExit.Text = UiText.Get("Exit");
        _sidebarHint.Text = UiText.Get("StatusHint");
        RefreshRules();
        RefreshEvents();
        RefreshSettings();
        UpdateMonitoringStatus();
    }

    private void RefreshRules()
    {
        if (_rulesGrid.Columns.Count == 0)
        {
            return;
        }

        _refreshingRules = true;
        try
        {
            _rulesGrid.Rows.Clear();
            foreach (MonitoringRule rule in _settings.Rules)
            {
                int index = _rulesGrid.Rows.Add(
                    rule.Name,
                    UiText.FormatRuleCondition(rule),
                    UiText.ActionCompact(rule.Action),
                    rule.Enabled);
                _rulesGrid.Rows[index].Tag = rule.Id;
                DataGridViewCell actionCell = _rulesGrid.Rows[index].Cells[2];
                Color actionColor = ActionColor(rule.Action);
                actionCell.ToolTipText = UiText.Action(rule.Action);
                actionCell.Style.ForeColor = actionColor;
                actionCell.Style.SelectionForeColor = actionColor;
                actionCell.Style.SelectionBackColor = SystemColors.Window;
            }
        }
        finally
        {
            _refreshingRules = false;
        }

        UpdateRuleButtons();
        _homeRules.Text = _settings.Rules.Count(rule => rule.Enabled).ToString(CultureInfo.CurrentCulture);
    }

    private void AddRule()
    {
        using RuleEditorForm editor = new();
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        lock (_settingsGate)
        {
            _settings.Rules.Add(editor.ResultRule);
            SaveSettings();
        }
        RefreshRules();
    }

    private void EditSelectedRule()
    {
        MonitoringRule? selected = SelectedRule();
        if (selected is null)
        {
            return;
        }

        using RuleEditorForm editor = new(selected);
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int index = _settings.Rules.FindIndex(rule => rule.Id == selected.Id);
        if (index >= 0)
        {
            lock (_settingsGate)
            {
                _settings.Rules[index] = editor.ResultRule;
                SaveSettings();
            }
            RefreshRules();
        }
    }

    private void DeleteSelectedRule()
    {
        MonitoringRule? selected = SelectedRule();
        if (selected is null)
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            string.Format(UiText.Get("DeleteRuleQuestion"), selected.Name),
            UiText.Get("DeleteRuleTitle"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);
        if (result != DialogResult.Yes)
        {
            return;
        }

        lock (_settingsGate)
        {
            _settings.Rules.RemoveAll(rule => rule.Id == selected.Id);
            SaveSettings();
        }
        RefreshRules();
    }

    private MonitoringRule? SelectedRule()
    {
        if (_rulesGrid.SelectedRows.Count == 0 ||
            _rulesGrid.SelectedRows[0].Tag is not Guid id)
        {
            return null;
        }

        return _settings.Rules.FirstOrDefault(rule => rule.Id == id);
    }

    private void UpdateRuleButtons()
    {
        bool selected = _rulesGrid.SelectedRows.Count > 0;
        _editRuleButton.Enabled = selected;
        _deleteRuleButton.Enabled = selected;
    }

    private void RuleEnabledChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_refreshingRules || e.RowIndex < 0 || e.ColumnIndex != 3)
        {
            return;
        }

        DataGridViewRow row = _rulesGrid.Rows[e.RowIndex];
        if (row.Tag is Guid id && row.Cells[3].Value is bool enabled)
        {
            MonitoringRule? rule = _settings.Rules.FirstOrDefault(item => item.Id == id);
            if (rule is not null)
            {
                lock (_settingsGate)
                {
                    rule.Enabled = enabled;
                    SaveSettings();
                }
                _homeRules.Text = _settings.Rules.Count(item => item.Enabled)
                    .ToString(CultureInfo.CurrentCulture);
            }
        }
    }

    private IReadOnlyList<MonitoringRule> GetRulesSnapshot()
    {
        lock (_settingsGate)
        {
            return _settings.Rules.Select(rule => rule.Copy()).ToArray();
        }
    }

    private async Task ToggleMonitoringAsync()
    {
        if (_engine.IsRunning)
        {
            await _engine.StopAsync();
            _monitoringError = false;
            UpdateMonitoringStatus();
        }
        else
        {
            StartMonitoring();
        }
    }

    private void StartMonitoring()
    {
        try
        {
            _monitoringError = false;
            _engine.Start();
            UpdateMonitoringStatus();
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                UiText.Get("NeedEnabledRule"),
                UiText.Get("AppTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }
    }

    private void EngineEventDetected(object? sender, ConnectionEvent connectionEvent)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() => HandleDetectedEvent(connectionEvent));
    }

    private void EngineEventCompleted(object? sender, ConnectionEvent connectionEvent)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            if (_pages["Events"].Visible)
            {
                RefreshEvents();
            }
        });
    }

    private void EngineMonitoringError(object? sender, Exception exception)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            _monitoringError = true;
            _sidebarHint.Text = exception.Message;
            UpdateMonitoringStatus();
        });
    }

    private void EngineMonitoringRecovered(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            _monitoringError = false;
            _sidebarHint.Text = UiText.Get("StatusHint");
            UpdateMonitoringStatus();
        });
    }

    private void HandleDetectedEvent(ConnectionEvent connectionEvent)
    {
        if (IsVisibleAfterDisplayCutoff(connectionEvent))
        {
            _events.Insert(0, connectionEvent);
        }
        if (_events.Count > 2000)
        {
            _events.RemoveRange(2000, _events.Count - 2000);
        }
        if (_pages["Events"].Visible)
        {
            RefreshEvents();
        }

        switch (connectionEvent.Action)
        {
            case MatchAction.TrayNotice:
                _trayNoticeCount++;
                UpdateTrayIcon();
                break;
            case MatchAction.PopupAlert:
                ShowUrgentAlert(connectionEvent);
                break;
        }
    }

    private void ShowUrgentAlert(ConnectionEvent connectionEvent)
    {
        if (_urgentAlert is { IsDisposed: false, Visible: true })
        {
            _urgentAlert.AddEvent(connectionEvent);
            return;
        }

        bool eligible = connectionEvent.RepeatAlertMinutes == 0 ||
            connectionEvent.RuleIds.Any(id =>
                !_lastPopupClosed.TryGetValue(id, out DateTimeOffset closed) ||
                DateTimeOffset.Now - closed >= TimeSpan.FromMinutes(connectionEvent.RepeatAlertMinutes));
        if (!eligible)
        {
            return;
        }

        _urgentAlert = new UrgentAlertForm(connectionEvent);
        _urgentAlert.ViewDetailsRequested += (_, _) =>
        {
            ShowMainWindow();
            ShowPage("Events");
        };
        _urgentAlert.FormClosed += (_, _) =>
        {
            DateTimeOffset closed = DateTimeOffset.Now;
            foreach (Guid id in _urgentAlert.RuleIds)
            {
                _lastPopupClosed[id] = closed;
            }

            _urgentAlert = null;
        };
        _urgentAlert.Show(this);
        if (_settings.AlertSound)
        {
            AlertSoundPlayer.Play(_settings.AlertVolumePercent);
        }
    }

    private IEnumerable<ConnectionEvent> FilteredEvents()
    {
        IEnumerable<ConnectionEvent> visible =
            _events.Where(IsVisibleAfterDisplayCutoff);
        string query = _eventSearch.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return visible;
        }

        return visible.Where(entry =>
            string.Join(" ", entry.RuleNames).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.RemoteAddress.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.RemotePort.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.LocalAddress.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.LocalPort.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            (entry.ProcessProductName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            (entry.ProcessCompanyName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false) ||
            entry.ParentProcesses.Any(parent =>
                parent.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (parent.ProductName?.Contains(query, StringComparison.OrdinalIgnoreCase) ?? false)) ||
            entry.RelatedServices.Any(service =>
                service.ServiceName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                service.DisplayName.Contains(query, StringComparison.OrdinalIgnoreCase)));
    }

    private void RefreshEvents()
    {
        if (_eventsGrid.Columns.Count == 0)
        {
            return;
        }

        _eventsGrid.Rows.Clear();
        foreach (ConnectionEvent entry in FilteredEvents().Take(2000))
        {
            int row = _eventsGrid.Rows.Add(
                entry.DetectedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                FormatConnectionStatus(entry),
                FormatObservedDuration(entry, DateTimeOffset.Now),
                FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                entry.ApplicationDisplayName,
                UiText.ActionMarker(entry.Action));
            _eventsGrid.Rows[row].Tag = entry;
            DataGridViewCell statusCell = _eventsGrid.Rows[row].Cells[1];
            Color statusColor = entry.IsActive ? Color.SeaGreen : Color.DimGray;
            statusCell.Style.ForeColor = statusColor;
            statusCell.Style.SelectionForeColor = SystemColors.HighlightText;
            statusCell.Style.Font = _eventStatusFont;
            _eventsGrid.Rows[row].Cells[4].ToolTipText =
                string.Format(
                    UiText.Get("ApplicationTooltip"),
                    entry.ProcessName,
                    entry.ProcessId,
                    entry.ProcessPath ?? UiText.Get("ProcessPathUnavailable"));
            DataGridViewCell actionCell = _eventsGrid.Rows[row].Cells[5];
            actionCell.ToolTipText = UiText.Action(entry.Action);
            Color actionColor = ActionColor(entry.Action);
            actionCell.Style.ForeColor = actionColor;
            actionCell.Style.SelectionForeColor = actionColor;
            actionCell.Style.SelectionBackColor = SystemColors.Window;
        }
        _clearEventsButton.Enabled = _events.Any(IsVisibleAfterDisplayCutoff);
    }

    private bool IsVisibleAfterDisplayCutoff(ConnectionEvent entry) =>
        _settings.EventLogDisplayCutoff is not DateTimeOffset cutoff ||
        entry.DetectedAt > cutoff;

    private void ClearEventDisplay()
    {
        if (MessageBox.Show(
                this,
                UiText.Get("ClearDisplayQuestion"),
                UiText.Get("ClearDisplayTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question) != DialogResult.Yes)
        {
            return;
        }

        DateTimeOffset cutoff = DateTimeOffset.Now;
        _settings.EventLogDisplayCutoff = cutoff;
        SaveSettings();
        _events.RemoveAll(entry => entry.DetectedAt <= cutoff);
        RefreshEvents();
    }

    private void ShowEventDetails(ConnectionEvent entry)
    {
        using EventDetailsForm details = new(entry);
        details.ShowDialog(this);
    }

    private void UpdateVisibleEventDurations()
    {
        if (!_pages.TryGetValue("Events", out Panel? page) || !page.Visible)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.Now;
        foreach (DataGridViewRow row in _eventsGrid.Rows)
        {
            if (row.Tag is ConnectionEvent { IsActive: true } entry)
            {
                row.Cells[2].Value = FormatObservedDuration(entry, now);
            }
        }
    }

    private static string FormatObservedDuration(
        ConnectionEvent entry,
        DateTimeOffset now)
    {
        TimeSpan? duration = entry.GetObservedDuration(now);
        if (duration is null)
        {
            return "—";
        }

        int totalHours = (int)Math.Floor(duration.Value.TotalHours);
        return $"{totalHours:00}:{duration.Value.Minutes:00}:{duration.Value.Seconds:00}";
    }

    private static string FormatConnectionStatus(ConnectionEvent entry) =>
        $"{(entry.IsActive ? '●' : '○')} " +
        UiText.Get(entry.IsActive ? "ConnectionActive" : "ConnectionEnded");

    private static string ActionLegendText(MatchAction action)
    {
        return $"{UiText.ActionMarker(action)}  {UiText.Action(action)}";
    }

    private static Color ActionColor(MatchAction action)
    {
        return action switch
        {
            MatchAction.SilentLog => Color.DimGray,
            MatchAction.TrayNotice => Color.DarkOrange,
            _ => Color.Crimson
        };
    }

    private async Task ExportEventsAsync()
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"connection-events-{DateTime.Now:yyyyMMdd-HHmmss}.csv"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        StringBuilder csv = new();
        csv.AppendLine(string.Join(',', new[]
        {
            UiText.Get("Time"), UiText.Get("ConnectionStatus"),
            UiText.Get("ObservedDuration"), UiText.Get("MatchedRules"),
            UiText.Get("MatchAction"), UiText.Get("TcpState"), UiText.Get("LocalEndpoint"),
            UiText.Get("RemoteEndpoint"), "PID", UiText.Get("ConnectionOwner"),
            UiText.Get("ProcessPath"), UiText.Get("ProductName"),
            UiText.Get("CompanyName"), UiText.Get("FileDescription"),
            UiText.Get("ParentProcesses"), UiText.Get("RelatedServices")
        }.Select(CsvEscape)));
        foreach (ConnectionEvent entry in FilteredEvents())
        {
            csv.AppendLine(string.Join(',', new[]
            {
                entry.DetectedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                UiText.Get(entry.IsActive ? "ConnectionActive" : "ConnectionEnded"),
                FormatObservedDuration(entry, DateTimeOffset.Now),
                string.Join(" | ", entry.RuleNames),
                UiText.Action(entry.Action),
                entry.State.ToString(),
                FormatEndpoint(entry.LocalAddress, entry.LocalPort),
                FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                entry.ProcessId.ToString(CultureInfo.InvariantCulture),
                entry.ProcessName,
                entry.ProcessPath ?? string.Empty,
                entry.ProcessProductName ?? string.Empty,
                entry.ProcessCompanyName ?? string.Empty,
                entry.ProcessFileDescription ?? string.Empty,
                FormatParentProcesses(entry.ParentProcesses),
                FormatRelatedServices(entry.RelatedServices)
            }.Select(CsvEscape)));
        }

        await File.WriteAllTextAsync(dialog.FileName, csv.ToString(), new UTF8Encoding(true));
        MessageBox.Show(
            UiText.Get("ExportComplete"),
            UiText.Get("AppTitle"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information);
    }

    private static string CsvEscape(string value)
    {
        return '"' + value.Replace("\"", "\"\"").Replace('\r', ' ').Replace('\n', ' ') + '"';
    }

    private static string FormatEndpoint(string address, int port)
    {
        return address.Contains(':', StringComparison.Ordinal)
            ? $"[{address}]:{port}"
            : $"{address}:{port}";
    }

    private static string FormatParentProcesses(
        IEnumerable<ProcessSnapshot> parents) =>
        string.Join(
            " -> ",
            parents.Select(parent =>
                $"{parent.ProcessName} (PID {parent.ProcessId})"));

    private static string FormatRelatedServices(
        IEnumerable<WindowsServiceSnapshot> services) =>
        string.Join(
            " | ",
            services.Select(service =>
                $"{service.DisplayName} [{service.ServiceName}] (PID {service.ProcessId})"));

    private void OpenLogFolder()
    {
        Directory.CreateDirectory(_logger.LogDirectory);
        Process.Start(new ProcessStartInfo
        {
            FileName = "explorer.exe",
            Arguments = $"\"{_logger.LogDirectory}\"",
            UseShellExecute = true
        });
    }

    private void RefreshSettings()
    {
        _refreshingSettings = true;
        try
        {
            _language.Items.Clear();
            _language.Items.AddRange(
            [
                new LanguageOption("zh-CN", UiText.Get("Chinese")),
                new LanguageOption("zh-TW", UiText.Get("TraditionalChinese")),
                new LanguageOption("en", UiText.Get("English")),
                new LanguageOption("es", UiText.Get("Spanish")),
                new LanguageOption("fr", UiText.Get("French")),
                new LanguageOption("de", UiText.Get("German")),
                new LanguageOption("pt-BR", UiText.Get("BrazilianPortuguese"))
            ]);
            _language.SelectedIndex = _language.Items
                .Cast<LanguageOption>()
                .Select((option, index) => (option, index))
                .First(pair => pair.option.Code == UiText.Language)
                .index;
            _startWithWindows.Checked = _settings.StartWithWindows;
            _resumeMonitoring.Checked = _settings.ResumeMonitoring;
            _alertSound.Checked = _settings.AlertSound;
            _alertVolume.Value = Math.Clamp(
                _settings.AlertVolumePercent,
                AppSettings.MinimumAlertVolumePercent,
                AppSettings.MaximumAlertVolumePercent);
            _logLimit.Value = Math.Clamp(
                _settings.LogLimitMb,
                AppSettings.MinimumLogLimitMb,
                AppSettings.MaximumLogLimitMb);
            _checkInterval.Value = Math.Clamp(
                _settings.CheckIntervalSeconds,
                AppSettings.MinimumCheckIntervalSeconds,
                AppSettings.MaximumCheckIntervalSeconds);
        }
        finally
        {
            _refreshingSettings = false;
        }
    }

    private void LanguageChanged(object? sender, EventArgs e)
    {
        if (_refreshingSettings || _language.SelectedItem is not LanguageOption option)
        {
            return;
        }

        _settings.Language = option.Code;
        UiText.SetLanguage(option.Code);
        SaveSettings();
        ApplyLanguage();
    }

    private void DrawLanguageItem(object? sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        LanguageOption? option = e.Index >= 0 && e.Index < _language.Items.Count
            ? _language.Items[e.Index] as LanguageOption
            : _language.SelectedItem as LanguageOption;
        if (option is not null)
        {
            bool selectedDisplay = e.State.HasFlag(DrawItemState.ComboBoxEdit);
            Rectangle bounds = selectedDisplay
                ? e.Bounds
                : new Rectangle(
                    e.Bounds.X + 4,
                    e.Bounds.Y,
                    Math.Max(0, e.Bounds.Width - 8),
                    e.Bounds.Height);
            TextFormatFlags flags = TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.NoPrefix |
                (selectedDisplay
                    ? TextFormatFlags.HorizontalCenter
                    : TextFormatFlags.Left);
            TextRenderer.DrawText(
                e.Graphics,
                option.Label,
                e.Font,
                bounds,
                e.ForeColor,
                flags);
        }

        e.DrawFocusRectangle();
    }

    private void StartWithWindowsChanged(object? sender, EventArgs e)
    {
        if (_refreshingSettings)
        {
            return;
        }

        try
        {
            StartupManager.SetEnabled(_startWithWindows.Checked);
            _settings.StartWithWindows = _startWithWindows.Checked;
            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format(UiText.Get("StartupSettingError"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _refreshingSettings = true;
            _startWithWindows.Checked = _settings.StartWithWindows;
            _refreshingSettings = false;
        }
    }

    private async Task LogLimitChangedAsync()
    {
        if (_refreshingSettings)
        {
            return;
        }

        int previousValue = _settings.LogLimitMb;
        int selectedValue = decimal.ToInt32(_logLimit.Value);
        try
        {
            await _logger.UpdateMaximumTotalBytesAsync(selectedValue * 1024L * 1024L);
            _settings.LogLimitMb = selectedValue;
            SaveSettings();
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                string.Format(UiText.Get("LogLimitUpdateError"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
            _refreshingSettings = true;
            _logLimit.Value = Math.Clamp(
                previousValue,
                AppSettings.MinimumLogLimitMb,
                AppSettings.MaximumLogLimitMb);
            _refreshingSettings = false;
        }
    }

    private void CheckIntervalChanged()
    {
        if (_refreshingSettings)
        {
            return;
        }

        decimal seconds = _checkInterval.Value;
        _settings.CheckIntervalSeconds = seconds;
        _engine.UpdateInterval(TimeSpan.FromSeconds((double)seconds));
        SaveSettings();
    }

    private async Task CheckForUpdatesAsync()
    {
        _checkUpdatesButton.Enabled = false;
        _checkUpdatesButton.Text = UiText.Get("CheckingUpdates");
        try
        {
            UpdateCheckResult result = await _updateCheckService.CheckAsync(
                CurrentVersion());
            if (result.IsUpdateAvailable)
            {
                DialogResult choice = MessageBox.Show(
                    this,
                    string.Format(
                        UiText.Get("UpdateAvailableMessage"),
                        result.LatestTag,
                        result.ReleaseName),
                    UiText.Get("UpdateAvailableTitle"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information);
                if (choice == DialogResult.Yes)
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = result.ReleasePage.AbsoluteUri,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                MessageBox.Show(
                    this,
                    string.Format(
                        UiText.Get("UpToDateMessage"),
                        result.CurrentVersion.ToString(3)),
                    UiText.Get("SoftwareUpdates"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }
        catch (Exception ex) when (
            ex is HttpRequestException or TaskCanceledException or
            InvalidDataException or UriFormatException)
        {
            MessageBox.Show(
                this,
                string.Format(UiText.Get("UpdateCheckFailed"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            _checkUpdatesButton.Enabled = true;
            _checkUpdatesButton.Text = UiText.Get("CheckNow");
        }
    }

    private static Version CurrentVersion() =>
        typeof(MainForm).Assembly.GetName().Version ?? new Version(1, 0, 0);

    private void SaveSettings()
    {
        lock (_settingsGate)
        {
            _settingsStore.Save(_settings);
        }
    }

    private void UpdateMonitoringStatus()
    {
        string status;
        Color color;
        if (_monitoringError)
        {
            status = UiText.Get("MonitoringInterrupted");
            color = Color.DarkOrange;
        }
        else if (_engine.IsRunning)
        {
            status = UiText.Get("MonitoringRunning");
            color = Color.ForestGreen;
        }
        else
        {
            status = UiText.Get("MonitoringStopped");
            color = SystemColors.GrayText;
        }

        _sidebarStatus.Text = status;
        _sidebarStatus.ForeColor = color;
        _homeStatus.Text = status;
        _homeStatus.ForeColor = color;
        _monitorButton.Text = UiText.Get(_engine.IsRunning ? "StopMonitoring" : "StartMonitoring");
        _trayToggle.Text = _monitorButton.Text;
        _homeRules.Text = _settings.Rules.Count(rule => rule.Enabled).ToString(CultureInfo.CurrentCulture);
        UpdateTrayIcon();
    }

    private void UpdateTrayIcon()
    {
        if (_monitoringError)
        {
            _notifyIcon.Icon = SystemIcons.Error;
            _notifyIcon.Text = Truncate(UiText.Get("MonitoringInterrupted"));
        }
        else if (_trayNoticeCount > 0)
        {
            _notifyIcon.Icon = SystemIcons.Warning;
            _notifyIcon.Text = Truncate(string.Format(UiText.Get("TrayNotices"), _trayNoticeCount));
        }
        else if (_engine.IsRunning)
        {
            _notifyIcon.Icon = _applicationIcon;
            _notifyIcon.Text = Truncate(UiText.Get("TrayNormal"));
        }
        else
        {
            _notifyIcon.Icon = _applicationIcon;
            _notifyIcon.Text = Truncate(UiText.Get("AppTitle"));
        }
    }

    private static string Truncate(string text)
    {
        return text.Length <= 63 ? text : text[..63];
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    private async void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_exitRequested || e.CloseReason != CloseReason.UserClosing)
        {
            return;
        }

        if (_engine.IsRunning)
        {
            DialogResult result = MessageBox.Show(
                UiText.Get("CloseWhileRunning"),
                UiText.Get("CloseWhileRunningTitle"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question);
            if (result == DialogResult.Yes)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            await ExitApplicationAsync();
        }
    }

    private async Task ExitApplicationAsync()
    {
        _exitRequested = true;
        await _engine.StopAsync();
        _notifyIcon.Visible = false;
        Close();
    }

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

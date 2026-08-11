using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Media;
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

    private readonly Label _sidebarStatus = new();
    private readonly Label _sidebarHint = new();
    private readonly Label _homeTitle = TitleLabel();
    private readonly Label _homeSubtitle = SubtitleLabel();
    private readonly Label _homeStatusCaption = new();
    private readonly Label _homeStatus = ValueLabel();
    private readonly Label _homeRulesCaption = new();
    private readonly Label _homeRules = ValueLabel();
    private readonly Label _homeIntervalCaption = new();
    private readonly Label _homeInterval = ValueLabel();
    private readonly Label _shortConnectionNote = SubtitleLabel();
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
    private readonly DataGridView _eventsGrid = NewGrid();

    private readonly Label _settingsTitle = TitleLabel();
    private readonly Label _settingsSubtitle = SubtitleLabel();
    private readonly ComboBox _language = new() { DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly Label _startWithWindowsCaption = new();
    private readonly CheckBox _startWithWindows = new();
    private readonly Label _startWithWindowsHint = SubtitleLabel();
    private readonly Label _resumeMonitoringCaption = new();
    private readonly CheckBox _resumeMonitoring = new();
    private readonly Label _resumeMonitoringHint = SubtitleLabel();
    private readonly Label _alertSoundCaption = new();
    private readonly CheckBox _alertSound = new();
    private readonly Label _alertSoundHint = SubtitleLabel();
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
        AutoSize = true,
        MinimumSize = new Size(150, 36)
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
            GetRulesSnapshot);
        _engine.EventDetected += EngineEventDetected;
        _engine.MonitoringError += EngineMonitoringError;
        _engine.MonitoringRecovered += EngineMonitoringRecovered;

        _notifyIcon = new NotifyIcon
        {
            Icon = SystemIcons.Application,
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
            Font = new Font("Segoe UI", 17F, FontStyle.Regular),
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
        Icon = SystemIcons.Shield;
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
        FlowLayoutPanel header = Header(_homeTitle, _homeSubtitle);
        _monitorButton.AutoSize = true;
        _monitorButton.MinimumSize = new Size(140, 38);
        _monitorButton.Click += async (_, _) => await ToggleMonitoringAsync();
        header.Controls.Add(_monitorButton);

        TableLayoutPanel summary = new()
        {
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            Dock = DockStyle.Top,
            Margin = new Padding(0, 24, 0, 20)
        };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.Controls.Add(SummaryBox(_homeStatusCaption, _homeStatus), 0, 0);
        summary.Controls.Add(SummaryBox(_homeRulesCaption, _homeRules), 1, 0);
        summary.Controls.Add(SummaryBox(_homeIntervalCaption, _homeInterval), 2, 0);

        _shortConnectionNote.Padding = new Padding(12);
        Panel body = new() { Dock = DockStyle.Fill };
        body.Controls.Add(_shortConnectionNote);
        body.Controls.Add(summary);
        body.Controls.Add(header);
        page.Controls.Add(body);
        return page;
    }

    private static Panel SummaryBox(Label caption, Label value)
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
        return panel;
    }

    private Panel BuildRulesPage()
    {
        Panel page = new();
        FlowLayoutPanel header = Header(_rulesTitle, _rulesSubtitle);
        _newRuleButton.AutoSize = true;
        _newRuleButton.MinimumSize = new Size(110, 36);
        _newRuleButton.Click += (_, _) => AddRule();
        header.Controls.Add(_newRuleButton);

        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 22 });
        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 48 });
        _rulesGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 22 });
        _rulesGrid.Columns.Add(new DataGridViewCheckBoxColumn { FillWeight = 8 });
        _rulesGrid.ReadOnly = false;
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
        FlowLayoutPanel header = Header(_eventsTitle, _eventsSubtitle);

        FlowLayoutPanel tools = new()
        {
            Dock = DockStyle.Top,
            Height = 44,
            Padding = new Padding(0, 5, 0, 5),
            WrapContents = false
        };
        _eventSearch.Width = 320;
        _eventSearch.TextChanged += (_, _) => RefreshEvents();
        _exportButton.AutoSize = true;
        _openLogsButton.AutoSize = true;
        _exportButton.Click += async (_, _) => await ExportEventsAsync();
        _openLogsButton.Click += (_, _) => OpenLogFolder();
        tools.Controls.AddRange([_eventSearch, _exportButton, _openLogsButton]);

        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 18 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 20 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 18 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 18 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 11 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 7 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 16 });
        _eventsGrid.Columns.Add(new DataGridViewTextBoxColumn { FillWeight = 18 });

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 48));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        header.Dock = DockStyle.Fill;
        tools.Dock = DockStyle.Fill;
        _eventsGrid.Dock = DockStyle.Fill;
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(tools, 0, 1);
        layout.Controls.Add(_eventsGrid, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private Panel BuildSettingsPage()
    {
        Panel page = new();
        FlowLayoutPanel header = Header(_settingsTitle, _settingsSubtitle);
        TableLayoutPanel settings = new()
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 20, 0, 0)
        };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        AddSetting(settings, "LanguageLabel", null, _language, 0);
        AddSetting(settings, string.Empty, _startWithWindowsHint, _startWithWindows, 1, _startWithWindowsCaption);
        AddSetting(settings, string.Empty, _resumeMonitoringHint, _resumeMonitoring, 2, _resumeMonitoringCaption);
        AddSetting(settings, string.Empty, _alertSoundHint, _alertSound, 3, _alertSoundCaption);
        AddSetting(settings, string.Empty, _logLimitHint, _logLimit, 4, _logLimitCaption);
        AddSetting(settings, string.Empty, _helpCenterHint, _helpCenterButton, 5, _helpCenterCaption);

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
        _logLimit.Validated += async (_, _) => await LogLimitChangedAsync();
        _helpCenterButton.Click += (_, _) =>
        {
            using HelpCenterForm helpCenter = new();
            helpCenter.ShowDialog(this);
        };

        Panel body = new() { Dock = DockStyle.Fill, AutoScroll = true };
        body.Controls.Add(settings);
        body.Controls.Add(header);
        header.Dock = DockStyle.Top;
        settings.Dock = DockStyle.Top;
        page.Controls.Add(body);
        return page;
    }

    private static FlowLayoutPanel Header(Label title, Label subtitle)
    {
        FlowLayoutPanel header = new()
        {
            Dock = DockStyle.Top,
            Height = 82,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false
        };
        FlowLayoutPanel text = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Width = 650,
            Height = 76
        };
        text.Controls.Add(title);
        text.Controls.Add(subtitle);
        header.Controls.Add(text);
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
            Margin = new Padding(0, 10, 12, 14)
        };
        Label label = explicitLabel ?? new Label();
        label.Name = string.IsNullOrEmpty(labelName) ? label.Name : labelName;
        label.AutoSize = true;
        label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        text.Controls.Add(label);
        if (hint is not null)
        {
            text.Controls.Add(hint);
        }

        value.Anchor = AnchorStyles.Right;
        value.Margin = new Padding(8, 12, 0, 0);
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
        _events.Clear();
        _events.AddRange(existing);
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
        _homeInterval.Text = UiText.Get("OneSecond");
        _shortConnectionNote.Text = UiText.Get("ShortConnectionNote");

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
        string[] eventHeaders =
        [
            "Time", "MatchedRules", "RemoteEndpoint", "LocalEndpoint",
            "TcpState", "PID", "Program", "MatchAction"
        ];
        for (int index = 0; index < eventHeaders.Length; index++)
        {
            _eventsGrid.Columns[index].HeaderText = UiText.Get(eventHeaders[index]);
        }

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
        _logLimitCaption.Text = UiText.Get("LogLimit");
        _logLimitHint.Text = UiText.Get("LogLimitHint");
        _helpCenterCaption.Text = UiText.Get("HelpCenter");
        _helpCenterHint.Text = UiText.Get("HelpCenterHint");
        _helpCenterButton.Text = UiText.Get("OpenHelpCenter");

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
                    UiText.Action(rule.Action),
                    rule.Enabled);
                _rulesGrid.Rows[index].Tag = rule.Id;
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
        _events.Insert(0, connectionEvent);
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
            SystemSounds.Exclamation.Play();
        }
    }

    private IEnumerable<ConnectionEvent> FilteredEvents()
    {
        string query = _eventSearch.Text.Trim();
        if (string.IsNullOrEmpty(query))
        {
            return _events;
        }

        return _events.Where(entry =>
            string.Join(" ", entry.RuleNames).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.RemoteAddress.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.RemotePort.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.LocalAddress.Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.LocalPort.ToString(CultureInfo.InvariantCulture).Contains(query, StringComparison.OrdinalIgnoreCase) ||
            entry.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase));
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
                string.Join(" | ", entry.RuleNames),
                FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                FormatEndpoint(entry.LocalAddress, entry.LocalPort),
                entry.State,
                entry.ProcessId,
                entry.ProcessName,
                UiText.Action(entry.Action));
            _eventsGrid.Rows[row].Tag = entry;
            _eventsGrid.Rows[row].Cells[6].ToolTipText =
                entry.ProcessPath ?? UiText.Get("ProcessPathUnavailable");
        }
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
            UiText.Get("Time"), UiText.Get("MatchedRules"), UiText.Get("MatchAction"),
            UiText.Get("TcpState"), UiText.Get("LocalEndpoint"), UiText.Get("RemoteEndpoint"),
            "PID", UiText.Get("Program"), "Path"
        }.Select(CsvEscape)));
        foreach (ConnectionEvent entry in FilteredEvents())
        {
            csv.AppendLine(string.Join(',', new[]
            {
                entry.DetectedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                string.Join(" | ", entry.RuleNames),
                UiText.Action(entry.Action),
                entry.State.ToString(),
                FormatEndpoint(entry.LocalAddress, entry.LocalPort),
                FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                entry.ProcessId.ToString(CultureInfo.InvariantCulture),
                entry.ProcessName,
                entry.ProcessPath ?? string.Empty
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
                new LanguageOption("en", UiText.Get("English"))
            ]);
            _language.SelectedIndex = UiText.IsChinese ? 0 : 1;
            _startWithWindows.Checked = _settings.StartWithWindows;
            _resumeMonitoring.Checked = _settings.ResumeMonitoring;
            _alertSound.Checked = _settings.AlertSound;
            _logLimit.Value = Math.Clamp(
                _settings.LogLimitMb,
                AppSettings.MinimumLogLimitMb,
                AppSettings.MaximumLogLimitMb);
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
            _notifyIcon.Icon = SystemIcons.Shield;
            _notifyIcon.Text = Truncate(UiText.Get("TrayNormal"));
        }
        else
        {
            _notifyIcon.Icon = SystemIcons.Application;
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
            _notifyIcon.Dispose();
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

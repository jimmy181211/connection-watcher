using ConnectionWatcher.App.Localization;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.App.UI;

public sealed class UrgentAlertForm : Form
{
    private readonly Label _rulesValue = NewValueLabel();
    private readonly Label _remoteValue = NewValueLabel();
    private readonly Label _localValue = NewValueLabel();
    private readonly Label _programValue = NewValueLabel();
    private readonly Label _firstValue = NewValueLabel();
    private readonly Label _latestValue = NewValueLabel();
    private readonly Label _countValue = NewValueLabel();
    private readonly Label _notice = new();
    private readonly Dictionary<string, Label> _labels = [];
    private readonly HashSet<Guid> _ruleIds = [];
    private DateTimeOffset _firstSeen;
    private int _count;

    public UrgentAlertForm(ConnectionEvent firstEvent)
    {
        BuildInterface();
        AddEvent(firstEvent);
        ApplyLanguage();
    }

    public event EventHandler? ViewDetailsRequested;

    public IReadOnlyCollection<Guid> RuleIds => _ruleIds;

    public void AddEvent(ConnectionEvent connectionEvent)
    {
        if (_count == 0)
        {
            _firstSeen = connectionEvent.DetectedAt;
        }

        _count++;
        foreach (Guid id in connectionEvent.RuleIds)
        {
            _ruleIds.Add(id);
        }

        _rulesValue.Text = string.Join(" | ", connectionEvent.RuleNames);
        _remoteValue.Text = $"{connectionEvent.RemoteAddress}:{connectionEvent.RemotePort}";
        _localValue.Text = $"{connectionEvent.LocalAddress}:{connectionEvent.LocalPort}";
        _programValue.Text = $"{connectionEvent.ProcessName} (PID {connectionEvent.ProcessId})";
        _firstValue.Text = _firstSeen.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        _latestValue.Text = connectionEvent.DetectedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");
        _countValue.Text = $"{_count} {UiText.Get("Times")}";
    }

    private static Label NewValueLabel()
    {
        return new Label
        {
            AutoSize = true,
            MaximumSize = new Size(310, 0),
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold)
        };
    }

    private void BuildInterface()
    {
        StartPosition = FormStartPosition.Manual;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        TopMost = true;
        ShowInTaskbar = false;
        ClientSize = new Size(500, 350);
        Font = new Font("Segoe UI", 9.5F);

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = 9
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 125));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Label heading = new()
        {
            Name = "Heading",
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            ForeColor = Color.Firebrick,
            Margin = new Padding(0, 0, 0, 14)
        };
        layout.Controls.Add(heading, 0, 0);
        layout.SetColumnSpan(heading, 2);

        AddRow(layout, "RulesLabel", _rulesValue, 1);
        AddRow(layout, "RemoteLabel", _remoteValue, 2);
        AddRow(layout, "LocalLabel", _localValue, 3);
        AddRow(layout, "ProgramLabel", _programValue, 4);
        AddRow(layout, "FirstLabel", _firstValue, 5);
        AddRow(layout, "LatestLabel", _latestValue, 6);
        AddRow(layout, "CountLabel", _countValue, 7);

        FlowLayoutPanel footer = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true
        };
        _notice.AutoSize = true;
        _notice.MaximumSize = new Size(450, 0);
        _notice.ForeColor = Color.Firebrick;
        FlowLayoutPanel buttons = new()
        {
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            Width = 450,
            Margin = new Padding(0, 14, 0, 0)
        };
        Button details = new() { Name = "DetailsButton", AutoSize = true, MinimumSize = new Size(110, 34) };
        Button close = new() { Name = "CloseButton", AutoSize = true, MinimumSize = new Size(90, 34) };
        details.Click += (_, _) => ViewDetailsRequested?.Invoke(this, EventArgs.Empty);
        close.Click += (_, _) => Close();
        buttons.Controls.AddRange([details, close]);
        footer.Controls.Add(_notice);
        footer.Controls.Add(buttons);
        layout.Controls.Add(footer, 0, 8);
        layout.SetColumnSpan(footer, 2);

        Controls.Add(layout);
        Shown += (_, _) => PositionAtBottomRight();
    }

    private void AddRow(TableLayoutPanel layout, string name, Label value, int row)
    {
        Label label = new() { AutoSize = true, Margin = new Padding(0, 6, 8, 6) };
        _labels[name] = label;
        value.Margin = new Padding(0, 6, 0, 6);
        layout.Controls.Add(label, 0, row);
        layout.Controls.Add(value, 1, row);
    }

    private void ApplyLanguage()
    {
        Text = UiText.Get("UrgentTitle");
        Controls.Find("Heading", true).Single().Text = UiText.Get("UrgentTitle");
        _labels["RulesLabel"].Text = UiText.Get("MatchedRules");
        _labels["RemoteLabel"].Text = UiText.Get("RemoteEndpoint");
        _labels["LocalLabel"].Text = UiText.Get("LocalEndpoint");
        _labels["ProgramLabel"].Text = UiText.Get("Program");
        _labels["FirstLabel"].Text = UiText.Get("FirstSeen");
        _labels["LatestLabel"].Text = UiText.Get("LatestSeen");
        _labels["CountLabel"].Text = UiText.Get("Occurrences");
        _notice.Text = UiText.Get("NotMalwareVerdict");
        Controls.Find("DetailsButton", true).Single().Text = UiText.Get("ViewDetails");
        Controls.Find("CloseButton", true).Single().Text = UiText.Get("Close");
        _countValue.Text = $"{_count} {UiText.Get("Times")}";
    }

    private void PositionAtBottomRight()
    {
        Rectangle area = Screen.FromControl(this).WorkingArea;
        Location = new Point(area.Right - Width - 16, area.Bottom - Height - 16);
    }
}

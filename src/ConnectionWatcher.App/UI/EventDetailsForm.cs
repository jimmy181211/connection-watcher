using System.Runtime.InteropServices;
using System.Text;
using ConnectionWatcher.App.Localization;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.App.UI;

public sealed class EventDetailsForm : Form
{
    private static readonly string[] FieldKeys =
    [
        "StartTime", "EndTime", "ConnectionStatus", "ObservedDuration",
        "MatchedRules", "RemoteEndpoint", "LocalEndpoint", "TcpState",
        "PID", "Program", "ProcessPath", "ActionColumn"
    ];

    private readonly ConnectionEvent _entry;
    private readonly Icon _applicationIcon = AppIconProvider.Load();
    private readonly Dictionary<string, Label> _labels = [];
    private readonly Dictionary<string, TextBox> _values = [];
    private readonly Label _heading = new() { Name = "EventDetailsHeading" };
    private readonly Label _description = new() { Name = "EventDetailsDescription" };
    private readonly Button _copyButton = new()
    {
        Name = "CopyDetailsButton",
        AutoSize = true,
        MinimumSize = new Size(130, 34)
    };
    private readonly Button _closeButton = new()
    {
        Name = "CloseButton",
        AutoSize = true,
        MinimumSize = new Size(90, 34)
    };
    private readonly System.Windows.Forms.Timer _refreshTimer = new()
    {
        Interval = 1000
    };
    private Font? _statusFont;

    public EventDetailsForm(ConnectionEvent entry)
    {
        _entry = entry;
        Icon = (Icon)_applicationIcon.Clone();
        BuildInterface();
        ApplyLanguage();
        UiFont.Apply(this);
        _statusFont = UiFont.Create(
            _values["ConnectionStatus"].Font.Size,
            FontStyle.Bold);
        UpdateValues();
        _refreshTimer.Tick += (_, _) => UpdateValues();
        _refreshTimer.Start();
    }

    private void BuildInterface()
    {
        StartPosition = FormStartPosition.CenterParent;
        ShowInTaskbar = false;
        MinimizeBox = false;
        MaximizeBox = false;
        MinimumSize = new Size(760, 680);
        ClientSize = new Size(780, 650);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 1,
            RowCount = 3
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 1,
            RowCount = 2,
            Margin = new Padding(0, 0, 0, 14)
        };
        _heading.AutoSize = true;
        _heading.Font = new Font("Segoe UI", 18F, FontStyle.Regular);
        _heading.Margin = new Padding(0, 0, 0, 4);
        _description.AutoSize = true;
        _description.ForeColor = SystemColors.GrayText;
        _description.MaximumSize = new Size(700, 0);
        header.Controls.Add(_heading, 0, 0);
        header.Controls.Add(_description, 0, 1);

        TableLayoutPanel details = new()
        {
            Name = "EventDetailsTable",
            Dock = DockStyle.Fill,
            AutoScroll = true,
            ColumnCount = 2,
            RowCount = FieldKeys.Length + 1,
            Padding = new Padding(0, 2, 8, 2)
        };
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        details.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (int index = 0; index < FieldKeys.Length; index++)
        {
            string key = FieldKeys[index];
            bool multiline = key == "MatchedRules";
            AddField(details, key, index, multiline);
        }
        details.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        TableLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            ColumnCount = 3,
            RowCount = 1,
            Margin = new Padding(0, 14, 0, 0)
        };
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        buttons.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        _copyButton.Click += (_, _) => CopyDetails();
        _closeButton.Click += (_, _) => Close();
        buttons.Controls.Add(_copyButton, 1, 0);
        buttons.Controls.Add(_closeButton, 2, 0);

        root.Controls.Add(header, 0, 0);
        root.Controls.Add(details, 0, 1);
        root.Controls.Add(buttons, 0, 2);
        Controls.Add(root);
    }

    private void AddField(
        TableLayoutPanel details,
        string key,
        int row,
        bool multiline)
    {
        Label label = new()
        {
            Name = $"{key}Label",
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            Margin = new Padding(0, 6, 12, 6)
        };
        TextBox value = new()
        {
            Name = $"{key}Value",
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.FixedSingle,
            Multiline = multiline,
            WordWrap = multiline,
            ScrollBars = multiline ? ScrollBars.Vertical : ScrollBars.None,
            MinimumSize = new Size(0, multiline ? 52 : 27),
            Margin = new Padding(0, 4, 0, 4)
        };
        _labels[key] = label;
        _values[key] = value;
        details.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        details.Controls.Add(label, 0, row);
        details.Controls.Add(value, 1, row);
    }

    private void ApplyLanguage()
    {
        Text = UiText.Get("EventDetailsTitle");
        _heading.Text = UiText.Get("EventDetailsTitle");
        _description.Text = UiText.Get("EventDetailsDescription");
        foreach (string key in FieldKeys)
        {
            _labels[key].Text = UiText.Get(key);
        }
        _copyButton.Text = UiText.Get("CopyDetails");
        _closeButton.Text = UiText.Get("Close");
    }

    private void UpdateValues()
    {
        DateTimeOffset now = DateTimeOffset.Now;
        _values["StartTime"].Text = FormatTime(_entry.DetectedAt);
        _values["EndTime"].Text = _entry.EndedAt is DateTimeOffset ended
            ? FormatTime(ended)
            : "—";
        _values["ConnectionStatus"].Text = FormatStatus(_entry);
        _values["ObservedDuration"].Text = FormatDuration(_entry, now);
        _values["MatchedRules"].Text = string.Join(" | ", _entry.RuleNames);
        _values["RemoteEndpoint"].Text = FormatEndpoint(
            _entry.RemoteAddress,
            _entry.RemotePort);
        _values["LocalEndpoint"].Text = FormatEndpoint(
            _entry.LocalAddress,
            _entry.LocalPort);
        _values["TcpState"].Text = _entry.State.ToString();
        _values["PID"].Text = _entry.ProcessId.ToString();
        _values["Program"].Text = _entry.ProcessName;
        _values["ProcessPath"].Text = _entry.ProcessPath ??
            UiText.Get("ProcessPathUnavailable");
        _values["ActionColumn"].Text =
            $"{UiText.ActionMarker(_entry.Action)}  {UiText.Action(_entry.Action)}";

        TextBox status = _values["ConnectionStatus"];
        status.ForeColor = _entry.IsActive ? Color.SeaGreen : Color.DimGray;
        status.Font = _statusFont;
    }

    private void CopyDetails()
    {
        try
        {
            Clipboard.SetText(BuildCopyText());
            _copyButton.Text = UiText.Get("Copied");
        }
        catch (ExternalException)
        {
            MessageBox.Show(
                this,
                UiText.Get("CopyFailed"),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private string BuildCopyText()
    {
        StringBuilder text = new();
        text.AppendLine(UiText.Get("EventDetailsTitle"));
        foreach (string key in FieldKeys)
        {
            text.Append(UiText.Get(key));
            text.Append(": ");
            text.AppendLine(_values[key].Text);
        }
        return text.ToString().TrimEnd();
    }

    private static string FormatTime(DateTimeOffset value) =>
        value.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss");

    private static string FormatEndpoint(string address, int port) =>
        $"{address}:{port}";

    private static string FormatStatus(ConnectionEvent entry) =>
        $"{(entry.IsActive ? '●' : '○')} " +
        UiText.Get(entry.IsActive ? "ConnectionActive" : "ConnectionEnded");

    private static string FormatDuration(
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

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _refreshTimer.Stop();
            _refreshTimer.Dispose();
            _statusFont?.Dispose();
            _applicationIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}

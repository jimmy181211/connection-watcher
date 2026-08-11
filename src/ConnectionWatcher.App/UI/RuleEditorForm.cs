using ConnectionWatcher.App.Localization;
using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Rules;

namespace ConnectionWatcher.App.UI;

public sealed class RuleEditorForm : Form
{
    private readonly TextBox _name = new();
    private readonly ComboBox _type = NewDropDown();
    private readonly TextBox _remoteIp = new();
    private readonly CheckBox _anyRemoteIp = new();
    private readonly TextBox _remotePort = new();
    private readonly CheckBox _anyRemotePort = new();
    private readonly TextBox _localPort = new();
    private readonly CheckBox _anyLocalPort = new();
    private readonly ComboBox _action = NewDropDown();
    private readonly ComboBox _repeat = NewDropDown();
    private readonly CheckBox _enabled = new();
    private readonly Label _preview = new();
    private readonly Label _error = new();
    private readonly Label _remoteIpLabel = new();
    private readonly Label _remotePortLabel = new();
    private readonly Label _localPortLabel = new();
    private readonly Label _repeatLabel = new();
    private readonly MonitoringRule _original;

    public RuleEditorForm(MonitoringRule? existing = null)
    {
        _original = existing?.Copy() ?? new MonitoringRule();
        ResultRule = _original.Copy();
        BuildInterface();
        LoadRule(_original);
        ApplyLocalizedText(existing is null);
        UpdateState();
    }

    public MonitoringRule ResultRule { get; private set; }

    private static ComboBox NewDropDown()
    {
        return new ComboBox
        {
            DropDownStyle = ComboBoxStyle.DropDownList,
            Dock = DockStyle.Fill
        };
    }

    private void BuildInterface()
    {
        StartPosition = FormStartPosition.CenterParent;
        MinimizeBox = false;
        MaximizeBox = false;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(680, 620);
        ClientSize = new Size(720, 680);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;
        SizeGripStyle = SizeGripStyle.Show;

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(18),
            ColumnCount = 2,
            RowCount = 10,
            AutoScroll = true,
            GrowStyle = TableLayoutPanelGrowStyle.FixedSize
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50));
        for (int row = 0; row < 7; row++)
        {
            layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        }
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _anyRemoteIp.AutoSize = true;
        _anyRemotePort.AutoSize = true;
        _anyLocalPort.AutoSize = true;
        _enabled.AutoSize = true;

        AddField(layout, new Label { Name = "NameLabel" }, _name, 0, 0, 2);
        AddField(layout, new Label { Name = "TypeLabel" }, _type, 1, 0);
        AddField(layout, new Label { Name = "ActionLabel" }, _action, 1, 1);
        AddField(layout, _remoteIpLabel, _remoteIp, 2, 0);
        AddField(layout, _remotePortLabel, _remotePort, 2, 1);
        layout.Controls.Add(_anyRemoteIp, 0, 3);
        layout.Controls.Add(_anyRemotePort, 1, 3);
        AddField(layout, _localPortLabel, _localPort, 4, 0);
        AddField(layout, _repeatLabel, _repeat, 4, 1);
        layout.Controls.Add(_anyLocalPort, 0, 5);
        layout.Controls.Add(_enabled, 1, 5);

        Label portHint = new()
        {
            Name = "PortHint",
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            Margin = new Padding(3, 8, 3, 8)
        };
        layout.Controls.Add(portHint, 0, 6);
        layout.SetColumnSpan(portHint, 2);

        GroupBox previewGroup = new()
        {
            Name = "PreviewGroup",
            Dock = DockStyle.Fill,
            Padding = new Padding(12),
            MinimumSize = new Size(0, 100)
        };
        _preview.Dock = DockStyle.Fill;
        _preview.AutoEllipsis = false;
        _preview.TextAlign = ContentAlignment.TopLeft;
        previewGroup.Controls.Add(_preview);
        layout.Controls.Add(previewGroup, 0, 7);
        layout.SetColumnSpan(previewGroup, 2);

        _error.AutoSize = true;
        _error.Dock = DockStyle.Fill;
        _error.ForeColor = Color.Firebrick;
        _error.Margin = new Padding(3, 8, 3, 8);
        layout.Controls.Add(_error, 0, 8);
        layout.SetColumnSpan(_error, 2);

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true,
            WrapContents = true,
            Padding = new Padding(0, 8, 0, 0)
        };
        Button save = new() { Name = "SaveButton", AutoSize = true, MinimumSize = new Size(90, 34) };
        Button cancel = new() { Name = "CancelButton", AutoSize = true, MinimumSize = new Size(90, 34) };
        save.Click += SaveClicked;
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.AddRange([save, cancel]);
        layout.Controls.Add(buttons, 0, 9);
        layout.SetColumnSpan(buttons, 2);

        foreach (Control control in new Control[]
                 {
                     _name, _type, _remoteIp, _anyRemoteIp, _remotePort,
                     _anyRemotePort, _localPort, _anyLocalPort, _action, _repeat
                 })
        {
            if (control is TextBox textBox)
            {
                textBox.TextChanged += (_, _) => UpdateState();
            }
            else if (control is CheckBox checkBox)
            {
                checkBox.CheckedChanged += (_, _) => UpdateState();
            }
            else if (control is ComboBox comboBox)
            {
                comboBox.SelectedIndexChanged += (_, _) => UpdateState();
            }
        }

        Controls.Add(layout);
        AcceptButton = save;
        CancelButton = cancel;
    }

    private static void AddField(
        TableLayoutPanel layout,
        Label label,
        Control field,
        int row,
        int column,
        int span = 1)
    {
        TableLayoutPanel container = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Margin = new Padding(3, 4, 10, 8),
            ColumnCount = 1,
            RowCount = 2
        };
        container.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        container.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        label.AutoSize = true;
        label.Dock = DockStyle.Fill;
        field.Dock = DockStyle.Top;
        field.MinimumSize = new Size(180, 30);
        container.Controls.Add(label, 0, 0);
        container.Controls.Add(field, 0, 1);
        layout.Controls.Add(container, column, row);
        if (span > 1)
        {
            layout.SetColumnSpan(container, span);
        }
    }

    private void LoadRule(MonitoringRule rule)
    {
        _name.Text = rule.Name;
        _type.Items.AddRange(
        [
            new Option<MonitoringRuleType>(MonitoringRuleType.TcpConnection, UiText.Get("TcpConnection")),
            new Option<MonitoringRuleType>(MonitoringRuleType.LocalListener, UiText.Get("LocalListener"))
        ]);
        SelectValue(_type, rule.Type);

        _action.Items.AddRange(
        [
            new Option<MatchAction>(MatchAction.SilentLog, UiText.Get("SilentLog")),
            new Option<MatchAction>(MatchAction.TrayNotice, UiText.Get("TrayNotice")),
            new Option<MatchAction>(MatchAction.PopupAlert, UiText.Get("PopupAlert"))
        ]);
        SelectValue(_action, rule.Action);

        _repeat.Items.AddRange(
        [
            new Option<int>(0, UiText.Get("EveryTime")),
            new Option<int>(1, UiText.Get("OneMinute")),
            new Option<int>(5, UiText.Get("FiveMinutes")),
            new Option<int>(15, UiText.Get("FifteenMinutes"))
        ]);
        SelectValue(_repeat, rule.RepeatAlertMinutes);

        _anyRemoteIp.Checked = string.IsNullOrWhiteSpace(rule.RemoteIp);
        _remoteIp.Text = rule.RemoteIp ?? string.Empty;
        _anyRemotePort.Checked = rule.RemotePort.IsAny;
        _remotePort.Text = rule.RemotePort.IsAny ? string.Empty : rule.RemotePort.ToString();
        _anyLocalPort.Checked = rule.LocalPort.IsAny;
        _localPort.Text = rule.LocalPort.IsAny ? string.Empty : rule.LocalPort.ToString();
        _enabled.Checked = rule.Enabled;
    }

    private void ApplyLocalizedText(bool isNew)
    {
        Text = UiText.Get(isNew ? "CreateRule" : "EditRule");
        Find("NameLabel").Text = UiText.Get("Name");
        Find("TypeLabel").Text = UiText.Get("RuleType");
        Find("ActionLabel").Text = UiText.Get("MatchAction");
        _remoteIpLabel.Text = UiText.Get("RemoteIp");
        _remotePortLabel.Text = UiText.Get("RemotePort");
        _localPortLabel.Text = UiText.Get("LocalPort");
        _repeatLabel.Text = UiText.Get("RepeatInterval");
        _anyRemoteIp.Text = UiText.Get("AnyRemoteIpCheck");
        _anyRemotePort.Text = UiText.Get("AnyRemotePortCheck");
        _anyLocalPort.Text = UiText.Get("AnyLocalPortCheck");
        _enabled.Text = UiText.Get("EnableAfterSave");
        Find("PortHint").Text = UiText.Get("PortFormat");
        Find("PreviewGroup").Text = UiText.Get("RulePreview");
        Find("SaveButton").Text = UiText.Get("Save");
        Find("CancelButton").Text = UiText.Get("Cancel");
    }

    private Control Find(string name)
    {
        return Controls.Find(name, searchAllChildren: true).Single();
    }

    private void UpdateState()
    {
        if (!IsHandleCreated && Controls.Count == 0)
        {
            return;
        }

        MonitoringRuleType type = Selected(_type, MonitoringRuleType.TcpConnection);
        bool listener = type == MonitoringRuleType.LocalListener;
        if (listener)
        {
            _anyRemoteIp.Checked = true;
            _anyRemotePort.Checked = true;
            _anyLocalPort.Checked = false;
        }

        _anyRemoteIp.Enabled = !listener;
        _anyRemotePort.Enabled = !listener;
        _remoteIp.Enabled = !listener && !_anyRemoteIp.Checked;
        _remotePort.Enabled = !listener && !_anyRemotePort.Checked;
        _anyLocalPort.Enabled = !listener;
        _localPort.Enabled = listener || !_anyLocalPort.Checked;
        _repeat.Enabled = Selected(_action, MatchAction.SilentLog) == MatchAction.PopupAlert;

        if (TryBuildRule(out MonitoringRule? rule, showErrors: false))
        {
            string condition = UiText.FormatRuleCondition(rule);
            _preview.Text = rule.Type == MonitoringRuleType.LocalListener
                ? string.Format(UiText.Get("PreviewListener"), UiText.FormatPort(rule.LocalPort), UiText.Action(rule.Action))
                : string.Format(UiText.Get("PreviewConnection"), condition, UiText.Action(rule.Action));
        }
        else
        {
            _preview.Text = UiText.Get("ConditionRequired");
        }
    }

    private void SaveClicked(object? sender, EventArgs e)
    {
        if (!TryBuildRule(out MonitoringRule? rule, showErrors: true))
        {
            return;
        }

        ResultRule = rule;
        DialogResult = DialogResult.OK;
        Close();
    }

    private bool TryBuildRule(out MonitoringRule rule, bool showErrors)
    {
        List<string> errors = [];
        bool listener = Selected(_type, MonitoringRuleType.TcpConnection) ==
            MonitoringRuleType.LocalListener;

        if (!PortRange.TryParse(_remotePort.Text, listener || _anyRemotePort.Checked, out PortRange remotePort))
        {
            errors.Add(UiText.Get("InvalidRemotePort"));
        }

        if (!PortRange.TryParse(_localPort.Text, !listener && _anyLocalPort.Checked, out PortRange localPort))
        {
            errors.Add(UiText.Get("InvalidLocalPort"));
        }

        rule = new MonitoringRule
        {
            Id = _original.Id,
            Name = _name.Text.Trim(),
            Type = listener ? MonitoringRuleType.LocalListener : MonitoringRuleType.TcpConnection,
            RemoteIp = listener || _anyRemoteIp.Checked ? null : _remoteIp.Text.Trim(),
            RemotePort = remotePort,
            LocalPort = localPort,
            Action = Selected(_action, MatchAction.SilentLog),
            RepeatAlertMinutes = Selected(_repeat, 5),
            Enabled = _enabled.Checked
        };

        foreach (RuleValidationError error in RuleValidator.Validate(rule))
        {
            errors.Add(UiText.Get(error switch
            {
                RuleValidationError.NameRequired => "NameRequired",
                RuleValidationError.InvalidRemoteIp => "InvalidRemoteIp",
                RuleValidationError.InvalidRemotePort => "InvalidRemotePort",
                RuleValidationError.InvalidLocalPort => "InvalidLocalPort",
                RuleValidationError.AtLeastOneConditionRequired => "ConditionRequired",
                RuleValidationError.LocalListenerPortRequired => "ListenerPortRequired",
                _ => "InvalidRepeatInterval"
            }));
        }

        if (showErrors)
        {
            _error.Text = string.Join(Environment.NewLine, errors.Distinct());
        }

        return errors.Count == 0;
    }

    private static T Selected<T>(ComboBox combo, T fallback)
    {
        return combo.SelectedItem is Option<T> option ? option.Value : fallback;
    }

    private static void SelectValue<T>(ComboBox combo, T value)
    {
        for (int index = 0; index < combo.Items.Count; index++)
        {
            if (combo.Items[index] is Option<T> option &&
                EqualityComparer<T>.Default.Equals(option.Value, value))
            {
                combo.SelectedIndex = index;
                return;
            }
        }

        combo.SelectedIndex = 0;
    }

    private sealed record Option<T>(T Value, string Label)
    {
        public override string ToString() => Label;
    }
}

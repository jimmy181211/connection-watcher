using System.Diagnostics;
using System.Text;
using ConnectionWatcher.App.Localization;

namespace ConnectionWatcher.App.UI;

public sealed class FeedbackForm : Form
{
    private const string GitHubIssueUrl =
        "https://github.com/jimmy181211/connection-watcher/issues/new";

    private readonly TextBox _feedbackText = new();
    private readonly CheckBox _includeDiagnostics = new();
    private readonly Label _description = new();
    private readonly Label _feedbackLabel = new();
    private readonly Label _diagnosticsHint = new();
    private readonly Button _continueButton = new();

    public FeedbackForm()
    {
        Icon = AppIconProvider.Load();
        BuildInterface();
        ApplyLanguage();
        UiFont.Apply(this);
    }

    private void BuildInterface()
    {
        Text = UiText.Get("FeedbackTitle");
        StartPosition = FormStartPosition.CenterParent;
        MinimumSize = new Size(620, 470);
        ClientSize = new Size(720, 540);
        AutoScaleMode = AutoScaleMode.Dpi;
        Font = new Font("Segoe UI", 9.5F);

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(20),
            ColumnCount = 1,
            RowCount = 5
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _description.AutoSize = true;
        _description.MaximumSize = new Size(660, 0);
        _description.Margin = new Padding(0, 0, 0, 16);

        _feedbackLabel.AutoSize = true;
        _feedbackLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _feedbackLabel.Margin = new Padding(0, 0, 0, 6);

        _feedbackText.Multiline = true;
        _feedbackText.ScrollBars = ScrollBars.Vertical;
        _feedbackText.MaxLength = 6000;
        _feedbackText.Dock = DockStyle.Fill;
        _feedbackText.MinimumSize = new Size(0, 190);
        _feedbackText.Margin = new Padding(0, 0, 0, 12);

        FlowLayoutPanel diagnostics = new()
        {
            AutoSize = true,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 16)
        };
        _includeDiagnostics.AutoSize = true;
        _diagnosticsHint.AutoSize = true;
        _diagnosticsHint.MaximumSize = new Size(660, 0);
        _diagnosticsHint.Margin = new Padding(24, 2, 0, 0);
        diagnostics.Controls.Add(_includeDiagnostics);
        diagnostics.Controls.Add(_diagnosticsHint);

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            WrapContents = false
        };
        Button cancel = new()
        {
            AutoSize = true,
            MinimumSize = new Size(100, 36),
            DialogResult = DialogResult.Cancel
        };
        _continueButton.AutoSize = true;
        _continueButton.MinimumSize = new Size(150, 36);
        _continueButton.Click += ContinueToGitHub;
        buttons.Controls.Add(cancel);
        buttons.Controls.Add(_continueButton);

        root.Controls.Add(_description, 0, 0);
        root.Controls.Add(_feedbackLabel, 0, 1);
        root.Controls.Add(_feedbackText, 0, 2);
        root.Controls.Add(diagnostics, 0, 3);
        root.Controls.Add(buttons, 0, 4);
        Controls.Add(root);

        CancelButton = cancel;
        AcceptButton = _continueButton;
    }

    private void ApplyLanguage()
    {
        Text = UiText.Get("FeedbackTitle");
        _description.Text = UiText.Get("FeedbackDescription");
        _feedbackLabel.Text = UiText.Get("FeedbackTextLabel");
        _feedbackText.PlaceholderText = UiText.Get("FeedbackPlaceholder");
        _includeDiagnostics.Text = UiText.Get("IncludeDiagnostics");
        _diagnosticsHint.Text = UiText.Get("DiagnosticsHint");
        _continueButton.Text = UiText.Get("ContinueToGitHub");
        if (CancelButton is Button cancel)
        {
            cancel.Text = UiText.Get("Cancel");
        }
    }

    private void ContinueToGitHub(object? sender, EventArgs e)
    {
        string feedback = _feedbackText.Text.Trim();
        if (string.IsNullOrWhiteSpace(feedback))
        {
            MessageBox.Show(
                this,
                UiText.Get("FeedbackEmpty"),
                UiText.Get("FeedbackTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            _feedbackText.Focus();
            return;
        }

        StringBuilder body = new();
        body.AppendLine("## Feedback");
        body.AppendLine();
        body.AppendLine(feedback);
        if (_includeDiagnostics.Checked)
        {
            body.AppendLine();
            body.AppendLine("## Basic diagnostic information");
            body.AppendLine();
            body.AppendLine($"- App version: {CurrentVersion()}");
            body.AppendLine($"- Windows version: {Environment.OSVersion.VersionString}");
            body.AppendLine($"- Interface language: {UiText.Language}");
            body.AppendLine("- Logs and network connection records: not attached");
        }

        string title = $"SocketSight feedback ({CurrentVersion()})";
        string url =
            $"{GitHubIssueUrl}?title={Uri.EscapeDataString(title)}" +
            $"&body={Uri.EscapeDataString(body.ToString())}";
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            });
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                string.Format(UiText.Get("FeedbackOpenFailed"), exception.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }

    private static Version CurrentVersion() =>
        typeof(FeedbackForm).Assembly.GetName().Version ?? new Version(1, 0, 0);
}

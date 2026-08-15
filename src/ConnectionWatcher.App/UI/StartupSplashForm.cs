using ConnectionWatcher.App.Localization;

namespace ConnectionWatcher.App.UI;

public sealed class StartupSplashForm : Form
{
    private readonly IReadOnlyList<string> _messages;
    private readonly Label _message = new()
    {
        Name = "StartupMessage",
        AutoSize = false,
        TextAlign = ContentAlignment.MiddleCenter,
        ForeColor = Color.FromArgb(74, 85, 104),
        Dock = DockStyle.Fill
    };
    private readonly System.Windows.Forms.Timer _messageTimer = new()
    {
        Interval = 2300
    };
    private int _messageIndex;
    private readonly Icon _icon;
    private readonly Bitmap _iconBitmap;

    public StartupSplashForm(StartupPresentation presentation, string language)
    {
        _messages = presentation.Messages;
        _icon = AppIconProvider.Load();
        using Icon largeIcon = new(_icon, 72, 72);
        _iconBitmap = largeIcon.ToBitmap();

        Name = "StartupSplashForm";
        Text = "SocketSight";
        Icon = _icon;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.None;
        ShowInTaskbar = false;
        TopMost = true;
        ClientSize = new Size(560, 270);
        BackColor = Color.FromArgb(247, 249, 252);
        Padding = new Padding(1);
        Font = new Font(FontFamily(language), 9F, FontStyle.Regular);

        Panel border = new()
        {
            Dock = DockStyle.Fill,
            BackColor = Color.FromArgb(210, 220, 232),
            Padding = new Padding(1)
        };
        Panel surface = new()
        {
            Dock = DockStyle.Fill,
            BackColor = BackColor,
            Padding = new Padding(34, 25, 34, 22)
        };
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            BackColor = BackColor
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 78));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 43));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 32));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 10));

        PictureBox logo = new()
        {
            Name = "StartupLogo",
            Image = _iconBitmap,
            SizeMode = PictureBoxSizeMode.CenterImage,
            Dock = DockStyle.Fill
        };
        Label brand = new()
        {
            Name = "StartupBrand",
            Text = "SocketSight",
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.MiddleCenter,
            ForeColor = Color.FromArgb(17, 43, 73),
            Font = new Font(FontFamily(language), 23F, FontStyle.Bold)
        };
        Label tagline = new()
        {
            Name = "StartupTagline",
            Text = presentation.Tagline,
            Dock = DockStyle.Fill,
            TextAlign = ContentAlignment.TopCenter,
            ForeColor = Color.FromArgb(45, 111, 157),
            Font = new Font(FontFamily(language), 10.5F, FontStyle.Regular)
        };
        _message.Font = new Font(FontFamily(language), 10F, FontStyle.Regular);
        _message.Text = _messages.FirstOrDefault() ?? string.Empty;
        ProgressBar progress = new()
        {
            Name = "StartupProgress",
            Dock = DockStyle.Fill,
            Style = ProgressBarStyle.Marquee,
            MarqueeAnimationSpeed = 28
        };

        layout.Controls.Add(logo, 0, 0);
        layout.Controls.Add(brand, 0, 1);
        layout.Controls.Add(tagline, 0, 2);
        layout.Controls.Add(_message, 0, 3);
        layout.Controls.Add(progress, 0, 4);
        surface.Controls.Add(layout);
        border.Controls.Add(surface);
        Controls.Add(border);

        _messageTimer.Tick += (_, _) => ShowNextMessage();
        _messageTimer.Start();
    }

    private static string FontFamily(string language) =>
        language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
            ? "Microsoft YaHei UI"
            : "Segoe UI";

    private void ShowNextMessage()
    {
        if (_messages.Count == 0)
        {
            return;
        }

        _messageIndex = (_messageIndex + 1) % _messages.Count;
        _message.Text = _messages[_messageIndex];
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _messageTimer.Stop();
            _messageTimer.Dispose();
            _iconBitmap.Dispose();
            _icon.Dispose();
        }

        base.Dispose(disposing);
    }
}

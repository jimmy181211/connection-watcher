using ConnectionWatcher.App.Localization;
using ConnectionWatcher.App.UI;
using ConnectionWatcher.Core.Configuration;
using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;

namespace ConnectionWatcher.UiSmoke;

internal static class Program
{
    [STAThread]
    private static int Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        string output = args.Length > 0 ? args[0] : Path.Combine(Path.GetTempPath(), "ConnectionWatcherUi");
        Directory.CreateDirectory(output);
        RenderMainWindow(output, "zh-CN", "zh");
        RenderMainWindow(output, "en", "en");
        RenderRuleEditor(output, "zh-CN", "zh");
        RenderRuleEditor(output, "en", "en");
        RenderUrgentAlert(output, "zh-CN", "zh");
        RenderUrgentAlert(output, "en", "en");
        RenderHelpCenter(output, "zh-CN", "zh");
        RenderHelpCenter(output, "en", "en");
        Console.WriteLine(output);
        return 0;
    }

    private static void RenderMainWindow(string output, string language, string filePrefix)
    {
        UiText.SetLanguage(language);
        string data = Path.Combine(output, "data-" + language);
        Directory.CreateDirectory(data);
        AppSettings settings = new()
        {
            Language = language,
            Rules = SampleRules()
        };
        SettingsStore store = new(data);
        store.Save(settings);
        CsvEventLogger logger = new(Path.Combine(data, "Logs"));
        using MainForm form = new(settings, store, logger)
        {
            Size = new Size(1100, 720),
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        Application.DoEvents();
        Thread.Sleep(100);
        Application.DoEvents();
        Capture(form, Path.Combine(output, $"main-{filePrefix}-rules.png"));
        if (language == "zh-CN")
        {
            ClickNavigation(form, UiText.Get("Home"));
            Capture(form, Path.Combine(output, "main-zh-home.png"));
            ClickNavigation(form, UiText.Get("Events"));
            Capture(form, Path.Combine(output, "main-zh-events.png"));
            ClickNavigation(form, UiText.Get("Settings"));
            Capture(form, Path.Combine(output, "main-zh-settings.png"));
        }
        form.Hide();
    }

    private static void ClickNavigation(Control root, string text)
    {
        Button button = Descendants(root)
            .OfType<Button>()
            .First(control => control.Text == text);
        button.PerformClick();
        Application.DoEvents();
    }

    private static IEnumerable<Control> Descendants(Control root)
    {
        foreach (Control child in root.Controls)
        {
            yield return child;
            foreach (Control descendant in Descendants(child))
            {
                yield return descendant;
            }
        }
    }

    private static void Capture(Form form, string path)
    {
        using Bitmap bitmap = new(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        bitmap.Save(path);
    }

    private static void RenderRuleEditor(string output, string language, string filePrefix)
    {
        UiText.SetLanguage(language);
        using RuleEditorForm form = new(SampleRules()[0])
        {
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        form.Size = form.MinimumSize;
        Application.DoEvents();
        AssertFullyVisible(form, form.Controls.Find("SaveButton", true).Single());
        AssertFullyVisible(form, form.Controls.Find("CancelButton", true).Single());
        using Bitmap bitmap = new(form.Width, form.Height);
        form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size));
        bitmap.Save(Path.Combine(output, $"rule-editor-{filePrefix}.png"));
        form.Hide();
    }

    private static void RenderUrgentAlert(string output, string language, string filePrefix)
    {
        UiText.SetLanguage(language);
        MonitoringRule rule = SampleRules()[0];
        ConnectionEvent entry = new()
        {
            DetectedAt = DateTimeOffset.Now,
            RuleIds = [rule.Id],
            RuleNames = [rule.Name],
            Action = MatchAction.PopupAlert,
            RepeatAlertMinutes = 5,
            LocalAddress = "172.20.10.2",
            LocalPort = 61659,
            RemoteAddress = "103.1.40.235",
            RemotePort = 1433,
            State = System.Net.NetworkInformation.TcpState.Established,
            ProcessId = 2480,
            ProcessName = "taskhostw.exe",
            ProcessPath = @"C:\Windows\System32\taskhostw.exe"
        };
        using UrgentAlertForm form = new(entry)
        {
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.AddEvent(entry);
        form.AddEvent(entry);
        form.Show();
        form.Size = form.MinimumSize;
        Application.DoEvents();
        AssertFullyVisible(form, form.Controls.Find("Notice", true).Single());
        AssertFullyVisible(form, form.Controls.Find("DetailsButton", true).Single());
        AssertFullyVisible(form, form.Controls.Find("CloseButton", true).Single());
        Capture(form, Path.Combine(output, $"urgent-alert-{filePrefix}.png"));
        form.Hide();
    }

    private static void RenderHelpCenter(string output, string language, string filePrefix)
    {
        UiText.SetLanguage(language);
        using HelpCenterForm form = new()
        {
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        form.Size = form.MinimumSize;
        Application.DoEvents();
        WebBrowser[] documents = Descendants(form).OfType<WebBrowser>().ToArray();
        for (int attempt = 0; attempt < 10 &&
             documents.Any(document => document.Document?.Body is null); attempt++)
        {
            Thread.Sleep(50);
            Application.DoEvents();
        }

        if (documents.Length != 2 || documents.Any(document =>
                (document.Document?.Body?.InnerText?.Length ?? 0) < 100))
        {
            throw new InvalidOperationException("Embedded help documents did not load.");
        }

        Capture(form, Path.Combine(output, $"help-center-{filePrefix}.png"));
        form.Hide();
    }

    private static void AssertFullyVisible(Form form, Control control)
    {
        Rectangle bounds = form.RectangleToClient(control.Parent!.RectangleToScreen(control.Bounds));
        if (!form.ClientRectangle.Contains(bounds))
        {
            throw new InvalidOperationException(
                $"Control '{control.Name}' is outside the visible client area: {bounds}.");
        }
    }

    private static List<MonitoringRule> SampleRules()
    {
        return
        [
            new MonitoringRule
            {
                Name = "UCSD报告的服务器",
                RemoteIp = "103.1.40.235",
                RemotePort = new PortRange(1433, 1433),
                LocalPort = PortRange.Any,
                Action = MatchAction.PopupAlert,
                RepeatAlertMinutes = 5,
                Enabled = true
            },
            new MonitoringRule
            {
                Name = "观察其他1433连接",
                RemoteIp = null,
                RemotePort = new PortRange(1433, 1433),
                LocalPort = PortRange.Any,
                Action = MatchAction.SilentLog,
                Enabled = true
            }
        ];
    }
}

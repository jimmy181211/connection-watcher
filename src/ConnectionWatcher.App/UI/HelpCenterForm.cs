using System.Diagnostics;
using System.Reflection;
using System.Text;
using ConnectionWatcher.App.Localization;
using Markdig;

namespace ConnectionWatcher.App.UI;

public sealed class HelpCenterForm : Form
{
    private static readonly MarkdownPipeline MarkdownPipeline =
        new MarkdownPipelineBuilder()
            .DisableHtml()
            .UseAdvancedExtensions()
            .Build();

    public HelpCenterForm()
    {
        Icon = AppIconProvider.Load();
        BuildInterface();
        UiFont.Apply(this);
    }

    private void BuildInterface()
    {
        Text = UiText.Get("HelpCenter");
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.Sizable;
        MinimumSize = new Size(680, 480);
        ClientSize = new Size(860, 640);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(14),
            ColumnCount = 1,
            RowCount = 2,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TabControl documents = new() { Dock = DockStyle.Fill };
        documents.TabPages.Add(
            CreateDocumentTab(UiText.Get("ProjectOverview"), "ProjectOverview")
        );
        documents.TabPages.Add(
            CreateDocumentTab(UiText.Get("UserGuide"), "UserGuide")
        );
        documents.TabPages.Add(
            CreateDocumentTab(UiText.Get("PrivacyPolicy"), "PrivacyPolicy")
        );
        documents.TabPages.Add(
            CreateDocumentTab(UiText.Get("TermsOfUse"), "TermsOfUse")
        );

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0),
        };
        Button close = new()
        {
            Text = UiText.Get("Close"),
            AutoSize = true,
            MinimumSize = new Size(90, 34),
        };
        close.Click += (_, _) => Close();
        buttons.Controls.Add(close);

        root.Controls.Add(documents, 0, 0);
        root.Controls.Add(buttons, 0, 1);
        Controls.Add(root);
        CancelButton = close;
    }

    private static TabPage CreateDocumentTab(string title, string documentName)
    {
        TabPage page = new(title) { Padding = new Padding(6) };
        WebBrowser viewer = new()
        {
            Name = "HelpDocument",
            Dock = DockStyle.Fill,
            AllowNavigation = true,
            IsWebBrowserContextMenuEnabled = true,
            ScriptErrorsSuppressed = true,
            WebBrowserShortcutsEnabled = true,
        };
        viewer.Navigating += ViewerNavigating;
        viewer.DocumentText = BuildHtml(LoadDocument(documentName));
        page.Controls.Add(viewer);
        return page;
    }

    /// <summary>
    /// Select the localized resource, fall back to English, then dispose the
    /// stream.
    /// </summary>
    private static string LoadDocument(string documentName)
    {
        string language = UiText.Language;
        string resourceName = (
            "ConnectionWatcher.Help."
            + $"{documentName}"
            + "."
            + $"{language}"
            + ".md"
        );
        Stream? stream = Assembly
            .GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);
        if (
            stream is null
            && !language.Equals("en", StringComparison.OrdinalIgnoreCase)
        )
        {
            resourceName = $"ConnectionWatcher.Help.{documentName}.en.md";
            stream = Assembly
                .GetExecutingAssembly()
                .GetManifestResourceStream(resourceName);
        }

        using Stream? documentStream = stream;

        if (stream is null)
        {
            return $"# {UiText.Get("HelpDocumentUnavailable")}";
        }

        using StreamReader reader = new(
            stream,
            Encoding.UTF8,
            detectEncodingFromByteOrderMarks: true
        );
        return reader.ReadToEnd();
    }

    private static string BuildHtml(string markdown)
    {
        string content = Markdown.ToHtml(markdown, MarkdownPipeline);
        string fontFamily = UiText.IsChinese
            ? ("\"Microsoft YaHei UI\", \"Microsoft " + "YaHei\", sans-serif")
            : "\"Segoe UI\", sans-serif";
        return (
            "<!DOCTYPE html>\r\n<html>\r\n<head>\r\n  <meta "
            + "charset=\"utf-8\">\r\n  <meta "
            + "http-equiv=\"X-UA-Compatible\" "
            + "content=\"IE=edge\">\r\n  <style>\r\n    html "
            + "{ background: #ffffff; }\r\n    body {\r\n  "
            + "    box-sizing: border-box;\r\n      "
            + "max-width: 940px;\r\n      margin: 0 "
            + "auto;\r\n      padding: 26px 30px 44px;\r\n "
            + "     color: #202124;\r\n      "
            + "background: #ffffff;\r\n      "
            + "font-family: "
            + $"{fontFamily}"
            + ";\r\n      font-size: 15px;\r\n      "
            + "line-height: 1.68;\r\n    }\r\n    h1, h2, "
            + "h3, h4 { color: #172b4d; line-height: "
            + "1.3; }\r\n    h1 {\r\n      margin: 0 0 "
            + "22px;\r\n      padding-bottom: 12px;\r\n    "
            + "  border-bottom: 2px solid #18a9c5;\r\n  "
            + "    font-size: 30px;\r\n    }\r\n    h2 {\r\n  "
            + "    margin: 32px 0 14px;\r\n      "
            + "padding-bottom: 7px;\r\n      "
            + "border-bottom: 1px solid #d9e2ec;\r\n    "
            + "  font-size: 23px;\r\n    }\r\n    h3 { "
            + "margin: 25px 0 10px; font-size: 18px; "
            + "}\r\n    h4 { margin: 20px 0 8px; "
            + "font-size: 16px; }\r\n    p { margin: "
            + "10px 0; }\r\n    ul, ol { margin: 10px "
            + "0; padding-left: 30px; }\r\n    li { "
            + "margin: 5px 0; }\r\n    strong { color: "
            + "#102a43; }\r\n    blockquote {\r\n      "
            + "margin: 16px 0;\r\n      padding: 10px "
            + "16px;\r\n      border-left: 4px solid "
            + "#18a9c5;\r\n      background: #f2f8fa;\r\n  "
            + "    color: #334e68;\r\n    }\r\n    code {\r\n "
            + "     padding: 2px 5px;\r\n      border: "
            + "1px solid #dbe4ea;\r\n      "
            + "border-radius: 4px;\r\n      background: "
            + "#f5f7f9;\r\n      color: #9c2f4d;\r\n      "
            + "font-family: Consolas, monospace;\r\n    "
            + "  font-size: 13px;\r\n    }\r\n    pre {\r\n   "
            + "   overflow: auto;\r\n      padding: "
            + "15px;\r\n      border: 1px solid "
            + "#d6e0e7;\r\n      border-radius: 6px;\r\n   "
            + "   background: #f5f7f9;\r\n      "
            + "line-height: 1.5;\r\n    }\r\n    pre code "
            + "{\r\n      padding: 0;\r\n      border: 0;\r\n "
            + "     background: transparent;\r\n      "
            + "color: #263238;\r\n    }\r\n    table { "
            + "width: 100%; border-collapse: "
            + "collapse; margin: 16px 0; }\r\n    th, "
            + "td { padding: 8px 10px; border: 1px "
            + "solid #cfd8df; text-align: left; }\r\n   "
            + " th { background: #edf5f7; }\r\n    hr { "
            + "border: 0; border-top: 1px solid "
            + "#d9e2ec; margin: 26px 0; }\r\n    a { "
            + "color: #087f9c; text-decoration: "
            + "none; }\r\n    a:hover { "
            + "text-decoration: underline; }\r\n  "
            + "</style>\r\n</head>\r\n<body>"
            + $"{content}"
            + "</body>\r\n</html>"
        );
    }

    private static void ViewerNavigating(
        object? sender,
        WebBrowserNavigatingEventArgs e
    )
    {
        Uri? url = e.Url;
        if (url is null || url.Scheme is not ("http" or "https"))
        {
            return;
        }

        e.Cancel = true;
        Process.Start(
            new ProcessStartInfo
            {
                FileName = url.AbsoluteUri,
                UseShellExecute = true,
            }
        );
    }
}

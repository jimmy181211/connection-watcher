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
            RowCount = 2
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        TabControl documents = new() { Dock = DockStyle.Fill };
        documents.TabPages.Add(CreateDocumentTab(
            UiText.Get("ProjectOverview"),
            "ProjectOverview"));
        documents.TabPages.Add(CreateDocumentTab(
            UiText.Get("UserGuide"),
            "UserGuide"));

        FlowLayoutPanel buttons = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            FlowDirection = FlowDirection.RightToLeft,
            Padding = new Padding(0, 10, 0, 0)
        };
        Button close = new()
        {
            Text = UiText.Get("Close"),
            AutoSize = true,
            MinimumSize = new Size(90, 34)
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
            WebBrowserShortcutsEnabled = true
        };
        viewer.Navigating += ViewerNavigating;
        viewer.DocumentText = BuildHtml(LoadDocument(documentName));
        page.Controls.Add(viewer);
        return page;
    }

    private static string LoadDocument(string documentName)
    {
        string language = UiText.Language;
        string resourceName = $"ConnectionWatcher.Help.{documentName}.{language}.md";
        using Stream? stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return $"# {UiText.Get("HelpDocumentUnavailable")}";
        }

        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static string BuildHtml(string markdown)
    {
        string content = Markdown.ToHtml(markdown, MarkdownPipeline);
        string fontFamily = UiText.IsChinese
            ? "\"Microsoft YaHei UI\", \"Microsoft YaHei\", sans-serif"
            : "\"Segoe UI\", sans-serif";
        return $$"""
            <!DOCTYPE html>
            <html>
            <head>
              <meta charset="utf-8">
              <meta http-equiv="X-UA-Compatible" content="IE=edge">
              <style>
                html { background: #ffffff; }
                body {
                  box-sizing: border-box;
                  max-width: 940px;
                  margin: 0 auto;
                  padding: 26px 30px 44px;
                  color: #202124;
                  background: #ffffff;
                  font-family: {{fontFamily}};
                  font-size: 15px;
                  line-height: 1.68;
                }
                h1, h2, h3, h4 { color: #172b4d; line-height: 1.3; }
                h1 {
                  margin: 0 0 22px;
                  padding-bottom: 12px;
                  border-bottom: 2px solid #18a9c5;
                  font-size: 30px;
                }
                h2 {
                  margin: 32px 0 14px;
                  padding-bottom: 7px;
                  border-bottom: 1px solid #d9e2ec;
                  font-size: 23px;
                }
                h3 { margin: 25px 0 10px; font-size: 18px; }
                h4 { margin: 20px 0 8px; font-size: 16px; }
                p { margin: 10px 0; }
                ul, ol { margin: 10px 0; padding-left: 30px; }
                li { margin: 5px 0; }
                strong { color: #102a43; }
                blockquote {
                  margin: 16px 0;
                  padding: 10px 16px;
                  border-left: 4px solid #18a9c5;
                  background: #f2f8fa;
                  color: #334e68;
                }
                code {
                  padding: 2px 5px;
                  border: 1px solid #dbe4ea;
                  border-radius: 4px;
                  background: #f5f7f9;
                  color: #9c2f4d;
                  font-family: Consolas, monospace;
                  font-size: 13px;
                }
                pre {
                  overflow: auto;
                  padding: 15px;
                  border: 1px solid #d6e0e7;
                  border-radius: 6px;
                  background: #f5f7f9;
                  line-height: 1.5;
                }
                pre code {
                  padding: 0;
                  border: 0;
                  background: transparent;
                  color: #263238;
                }
                table { width: 100%; border-collapse: collapse; margin: 16px 0; }
                th, td { padding: 8px 10px; border: 1px solid #cfd8df; text-align: left; }
                th { background: #edf5f7; }
                hr { border: 0; border-top: 1px solid #d9e2ec; margin: 26px 0; }
                a { color: #087f9c; text-decoration: none; }
                a:hover { text-decoration: underline; }
              </style>
            </head>
            <body>{{content}}</body>
            </html>
            """;
    }

    private static void ViewerNavigating(object? sender, WebBrowserNavigatingEventArgs e)
    {
        Uri? url = e.Url;
        if (url is null || url.Scheme is not ("http" or "https"))
        {
            return;
        }

        e.Cancel = true;
        Process.Start(new ProcessStartInfo
        {
            FileName = url.AbsoluteUri,
            UseShellExecute = true
        });
    }
}

using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using ConnectionWatcher.App.Localization;

namespace ConnectionWatcher.App.UI;

public sealed class HelpCenterForm : Form
{
    private static readonly Regex MarkdownLink = new(
        @"\[([^\]]+)\]\(([^)]+)\)",
        RegexOptions.Compiled);

    public HelpCenterForm()
    {
        BuildInterface();
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
        TabPage page = new(title) { Padding = new Padding(10) };
        RichTextBox viewer = new()
        {
            Dock = DockStyle.Fill,
            ReadOnly = true,
            BackColor = SystemColors.Window,
            BorderStyle = BorderStyle.None,
            DetectUrls = true,
            ScrollBars = RichTextBoxScrollBars.Vertical
        };
        RenderMarkdown(viewer, LoadDocument(documentName));
        viewer.SelectionStart = 0;
        viewer.ScrollToCaret();
        page.Controls.Add(viewer);
        return page;
    }

    private static string LoadDocument(string documentName)
    {
        string language = UiText.IsChinese ? "zh-CN" : "en";
        string resourceName = $"ConnectionWatcher.Help.{documentName}.{language}.md";
        using Stream? stream = Assembly.GetExecutingAssembly()
            .GetManifestResourceStream(resourceName);
        if (stream is null)
        {
            return UiText.Get("HelpDocumentUnavailable");
        }

        using StreamReader reader = new(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        return reader.ReadToEnd();
    }

    private static void RenderMarkdown(RichTextBox viewer, string markdown)
    {
        bool codeBlock = false;
        foreach (string rawLine in markdown.Replace("\r\n", "\n").Split('\n'))
        {
            string line = rawLine;
            if (line.TrimStart().StartsWith("```", StringComparison.Ordinal))
            {
                codeBlock = !codeBlock;
                continue;
            }

            float size = 10F;
            FontStyle style = FontStyle.Regular;
            string fontName = codeBlock ? "Consolas" : "Segoe UI";
            if (!codeBlock && line.StartsWith("### ", StringComparison.Ordinal))
            {
                line = line[4..];
                size = 11F;
                style = FontStyle.Bold;
            }
            else if (!codeBlock && line.StartsWith("## ", StringComparison.Ordinal))
            {
                line = line[3..];
                size = 13F;
                style = FontStyle.Bold;
            }
            else if (!codeBlock && line.StartsWith("# ", StringComparison.Ordinal))
            {
                line = line[2..];
                size = 16F;
                style = FontStyle.Bold;
            }
            else if (!codeBlock && line.StartsWith("- ", StringComparison.Ordinal))
            {
                line = "• " + line[2..];
            }

            if (!codeBlock)
            {
                line = MarkdownLink.Replace(line, "$1 ($2)")
                    .Replace("**", string.Empty, StringComparison.Ordinal)
                    .Replace("`", string.Empty, StringComparison.Ordinal);
            }

            using Font font = new(fontName, size, style);
            viewer.SelectionFont = font;
            viewer.SelectionColor = SystemColors.WindowText;
            viewer.AppendText(line + Environment.NewLine);
        }
    }
}

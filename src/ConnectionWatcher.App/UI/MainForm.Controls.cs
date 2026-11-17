using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using ConnectionWatcher.App.Localization;
using ConnectionWatcher.App.Services;
using ConnectionWatcher.Core.Configuration;
using ConnectionWatcher.Core.Logging;
using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Monitoring;

namespace ConnectionWatcher.App.UI;

// Shared control factories; preserve the existing sizes and styles.
public sealed partial class MainForm
{
    private static Label TitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 20F, FontStyle.Regular),
            Margin = new Padding(0, 0, 0, 4),
        };
    }

    private static Label SubtitleLabel()
    {
        return new Label
        {
            AutoSize = true,
            ForeColor = SystemColors.GrayText,
            MaximumSize = new Size(780, 0),
        };
    }

    private static Label ValueLabel()
    {
        return new Label
        {
            AutoSize = true,
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
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
            SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        };
    }

    private static void ConfigureLegendItem(Label label, MatchAction action)
    {
        label.Dock = DockStyle.Fill;
        label.AutoEllipsis = true;
        label.TextAlign = ContentAlignment.MiddleLeft;
        label.ForeColor = ActionColor(action);
        label.Margin = Padding.Empty;
    }

    private static Panel SummaryBox(
        Label caption,
        Control value,
        Label? hint = null
    )
    {
        Panel panel = new()
        {
            Dock = DockStyle.Fill,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(16),
            Margin = new Padding(0, 0, 12, 0),
            Height = 105,
        };
        caption.AutoSize = true;
        caption.ForeColor = SystemColors.GrayText;
        caption.Location = new Point(16, 16);
        value.Location = new Point(16, 46);
        panel.Controls.Add(caption);
        panel.Controls.Add(value);
        if (hint is not null)
        {
            hint.AutoSize = false;
            hint.Location = new Point(16, 84);
            hint.Size = new Size(225, 34);
            hint.AutoEllipsis = true;
            panel.Controls.Add(hint);
        }
        return panel;
    }

    private static TableLayoutPanel Header(
        Label title,
        Label subtitle,
        Control? action = null
    )
    {
        TableLayoutPanel header = new()
        {
            Dock = DockStyle.Top,
            Height = 82,
            ColumnCount = action is null ? 1 : 2,
            RowCount = 1,
            Margin = Padding.Empty,
        };
        header.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        if (action is not null)
        {
            header.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        }
        header.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        TableLayoutPanel text = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2,
            Margin = Padding.Empty,
            Padding = Padding.Empty,
        };
        text.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        text.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        text.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        title.Dock = DockStyle.Fill;
        subtitle.AutoSize = false;
        subtitle.Dock = DockStyle.Fill;
        subtitle.AutoEllipsis = true;
        subtitle.Margin = Padding.Empty;
        text.Controls.Add(title, 0, 0);
        text.Controls.Add(subtitle, 0, 1);
        header.Controls.Add(text, 0, 0);
        if (action is not null)
        {
            action.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            action.Margin = new Padding(12, 0, 0, 0);
            header.Controls.Add(action, 1, 0);
        }

        return header;
    }

    private static void AddSetting(
        TableLayoutPanel layout,
        string labelName,
        Label? hint,
        Control value,
        int row,
        Label? explicitLabel = null
    )
    {
        FlowLayoutPanel text = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            // Keep each setting visually separated without making the page feel
            // sparse.
            Margin = new Padding(0, 7, 12, 14),
        };
        Label label = explicitLabel ?? new Label();
        label.Name = string.IsNullOrEmpty(labelName) ? label.Name : labelName;
        label.AutoSize = true;
        label.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        text.Controls.Add(label);
        if (hint is not null)
        {
            hint.Margin = new Padding(0, 2, 0, 0);
            text.Controls.Add(hint);
        }

        value.Anchor = AnchorStyles.Right;
        value.Margin = new Padding(8, 11, 0, 0);
        layout.Controls.Add(text, 0, row);
        layout.Controls.Add(value, 1, row);
    }
}

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

// Build the monitoring summary and action legend.
public sealed partial class MainForm
{
    private Panel BuildHomePage()
    {
        Panel page = new();
        _homeTitle.Name = "HomeTitle";
        _homeSubtitle.Name = "HomeSubtitle";
        _monitorButton.Name = "MonitorButton";
        _monitorButton.AutoSize = true;
        _monitorButton.MinimumSize = new Size(180, 38);
        _monitorButton.Padding = new Padding(12, 2, 12, 2);
        _monitorButton.Click += async (_, _) => await ToggleMonitoringAsync();
        TableLayoutPanel header = Header(
            _homeTitle,
            _homeSubtitle,
            _monitorButton
        );

        TableLayoutPanel summary = new()
        {
            Name = "HomeSummary",
            AutoSize = false,
            ColumnCount = 3,
            RowCount = 1,
            Dock = DockStyle.Fill,
            Margin = new Padding(0, 24, 0, 0),
        };
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        summary.Controls.Add(SummaryBox(_homeStatusCaption, _homeStatus), 0, 0);
        summary.Controls.Add(SummaryBox(_homeRulesCaption, _homeRules), 1, 0);
        FlowLayoutPanel intervalValue = new()
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
        };
        _checkInterval.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        _checkIntervalUnit.Font = new Font("Segoe UI", 10F);
        _checkIntervalUnit.Margin = new Padding(4, 7, 0, 0);
        intervalValue.Controls.AddRange([_checkInterval, _checkIntervalUnit]);
        summary.Controls.Add(
            SummaryBox(_homeIntervalCaption, intervalValue),
            2,
            0
        );
        _checkInterval.ValueChanged += (_, _) => CheckIntervalChanged();

        TableLayoutPanel legend = BuildActionLegend();

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
            Margin = Padding.Empty,
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 142));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        header.Dock = DockStyle.Fill;
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(summary, 0, 1);
        layout.Controls.Add(legend, 0, 2);
        page.Controls.Add(layout);
        return page;
    }

    private TableLayoutPanel BuildActionLegend()
    {
        TableLayoutPanel legend = new()
        {
            Name = "ActionLegend",
            Dock = DockStyle.Fill,
            ColumnCount = 3,
            RowCount = 2,
            BorderStyle = BorderStyle.FixedSingle,
            Padding = new Padding(12, 8, 12, 8),
            Margin = new Padding(0, 12, 12, 0),
        };
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.33F));
        legend.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.34F));
        legend.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        legend.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        _actionLegendTitle.AutoSize = true;
        _actionLegendTitle.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _actionLegendTitle.Margin = new Padding(0, 0, 0, 5);
        legend.Controls.Add(_actionLegendTitle, 0, 0);
        legend.SetColumnSpan(_actionLegendTitle, 3);

        ConfigureLegendItem(_silentActionLegend, MatchAction.SilentLog);
        ConfigureLegendItem(_trayActionLegend, MatchAction.TrayNotice);
        ConfigureLegendItem(_popupActionLegend, MatchAction.PopupAlert);
        legend.Controls.Add(_silentActionLegend, 0, 1);
        legend.Controls.Add(_trayActionLegend, 1, 1);
        legend.Controls.Add(_popupActionLegend, 2, 1);
        return legend;
    }
}

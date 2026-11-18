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

// Display, inspect, and export the observed connection history.
public sealed partial class MainForm
{
    private Panel BuildEventsPage()
    {
        Panel page = new();
        _eventsTitle.Name = "EventsTitle";
        TableLayoutPanel header = Header(_eventsTitle, _eventsSubtitle);

        FlowLayoutPanel tools = new()
        {
            Dock = DockStyle.Fill,
            AutoSize = true,
            Padding = new Padding(0, 5, 0, 5),
            WrapContents = true,
        };
        _eventSearch.Width = 250;
        _eventSearch.Margin = new Padding(0, 3, 8, 3);
        _eventSearch.TextChanged += (_, _) => RefreshEvents();
        _exportButton.AutoSize = true;
        _openLogsButton.AutoSize = true;
        _exportButton.MinimumSize = new Size(96, 34);
        _openLogsButton.MinimumSize = new Size(118, 34);
        _exportButton.Click += async (_, _) => await ExportEventsAsync();
        _openLogsButton.Click += (_, _) => OpenLogFolder();
        _clearEventsButton.Click += (_, _) => ClearEventDisplay();
        tools.Controls.AddRange(
            [_eventSearch, _exportButton, _openLogsButton, _clearEventsButton]
        );

        _eventsGrid.Name = "EventsGrid";
        _eventsGrid.ColumnHeadersDefaultCellStyle.WrapMode =
            DataGridViewTriState.True;
        _eventsGrid.ColumnHeadersDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _eventsGrid.RowsDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleLeft;
        _eventsGrid.ColumnHeadersHeightSizeMode =
            DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 18,
                MinimumWidth = 120,
            }
        );
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn { FillWeight = 13, MinimumWidth = 92 }
        );
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 14,
                MinimumWidth = 100,
            }
        );
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 24,
                MinimumWidth = 140,
            }
        );
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 20,
                MinimumWidth = 110,
            }
        );
        _eventsGrid.Columns.Add(
            new DataGridViewTextBoxColumn { FillWeight = 11, MinimumWidth = 68 }
        );
        _eventsGrid.CellDoubleClick += (_, args) =>
        {
            if (
                args.RowIndex >= 0
                && _eventsGrid.Rows[args.RowIndex].Tag is ConnectionEvent entry
            )
            {
                ShowEventDetails(entry);
            }
        };

        _eventDetailsHint.Name = "EventDetailsHint";
        _eventDetailsHint.Dock = DockStyle.Fill;
        _eventDetailsHint.TextAlign = ContentAlignment.MiddleLeft;

        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 82));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30));
        header.Dock = DockStyle.Fill;
        tools.Dock = DockStyle.Fill;
        _eventsGrid.Dock = DockStyle.Fill;
        layout.Controls.Add(header, 0, 0);
        layout.Controls.Add(tools, 0, 1);
        layout.Controls.Add(_eventsGrid, 0, 2);
        layout.Controls.Add(_eventDetailsHint, 0, 3);
        page.Controls.Add(layout);
        return page;
    }

    /// <summary>
    /// Apply search and display cutoffs without deleting logged events.
    /// </summary>
    private void RefreshEvents()
    {
        if (_eventsGrid.Columns.Count == 0)
        {
            return;
        }

        _eventsGrid.Rows.Clear();
        foreach (ConnectionEvent entry in FilteredEvents().Take(2000))
        {
            int row = _eventsGrid.Rows.Add(
                entry.DetectedAt.LocalDateTime.ToString("yyyy-MM-dd HH:mm:ss"),
                FormatConnectionStatus(entry),
                FormatObservedDuration(entry, DateTimeOffset.Now),
                FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                entry.ApplicationDisplayName,
                UiText.ActionMarker(entry.Action)
            );
            _eventsGrid.Rows[row].Tag = entry;
            DataGridViewCell statusCell = _eventsGrid.Rows[row].Cells[1];
            Color statusColor = entry.IsActive ? Color.SeaGreen : Color.DimGray;
            statusCell.Style.ForeColor = statusColor;
            statusCell.Style.SelectionForeColor = SystemColors.HighlightText;
            statusCell.Style.Font = _eventStatusFont;
            _eventsGrid.Rows[row].Cells[4].ToolTipText = string.Format(
                UiText.Get("ApplicationTooltip"),
                entry.ProcessName,
                entry.ProcessId,
                entry.ProcessPath ?? UiText.Get("ProcessPathUnavailable")
            );
            DataGridViewCell actionCell = _eventsGrid.Rows[row].Cells[5];
            actionCell.ToolTipText = UiText.Action(entry.Action);
            Color actionColor = ActionColor(entry.Action);
            actionCell.Style.ForeColor = actionColor;
            actionCell.Style.SelectionForeColor = actionColor;
            actionCell.Style.SelectionBackColor = SystemColors.Window;
        }
        _clearEventsButton.Enabled = _events.Any(IsVisibleAfterDisplayCutoff);
    }

    private bool IsVisibleAfterDisplayCutoff(ConnectionEvent entry) =>
        _settings.EventLogDisplayCutoff is not DateTimeOffset cutoff
        || entry.DetectedAt > cutoff;

    /// <summary>
    /// Advance the display cutoff while retaining the underlying CSV history.
    /// </summary>
    private void ClearEventDisplay()
    {
        if (
            MessageBox.Show(
                this,
                UiText.Get("ClearDisplayQuestion"),
                UiText.Get("ClearDisplayTitle"),
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question
            ) != DialogResult.Yes
        )
        {
            return;
        }

        DateTimeOffset cutoff = DateTimeOffset.Now;
        _settings.EventLogDisplayCutoff = cutoff;
        SaveSettings();
        _events.RemoveAll(entry => entry.DetectedAt <= cutoff);
        RefreshEvents();
    }

    private void ShowEventDetails(ConnectionEvent entry)
    {
        using EventDetailsForm details = new(entry);
        details.ShowDialog(this);
    }

    private void UpdateVisibleEventDurations()
    {
        if (!_pages.TryGetValue("Events", out Panel? page) || !page.Visible)
        {
            return;
        }

        DateTimeOffset now = DateTimeOffset.Now;
        foreach (DataGridViewRow row in _eventsGrid.Rows)
        {
            if (row.Tag is ConnectionEvent { IsActive: true } entry)
            {
                row.Cells[2].Value = FormatObservedDuration(entry, now);
            }
        }
    }

    private static string FormatObservedDuration(
        ConnectionEvent entry,
        DateTimeOffset now
    )
    {
        TimeSpan? duration = entry.GetObservedDuration(now);
        if (duration is null)
        {
            return "—";
        }

        int totalHours = (int)Math.Floor(duration.Value.TotalHours);
        return (
            $"{totalHours:00}"
            + ":"
            + $"{duration.Value.Minutes:00}"
            + ":"
            + $"{duration.Value.Seconds:00}"
        );
    }

    private static string FormatConnectionStatus(ConnectionEvent entry) =>
        $"{(entry.IsActive ? '●' : '○')} "
        + UiText.Get(entry.IsActive ? "ConnectionActive" : "ConnectionEnded");

    private static string ActionLegendText(MatchAction action)
    {
        return $"{UiText.ActionMarker(action)}  {UiText.Action(action)}";
    }

    private static Color ActionColor(MatchAction action)
    {
        return action switch
        {
            MatchAction.SilentLog => Color.DimGray,
            MatchAction.TrayNotice => Color.DarkOrange,
            _ => Color.Crimson,
        };
    }

    /// <summary>
    /// Write the selected history to the path explicitly chosen by the user.
    /// </summary>
    private async Task ExportEventsAsync()
    {
        using SaveFileDialog dialog = new()
        {
            Filter = "CSV (*.csv)|*.csv",
            FileName = $"connection-events-{DateTime.Now:yyyyMMdd-HHmmss}.csv",
        };
        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        StringBuilder csv = new();
        csv.AppendLine(
            string.Join(
                ',',
                new[]
                {
                    UiText.Get("Time"),
                    UiText.Get("ConnectionStatus"),
                    UiText.Get("ObservedDuration"),
                    UiText.Get("MatchedRules"),
                    UiText.Get("MatchAction"),
                    UiText.Get("TcpState"),
                    UiText.Get("LocalEndpoint"),
                    UiText.Get("RemoteEndpoint"),
                    "PID",
                    UiText.Get("ConnectionOwner"),
                    UiText.Get("ProcessPath"),
                    UiText.Get("ProductName"),
                    UiText.Get("CompanyName"),
                    UiText.Get("FileDescription"),
                    UiText.Get("ParentProcesses"),
                    UiText.Get("RelatedServices"),
                }.Select(CsvEscape)
            )
        );
        foreach (ConnectionEvent entry in FilteredEvents())
        {
            csv.AppendLine(
                string.Join(
                    ',',
                    new[]
                    {
                        entry.DetectedAt.LocalDateTime.ToString(
                            "yyyy-MM-dd HH:mm:ss"
                        ),
                        UiText.Get(
                            entry.IsActive
                                ? "ConnectionActive"
                                : "ConnectionEnded"
                        ),
                        FormatObservedDuration(entry, DateTimeOffset.Now),
                        string.Join(" | ", entry.RuleNames),
                        UiText.Action(entry.Action),
                        entry.State.ToString(),
                        FormatEndpoint(entry.LocalAddress, entry.LocalPort),
                        FormatEndpoint(entry.RemoteAddress, entry.RemotePort),
                        entry.ProcessId.ToString(CultureInfo.InvariantCulture),
                        entry.ProcessName,
                        entry.ProcessPath ?? string.Empty,
                        entry.ProcessProductName ?? string.Empty,
                        entry.ProcessCompanyName ?? string.Empty,
                        entry.ProcessFileDescription ?? string.Empty,
                        FormatParentProcesses(entry.ParentProcesses),
                        FormatRelatedServices(entry.RelatedServices),
                    }.Select(CsvEscape)
                )
            );
        }

        await File.WriteAllTextAsync(
            dialog.FileName,
            csv.ToString(),
            new UTF8Encoding(true)
        );
        MessageBox.Show(
            UiText.Get("ExportComplete"),
            UiText.Get("AppTitle"),
            MessageBoxButtons.OK,
            MessageBoxIcon.Information
        );
    }

    private static string CsvEscape(string value)
    {
        return '"'
            + value.Replace("\"", "\"\"").Replace('\r', ' ').Replace('\n', ' ')
            + '"';
    }

    private static string FormatEndpoint(string address, int port)
    {
        return address.Contains(':', StringComparison.Ordinal)
            ? $"[{address}]:{port}"
            : $"{address}:{port}";
    }

    private static string FormatParentProcesses(
        IEnumerable<ProcessSnapshot> parents
    ) =>
        string.Join(
            " -> ",
            parents.Select(parent =>
                $"{parent.ProcessName} (PID {parent.ProcessId})"
            )
        );

    private static string FormatRelatedServices(
        IEnumerable<WindowsServiceSnapshot> services
    ) =>
        string.Join(
            " | ",
            services.Select(service =>
                (
                    $"{service.DisplayName}"
                    + " ["
                    + $"{service.ServiceName}"
                    + "] (PID "
                    + $"{service.ProcessId}"
                    + ")"
                )
            )
        );

    private void OpenLogFolder()
    {
        Directory.CreateDirectory(_logger.LogDirectory);
        Process.Start(
            new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"\"{_logger.LogDirectory}\"",
                UseShellExecute = true,
            }
        );
    }
}

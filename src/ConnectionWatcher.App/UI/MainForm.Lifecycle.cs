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

// Coordinate tray visibility, startup, and orderly monitoring shutdown.
public sealed partial class MainForm
{
    private void BuildTrayMenu()
    {
        _trayMenu.Items.AddRange(
            [_trayOpen, _trayToggle, new ToolStripSeparator(), _trayExit]
        );
        _trayOpen.Click += (_, _) => ShowMainWindow();
        _trayToggle.Click += async (_, _) => await ToggleMonitoringAsync();
        _trayExit.Click += async (_, _) => await ExitApplicationAsync();
    }

    private async void MainFormShown(object? sender, EventArgs e)
    {
        IReadOnlyList<ConnectionEvent> existing =
            await _logger.ReadRecentAsync();
        foreach (
            ConnectionEvent entry in existing.Where(entry => entry.IsActive)
        )
        {
            entry.MarkHistoricalInactive();
        }
        _events.Clear();
        _events.AddRange(existing.Where(IsVisibleAfterDisplayCutoff));
        RefreshEvents();

        if (
            _settings.ResumeMonitoring
            && _settings.Rules.Any(rule => rule.Enabled)
        )
        {
            StartMonitoring();
        }
    }

    private void ShowMainWindow()
    {
        Show();
        WindowState = FormWindowState.Normal;
        Activate();
    }

    /// <summary>
    /// Keep tray monitoring active unless the user explicitly exits.
    /// </summary>
    private async void MainFormClosing(object? sender, FormClosingEventArgs e)
    {
        if (_exitRequested || e.CloseReason != CloseReason.UserClosing)
        {
            return;
        }

        if (_engine.IsRunning)
        {
            DialogResult result = MessageBox.Show(
                UiText.Get("CloseWhileRunning"),
                UiText.Get("CloseWhileRunningTitle"),
                MessageBoxButtons.YesNoCancel,
                MessageBoxIcon.Question
            );
            if (result == DialogResult.Yes)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            if (result == DialogResult.Cancel)
            {
                e.Cancel = true;
                return;
            }

            e.Cancel = true;
            await ExitApplicationAsync();
        }
    }

    /// <summary>
    /// Stop monitoring before disposing the tray icon and closing the form.
    /// </summary>
    private async Task ExitApplicationAsync()
    {
        _exitRequested = true;
        await _engine.StopAsync();
        _notifyIcon.Visible = false;
        Close();
    }
}

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

// Marshal monitoring events to the UI and maintain alert state.
public sealed partial class MainForm
{
    private async Task ToggleMonitoringAsync()
    {
        if (_engine.IsRunning)
        {
            await _engine.StopAsync();
            _monitoringError = false;
            UpdateMonitoringStatus();
        }
        else
        {
            StartMonitoring();
        }
    }

    private void StartMonitoring()
    {
        try
        {
            _monitoringError = false;
            _engine.Start();
            UpdateMonitoringStatus();
        }
        catch (InvalidOperationException)
        {
            MessageBox.Show(
                UiText.Get("NeedEnabledRule"),
                UiText.Get("AppTitle"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information
            );
        }
    }

    private void EngineEventDetected(
        object? sender,
        ConnectionEvent connectionEvent
    )
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() => HandleDetectedEvent(connectionEvent));
    }

    private void EngineEventCompleted(
        object? sender,
        ConnectionEvent connectionEvent
    )
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            if (_pages["Events"].Visible)
            {
                RefreshEvents();
            }
        });
    }

    private void EngineMonitoringError(object? sender, Exception exception)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            _monitoringError = true;
            _sidebarHint.Text = exception.Message;
            UpdateMonitoringStatus();
        });
    }

    private void EngineMonitoringRecovered(object? sender, EventArgs e)
    {
        if (IsDisposed)
        {
            return;
        }

        BeginInvoke(() =>
        {
            _monitoringError = false;
            _sidebarHint.Text = UiText.Get("StatusHint");
            UpdateMonitoringStatus();
        });
    }

    /// <summary>
    /// Record the event and apply only the action selected by its rule.
    /// </summary>
    private void HandleDetectedEvent(ConnectionEvent connectionEvent)
    {
        if (IsVisibleAfterDisplayCutoff(connectionEvent))
        {
            _events.Insert(0, connectionEvent);
        }
        if (_events.Count > 2000)
        {
            _events.RemoveRange(2000, _events.Count - 2000);
        }
        if (_pages["Events"].Visible)
        {
            RefreshEvents();
        }

        switch (connectionEvent.Action)
        {
            case MatchAction.TrayNotice:
                _trayNoticeCount++;
                UpdateTrayIcon();
                break;
            case MatchAction.PopupAlert:
                ShowUrgentAlert(connectionEvent);
                break;
        }
    }

    /// <summary>
    /// Reuse the alert window and honor the per-rule popup cooldown.
    /// </summary>
    private void ShowUrgentAlert(ConnectionEvent connectionEvent)
    {
        if (_urgentAlert is { IsDisposed: false, Visible: true })
        {
            _urgentAlert.AddEvent(connectionEvent);
            return;
        }

        bool eligible =
            connectionEvent.RepeatAlertMinutes == 0
            || connectionEvent.RuleIds.Any(id =>
                !_lastPopupClosed.TryGetValue(id, out DateTimeOffset closed)
                || DateTimeOffset.Now - closed
                    >= TimeSpan.FromMinutes(connectionEvent.RepeatAlertMinutes)
            );
        if (!eligible)
        {
            return;
        }

        _urgentAlert = new UrgentAlertForm(connectionEvent);
        _urgentAlert.ViewDetailsRequested += (_, _) =>
        {
            ShowMainWindow();
            ShowPage("Events");
        };
        _urgentAlert.FormClosed += (_, _) =>
        {
            DateTimeOffset closed = DateTimeOffset.Now;
            foreach (Guid id in _urgentAlert.RuleIds)
            {
                _lastPopupClosed[id] = closed;
            }

            _urgentAlert = null;
        };
        _urgentAlert.Show(this);
        if (_settings.AlertSound)
        {
            AlertSoundPlayer.Play(_settings.AlertVolumePercent);
        }
    }

    private void UpdateMonitoringStatus()
    {
        string status;
        Color color;
        if (_monitoringError)
        {
            status = UiText.Get("MonitoringInterrupted");
            color = Color.DarkOrange;
        }
        else if (_engine.IsRunning)
        {
            status = UiText.Get("MonitoringRunning");
            color = Color.ForestGreen;
        }
        else
        {
            status = UiText.Get("MonitoringStopped");
            color = SystemColors.GrayText;
        }

        _sidebarStatus.Text = status;
        _sidebarStatus.ForeColor = color;
        _homeStatus.Text = status;
        _homeStatus.ForeColor = color;
        _monitorButton.Text = UiText.Get(
            _engine.IsRunning ? "StopMonitoring" : "StartMonitoring"
        );
        _trayToggle.Text = _monitorButton.Text;
        _homeRules.Text = _settings
            .Rules.Count(rule => rule.Enabled)
            .ToString(CultureInfo.CurrentCulture);
        UpdateTrayIcon();
    }

    private void UpdateTrayIcon()
    {
        if (_monitoringError)
        {
            _notifyIcon.Icon = SystemIcons.Error;
            _notifyIcon.Text = Truncate(UiText.Get("MonitoringInterrupted"));
        }
        else if (_trayNoticeCount > 0)
        {
            _notifyIcon.Icon = SystemIcons.Warning;
            _notifyIcon.Text = Truncate(
                string.Format(UiText.Get("TrayNotices"), _trayNoticeCount)
            );
        }
        else if (_engine.IsRunning)
        {
            _notifyIcon.Icon = _applicationIcon;
            _notifyIcon.Text = Truncate(UiText.Get("TrayNormal"));
        }
        else
        {
            _notifyIcon.Icon = _applicationIcon;
            _notifyIcon.Text = Truncate(UiText.Get("AppTitle"));
        }
    }

    private static string Truncate(string text)
    {
        return text.Length <= 63 ? text : text[..63];
    }
}

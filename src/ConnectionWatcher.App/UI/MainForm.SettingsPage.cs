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

// Apply local preferences and language changes to existing controls.
public sealed partial class MainForm
{
    private Panel BuildSettingsPage()
    {
        Panel page = new();
        _settingsTitle.Name = "SettingsTitle";
        TableLayoutPanel header = Header(_settingsTitle, _settingsSubtitle);
        TableLayoutPanel settings = new()
        {
            Name = "SettingsTable",
            Dock = DockStyle.Top,
            AutoSize = true,
            ColumnCount = 2,
            Padding = new Padding(0, 20, 12, 0),
        };
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 70));
        settings.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30));

        AddSetting(settings, "LanguageLabel", null, _language, 0);
        AddSetting(
            settings,
            string.Empty,
            _startWithWindowsHint,
            _startWithWindows,
            1,
            _startWithWindowsCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _resumeMonitoringHint,
            _resumeMonitoring,
            2,
            _resumeMonitoringCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _alertSoundHint,
            _alertSound,
            3,
            _alertSoundCaption
        );
        _alertVolumeControls.Controls.AddRange(
            [_testAlertSoundButton, _alertVolume]
        );
        AddSetting(
            settings,
            string.Empty,
            _alertVolumeHint,
            _alertVolumeControls,
            4,
            _alertVolumeCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _logLimitHint,
            _logLimit,
            5,
            _logLimitCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _helpCenterHint,
            _helpCenterButton,
            6,
            _helpCenterCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _feedbackHint,
            _feedbackButton,
            7,
            _feedbackCaption
        );
        AddSetting(
            settings,
            string.Empty,
            _updateHint,
            _checkUpdatesButton,
            8,
            _updateCaption
        );

        _language.DrawItem += DrawLanguageItem;
        _language.SelectedIndexChanged += LanguageChanged;
        _startWithWindows.CheckedChanged += StartWithWindowsChanged;
        _resumeMonitoring.CheckedChanged += (_, _) =>
        {
            if (_refreshingSettings)
                return;
            _settings.ResumeMonitoring = _resumeMonitoring.Checked;
            SaveSettings();
        };
        _alertSound.CheckedChanged += (_, _) =>
        {
            if (_refreshingSettings)
                return;
            _settings.AlertSound = _alertSound.Checked;
            SaveSettings();
        };
        _alertVolume.ValueChanged += (_, _) =>
        {
            if (_refreshingSettings)
                return;
            _settings.AlertVolumePercent = decimal.ToInt32(_alertVolume.Value);
            SaveSettings();
        };
        _testAlertSoundButton.Click += (_, _) =>
            AlertSoundPlayer.Play(decimal.ToInt32(_alertVolume.Value));
        _logLimit.Validated += async (_, _) => await LogLimitChangedAsync();
        _helpCenterButton.Click += (_, _) =>
        {
            using HelpCenterForm helpCenter = new();
            helpCenter.ShowDialog(this);
        };
        _feedbackButton.Click += (_, _) =>
        {
            using FeedbackForm feedback = new();
            feedback.ShowDialog(this);
        };
        _checkUpdatesButton.Click += async (_, _) =>
            await CheckForUpdatesAsync();

        Panel body = new() { Dock = DockStyle.Fill, AutoScroll = true };
        body.Controls.Add(settings);
        body.Controls.Add(header);
        header.Dock = DockStyle.Top;
        settings.Dock = DockStyle.Top;
        page.Controls.Add(body);
        return page;
    }

    private void ApplyLanguage()
    {
        UiFont.Apply(this);
        _eventStatusFont?.Dispose();
        _eventStatusFont = new Font(_eventsGrid.Font, FontStyle.Bold);
        Text = UiText.Get("AppTitle");
        foreach (string key in _navButtons.Keys)
        {
            _navButtons[key].Text = UiText.Get(key);
        }

        _homeTitle.Text = UiText.Get("Home");
        _homeSubtitle.Text = UiText.Get("StatusHint");
        _homeStatusCaption.Text = UiText.Get("MonitoringStatus");
        _homeRulesCaption.Text = UiText.Get("EnabledRules");
        _homeIntervalCaption.Text = UiText.Get("CheckInterval");
        _checkIntervalUnit.Text = UiText.Get("SecondsUnit");
        _actionLegendTitle.Text = UiText.Get("ActionLegend");
        _silentActionLegend.Text = ActionLegendText(MatchAction.SilentLog);
        _trayActionLegend.Text = ActionLegendText(MatchAction.TrayNotice);
        _popupActionLegend.Text = ActionLegendText(MatchAction.PopupAlert);

        _rulesTitle.Text = UiText.Get("Rules");
        _rulesSubtitle.Text = UiText.Get("RulesDescription");
        _newRuleButton.Text = UiText.Get("NewRule");
        _editRuleButton.Text = UiText.Get("Edit");
        _deleteRuleButton.Text = UiText.Get("Delete");
        string[] ruleHeaders = ["Name", "Condition", "MatchAction", "Enabled"];
        for (int index = 0; index < ruleHeaders.Length; index++)
        {
            _rulesGrid.Columns[index].HeaderText = UiText.Get(
                ruleHeaders[index]
            );
        }

        _eventsTitle.Text = UiText.Get("Events");
        _eventsSubtitle.Text = UiText.Get("EventsDescription");
        _eventSearch.PlaceholderText = UiText.Get("SearchEvents");
        _exportButton.Text = UiText.Get("ExportCsv");
        _openLogsButton.Text = UiText.Get("OpenLogFolder");
        _clearEventsButton.Text = UiText.Get("ClearDisplay");
        string[] eventHeaders =
        [
            "Time",
            "ConnectionStatus",
            "ObservedDuration",
            "RemoteEndpoint",
            "Application",
            "ActionColumn",
        ];
        for (int index = 0; index < eventHeaders.Length; index++)
        {
            _eventsGrid.Columns[index].HeaderText = UiText.Get(
                eventHeaders[index]
            );
        }
        _eventDetailsHint.Text = UiText.Get("EventDoubleClickHint");

        _settingsTitle.Text = UiText.Get("Settings");
        _settingsSubtitle.Text = UiText.Get("Privacy");
        Controls.Find("LanguageLabel", true).Single().Text = UiText.Get(
            "Language"
        );
        _startWithWindowsCaption.Text = UiText.Get("StartWithWindows");
        _startWithWindows.Text = string.Empty;
        _startWithWindowsHint.Text = UiText.Get("StartWithWindowsHint");
        _resumeMonitoringCaption.Text = UiText.Get("ResumeMonitoring");
        _resumeMonitoring.Text = string.Empty;
        _resumeMonitoringHint.Text = UiText.Get("ResumeMonitoringHint");
        _alertSoundCaption.Text = UiText.Get("AlertSound");
        _alertSound.Text = string.Empty;
        _alertSoundHint.Text = UiText.Get("AlertSoundHint");
        _testAlertSoundButton.Text = UiText.Get("TestSound");
        _alertVolumeCaption.Text = UiText.Get("AlertVolume");
        _alertVolumeHint.Text = UiText.Get("AlertVolumeHint");
        _logLimitCaption.Text = UiText.Get("LogLimit");
        _logLimitHint.Text = UiText.Get("LogLimitHint");
        _helpCenterCaption.Text = UiText.Get("HelpCenter");
        _helpCenterHint.Text = UiText.Get("HelpCenterHint");
        _helpCenterButton.Text = UiText.Get("OpenHelpCenter");
        _feedbackCaption.Text = UiText.Get("Feedback");
        _feedbackHint.Text = UiText.Get("FeedbackHint");
        _feedbackButton.Text = UiText.Get("OpenFeedback");
        _updateCaption.Text = string.Format(
            UiText.Get("SoftwareUpdatesWithVersion"),
            CurrentVersion().ToString(3)
        );
        _updateHint.Text = UiText.Get("SoftwareUpdatesHint");
        _checkUpdatesButton.Text = UiText.Get("CheckNow");

        _trayOpen.Text = UiText.Get("Open");
        _trayExit.Text = UiText.Get("Exit");
        _sidebarHint.Text = UiText.Get("StatusHint");
        RefreshRules();
        RefreshEvents();
        RefreshSettings();
        UpdateMonitoringStatus();
    }

    private void RefreshSettings()
    {
        _refreshingSettings = true;
        try
        {
            _language.Items.Clear();
            _language.Items.AddRange(
                [
                    new LanguageOption("zh-CN", UiText.Get("Chinese")),
                    new LanguageOption(
                        "zh-TW",
                        UiText.Get("TraditionalChinese")
                    ),
                    new LanguageOption("en", UiText.Get("English")),
                    new LanguageOption("es", UiText.Get("Spanish")),
                    new LanguageOption("fr", UiText.Get("French")),
                    new LanguageOption("de", UiText.Get("German")),
                    new LanguageOption(
                        "pt-BR",
                        UiText.Get("BrazilianPortuguese")
                    ),
                ]
            );
            _language.SelectedIndex = _language
                .Items.Cast<LanguageOption>()
                .Select((option, index) => (option, index))
                .First(pair => pair.option.Code == UiText.Language)
                .index;
            _startWithWindows.Checked = _settings.StartWithWindows;
            _resumeMonitoring.Checked = _settings.ResumeMonitoring;
            _alertSound.Checked = _settings.AlertSound;
            _alertVolume.Value = Math.Clamp(
                _settings.AlertVolumePercent,
                AppSettings.MinimumAlertVolumePercent,
                AppSettings.MaximumAlertVolumePercent
            );
            _logLimit.Value = Math.Clamp(
                _settings.LogLimitMb,
                AppSettings.MinimumLogLimitMb,
                AppSettings.MaximumLogLimitMb
            );
            _checkInterval.Value = Math.Clamp(
                _settings.CheckIntervalSeconds,
                AppSettings.MinimumCheckIntervalSeconds,
                AppSettings.MaximumCheckIntervalSeconds
            );
        }
        finally
        {
            _refreshingSettings = false;
        }
    }

    private void LanguageChanged(object? sender, EventArgs e)
    {
        if (
            _refreshingSettings
            || _language.SelectedItem is not LanguageOption option
        )
        {
            return;
        }

        _settings.Language = option.Code;
        UiText.SetLanguage(option.Code);
        SaveSettings();
        ApplyLanguage();
    }

    private void DrawLanguageItem(object? sender, DrawItemEventArgs e)
    {
        e.DrawBackground();
        LanguageOption? option =
            e.Index >= 0 && e.Index < _language.Items.Count
                ? _language.Items[e.Index] as LanguageOption
                : _language.SelectedItem as LanguageOption;
        if (option is not null)
        {
            bool selectedDisplay = e.State.HasFlag(DrawItemState.ComboBoxEdit);
            Rectangle bounds = selectedDisplay
                ? e.Bounds
                : new Rectangle(
                    e.Bounds.X + 4,
                    e.Bounds.Y,
                    Math.Max(0, e.Bounds.Width - 8),
                    e.Bounds.Height
                );
            TextFormatFlags flags =
                TextFormatFlags.VerticalCenter
                | TextFormatFlags.EndEllipsis
                | TextFormatFlags.NoPrefix
                | (
                    selectedDisplay
                        ? TextFormatFlags.HorizontalCenter
                        : TextFormatFlags.Left
                );
            TextRenderer.DrawText(
                e.Graphics,
                option.Label,
                e.Font,
                bounds,
                e.ForeColor,
                flags
            );
        }

        e.DrawFocusRectangle();
    }

    private void StartWithWindowsChanged(object? sender, EventArgs e)
    {
        if (_refreshingSettings)
        {
            return;
        }

        try
        {
            StartupManager.SetEnabled(_startWithWindows.Checked);
            _settings.StartWithWindows = _startWithWindows.Checked;
            SaveSettings();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                string.Format(UiText.Get("StartupSettingError"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            _refreshingSettings = true;
            _startWithWindows.Checked = _settings.StartWithWindows;
            _refreshingSettings = false;
        }
    }

    /// <summary>
    /// Update the logger's bound before persisting the selected limit.
    /// </summary>
    private async Task LogLimitChangedAsync()
    {
        if (_refreshingSettings)
        {
            return;
        }

        int previousValue = _settings.LogLimitMb;
        int selectedValue = decimal.ToInt32(_logLimit.Value);
        try
        {
            await _logger.UpdateMaximumTotalBytesAsync(
                selectedValue * 1024L * 1024L
            );
            _settings.LogLimitMb = selectedValue;
            SaveSettings();
        }
        catch (Exception ex)
            when (ex is IOException or UnauthorizedAccessException)
        {
            MessageBox.Show(
                string.Format(UiText.Get("LogLimitUpdateError"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
            _refreshingSettings = true;
            _logLimit.Value = Math.Clamp(
                previousValue,
                AppSettings.MinimumLogLimitMb,
                AppSettings.MaximumLogLimitMb
            );
            _refreshingSettings = false;
        }
    }

    private void CheckIntervalChanged()
    {
        if (_refreshingSettings)
        {
            return;
        }

        decimal seconds = _checkInterval.Value;
        _settings.CheckIntervalSeconds = seconds;
        _engine.UpdateInterval(TimeSpan.FromSeconds((double)seconds));
        SaveSettings();
    }

    /// <summary>
    /// Run the user-requested release check without downloading an installer.
    /// </summary>
    private async Task CheckForUpdatesAsync()
    {
        _checkUpdatesButton.Enabled = false;
        _checkUpdatesButton.Text = UiText.Get("CheckingUpdates");
        try
        {
            UpdateCheckResult result = await _updateCheckService.CheckAsync(
                CurrentVersion()
            );
            if (result.IsUpdateAvailable)
            {
                DialogResult choice = MessageBox.Show(
                    this,
                    string.Format(
                        UiText.Get("UpdateAvailableMessage"),
                        result.LatestTag,
                        result.ReleaseName
                    ),
                    UiText.Get("UpdateAvailableTitle"),
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Information
                );
                if (choice == DialogResult.Yes)
                {
                    Process.Start(
                        new ProcessStartInfo
                        {
                            FileName = result.ReleasePage.AbsoluteUri,
                            UseShellExecute = true,
                        }
                    );
                }
            }
            else
            {
                MessageBox.Show(
                    this,
                    string.Format(
                        UiText.Get("UpToDateMessage"),
                        result.CurrentVersion.ToString(3)
                    ),
                    UiText.Get("SoftwareUpdates"),
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information
                );
            }
        }
        catch (Exception ex)
            when (ex
                    is HttpRequestException
                        or TaskCanceledException
                        or InvalidDataException
                        or UriFormatException
            )
        {
            MessageBox.Show(
                this,
                string.Format(UiText.Get("UpdateCheckFailed"), ex.Message),
                UiText.Get("Error"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error
            );
        }
        finally
        {
            _checkUpdatesButton.Enabled = true;
            _checkUpdatesButton.Text = UiText.Get("CheckNow");
        }
    }

    private void SaveSettings()
    {
        lock (_settingsGate)
        {
            _settingsStore.Save(_settings);
        }
    }
}

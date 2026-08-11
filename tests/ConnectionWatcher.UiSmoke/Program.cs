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
        RenderMainWindow(output, "zh-TW", "zh-tw");
        RenderMainWindow(output, "en", "en");
        RenderMainWindow(output, "es", "es");
        RenderMainWindow(output, "fr", "fr");
        RenderMainWindow(output, "de", "de");
        RenderMainWindow(output, "pt-BR", "pt-br");
        RenderRuleEditor(output, "zh-CN", "zh");
        RenderRuleEditor(output, "zh-TW", "zh-tw");
        RenderRuleEditor(output, "en", "en");
        RenderRuleEditor(output, "es", "es");
        RenderRuleEditor(output, "fr", "fr");
        RenderRuleEditor(output, "de", "de");
        RenderRuleEditor(output, "pt-BR", "pt-br");
        RenderUrgentAlert(output, "zh-CN", "zh");
        RenderUrgentAlert(output, "zh-TW", "zh-tw");
        RenderUrgentAlert(output, "en", "en");
        RenderUrgentAlert(output, "es", "es");
        RenderUrgentAlert(output, "fr", "fr");
        RenderUrgentAlert(output, "de", "de");
        RenderUrgentAlert(output, "pt-BR", "pt-br");
        RenderEventDetails(output, "zh-CN", "zh");
        RenderEventDetails(output, "zh-TW", "zh-tw");
        RenderEventDetails(output, "en", "en");
        RenderEventDetails(output, "es", "es");
        RenderEventDetails(output, "fr", "fr");
        RenderEventDetails(output, "de", "de");
        RenderEventDetails(output, "pt-BR", "pt-br");
        RenderHelpCenter(output, "zh-CN", "zh");
        RenderHelpCenter(output, "zh-TW", "zh-tw");
        RenderHelpCenter(output, "en", "en");
        RenderHelpCenter(output, "es", "es");
        RenderHelpCenter(output, "fr", "fr");
        RenderHelpCenter(output, "de", "de");
        RenderHelpCenter(output, "pt-BR", "pt-br");
        RenderLanguageSelection(output);
        AssertEmbeddedAlertSound();
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
        DataGridView rulesGrid = form.Controls.Find("RulesGrid", true)
            .OfType<DataGridView>()
            .Single();
        AssertRuleGridLayout(rulesGrid);
        AssertRuleActionMarkers(rulesGrid);
        AssertPageTitles(form, language);
        form.Size = form.MinimumSize;
        Application.DoEvents();
        AssertRuleGridLayout(rulesGrid);
        Capture(form, Path.Combine(output, $"main-{filePrefix}-rules-minimum.png"));
        form.Size = new Size(1100, 720);
        Application.DoEvents();

        ClickNavigation(form, UiText.Get("Home"));
        Capture(form, Path.Combine(output, $"main-{filePrefix}-home.png"));
        Button monitorButton = form.Controls.Find("MonitorButton", true)
            .OfType<Button>()
            .Single();
        Label homeSubtitle = form.Controls.Find("HomeSubtitle", true)
            .OfType<Label>()
            .Single();
        form.Size = form.MinimumSize;
        Application.DoEvents();
        AssertFullyVisible(form, monitorButton);
        AssertTextFits(monitorButton);
        AssertNoHorizontalOverlap(form, homeSubtitle, monitorButton);
        AssertActionLegend(form);
        AssertHomeLayout(form);
        Capture(form, Path.Combine(output, $"main-{filePrefix}-home-minimum.png"));
        AssertFontFamily(
            form,
            language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft YaHei UI"
                : "Segoe UI");

        PopulateSampleEvents(form);
        form.Size = new Size(1100, 720);
        Application.DoEvents();
        ClickNavigation(form, UiText.Get("Events"));
        AssertEventActionMarkers(form);
        AssertEventDoubleClickOpensDetails(form);
        Label eventHint = form.Controls.Find("EventDetailsHint", true)
            .OfType<Label>()
            .Single();
        AssertFullyVisible(form, eventHint);
        AssertTextFits(eventHint);
        Capture(form, Path.Combine(output, $"main-{filePrefix}-events.png"));

        ClickNavigation(form, UiText.Get("Settings"));
        ComboBox languageBox = form.Controls.Find("LanguageComboBox", true)
            .OfType<ComboBox>()
            .Single();
        if (languageBox.Items.Count != 7)
        {
            throw new InvalidOperationException(
                $"Expected 7 interface languages, found {languageBox.Items.Count}.");
        }
        Button testSound = form.Controls.Find("TestAlertSoundButton", true)
            .OfType<Button>()
            .Single();
        AssertFullyVisible(form, testSound);
        AssertTextFits(testSound);
        AssertSettingsControlAlignment(form, testSound);
        Capture(form, Path.Combine(output, $"main-{filePrefix}-settings.png"));
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
        AssertFontFamily(
            form,
            language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft YaHei UI"
                : "Segoe UI");
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
        AssertFontFamily(
            form,
            language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft YaHei UI"
                : "Segoe UI");
        Control notice = form.Controls.Find("Notice", true).Single();
        AssertFullyVisible(form, notice);
        AssertTextFits(notice);
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
        AssertFontFamily(
            form,
            language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft YaHei UI"
                : "Segoe UI");
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

    private static void RenderEventDetails(
        string output,
        string language,
        string filePrefix)
    {
        UiText.SetLanguage(language);
        ConnectionEvent entry = SampleEvents()[0];
        using EventDetailsForm form = new(entry)
        {
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        form.Size = form.MinimumSize;
        Application.DoEvents();
        AssertFontFamily(
            form,
            language.StartsWith("zh", StringComparison.OrdinalIgnoreCase)
                ? "Microsoft YaHei UI"
                : "Segoe UI");

        Button copy = form.Controls.Find("CopyDetailsButton", true)
            .OfType<Button>()
            .Single();
        Button close = form.Controls.Find("CloseButton", true)
            .OfType<Button>()
            .Single();
        AssertFullyVisible(form, copy);
        AssertFullyVisible(form, close);
        AssertTextFits(copy);
        AssertTextFits(close);

        string[] valueNames =
        [
            "StartTimeValue", "EndTimeValue", "ConnectionStatusValue",
            "ObservedDurationValue", "MatchedRulesValue",
            "RemoteEndpointValue", "LocalEndpointValue", "TcpStateValue",
            "PIDValue", "ProgramValue", "ProcessPathValue", "ActionColumnValue"
        ];
        TextBox[] values = valueNames
            .Select(name => form.Controls.Find(name, true).OfType<TextBox>().Single())
            .ToArray();
        if (values.Any(value => string.IsNullOrWhiteSpace(value.Text)) ||
            !values[2].Text.StartsWith("●", StringComparison.Ordinal) ||
            values[3].Text.Count(character => character == ':') != 2)
        {
            throw new InvalidOperationException(
                "The event details window does not show every required field.");
        }
        string[] fieldKeys = valueNames
            .Select(name => name[..^"Value".Length])
            .ToArray();
        for (int index = 0; index < fieldKeys.Length; index++)
        {
            Label label = form.Controls.Find($"{fieldKeys[index]}Label", true)
                .OfType<Label>()
                .Single();
            int labelCenter = label.Top + label.Height / 2;
            int valueCenter = values[index].Top + values[index].Height / 2;
            if (Math.Abs(labelCenter - valueCenter) > 6)
            {
                throw new InvalidOperationException(
                    $"Event detail field '{fieldKeys[index]}' is misaligned.");
            }
        }
        System.Reflection.MethodInfo buildCopyText = typeof(EventDetailsForm)
            .GetMethod(
                "BuildCopyText",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException(
                "The event details copy payload was not found.");
        string copied = (string)buildCopyText.Invoke(form, null)!;
        if (!copied.Contains(entry.RemoteAddress, StringComparison.Ordinal) ||
            !copied.Contains(entry.ProcessPath!, StringComparison.Ordinal) ||
            !copied.Contains(entry.RuleNames[0], StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Copy details does not include the full event record.");
        }

        Capture(form, Path.Combine(output, $"event-details-{filePrefix}.png"));
        form.Hide();
    }

    private static void RenderLanguageSelection(string output)
    {
        using LanguageSelectionForm form = new()
        {
            Opacity = 0,
            ShowInTaskbar = false
        };
        form.Show();
        Application.DoEvents();
        Button[] languageButtons = Descendants(form).OfType<Button>().ToArray();
        if (languageButtons.Length != 7)
        {
            throw new InvalidOperationException(
                "The first-run language selector must offer seven languages.");
        }
        foreach (Button button in languageButtons)
        {
            AssertFullyVisible(form, button);
            AssertTextFits(button);
        }
        Capture(form, Path.Combine(output, "language-selection.png"));
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

    private static void AssertTextFits(Control control)
    {
        if (control is Label label)
        {
            Size measured = TextRenderer.MeasureText(
                label.Text,
                label.Font,
                new Size(Math.Max(1, label.ClientSize.Width), int.MaxValue),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPadding);
            // TextRenderer includes a few pixels of internal leading that a
            // WinForms Label does not report in ClientSize.
            if (measured.Height > label.ClientSize.Height + 6)
            {
                throw new InvalidOperationException(
                    $"Control '{control.Name}' clips its wrapped text: " +
                    $"required height {measured.Height}, actual {label.ClientSize.Height}.");
            }
            return;
        }

        Size preferred = control.GetPreferredSize(Size.Empty);
        if (preferred.Width > control.Width || preferred.Height > control.Height)
        {
            throw new InvalidOperationException(
                $"Control '{control.Name}' clips its text: " +
                $"preferred {preferred}, actual {control.Size}.");
        }
    }

    private static void AssertNoHorizontalOverlap(
        Form form,
        Control left,
        Control right)
    {
        Rectangle leftBounds = form.RectangleToClient(
            left.Parent!.RectangleToScreen(left.Bounds));
        Rectangle rightBounds = form.RectangleToClient(
            right.Parent!.RectangleToScreen(right.Bounds));
        if (leftBounds.Right > rightBounds.Left - 10)
        {
            throw new InvalidOperationException(
                $"Control '{left.Name}' overlaps '{right.Name}'.");
        }
    }

    private static void AssertActionLegend(Form form)
    {
        Label[] labels =
        [
            form.Controls.Find("SilentActionLegend", true).OfType<Label>().Single(),
            form.Controls.Find("TrayActionLegend", true).OfType<Label>().Single(),
            form.Controls.Find("PopupActionLegend", true).OfType<Label>().Single()
        ];
        string[] markers = ["1 ●", "2 ▲", "3 ◆"];
        for (int index = 0; index < labels.Length; index++)
        {
            if (!labels[index].Text.StartsWith(markers[index], StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Legend item {index} does not start with '{markers[index]}'.");
            }
            AssertFullyVisible(form, labels[index]);
            AssertTextFits(labels[index]);
        }
        if (labels.Select(label => label.ForeColor.ToArgb()).Distinct().Count() != 3)
        {
            throw new InvalidOperationException(
                "The three action legend items must use distinct colors.");
        }
    }

    private static void AssertHomeLayout(Form form)
    {
        Label subtitle = form.Controls.Find("HomeSubtitle", true)
            .OfType<Label>()
            .Single();
        TableLayoutPanel summary = form.Controls.Find("HomeSummary", true)
            .OfType<TableLayoutPanel>()
            .Single();
        Rectangle subtitleBounds = form.RectangleToClient(
            subtitle.Parent!.RectangleToScreen(subtitle.Bounds));
        Rectangle summaryBounds = form.RectangleToClient(
            summary.Parent!.RectangleToScreen(summary.Bounds));
        DataGridView rulesGrid = form.Controls.Find("RulesGrid", true)
            .OfType<DataGridView>()
            .Single();
        Rectangle rulesBounds = form.RectangleToClient(
            rulesGrid.Parent!.RectangleToScreen(rulesGrid.Bounds));
        if (summaryBounds.Top - subtitleBounds.Bottom < 20 ||
            Math.Abs(summaryBounds.Top - rulesBounds.Top) > 32)
        {
            throw new InvalidOperationException(
                "The Home title-to-content spacing is inconsistent with other pages.");
        }
    }

    private static void AssertSettingsControlAlignment(Form form, Button testSound)
    {
        CheckBox start = form.Controls.Find("StartWithWindowsCheckBox", true)
            .OfType<CheckBox>()
            .Single();
        CheckBox resume = form.Controls.Find("ResumeMonitoringCheckBox", true)
            .OfType<CheckBox>()
            .Single();
        CheckBox alert = form.Controls.Find("AlertSoundCheckBox", true)
            .OfType<CheckBox>()
            .Single();
        NumericUpDown volume = form.Controls.Find("AlertVolumeInput", true)
            .OfType<NumericUpDown>()
            .Single();
        int[] leftEdges = [ControlLeft(form, start), ControlLeft(form, resume), ControlLeft(form, alert)];
        if (leftEdges.Max() - leftEdges.Min() > 5 ||
            ControlLeft(form, testSound) >= ControlLeft(form, alert))
        {
            throw new InvalidOperationException(
                "Settings checkboxes are not aligned or Test sound is not before its checkbox.");
        }
        if (volume.Minimum != AppSettings.MinimumAlertVolumePercent ||
            volume.Maximum != AppSettings.MaximumAlertVolumePercent ||
            volume.Increment != 5 ||
            volume.Value != AppSettings.DefaultAlertVolumePercent)
        {
            throw new InvalidOperationException(
                "The alert volume control does not use the expected 10–100% range and 40% default.");
        }
        AssertFullyVisible(form, volume);
    }

    private static int ControlLeft(Form form, Control control)
    {
        return form.RectangleToClient(
            control.Parent!.RectangleToScreen(control.Bounds)).Left;
    }

    private static void AssertEmbeddedAlertSound()
    {
        Type soundType = typeof(MainForm).Assembly.GetType(
            "ConnectionWatcher.App.Services.AlertSoundPlayer") ??
            throw new InvalidOperationException("AlertSoundPlayer was not found.");
        System.Reflection.MethodInfo createWave = soundType.GetMethod(
            "CreateWave",
            System.Reflection.BindingFlags.Static |
            System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException("Embedded alert wave generator was not found.");
        byte[] lowWave = (byte[])createWave.Invoke(null, [10])!;
        byte[] fullWave = (byte[])createWave.Invoke(null, [100])!;
        if (!IsWave(lowWave) || !IsWave(fullWave) ||
            PeakAmplitude(fullWave) <= PeakAmplitude(lowWave) * 8)
        {
            throw new InvalidOperationException(
                "The embedded alert sound is invalid or its volume is not scaled.");
        }
    }

    private static bool IsWave(byte[] wave) =>
        wave.Length >= 1_000 &&
        System.Text.Encoding.ASCII.GetString(wave, 0, 4) == "RIFF" &&
        System.Text.Encoding.ASCII.GetString(wave, 8, 4) == "WAVE";

    private static int PeakAmplitude(byte[] wave)
    {
        int peak = 0;
        for (int offset = 44; offset + 1 < wave.Length; offset += 2)
        {
            peak = Math.Max(peak, Math.Abs((int)BitConverter.ToInt16(wave, offset)));
        }

        return peak;
    }

    private static void AssertPageTitles(Form form, string language)
    {
        string[] names = ["HomeTitle", "RulesTitle", "EventsTitle", "SettingsTitle"];
        foreach (string name in names)
        {
            Label title = form.Controls.Find(name, true).OfType<Label>().Single();
            if (title.Font.Size < 19.5F)
            {
                throw new InvalidOperationException(
                    $"Page title '{name}' is smaller than 20 pt.");
            }
        }
        if (language == "en" &&
            (UiText.Get("Rules") != "Monitoring Rules" ||
             UiText.Get("Events") != "Event Log"))
        {
            throw new InvalidOperationException(
                "English page titles must use title case.");
        }
    }

    private static void AssertRuleActionMarkers(DataGridView grid)
    {
        MatchAction[] actions =
        [
            MatchAction.PopupAlert,
            MatchAction.SilentLog,
            MatchAction.TrayNotice
        ];
        if (grid.Rows.Count != actions.Length)
        {
            throw new InvalidOperationException(
                "The rule marker test requires exactly three rows.");
        }
        for (int index = 0; index < actions.Length; index++)
        {
            DataGridViewCell cell = grid.Rows[index].Cells[2];
            if (!string.Equals(
                    cell.Value?.ToString(),
                    UiText.ActionCompact(actions[index]),
                    StringComparison.Ordinal) ||
                !string.Equals(
                    cell.ToolTipText,
                    UiText.Action(actions[index]),
                    StringComparison.Ordinal) ||
                cell.Style.SelectionForeColor != cell.Style.ForeColor)
            {
                throw new InvalidOperationException(
                    $"Rule action marker {index} is incorrect.");
            }
        }
        if (grid.Rows.Cast<DataGridViewRow>()
                .Select(row => row.Cells[2].Style.ForeColor.ToArgb())
                .Distinct()
                .Count() != 3)
        {
            throw new InvalidOperationException(
                "Rule action markers must use three distinct colors.");
        }
    }

    private static void PopulateSampleEvents(MainForm form)
    {
        System.Reflection.FieldInfo eventsField = typeof(MainForm).GetField(
            "_events",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException("MainForm event storage was not found.");
        List<ConnectionEvent> events =
            (List<ConnectionEvent>)eventsField.GetValue(form)!;
        events.Clear();
        events.AddRange(SampleEvents());
        System.Reflection.MethodInfo refresh = typeof(MainForm).GetMethod(
            "RefreshEvents",
            System.Reflection.BindingFlags.Instance |
            System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException("RefreshEvents was not found.");
        refresh.Invoke(form, null);
        Application.DoEvents();
    }

    private static void AssertEventActionMarkers(Form form)
    {
        DataGridView grid = form.Controls.Find("EventsGrid", true)
            .OfType<DataGridView>()
            .Single();
        MatchAction[] actions =
        [
            MatchAction.SilentLog,
            MatchAction.TrayNotice,
            MatchAction.PopupAlert
        ];
        if (grid.Rows.Count != actions.Length)
        {
            throw new InvalidOperationException(
                "The event marker test requires exactly three rows.");
        }
        for (int index = 0; index < actions.Length; index++)
        {
            DataGridViewCell cell = grid.Rows[index].Cells[5];
            if (!string.Equals(
                    cell.Value?.ToString(),
                    UiText.ActionMarker(actions[index]),
                    StringComparison.Ordinal) ||
                !string.Equals(
                    cell.ToolTipText,
                    UiText.Action(actions[index]),
                    StringComparison.Ordinal))
            {
                throw new InvalidOperationException(
                    $"Event action marker {index} is incorrect.");
            }
        }
        if (grid.Rows.Cast<DataGridViewRow>()
                .Select(row => row.Cells[5].Style.ForeColor.ToArgb())
                .Distinct()
                .Count() != 3)
        {
            throw new InvalidOperationException(
                "Event action markers must use three distinct colors.");
        }
        if (grid.Rows.Cast<DataGridViewRow>().Any(row =>
                row.Cells[5].Style.SelectionForeColor !=
                row.Cells[5].Style.ForeColor))
        {
            throw new InvalidOperationException(
                "Selected event markers must keep their action color.");
        }
        if (grid.Columns.Count != 6 ||
            grid.Rows[0].Cells[1].Value?.ToString() != $"● {UiText.Get("ConnectionActive")}" ||
            grid.Rows[1].Cells[1].Value?.ToString() != $"○ {UiText.Get("ConnectionEnded")}" ||
            grid.Rows[0].Cells[2].Value?.ToString() is not string activeDuration ||
            activeDuration.Count(character => character == ':') != 2 ||
            grid.Rows[1].Cells[2].Value?.ToString() != "—")
        {
            throw new InvalidOperationException(
                "The Event Log does not clearly show status and observed duration.");
        }
        if (grid.Rows[0].Cells[1].Style.SelectionForeColor !=
            SystemColors.HighlightText)
        {
            throw new InvalidOperationException(
                "Active status must remain readable when its row is selected.");
        }

        int totalWidth = grid.Columns
            .Cast<DataGridViewColumn>()
            .Where(column => column.Visible)
            .Sum(column => column.Width);
        if (totalWidth > grid.ClientSize.Width + 2)
        {
            throw new InvalidOperationException(
                "The Event Log overview requires horizontal scrolling.");
        }
    }

    private static void AssertEventDoubleClickOpensDetails(Form form)
    {
        DataGridView grid = form.Controls.Find("EventsGrid", true)
            .OfType<DataGridView>()
            .Single();
        bool opened = false;
        using System.Windows.Forms.Timer closeTimer = new() { Interval = 50 };
        closeTimer.Tick += (_, _) =>
        {
            EventDetailsForm? details = Application.OpenForms
                .OfType<EventDetailsForm>()
                .FirstOrDefault();
            if (details is null)
            {
                return;
            }

            opened = true;
            details.Close();
        };
        System.Reflection.MethodInfo raiseDoubleClick = typeof(DataGridView)
            .GetMethod(
                "OnCellDoubleClick",
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic) ??
            throw new InvalidOperationException(
                "Could not raise the Event Log double-click action.");
        closeTimer.Start();
        raiseDoubleClick.Invoke(
            grid,
            [new DataGridViewCellEventArgs(0, 0)]);
        closeTimer.Stop();
        if (!opened)
        {
            throw new InvalidOperationException(
                "Double-clicking an Event Log row did not open Event details.");
        }
    }

    private static void AssertRuleGridLayout(DataGridView grid)
    {
        if (grid.Columns.Count != 4 || grid.Columns[3].Width < 100)
        {
            throw new InvalidOperationException(
                "The Enabled column does not have enough horizontal space.");
        }

        int totalWidth = grid.Columns
            .Cast<DataGridViewColumn>()
            .Where(column => column.Visible)
            .Sum(column => column.Width);
        if (totalWidth > grid.ClientSize.Width)
        {
            throw new InvalidOperationException(
                $"The rule columns require {totalWidth}px but the grid provides " +
                $"{grid.ClientSize.Width}px.");
        }

        foreach (DataGridViewColumn column in grid.Columns)
        {
            if (column.DefaultCellStyle.Alignment !=
                    DataGridViewContentAlignment.MiddleCenter ||
                column.HeaderCell.InheritedStyle.Alignment !=
                    DataGridViewContentAlignment.MiddleCenter)
            {
                throw new InvalidOperationException(
                    $"Rule column {column.Index} is not centered.");
            }
        }
    }

    private static void AssertFontFamily(Control root, string expectedFamily)
    {
        Control? mismatch = Descendants(root)
            .Prepend(root)
            .FirstOrDefault(control => !string.Equals(
                control.Font.FontFamily.Name,
                expectedFamily,
                StringComparison.OrdinalIgnoreCase));
        if (mismatch is not null)
        {
            throw new InvalidOperationException(
                $"Control '{mismatch.Name}' uses '{mismatch.Font.FontFamily.Name}' " +
                $"instead of '{expectedFamily}'.");
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
            },
            new MonitoringRule
            {
                Name = "托盘提醒示例",
                RemoteIp = "198.51.100.25",
                RemotePort = new PortRange(443, 443),
                LocalPort = PortRange.Any,
                Action = MatchAction.TrayNotice,
                Enabled = true
            }
        ];
    }

    private static List<ConnectionEvent> SampleEvents()
    {
        MatchAction[] actions =
        [
            MatchAction.SilentLog,
            MatchAction.TrayNotice,
            MatchAction.PopupAlert
        ];
        DateTimeOffset firstSeen = DateTimeOffset.Now.AddMinutes(-2);
        List<ConnectionEvent> events = actions.Select((action, index) => new ConnectionEvent
        {
            DetectedAt = firstSeen.AddSeconds(index * 10),
            RuleIds = [Guid.NewGuid()],
            RuleNames = [$"Sample rule {index + 1}"],
            Action = action,
            LocalAddress = "172.20.10.2",
            LocalPort = 61000 + index,
            RemoteAddress = "103.1.40.235",
            RemotePort = 1433,
            State = System.Net.NetworkInformation.TcpState.Established,
            ProcessId = 2400 + index,
            ProcessName = $"sample{index + 1}.exe",
            ProcessPath = $@"C:\Sample\sample{index + 1}.exe"
        }).ToList();
        events[1].MarkHistoricalInactive();
        return events;
    }
}

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

// Assemble the shell and switch pages without recreating their state.
public sealed partial class MainForm
{
    private void BuildInterface()
    {
        Text = UiText.Get("AppTitle");
        Icon = (Icon)_applicationIcon.Clone();
        StartPosition = FormStartPosition.CenterScreen;
        MinimumSize = new Size(920, 620);
        ClientSize = new Size(1100, 720);
        Font = new Font("Segoe UI", 9.5F);
        AutoScaleMode = AutoScaleMode.Dpi;

        TableLayoutPanel root = new()
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 1,
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 205));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

        Panel sidebar = BuildSidebar();
        Panel content = new()
        {
            Dock = DockStyle.Fill,
            Padding = new Padding(24),
        };
        _pages["Home"] = BuildHomePage();
        _pages["Rules"] = BuildRulesPage();
        _pages["Events"] = BuildEventsPage();
        _pages["Settings"] = BuildSettingsPage();
        foreach (Panel page in _pages.Values)
        {
            page.Dock = DockStyle.Fill;
            page.Visible = false;
            content.Controls.Add(page);
        }

        root.Controls.Add(sidebar, 0, 0);
        root.Controls.Add(content, 1, 0);
        Controls.Add(root);
        ShowPage("Rules");
    }

    private Panel BuildSidebar()
    {
        Panel sidebar = new()
        {
            Dock = DockStyle.Fill,
            BackColor = SystemColors.ControlLight,
            Padding = new Padding(10),
        };
        TableLayoutPanel layout = new()
        {
            Dock = DockStyle.Fill,
            RowCount = 6,
            ColumnCount = 1,
        };
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        layout.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        int row = 0;
        foreach (string key in new[] { "Home", "Rules", "Events", "Settings" })
        {
            Button button = new()
            {
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                Height = 44,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(12, 0, 0, 0),
                Margin = new Padding(0, 0, 0, 5),
            };
            button.FlatAppearance.BorderSize = 0;
            button.Click += (_, _) => ShowPage(key);
            _navButtons[key] = button;
            layout.Controls.Add(button, 0, row++);
        }

        FlowLayoutPanel status = new()
        {
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            AutoSize = true,
            Dock = DockStyle.Bottom,
            Padding = new Padding(8, 12, 8, 4),
        };
        _sidebarStatus.AutoSize = true;
        _sidebarStatus.Font = new Font("Segoe UI", 9.5F, FontStyle.Bold);
        _sidebarHint.AutoSize = true;
        _sidebarHint.MaximumSize = new Size(170, 0);
        _sidebarHint.ForeColor = SystemColors.GrayText;
        status.Controls.Add(_sidebarStatus);
        status.Controls.Add(_sidebarHint);
        layout.Controls.Add(status, 0, 5);
        sidebar.Controls.Add(layout);
        return sidebar;
    }

    private void ShowPage(string key)
    {
        foreach ((string pageKey, Panel page) in _pages)
        {
            page.Visible = pageKey == key;
        }

        foreach ((string buttonKey, Button button) in _navButtons)
        {
            button.BackColor =
                buttonKey == key
                    ? SystemColors.GradientActiveCaption
                    : SystemColors.ControlLight;
        }

        if (key == "Events")
        {
            _trayNoticeCount = 0;
            UpdateTrayIcon();
            RefreshEvents();
        }
    }
}

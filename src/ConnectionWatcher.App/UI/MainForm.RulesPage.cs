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

// Own rule editing and refresh guards for the rules grid.
public sealed partial class MainForm
{
    private Panel BuildRulesPage()
    {
        Panel page = new();
        _rulesTitle.Name = "RulesTitle";
        _newRuleButton.AutoSize = true;
        _newRuleButton.MinimumSize = new Size(110, 36);
        _newRuleButton.Click += (_, _) => AddRule();
        TableLayoutPanel header = Header(
            _rulesTitle,
            _rulesSubtitle,
            _newRuleButton
        );

        _rulesGrid.Name = "RulesGrid";
        _rulesGrid.ColumnHeadersDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _rulesGrid.RowsDefaultCellStyle.Alignment =
            DataGridViewContentAlignment.MiddleCenter;
        _rulesGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 20,
                MinimumWidth = 120,
            }
        );
        _rulesGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 42,
                MinimumWidth = 235,
            }
        );
        _rulesGrid.Columns.Add(
            new DataGridViewTextBoxColumn
            {
                FillWeight = 24,
                MinimumWidth = 160,
            }
        );
        _rulesGrid.Columns.Add(
            new DataGridViewCheckBoxColumn
            {
                FillWeight = 14,
                MinimumWidth = 100,
            }
        );
        _rulesGrid.ReadOnly = false;
        foreach (DataGridViewColumn column in _rulesGrid.Columns)
        {
            column.DefaultCellStyle.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
            column.HeaderCell.Style.Alignment =
                DataGridViewContentAlignment.MiddleCenter;
        }
        foreach (
            DataGridViewColumn column in _rulesGrid
                .Columns.Cast<DataGridViewColumn>()
                .Take(3)
        )
        {
            column.ReadOnly = true;
        }
        _rulesGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_rulesGrid.IsCurrentCellDirty)
            {
                _rulesGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        _rulesGrid.CellValueChanged += RuleEnabledChanged;
        _rulesGrid.SelectionChanged += (_, _) => UpdateRuleButtons();
        _rulesGrid.CellDoubleClick += (_, args) =>
        {
            if (args.RowIndex >= 0)
            {
                EditSelectedRule();
            }
        };

        FlowLayoutPanel actions = new()
        {
            Dock = DockStyle.Bottom,
            AutoSize = true,
            Padding = new Padding(0, 10, 0, 0),
        };
        _editRuleButton.AutoSize = true;
        _deleteRuleButton.AutoSize = true;
        _editRuleButton.Click += (_, _) => EditSelectedRule();
        _deleteRuleButton.Click += (_, _) => DeleteSelectedRule();
        actions.Controls.AddRange([_editRuleButton, _deleteRuleButton]);

        Panel body = new() { Dock = DockStyle.Fill };
        body.Controls.Add(_rulesGrid);
        body.Controls.Add(actions);
        body.Controls.Add(header);
        _rulesGrid.BringToFront();
        _rulesGrid.Margin = new Padding(
            0,
            header.Height + 20,
            0,
            actions.Height
        );
        page.Controls.Add(body);

        header.Dock = DockStyle.Top;
        actions.Dock = DockStyle.Bottom;
        _rulesGrid.Dock = DockStyle.Fill;
        return page;
    }

    /// <summary>
    /// Refresh rows while suppressing change handlers triggered by binding.
    /// </summary>
    private void RefreshRules()
    {
        if (_rulesGrid.Columns.Count == 0)
        {
            return;
        }

        _refreshingRules = true;
        try
        {
            _rulesGrid.Rows.Clear();
            foreach (MonitoringRule rule in _settings.Rules)
            {
                int index = _rulesGrid.Rows.Add(
                    rule.Name,
                    UiText.FormatRuleCondition(rule),
                    UiText.ActionCompact(rule.Action),
                    rule.Enabled
                );
                _rulesGrid.Rows[index].Tag = rule.Id;
                DataGridViewCell actionCell = _rulesGrid.Rows[index].Cells[2];
                Color actionColor = ActionColor(rule.Action);
                actionCell.ToolTipText = UiText.Action(rule.Action);
                actionCell.Style.ForeColor = actionColor;
                actionCell.Style.SelectionForeColor = actionColor;
                actionCell.Style.SelectionBackColor = SystemColors.Window;
            }
        }
        finally
        {
            _refreshingRules = false;
        }

        UpdateRuleButtons();
        _homeRules.Text = _settings
            .Rules.Count(rule => rule.Enabled)
            .ToString(CultureInfo.CurrentCulture);
    }

    private void AddRule()
    {
        using RuleEditorForm editor = new();
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        lock (_settingsGate)
        {
            _settings.Rules.Add(editor.ResultRule);
            SaveSettings();
        }
        RefreshRules();
    }

    private void EditSelectedRule()
    {
        MonitoringRule? selected = SelectedRule();
        if (selected is null)
        {
            return;
        }

        using RuleEditorForm editor = new(selected);
        if (editor.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        int index = _settings.Rules.FindIndex(rule => rule.Id == selected.Id);
        if (index >= 0)
        {
            lock (_settingsGate)
            {
                _settings.Rules[index] = editor.ResultRule;
                SaveSettings();
            }
            RefreshRules();
        }
    }

    /// <summary>
    /// Confirm the selected rule's deletion without removing past events.
    /// </summary>
    private void DeleteSelectedRule()
    {
        MonitoringRule? selected = SelectedRule();
        if (selected is null)
        {
            return;
        }

        DialogResult result = MessageBox.Show(
            string.Format(UiText.Get("DeleteRuleQuestion"), selected.Name),
            UiText.Get("DeleteRuleTitle"),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question
        );
        if (result != DialogResult.Yes)
        {
            return;
        }

        lock (_settingsGate)
        {
            _settings.Rules.RemoveAll(rule => rule.Id == selected.Id);
            SaveSettings();
        }
        RefreshRules();
    }

    private void UpdateRuleButtons()
    {
        bool selected = _rulesGrid.SelectedRows.Count > 0;
        _editRuleButton.Enabled = selected;
        _deleteRuleButton.Enabled = selected;
    }

    /// <summary>
    /// Persist user changes while ignoring refresh-generated grid events.
    /// </summary>
    private void RuleEnabledChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (_refreshingRules || e.RowIndex < 0 || e.ColumnIndex != 3)
        {
            return;
        }

        DataGridViewRow row = _rulesGrid.Rows[e.RowIndex];
        if (row.Tag is Guid id && row.Cells[3].Value is bool enabled)
        {
            MonitoringRule? rule = _settings.Rules.FirstOrDefault(item =>
                item.Id == id
            );
            if (rule is not null)
            {
                lock (_settingsGate)
                {
                    rule.Enabled = enabled;
                    SaveSettings();
                }
                _homeRules.Text = _settings
                    .Rules.Count(item => item.Enabled)
                    .ToString(CultureInfo.CurrentCulture);
            }
        }
    }

    /// <summary>
    /// Copy enabled rules while holding the same lock used by settings writes.
    /// </summary>
    private IReadOnlyList<MonitoringRule> GetRulesSnapshot()
    {
        lock (_settingsGate)
        {
            return _settings.Rules.Select(rule => rule.Copy()).ToArray();
        }
    }
}

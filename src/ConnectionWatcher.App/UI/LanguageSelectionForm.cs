namespace ConnectionWatcher.App.UI;

public sealed class LanguageSelectionForm : Form
{
    public LanguageSelectionForm()
    {
        Icon = AppIconProvider.Load();
        Text = "Choose language";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(780, 245);
        Font = new Font("Segoe UI", 10F);

        Label prompt = new()
        {
            Text = "Choose the interface language",
            AutoSize = true,
            Location = new Point(30, 24),
            Font = new Font("Segoe UI", 11F, FontStyle.Bold)
        };
        TableLayoutPanel choices = new()
        {
            Location = new Point(26, 66),
            Size = new Size(728, 132),
            ColumnCount = 4,
            RowCount = 2
        };
        for (int index = 0; index < 4; index++)
        {
            choices.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));
        }
        choices.RowStyles.Add(new RowStyle(SizeType.Percent, 50));
        choices.RowStyles.Add(new RowStyle(SizeType.Percent, 50));

        (string Code, string Label)[] languages =
        [
            ("en", "English"),
            ("zh-CN", "简体中文"),
            ("zh-TW", "繁體中文"),
            ("es", "Español"),
            ("fr", "Français"),
            ("de", "Deutsch"),
            ("pt-BR", "Português (Brasil)")
        ];
        for (int index = 0; index < languages.Length; index++)
        {
            (string code, string label) = languages[index];
            Button button = new()
            {
                Text = label,
                Dock = DockStyle.Fill,
                Margin = new Padding(8),
                MinimumSize = new Size(150, 42)
            };
            button.Click += (_, _) => Select(code);
            choices.Controls.Add(button, index % 4, index / 4);
        }

        Controls.AddRange([prompt, choices]);
        UiFont.Apply(this, bilingual: true);
    }

    public string SelectedLanguage { get; private set; } = string.Empty;

    private void Select(string language)
    {
        SelectedLanguage = language;
        DialogResult = DialogResult.OK;
        Close();
    }
}

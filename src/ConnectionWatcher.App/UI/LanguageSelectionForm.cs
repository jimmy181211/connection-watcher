namespace ConnectionWatcher.App.UI;

public sealed class LanguageSelectionForm : Form
{
    public LanguageSelectionForm()
    {
        Text = "Choose Language / 请选择语言";
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        ClientSize = new Size(380, 165);
        Font = new Font("Segoe UI", 10F);

        Label prompt = new()
        {
            Text = "请选择界面语言 / Choose the interface language",
            AutoSize = true,
            Location = new Point(30, 28)
        };
        Button chinese = new()
        {
            Text = "中文",
            Size = new Size(140, 42),
            Location = new Point(30, 82)
        };
        Button english = new()
        {
            Text = "English",
            Size = new Size(140, 42),
            Location = new Point(205, 82)
        };

        chinese.Click += (_, _) => Select("zh-CN");
        english.Click += (_, _) => Select("en");
        Controls.AddRange([prompt, chinese, english]);
    }

    public string SelectedLanguage { get; private set; } = string.Empty;

    private void Select(string language)
    {
        SelectedLanguage = language;
        DialogResult = DialogResult.OK;
        Close();
    }
}

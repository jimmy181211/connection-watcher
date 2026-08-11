using ConnectionWatcher.App.Localization;

namespace ConnectionWatcher.App.UI;

internal static class UiFont
{
    private const string EnglishFamily = "Segoe UI";
    private const string ChineseFamily = "Microsoft YaHei UI";

    public static string FamilyName => UiText.IsChinese
        ? ChineseFamily
        : EnglishFamily;

    public static Font Create(float size, FontStyle style = FontStyle.Regular)
    {
        return new Font(FamilyName, size, style);
    }

    public static void Apply(Control root, bool bilingual = false)
    {
        string family = bilingual ? ChineseFamily : FamilyName;
        ApplyRecursive(root, family);
    }

    private static void ApplyRecursive(Control control, string family)
    {
        Font current = control.Font;
        if (!string.Equals(
                current.FontFamily.Name,
                family,
                StringComparison.OrdinalIgnoreCase))
        {
            control.Font = new Font(
                family,
                current.Size,
                current.Style,
                current.Unit,
                current.GdiCharSet,
                current.GdiVerticalFont);
        }

        foreach (Control child in control.Controls)
        {
            ApplyRecursive(child, family);
        }
    }
}

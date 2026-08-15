namespace ConnectionWatcher.App.Services;

public static class InstallerLanguagePreference
{
    public const string FileName = "install-language.txt";

    public static string? Consume(string applicationDirectory)
    {
        string path = Path.Combine(applicationDirectory, FileName);
        if (!File.Exists(path))
        {
            return null;
        }

        string selectedLanguage;
        try
        {
            selectedLanguage = File.ReadAllText(path).Trim();
        }
        catch (IOException)
        {
            return null;
        }
        catch (UnauthorizedAccessException)
        {
            return null;
        }

        try
        {
            File.Delete(path);
        }
        catch (IOException)
        {
            // The selected language can still be applied for this launch.
        }
        catch (UnauthorizedAccessException)
        {
            // The selected language can still be applied for this launch.
        }

        return IsSupported(selectedLanguage) ? selectedLanguage : null;
    }

    public static bool IsSupported(string language) =>
        language is "zh-CN" or "zh-TW" or "en" or "es" or "fr" or "de" or "pt-BR";
}

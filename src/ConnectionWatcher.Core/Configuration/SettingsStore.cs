using System.Text.Json;
using ConnectionWatcher.Core.Models;
using ConnectionWatcher.Core.Rules;

namespace ConnectionWatcher.Core.Configuration;

public sealed class SettingsStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public SettingsStore(string rootDirectory)
    {
        RootDirectory = rootDirectory;
        SettingsPath = Path.Combine(rootDirectory, "config.json");
    }

    public string RootDirectory { get; }
    public string SettingsPath { get; }

    public AppSettings Load()
    {
        Directory.CreateDirectory(RootDirectory);
        if (!File.Exists(SettingsPath))
        {
            return new AppSettings();
        }

        try
        {
            string json = File.ReadAllText(SettingsPath);
            AppSettings settings = JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ??
                new AppSettings();
            Normalize(settings);
            return settings;
        }
        catch (Exception ex) when (ex is JsonException or IOException or UnauthorizedAccessException)
        {
            string backup = Path.Combine(
                RootDirectory,
                $"config-invalid-{DateTime.Now:yyyyMMdd-HHmmss}.json");
            try
            {
                File.Copy(SettingsPath, backup, overwrite: false);
            }
            catch
            {
                // A damaged configuration must not prevent the app from opening.
            }

            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        Directory.CreateDirectory(RootDirectory);
        string temporaryPath = SettingsPath + ".tmp";
        string json = JsonSerializer.Serialize(settings, JsonOptions);
        File.WriteAllText(temporaryPath, json);
        File.Move(temporaryPath, SettingsPath, overwrite: true);
    }

    private static void Normalize(AppSettings settings)
    {
        settings.Rules ??= [];
        HashSet<Guid> ids = [];
        foreach (MonitoringRule rule in settings.Rules)
        {
            rule.RemotePort ??= PortRange.Any;
            rule.LocalPort ??= PortRange.Any;
            if (rule.Id == Guid.Empty || !ids.Add(rule.Id))
            {
                rule.Id = Guid.NewGuid();
                ids.Add(rule.Id);
            }

            if (RuleValidator.Validate(rule).Count > 0)
            {
                rule.Enabled = false;
            }
        }

        settings.LogLimitMb = 25;
    }
}

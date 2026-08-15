using Microsoft.Win32;

namespace ConnectionWatcher.App.Services;

public static class StartupManager
{
    private const string RunKey = @"Software\Microsoft\Windows\CurrentVersion\Run";
    private const string ValueName = "SocketSight";
    private const string LegacyValueName = "ConnectionWatcher";

    public static void SetEnabled(bool enabled)
    {
        using RegistryKey key = Registry.CurrentUser.CreateSubKey(RunKey, writable: true);
        if (enabled)
        {
            string executable = Environment.ProcessPath ??
                throw new InvalidOperationException("Executable path is unavailable.");
            key.SetValue(ValueName, $"\"{executable}\"");
            key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
        }
        else
        {
            key.DeleteValue(ValueName, throwOnMissingValue: false);
            key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
        }
    }

    public static void TryMigrateLegacyRegistration()
    {
        try
        {
            using RegistryKey? key = Registry.CurrentUser.OpenSubKey(
                RunKey,
                writable: true);
            if (key?.GetValue(LegacyValueName) is null)
            {
                return;
            }

            string executable = Environment.ProcessPath ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(executable))
            {
                key.SetValue(ValueName, $"\"{executable}\"");
                key.DeleteValue(LegacyValueName, throwOnMissingValue: false);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Keep the app usable if Windows prevents registry migration.
        }
        catch (System.Security.SecurityException)
        {
            // Keep the app usable if Windows prevents registry migration.
        }
    }
}

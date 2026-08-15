using System.Diagnostics;
using ConnectionWatcher.App.Localization;
using ConnectionWatcher.App.Services;
using ConnectionWatcher.App.UI;
using ConnectionWatcher.Core.Configuration;
using ConnectionWatcher.Core.Logging;

namespace ConnectionWatcher.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using Mutex instance = new(
            initiallyOwned: true,
            name: @"Local\ConnectionWatcher-6F695A7A-5E57-4B21-86A4-A487B45D67DE",
            createdNew: out bool createdNew);
        if (!createdNew)
        {
            MessageBox.Show(
                "SocketSight is already running.\n\n" +
                "SocketSight 已经在运行。\n\n" +
                "SocketSight ya está en ejecución.",
                "SocketSight",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        long startupStarted = Stopwatch.GetTimestamp();

        string dataRoot = Environment.GetEnvironmentVariable("SOCKETSIGHT_DATA_DIR") ??
            Environment.GetEnvironmentVariable("CONNECTION_WATCHER_DATA_DIR") ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ConnectionWatcher");
        SettingsStore settingsStore = new(dataRoot);
        AppSettings settings = settingsStore.Load();

        StartupManager.TryMigrateLegacyRegistration();

        string? installerLanguage = InstallerLanguagePreference.Consume(
            AppContext.BaseDirectory);
        if (installerLanguage is not null)
        {
            settings.Language = installerLanguage;
            settingsStore.Save(settings);
        }
        else if (string.IsNullOrWhiteSpace(settings.Language))
        {
            using LanguageSelectionForm selection = new();
            if (selection.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            settings.Language = selection.SelectedLanguage;
            settingsStore.Save(settings);
        }

        UiText.SetLanguage(settings.Language);
        const int maximumLogFiles = 5;
        long maximumTotalBytes = settings.LogLimitMb * 1024L * 1024L;
        CsvEventLogger logger = new(
            Path.Combine(dataRoot, "Logs"),
            maximumFileBytes: maximumTotalBytes / maximumLogFiles,
            maximumFiles: maximumLogFiles);

        Application.ThreadException += (_, args) =>
            MessageBox.Show(
                args.Exception.Message,
                UiText.Get("UnexpectedError"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

        TimeSpan splashThreshold = TimeSpan.FromMilliseconds(500);
        TimeSpan elapsed = Stopwatch.GetElapsedTime(startupStarted);
        TimeSpan splashDelay = elapsed >= splashThreshold
            ? TimeSpan.Zero
            : splashThreshold - elapsed;
        using DelayedSplashScreen splash = new(
            StartupText.Get(settings.Language),
            settings.Language,
            splashDelay);
        splash.Start();
        using MainForm mainForm = new(settings, settingsStore, logger);
        mainForm.Shown += (_, _) => splash.Complete();
        Application.Run(mainForm);
        GC.KeepAlive(instance);
    }
}

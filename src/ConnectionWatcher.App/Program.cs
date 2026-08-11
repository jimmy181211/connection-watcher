using ConnectionWatcher.App.Localization;
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
                "TCP Connection Watcher is already running.\n\nTCP连接监视器已经在运行。",
                "TCP Connection Watcher",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        string dataRoot = Environment.GetEnvironmentVariable("CONNECTION_WATCHER_DATA_DIR") ??
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ConnectionWatcher");
        SettingsStore settingsStore = new(dataRoot);
        AppSettings settings = settingsStore.Load();

        if (string.IsNullOrWhiteSpace(settings.Language))
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
        CsvEventLogger logger = new(
            Path.Combine(dataRoot, "Logs"),
            maximumFileBytes: 5 * 1024 * 1024,
            maximumFiles: 5);

        Application.ThreadException += (_, args) =>
            MessageBox.Show(
                args.Exception.Message,
                UiText.Get("UnexpectedError"),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);

        using MainForm mainForm = new(settings, settingsStore, logger);
        Application.Run(mainForm);
        GC.KeepAlive(instance);
    }
}

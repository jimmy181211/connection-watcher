namespace ConnectionWatcher.App.UI;

internal static class AppIconProvider
{
    private const string ResourceName = "ConnectionWatcher.AppIcon.ico";

    public static Icon Load()
    {
        using Stream? stream = typeof(AppIconProvider).Assembly
            .GetManifestResourceStream(ResourceName);
        if (stream is not null)
        {
            using Icon embedded = new(stream);
            return (Icon)embedded.Clone();
        }

        return Icon.ExtractAssociatedIcon(Application.ExecutablePath) ??
            (Icon)SystemIcons.Application.Clone();
    }
}

using System.Net.Http.Headers;
using System.Text.Json;

namespace ConnectionWatcher.App.Services;

public sealed record UpdateCheckResult(
    Version CurrentVersion,
    Version LatestVersion,
    string LatestTag,
    string ReleaseName,
    Uri ReleasePage,
    bool IsUpdateAvailable);

public sealed class UpdateCheckService
{
    private static readonly Uri LatestReleaseApi = new(
        "https://api.github.com/repos/jimmy181211/connection-watcher/releases/latest");
    private readonly HttpClient _client;

    public UpdateCheckService(HttpClient? client = null)
    {
        _client = client ?? new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(12)
        };
        if (!_client.DefaultRequestHeaders.UserAgent.Any())
        {
            _client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("SocketSight", "1.3"));
        }
    }

    public async Task<UpdateCheckResult> CheckAsync(
        Version currentVersion,
        CancellationToken cancellationToken = default)
    {
        using HttpResponseMessage response = await _client.GetAsync(
            LatestReleaseApi,
            cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        await using Stream content = await response.Content.ReadAsStreamAsync(
            cancellationToken).ConfigureAwait(false);
        using JsonDocument document = await JsonDocument.ParseAsync(
            content,
            cancellationToken: cancellationToken).ConfigureAwait(false);
        JsonElement root = document.RootElement;
        string tag = RequiredString(root, "tag_name");
        string releaseName = OptionalString(root, "name") ?? tag;
        string page = RequiredString(root, "html_url");
        if (!TryParseVersion(tag, out Version? latestVersion))
        {
            throw new InvalidDataException(
                $"GitHub returned an unsupported release version: {tag}");
        }

        return new UpdateCheckResult(
            currentVersion,
            latestVersion!,
            tag,
            releaseName,
            new Uri(page),
            latestVersion! > currentVersion);
    }

    public static bool TryParseVersion(string value, out Version? version)
    {
        string normalized = value.Trim();
        if (normalized.StartsWith('v') || normalized.StartsWith('V'))
        {
            normalized = normalized[1..];
        }

        int suffix = normalized.IndexOfAny(['-', '+']);
        if (suffix >= 0)
        {
            normalized = normalized[..suffix];
        }

        return Version.TryParse(normalized, out version);
    }

    private static string RequiredString(JsonElement root, string name) =>
        OptionalString(root, name) ??
        throw new InvalidDataException($"GitHub response is missing {name}.");

    private static string? OptionalString(JsonElement root, string name) =>
        root.TryGetProperty(name, out JsonElement value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}

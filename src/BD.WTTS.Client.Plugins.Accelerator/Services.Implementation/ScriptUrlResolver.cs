using System.Text.Json;
using System.Text.RegularExpressions;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.Services.Implementation;

public sealed class ScriptUrlResolver(HttpClient httpClient)
{
    static readonly Regex PageLinks = new("""\bhref\s*=\s*["'](?<url>[^"']+)["']""",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromMilliseconds(100));

    public async Task<ScriptResolvedUrl> ResolveAsync(string url, CancellationToken cancellationToken = default)
    {
        if (!TryGetHttpUri(url, out var uri))
            throw new ArgumentException(Strings.Script_Inspect_Warn_InvalidUrl, nameof(url));

        var kind = ClassifyHost(uri.Host);
        var path = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var download = uri.AbsoluteUri;
        string? sourcePage = null;

        if (IsGitHubBlob(uri, path))
        {
            download = "https://raw.githubusercontent.com/" + string.Join("/", path.Take(2).Concat(path.Skip(3)));
            sourcePage = uri.AbsoluteUri;
        }
        else if (!IsScriptPath(uri))
        {
            if (kind == ScriptSourceKind.GreasyFork && GetScriptId(path, "scripts") is { } greasyForkId)
            {
                var origin = IsHost(uri.Host, "sleazyfork.org") ? "https://sleazyfork.org" : "https://greasyfork.org";
                sourcePage = uri.AbsoluteUri;
                download = await GetGreasyForkCodeUrlAsync(origin, greasyForkId, cancellationToken);
            }
            else if (kind == ScriptSourceKind.ScriptCat && GetScriptId(path, "script-show-page") is { } scriptCatId)
            {
                sourcePage = uri.AbsoluteUri;
                download = await GetScriptCatCodeUrlAsync(path, scriptCatId, cancellationToken);
            }
        }
        return new(url.Trim(), download, sourcePage, kind, ScriptExternalLinks.GetSourceLabel(kind));
    }

    async Task<string> GetGreasyForkCodeUrlAsync(string origin, string id, CancellationToken cancellationToken)
    {
        try
        {
            using var response = await httpClient.GetAsync($"{origin}/scripts/{id}.json",
                HttpCompletionOption.ResponseHeadersRead, cancellationToken);
            if (response.IsSuccessStatusCode)
            {
                await using var body = await response.Content.ReadAsStreamAsync(cancellationToken);
                using var json = await JsonDocument.ParseAsync(body, cancellationToken: cancellationToken);
                if (json.RootElement.ValueKind == JsonValueKind.Object
                    && json.RootElement.TryGetProperty("code_url", out var code)
                    && code.ValueKind == JsonValueKind.String
                    && TryGetHttpUri(code.GetString(), out var codeUri))
                    return codeUri.AbsoluteUri;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or System.Text.Json.JsonException
            || ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // Greasy Fork also exposes a direct download endpoint.
        }
        return $"{origin}/scripts/{id}/code/script.user.js";
    }

    async Task<string> GetScriptCatCodeUrlAsync(string[] path, string id, CancellationToken cancellationToken)
    {
        var origin = new Uri("https://scriptcat.org");
        var pageIndex = Array.FindIndex(path, segment => segment.Equals("script-show-page", StringComparison.OrdinalIgnoreCase));
        var page = new Uri(origin, string.Join("/", path.Take(pageIndex + 2)));
        var codePath = $"/scripts/code/{id}/";
        try
        {
            // ScriptCat's install link can include a filename specific to the script.
            var html = await httpClient.GetStringAsync(page, cancellationToken);
            foreach (Match link in PageLinks.Matches(html))
            {
                var href = WebUtility.HtmlDecode(link.Groups["url"].Value);
                if (Uri.TryCreate(origin, href, out var target)
                    && TryGetHttpUri(target.AbsoluteUri, out _)
                    && IsHost(target.Host, origin.Host)
                    && target.AbsolutePath.StartsWith(codePath, StringComparison.Ordinal)
                    && target.AbsolutePath.EndsWith(".user.js", StringComparison.OrdinalIgnoreCase))
                    return target.AbsoluteUri;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or IOException or RegexMatchTimeoutException
            || ex is OperationCanceledException && !cancellationToken.IsCancellationRequested)
        {
            // Fall back to the id-based download route.
        }
        return new Uri(origin, codePath + "script.user.js").AbsoluteUri;
    }

    public static bool CanResolveScriptLink(string? url)
    {
        if (!TryGetHttpUri(url, out var uri))
            return false;

        var path = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return IsScriptPath(uri) || IsGitHubBlob(uri, path) || ClassifyHost(uri.Host) switch
        {
            ScriptSourceKind.ScriptCat => GetScriptId(path, "script-show-page") != null,
            ScriptSourceKind.GreasyFork => GetScriptId(path, "scripts") != null,
            _ => false,
        };
    }

    public static bool TryGetHttpUri(string? url, [NotNullWhen(true)] out Uri? uri)
    {
        if (Uri.TryCreate(url?.Trim(), UriKind.Absolute, out var candidate)
            && (candidate.Scheme == Uri.UriSchemeHttp || candidate.Scheme == Uri.UriSchemeHttps)
            && candidate.UserInfo.Length == 0 && candidate.Host.Length != 0)
        {
            uri = candidate;
            return true;
        }
        uri = null;
        return false;
    }

    public static ScriptSourceKind Classify(ScriptDTO? script, IEnumerable<string?>? links = null)
    {
        if (script?.IsBasics == true || script?.Id == ScriptExternalLinks.BasicsScriptId)
            return ScriptSourceKind.Basics;
        if (script?.Id != null)
            return ScriptSourceKind.Workshop;

        links ??= [script?.SourceLink, script?.DownloadLink, script?.UpdateLink];
        var kind = ScriptSourceKind.LocalFile;
        foreach (var link in links)
        {
            if (!TryGetHttpUri(link, out var uri))
                continue;

            var hostKind = ClassifyHost(uri.Host);
            if (hostKind is ScriptSourceKind.ScriptCat or ScriptSourceKind.GreasyFork)
                return hostKind;
            kind = ScriptSourceKind.ExternalUrl;
        }
        return kind;
    }

    internal static bool IsMetadataUrl(string? url)
        => TryGetHttpUri(url, out var uri) && uri.AbsolutePath.EndsWith(".meta.js", StringComparison.OrdinalIgnoreCase);

    internal static string GetCodeUrl(string url)
    {
        if (!IsMetadataUrl(url) || !TryGetHttpUri(url, out var uri))
            return url;

        return new UriBuilder(uri) { Path = uri.AbsolutePath[..^".meta.js".Length] + ".user.js" }.Uri.AbsoluteUri;
    }

    static bool IsScriptPath(Uri uri) => uri.AbsolutePath.EndsWith(".js", StringComparison.OrdinalIgnoreCase);

    static bool IsGitHubBlob(Uri uri, string[] path)
        => uri.Host.Equals("github.com", StringComparison.OrdinalIgnoreCase)
            && path.Length >= 5 && path[2] == "blob";

    static string? GetScriptId(string[] path, string segment)
    {
        var index = Array.FindIndex(path, part => part.Equals(segment, StringComparison.OrdinalIgnoreCase));
        if (index < 0 || index + 1 >= path.Length)
            return null;

        var id = path[index + 1].Split('-', 2)[0];
        return id.Length != 0 && id.All(char.IsAsciiDigit) ? id : null;
    }

    static bool IsHost(string host, string domain)
        => host.Equals(domain, StringComparison.OrdinalIgnoreCase)
            || host.EndsWith("." + domain, StringComparison.OrdinalIgnoreCase);

    static ScriptSourceKind ClassifyHost(string host)
    {
        if (IsHost(host, "scriptcat.org"))
            return ScriptSourceKind.ScriptCat;
        if (IsHost(host, "greasyfork.org") || IsHost(host, "sleazyfork.org"))
            return ScriptSourceKind.GreasyFork;
        return ScriptSourceKind.ExternalUrl;
    }
}

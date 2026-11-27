using System.Globalization;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.Services.Implementation;

public static class ScriptMetadataParser
{
    static readonly HashSet<string> BroadMatches = new(StringComparer.OrdinalIgnoreCase)
    {
        "*://*/*", "http*://*/*", "https://*/*", "http://*/*", "*", "<all_urls>",
    };

    static readonly HashSet<string> PrivilegedGrants = new(StringComparer.OrdinalIgnoreCase)
    {
        "GM_xmlhttpRequest", "GM.xmlHttpRequest", "unsafeWindow", "GM_cookie", "GM.cookie",
        "GM_download", "GM.download", "GM_webRequest", "GM.webRequest", "GM_setClipboard", "GM.setClipboard",
    };

    public static ScriptInspectionResult Parse(string content, string? filePath = null,
        ScriptResolvedUrl? resolvedUrl = null, IEnumerable<ScriptDTO>? existingScripts = null,
        Guid? pid = null, CultureInfo? culture = null, ScriptDTO? installedScript = null)
    {
        var metadata = ReadMetadata(content, out var header);
        var hasHeader = header.Length != 0;
        string Get(params string[] keys) => GetValues(metadata, keys).FirstOrDefault() ?? string.Empty;

        culture ??= CultureInfo.CurrentUICulture;
        var declaredName = GetLocalized(metadata, "name", culture);
        var name = declaredName.Length == 0
            ? installedScript?.Name ?? (filePath == null ? string.Empty : Path.GetFileNameWithoutExtension(filePath))
            : declaredName;
        var version = FirstNonEmpty(Get("version"), installedScript?.Version);
        var matches = GetValues(metadata, ["match"]);
        var includes = GetValues(metadata, ["include"]);
        var effectiveMatches = matches.Concat(includes).Distinct(StringComparer.Ordinal).ToArray();
        var grants = GetValues(metadata, ["grant"]);
        var homepage = FirstNonEmpty(Get("homepageURL", "homepage", "website", "source"),
            resolvedUrl?.SourcePageUrl, installedScript?.SourceLink);
        var download = FirstNonEmpty(Get("downloadURL"), resolvedUrl?.DownloadUrl, installedScript?.DownloadLink);
        var update = FirstNonEmpty(Get("updateURL"), installedScript?.UpdateLink);
        var existing = installedScript ?? FindExisting(existingScripts, pid, name,
            [homepage, download, update, resolvedUrl?.OriginalInputUrl, resolvedUrl?.SourcePageUrl, resolvedUrl?.DownloadUrl]);
        var sourceKind = resolvedUrl?.SourceKind ?? (installedScript == null && filePath != null
            ? ScriptSourceKind.LocalFile : ScriptUrlResolver.Classify(installedScript, [download, update, homepage]));

        return new()
        {
            HasUserScriptHeader = hasHeader,
            Name = name,
            Version = version,
            Author = FirstNonEmpty(Get("author"), installedScript?.AuthorName),
            Description = FirstNonEmpty(GetLocalized(metadata, "description", culture), installedScript?.Describe),
            Namespace = Get("namespace"),
            IconUrl = FirstNonEmpty(Get("icon", "icon64", "iconURL"), installedScript?.IconUrl),
            HomepageUrl = homepage,
            DownloadUrl = download,
            UpdateUrl = update,
            RunAt = Get("run-at"),
            ContentByteLength = Encoding.UTF8.GetByteCount(content),
            HeaderPreview = header,
            RawContent = content,
            SourceFilePath = filePath,
            SourcePageUrl = resolvedUrl?.SourcePageUrl,
            SourceKind = sourceKind,
            SourceOriginLabel = resolvedUrl?.SourceLabel ?? ScriptExternalLinks.GetSourceLabel(sourceKind),
            MatchRules = matches,
            IncludeRules = includes,
            EffectiveMatchRules = effectiveMatches,
            ExcludeRules = GetValues(metadata, ["exclude", "exclude-match"]),
            Grants = grants,
            Connects = GetValues(metadata, ["connect"]),
            Requires = GetValues(metadata, ["require"]),
            Warnings = GetWarnings(hasHeader, effectiveMatches, grants, update, download),
            ExistingScript = existing,
            IsUpgrade = existing != null && ScriptVersionComparer.Compare(version, existing.Version) > 0,
        };
    }

    static Dictionary<string, List<string>> ReadMetadata(string content, out string header)
    {
        var metadata = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        using var reader = new StringReader(content);
        var preview = new StringBuilder();
        header = string.Empty;
        while (reader.ReadLine() is { } line)
        {
            var trimmed = line.Trim().TrimStart('\uFEFF').TrimStart();
            var isComment = trimmed.StartsWith("//", StringComparison.Ordinal);
            var comment = isComment ? trimmed[2..].TrimStart() : string.Empty;
            if (preview.Length == 0)
            {
                if (comment.Equals("==UserScript==", StringComparison.OrdinalIgnoreCase))
                    preview.AppendLine(trimmed);
                continue;
            }

            preview.AppendLine(line.TrimEnd());
            if (comment.Equals("==/UserScript==", StringComparison.OrdinalIgnoreCase))
            {
                header = preview.ToString().TrimEnd();
                return metadata;
            }

            if (!isComment)
            {
                if (trimmed.Length == 0)
                    continue;
                break;
            }

            if (!comment.StartsWith('@'))
                continue;

            var keyEnd = 1;
            while (keyEnd < comment.Length && !char.IsWhiteSpace(comment[keyEnd]))
                keyEnd++;
            if (keyEnd == comment.Length)
                continue;

            var key = comment[1..keyEnd];
            var value = comment[keyEnd..].Trim();
            if (key.Length == 0 || value.Length == 0)
                continue;
            if (!metadata.TryGetValue(key, out var values))
                metadata.Add(key, values = []);
            values.Add(value);
        }

        metadata.Clear();
        return metadata;
    }

    static string[] GetValues(Dictionary<string, List<string>> metadata, string[] keys)
        => keys.Where(metadata.ContainsKey).SelectMany(key => metadata[key]).Distinct(StringComparer.Ordinal).ToArray();

    static string GetLocalized(Dictionary<string, List<string>> metadata, string key, CultureInfo culture)
    {
        for (var candidate = culture; candidate.Name.Length != 0; candidate = candidate.Parent)
        {
            if (metadata.TryGetValue(key + ":" + candidate.Name, out var translations))
                return translations[0];
        }

        return metadata.TryGetValue(key, out var values) ? values[0] : string.Empty;
    }

    static string FirstNonEmpty(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value)) ?? string.Empty;

    static ScriptDTO? FindExisting(IEnumerable<ScriptDTO>? scripts, Guid? pid, string name, string?[] links)
    {
        var candidates = scripts?.Where(script => script.Id != ScriptExternalLinks.BasicsScriptId
            || pid == ScriptExternalLinks.BasicsScriptId).ToArray() ?? [];
        var urls = links.Select(link => ScriptUrlResolver.TryGetHttpUri(link, out var uri) ? uri : null)
            .OfType<Uri>().ToHashSet();
        return (pid.HasValue ? candidates.FirstOrDefault(script => script.Id == pid) : null)
            ?? candidates.FirstOrDefault(script => new[] { script.SourceLink, script.DownloadLink, script.UpdateLink }
                .Any(link => ScriptUrlResolver.TryGetHttpUri(link, out var uri) && urls.Contains(uri)))
            ?? (name.Length == 0 ? null : candidates.FirstOrDefault(script =>
                name.Equals(script.Name, StringComparison.OrdinalIgnoreCase)));
    }

    static IReadOnlyList<ScriptRuleWarning> GetWarnings(bool hasHeader, string[] matches,
        string[] grants, string update, string download)
    {
        var warnings = new List<ScriptRuleWarning>();
        if (!hasHeader)
            warnings.Add(new(ScriptRuleWarningSeverity.Critical,
                Strings.Script_Inspect_Warn_NoHeaderTitle, Strings.Script_Inspect_Warn_NoHeaderDetail));
        if (matches.Length == 0)
            warnings.Add(new(ScriptRuleWarningSeverity.Warning,
                Strings.Script_Inspect_Warn_NoMatchTitle, Strings.Script_Inspect_Warn_NoMatchDetail));
        else if (matches.Any(BroadMatches.Contains))
            warnings.Add(new(ScriptRuleWarningSeverity.Warning,
                Strings.Script_Inspect_Warn_BroadMatchTitle, Strings.Script_Inspect_Warn_BroadMatchDetail));

        var privilegedGrants = grants.Where(PrivilegedGrants.Contains).ToArray();
        if (privilegedGrants.Length != 0)
            warnings.Add(new(ScriptRuleWarningSeverity.Warning, Strings.Script_Inspect_Warn_HighPrivGrantTitle,
                string.Format(Strings.Script_Inspect_Warn_HighPrivGrantDetail, string.Join(", ", privilegedGrants))));

        if (!ScriptUrlResolver.TryGetHttpUri(update, out _) && !ScriptUrlResolver.TryGetHttpUri(download, out _))
            warnings.Add(new(ScriptRuleWarningSeverity.Info,
                Strings.Script_Inspect_Warn_NoUpdateUrlTitle, Strings.Script_Inspect_Warn_NoUpdateUrlDetail));

        return warnings;
    }
}

using BD.WTTS.Client.Resources;

namespace BD.WTTS.Enums
{
    public enum ScriptFilterKind : byte { All, Enabled, Disabled, Updatable }

    public enum ScriptSourceKind : byte { Basics, Workshop, ScriptCat, GreasyFork, ExternalUrl, LocalFile }

    public enum ScriptRuleWarningSeverity : byte { Info, Warning, Critical }

    public enum ScriptUpdateCheckStatus : byte { UpToDate, UpdateAvailable, NoUpdateUrl, Failed }
}

namespace BD.WTTS.Models
{
    public static class ScriptExternalLinks
    {
        public const string ScriptCatUrl = "https://scriptcat.org/zh-CN";
        public const string GreasyForkUrl = "https://greasyfork.org/";
        public static readonly Guid BasicsScriptId = new("00000000-0000-0000-0000-000000000001");

        public static string GetSourceLabel(ScriptSourceKind kind) => kind switch
        {
            ScriptSourceKind.Basics => Strings.Script_Source_Basics,
            ScriptSourceKind.Workshop => Strings.Script_Source_Workshop,
            ScriptSourceKind.ScriptCat => Strings.Script_Source_ScriptCat,
            ScriptSourceKind.GreasyFork => Strings.Script_Source_GreasyFork,
            ScriptSourceKind.ExternalUrl => Strings.Script_Source_ExternalUrl,
            _ => Strings.Script_Source_LocalFile,
        };
    }

    public sealed record ScriptResolvedUrl(
        string OriginalInputUrl, string DownloadUrl, string? SourcePageUrl, ScriptSourceKind SourceKind, string SourceLabel);

    public sealed record ScriptRuleWarning(ScriptRuleWarningSeverity Severity, string Title, string Detail)
    {
        public bool IsInfo => Severity == ScriptRuleWarningSeverity.Info;
        public bool IsWarning => Severity == ScriptRuleWarningSeverity.Warning;
        public bool IsCritical => Severity == ScriptRuleWarningSeverity.Critical;
    }

    public sealed record ScriptInspectionResult
    {
        public bool HasUserScriptHeader { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Version { get; init; } = string.Empty;
        public string Author { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public string Namespace { get; init; } = string.Empty;
        public string? IconUrl { get; init; }
        public string HomepageUrl { get; init; } = string.Empty;
        public string DownloadUrl { get; init; } = string.Empty;
        public string UpdateUrl { get; init; } = string.Empty;
        public string RunAt { get; init; } = string.Empty;
        public long ContentByteLength { get; init; }
        public string HeaderPreview { get; init; } = string.Empty;
        public string RawContent { get; init; } = string.Empty;
        public string? SourceFilePath { get; init; }
        public string? SourcePageUrl { get; init; }
        public ScriptSourceKind SourceKind { get; init; }
        public string SourceOriginLabel { get; init; } = string.Empty;
        public IReadOnlyList<string> MatchRules { get; init; } = [];
        public IReadOnlyList<string> IncludeRules { get; init; } = [];
        public IReadOnlyList<string> EffectiveMatchRules { get; init; } = [];
        public IReadOnlyList<string> ExcludeRules { get; init; } = [];
        public IReadOnlyList<string> Grants { get; init; } = [];
        public IReadOnlyList<string> Connects { get; init; } = [];
        public IReadOnlyList<string> Requires { get; init; } = [];
        public IReadOnlyList<ScriptRuleWarning> Warnings { get; init; } = [];
        public ScriptDTO? ExistingScript { get; init; }
        public bool IsUpgrade { get; init; }
        public string? ExistingVersion => ExistingScript?.Version;
        internal bool PreserveExistingEnable { get; init; }

        public ScriptDTO ToScriptDto(bool enable = true, bool isCompile = true) => new()
        {
            Name = Name,
            Version = Version,
            AuthorName = Author,
            Describe = Description,
            IconUrl = IconUrl,
            SourceLink = SourcePageUrl ?? HomepageUrl,
            DownloadLink = DownloadUrl,
            UpdateLink = UpdateUrl,
            MatchDomainNames = string.Join(ApiConstants.GeneralSeparator, EffectiveMatchRules),
            ExcludeDomainNames = string.Join(ApiConstants.GeneralSeparator, ExcludeRules),
            RequiredJs = string.Join(ApiConstants.GeneralSeparator, Requires),
            DependentGreasyForkFunction = Grants.Any(grant => !grant.Equals("none", StringComparison.OrdinalIgnoreCase)),
            Disable = !enable,
            IsCompile = isCompile,
            FilePath = SourceFilePath ?? string.Empty,
            Content = RawContent.Replace("</script>", "<\\/script>", StringComparison.OrdinalIgnoreCase),
        };
    }

    public sealed record ScriptUpdateCheckResult(ScriptDTO Script, ScriptUpdateCheckStatus Status,
        string? RemoteVersion = null, string? DownloadUrl = null, string? UpdateUrl = null, string? ErrorMessage = null);

    public sealed record ScriptBatchUpdateCheckResult(int CheckedCount, int UpdateAvailableCount, int FailedCount,
        int SkippedCount, IReadOnlyList<ScriptUpdateCheckResult> Results);
}

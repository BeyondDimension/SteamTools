using System.Text.RegularExpressions;
using BD.WTTS.Enums;

namespace BD.WTTS.Enums
{
    public enum ScriptInjectScopeMode : byte
    {
        All,
        SelectedUserAgents,
    }

    [Flags]
    public enum ScriptUserAgentPreset : uint
    {
        None = 0,
        SteamBrowser = 1 << 0,
        Chrome = 1 << 1,
        Edge = 1 << 2,
    }
}

namespace BD.WTTS.Models
{
    public readonly record struct ScriptUserAgentMatchResult(bool IsMatched, string? MatchedReason);

    public static class ScriptUserAgentMatcher
    {
        static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(100);
        static readonly string[] EdgeTokens = ["Edg/", "EdgA/", "EdgiOS/", "Edge/"];
        static readonly string[] OtherChromeTokens =
            ["OPR/", "Opera", "Vivaldi/", "YaBrowser/", "SamsungBrowser/", "Electron/"];

        public static IReadOnlyList<string> ParseCustomRules(string? customRulesText)
            => customRulesText?.Split(['\r', '\n'], StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(rule => !rule.StartsWith('#'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray() ?? [];

        public static bool TryValidateCustomRule(string? rawRule, out string normalizedRule, out string? errorReason)
        {
            normalizedRule = rawRule?.Trim() ?? string.Empty;
            errorReason = null;

            if (normalizedRule.Length == 0 || normalizedRule.StartsWith('#') || normalizedRule.IndexOfAny(['\r', '\n']) >= 0)
            {
#if APP_REVERSE_PROXY
                errorReason = "Enter one User-Agent rule.";
#else
                errorReason = Client.Resources.Strings.Script_Settings_CustomUaPlaceholder;
#endif
                return false;
            }

            try
            {
                if (normalizedRule.StartsWith('/'))
                    CreateRegex(normalizedRule);
                return true;
            }
            catch (ArgumentException e)
            {
#if APP_REVERSE_PROXY
                errorReason = e.Message;
#else
                errorReason = string.Format(Client.Resources.Strings.Script_Settings_CustomUaInvalidRegex, e.Message);
#endif
                return false;
            }
        }

        public static ScriptUserAgentMatchResult Evaluate(string? userAgent, ScriptInjectScopeMode mode,
            ScriptUserAgentPreset presets, string? customRulesText, bool legacyOnlySteamBrowser = false)
            => Evaluate(userAgent, mode, presets,
                mode == ScriptInjectScopeMode.All && !legacyOnlySteamBrowser ? [] : ParseCustomRules(customRulesText),
                legacyOnlySteamBrowser);

        public static ScriptUserAgentMatchResult Evaluate(string? userAgent, ScriptInjectScopeMode mode,
            ScriptUserAgentPreset presets, IReadOnlyList<string>? customRules, bool legacyOnlySteamBrowser = false)
        {
            if (mode == ScriptInjectScopeMode.All)
            {
                if (legacyOnlySteamBrowser && presets == ScriptUserAgentPreset.None && (customRules?.Count ?? 0) == 0)
                    presets = ScriptUserAgentPreset.SteamBrowser;
                else
                    return new(true, nameof(ScriptInjectScopeMode.All));
            }

            if (string.IsNullOrEmpty(userAgent))
                return default;

            const StringComparison comparison = StringComparison.OrdinalIgnoreCase;
            var steam = userAgent.Contains("Valve Steam", comparison);
            var edge = !steam && EdgeTokens.Any(token => userAgent.Contains(token, comparison));

            if (steam && presets.HasFlag(ScriptUserAgentPreset.SteamBrowser))
                return new(true, nameof(ScriptUserAgentPreset.SteamBrowser));
            if (edge && presets.HasFlag(ScriptUserAgentPreset.Edge))
                return new(true, nameof(ScriptUserAgentPreset.Edge));
            if (presets.HasFlag(ScriptUserAgentPreset.Chrome) && !steam && !edge
                && (userAgent.Contains("Chrome/", comparison) || userAgent.Contains("CriOS/", comparison))
                && !OtherChromeTokens.Any(token => userAgent.Contains(token, comparison)))
                return new(true, nameof(ScriptUserAgentPreset.Chrome));

            if (customRules is not null)
            {
                foreach (var rule in customRules)
                {
                    if (MatchesRule(userAgent, rule))
                        return new(true, rule);
                }
            }

            return default;
        }

        public static bool IsAllowed(string? userAgent, ScriptInjectScopeMode mode, ScriptUserAgentPreset presets,
            string? customRulesText, bool legacyOnlySteamBrowser = false)
            => Evaluate(userAgent, mode, presets, customRulesText, legacyOnlySteamBrowser).IsMatched;

        public static bool IsAllowed(string? userAgent, ScriptInjectScopeMode mode, ScriptUserAgentPreset presets,
            IReadOnlyList<string>? customRules, bool legacyOnlySteamBrowser = false)
            => Evaluate(userAgent, mode, presets, customRules, legacyOnlySteamBrowser).IsMatched;

        static bool MatchesRule(string userAgent, string? rule)
        {
            if (string.IsNullOrWhiteSpace(rule) || rule.StartsWith('#'))
                return false;

            try
            {
                if (rule.StartsWith('/'))
                    return CreateRegex(rule).IsMatch(userAgent);
                if (rule.IndexOfAny(['*', '?']) >= 0)
                {
                    var pattern = Regex.Escape(rule).Replace(@"\*", ".*").Replace(@"\?", ".");
                    return Regex.IsMatch(userAgent, pattern, RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, MatchTimeout);
                }

                return userAgent.Contains(rule, StringComparison.OrdinalIgnoreCase);
            }
            catch (Exception e) when (e is ArgumentException or RegexMatchTimeoutException)
            {
                return false;
            }
        }

        static Regex CreateRegex(string rule)
        {
            var delimiter = rule.LastIndexOf('/');
            if (delimiter <= 1)
                throw new ArgumentException(rule);

            var flags = rule.AsSpan(delimiter + 1);
            var options = RegexOptions.CultureInvariant;
            if (flags.IsEmpty)
                options |= RegexOptions.IgnoreCase;
            foreach (var flag in flags)
            {
                options |= flag switch
                {
                    'i' => RegexOptions.IgnoreCase,
                    'm' => RegexOptions.Multiline,
                    's' => RegexOptions.Singleline,
                    _ => throw new ArgumentException(rule),
                };
            }

            return new Regex(rule[1..delimiter], options, MatchTimeout);
        }
    }
}

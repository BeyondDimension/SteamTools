using BD.WTTS.Settings;
using BD.WTTS.Settings.Abstractions;
using MemoryPack;
using SJsonSerializer = System.Text.Json.JsonSerializer;

namespace BD.WTTS.UnitTest;

public sealed partial class ScriptUserAgentMatcherTest
{
    const string SteamUa = "Mozilla/5.0 Chrome/130.0 Valve Steam Client";
    const string ChromeUa = "Mozilla/5.0 Chrome/130.0 Safari/537.36";
    const string EdgeUa = "Mozilla/5.0 Chrome/130.0 Edg/130.0";
    const string FirefoxUa = "Mozilla/5.0 Firefox/130.0";
    const ScriptInjectScopeMode Selected = ScriptInjectScopeMode.SelectedUserAgents;

    [TestCase(null)]
    [TestCase("")]
    [TestCase(FirefoxUa)]
    public void AllAllowsAnyUserAgent(string? userAgent)
    {
        var result = ScriptUserAgentMatcher.Evaluate(userAgent, ScriptInjectScopeMode.All,
            ScriptUserAgentPreset.None, (string?)null);

        Assert.That(result, Is.EqualTo(new ScriptUserAgentMatchResult(true, nameof(ScriptInjectScopeMode.All))));
    }

    [TestCase(SteamUa, true)]
    [TestCase(ChromeUa, false)]
    [TestCase(null, false)]
    public void LegacyFlagLimitsAllScopeToSteamWhenScopeFieldsAreEmpty(string? userAgent, bool expected)
        => Assert.That(ScriptUserAgentMatcher.IsAllowed(userAgent, ScriptInjectScopeMode.All,
            ScriptUserAgentPreset.None, "# ignored\n  ", legacyOnlySteamBrowser: true), Is.EqualTo(expected));

    [Test]
    public void LegacyFlagDoesNotOverrideExplicitScopeSettings()
    {
        Assert.Multiple(() =>
        {
            Assert.That(ScriptUserAgentMatcher.IsAllowed(FirefoxUa, ScriptInjectScopeMode.All,
                ScriptUserAgentPreset.Chrome, (string?)null, legacyOnlySteamBrowser: true), Is.True);
            Assert.That(ScriptUserAgentMatcher.IsAllowed(FirefoxUa, ScriptInjectScopeMode.All,
                ScriptUserAgentPreset.None, "MyApp", legacyOnlySteamBrowser: true), Is.True);
            Assert.That(ScriptUserAgentMatcher.IsAllowed(FirefoxUa, Selected,
                ScriptUserAgentPreset.None, (string?)null, legacyOnlySteamBrowser: true), Is.False);
        });
    }

    [TestCase(SteamUa, true)]
    [TestCase("mozilla/5.0 valve steam", true)]
    [TestCase(ChromeUa, false)]
    [TestCase(EdgeUa, false)]
    [TestCase(FirefoxUa, false)]
    [TestCase("", false)]
    public void SteamPresetRequiresValveSteam(string userAgent, bool expected)
        => Assert.That(MatchesPreset(userAgent, ScriptUserAgentPreset.SteamBrowser), Is.EqualTo(expected));

    [TestCase(ChromeUa)]
    [TestCase("Mozilla/5.0 (iPhone) CriOS/130.0 Mobile Safari/604.1")]
    public void ChromePresetAcceptsDesktopAndIosChrome(string userAgent)
        => Assert.That(MatchesPreset(userAgent, ScriptUserAgentPreset.Chrome), Is.True);

    [TestCase("Edg/130.0")]
    [TestCase("EdgA/130.0")]
    [TestCase("EdgiOS/130.0")]
    [TestCase("Edge/18.0")]
    [TestCase("Valve Steam")]
    [TestCase("OPR/120.0")]
    [TestCase("Opera")]
    [TestCase("Vivaldi/7.0")]
    [TestCase("YaBrowser/24.0")]
    [TestCase("SamsungBrowser/28.0")]
    [TestCase("Electron/35.0")]
    public void ChromePresetRejectsOtherChromiumBrowsers(string token)
        => Assert.That(MatchesPreset(ChromeUa + " " + token, ScriptUserAgentPreset.Chrome), Is.False);

    [TestCase("Edg/130.0")]
    [TestCase("EdgA/130.0")]
    [TestCase("EdgiOS/130.0")]
    [TestCase("Edge/18.0")]
    public void EdgePresetMatchesEachEdgeToken(string token)
        => Assert.That(MatchesPreset(ChromeUa + " " + token, ScriptUserAgentPreset.Edge), Is.True);

    [TestCase(ChromeUa)]
    [TestCase(SteamUa + " Edg/130.0")]
    [TestCase(FirefoxUa)]
    public void EdgePresetRejectsOtherBrowsers(string userAgent)
        => Assert.That(MatchesPreset(userAgent, ScriptUserAgentPreset.Edge), Is.False);

    [TestCase(ChromeUa, ScriptUserAgentPreset.Chrome | ScriptUserAgentPreset.Edge, true)]
    [TestCase(EdgeUa, ScriptUserAgentPreset.Chrome | ScriptUserAgentPreset.Edge, true)]
    [TestCase(SteamUa, ScriptUserAgentPreset.Chrome | ScriptUserAgentPreset.Edge, false)]
    [TestCase(SteamUa, ScriptUserAgentPreset.SteamBrowser | ScriptUserAgentPreset.Edge, true)]
    [TestCase(EdgeUa, ScriptUserAgentPreset.SteamBrowser | ScriptUserAgentPreset.Edge, true)]
    [TestCase(ChromeUa, ScriptUserAgentPreset.SteamBrowser | ScriptUserAgentPreset.Edge, false)]
    [TestCase(FirefoxUa, ScriptUserAgentPreset.Chrome, false)]
    public void PresetsUseOrMatching(string userAgent, ScriptUserAgentPreset presets, bool expected)
        => Assert.That(MatchesPreset(userAgent, presets), Is.EqualTo(expected));

    [TestCase(FirefoxUa, "firefox/", true)]
    [TestCase("prefix CUSTOM/1 build/42 suffix", "custom/*build/4?", true)]
    [TestCase("custom/1 build/4", "custom/*build/4?", false)]
    [TestCase("CUSTOM/1", @"/custom\/\d/", true)]
    [TestCase("CUSTOM/1", @"/custom\/\d/i", true)]
    [TestCase("Before\nAgent\nDetails\nAfter", "/^Agent.Details$/ms", true)]
    [TestCase("Before\nagent\nDetails\nAfter", "/^Agent.Details$/ms", false)]
    [TestCase(FirefoxUa, "# Firefox/", false)]
    public void CustomRulesMatchBySyntax(string userAgent, string rule, bool expected)
    {
        var result = ScriptUserAgentMatcher.Evaluate(userAgent, Selected, ScriptUserAgentPreset.None, rule);

        Assert.That(result, Is.EqualTo(new ScriptUserAgentMatchResult(expected, expected ? rule : null)));
    }

    [Test]
    public void CustomRulesParserSkipsCommentsAndDeduplicatesIgnoringCase()
        => Assert.That(ScriptUserAgentMatcher.ParseCustomRules(" # comment\r\n Firefox/\r\nfirefox/\n\n *MyApp* "),
            Is.EqualTo(new[] { "Firefox/", "*MyApp*" }));

    [TestCase(null)]
    [TestCase(" ")]
    [TestCase("one\r\ntwo")]
    [TestCase("/[/")]
    [TestCase("/Firefox/x")]
    public void InvalidCustomRulesReturnAnError(string? input)
    {
        var valid = ScriptUserAgentMatcher.TryValidateCustomRule(input, out _, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.False);
            Assert.That(error, Is.Not.Null.And.Not.Empty);
        });
    }

    [Test]
    public void ValidCustomRuleIsTrimmed()
    {
        var valid = ScriptUserAgentMatcher.TryValidateCustomRule(" /firefox/i  ", out var rule, out var error);

        Assert.Multiple(() =>
        {
            Assert.That(valid, Is.True);
            Assert.That(rule, Is.EqualTo("/firefox/i"));
            Assert.That(error, Is.Null);
        });
    }

    [Test]
    public void InvalidRegularExpressionDoesNotPreventLaterRulesFromMatching()
    {
        var denied = ScriptUserAgentMatcher.Evaluate(FirefoxUa, Selected, ScriptUserAgentPreset.None, "/[/");
        var allowed = ScriptUserAgentMatcher.Evaluate(FirefoxUa, Selected, ScriptUserAgentPreset.None, new[] { "/[/", "Firefox/" });

        Assert.Multiple(() =>
        {
            Assert.That(denied.IsMatched, Is.False);
            Assert.That(allowed, Is.EqualTo(new ScriptUserAgentMatchResult(true, "Firefox/")));
        });
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase(SteamUa)]
    [TestCase(ChromeUa)]
    public void EmptySelectedScopeRejectsEveryUserAgent(string? userAgent)
        => Assert.That(ScriptUserAgentMatcher.IsAllowed(userAgent, Selected, ScriptUserAgentPreset.None, ""), Is.False);

    [Test]
    public void MemoryPackRoundTripPreservesScriptScope()
    {
        var settings = default(ReverseProxySettings) with
        {
            ScriptInjectScopeMode = Selected,
            ScriptUserAgentPresets = ScriptUserAgentPreset.Chrome | ScriptUserAgentPreset.Edge,
            ScriptCustomUserAgents = "Firefox/\n*MyApp*",
        };

        var restored = MemoryPackSerializer.Deserialize<ReverseProxySettings>(MemoryPackSerializer.Serialize(settings));

        Assert.That(restored, Is.EqualTo(settings));
    }

    [Test]
    public void MemoryPackReadsLegacyPayloadWithDefaultNewFields()
    {
        var legacy = new LegacyReverseProxySettings(true, true, 26501, "legacy-token");
        var restored = MemoryPackSerializer.Deserialize<ReverseProxySettings>(MemoryPackSerializer.Serialize(legacy));

        Assert.Multiple(() =>
        {
            Assert.That(restored.IsEnableScript, Is.True);
            Assert.That(restored.IsOnlyWorkSteamBrowser, Is.True);
            Assert.That(restored.ProxyPort, Is.EqualTo(26501));
            Assert.That(restored.ServerSideProxyToken, Is.EqualTo("legacy-token"));
            Assert.That(restored.ScriptInjectScopeMode, Is.EqualTo(ScriptInjectScopeMode.All));
            Assert.That(restored.ScriptUserAgentPresets, Is.EqualTo(ScriptUserAgentPreset.None));
            Assert.That(restored.ScriptCustomUserAgents, Is.Null);
        });
    }

    [Test]
    public void LegacyProxySettingsJsonKeepsSteamPreferenceAndDefaultsScope()
    {
        var settings = DeserializeSettings<ProxySettings_>("""{"IsOnlyWorkSteamBrowser":true}""");

        Assert.Multiple(() =>
        {
            Assert.That(settings.IsOnlyWorkSteamBrowser, Is.True);
            Assert.That(settings.ScriptInjectScopeMode, Is.EqualTo(ScriptInjectScopeMode.All));
            Assert.That(settings.ScriptUserAgentPresets, Is.EqualTo(ScriptUserAgentPreset.None));
            Assert.That(settings.ScriptCustomUserAgents, Is.Null);
            Assert.That(settings.ScriptScopeMigrated, Is.False);
        });
    }

    [Test]
    public void ProxySettingsJsonReadsNewFieldsAlongsideTheLegacyProperty()
    {
        var settings = DeserializeSettings<ProxySettings_>("""
            {
                "IsOnlyWorkSteamBrowser": true,
                "ScriptInjectScopeMode": "SelectedUserAgents",
                "ScriptUserAgentPresets": "Chrome, Edge",
                "ScriptCustomUserAgents": "Firefox/",
                "ScriptScopeMigrated": true
            }
            """);

        Assert.Multiple(() =>
        {
            Assert.That(settings.IsOnlyWorkSteamBrowser, Is.True);
            Assert.That(settings.ScriptInjectScopeMode, Is.EqualTo(Selected));
            Assert.That(settings.ScriptUserAgentPresets, Is.EqualTo(ScriptUserAgentPreset.Chrome | ScriptUserAgentPreset.Edge));
            Assert.That(settings.ScriptCustomUserAgents, Is.EqualTo("Firefox/"));
            Assert.That(settings.ScriptScopeMigrated, Is.True);
        });
    }

    static bool MatchesPreset(string userAgent, ScriptUserAgentPreset preset)
        => ScriptUserAgentMatcher.IsAllowed(userAgent, Selected, preset, (string?)null);

    static T DeserializeSettings<T>(string json) where T : class, ISettings<T>, new()
        => SJsonSerializer.Deserialize(json, T.JsonTypeInfo)!;

    // The old wire format stopped at field 22. Unused fields are left as gaps.
    [MemoryPackable(GenerateType.VersionTolerant, SerializeLayout.Explicit)]
    readonly partial record struct LegacyReverseProxySettings(
        [property: MemoryPackOrder(2)] bool IsEnableScript,
        [property: MemoryPackOrder(3)] bool IsOnlyWorkSteamBrowser,
        [property: MemoryPackOrder(4)] ushort ProxyPort,
        [property: MemoryPackOrder(22)] string? ServerSideProxyToken);
}

using System.Globalization;
using BD.WTTS.Client.Resources;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UnitTest;

public sealed class ScriptMetadataParserTest
{
    const string DownloadUrl = "https://example.test/demo.user.js";
    const string UpdateUrl = "https://example.test/demo.meta.js";
    const string HomepageUrl = "https://example.test/demo";
    const string IconUrl = "https://example.test/icon.png";
    const string CoreMetadata = """
        // ==UserScript==
        // @namespace example.namespace
        // @name Demo Script
        // @version 1.10.0
        // @description Default description
        // @author Demo Author
        // ==/UserScript==
        console.log("test");
        """;

    [TestCase("\n", "")]
    [TestCase("\r\n", "")]
    [TestCase("\n", "\uFEFF")]
    public void ReadsMetadataWithSupportedLineEndingsAndBom(string newline, string prefix)
    {
        var content = prefix + CoreMetadata.ReplaceLineEndings(newline);
        var result = ScriptMetadataParser.Parse(content);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasUserScriptHeader, Is.True);
            Assert.That(result.Name, Is.EqualTo("Demo Script"));
            Assert.That(result.Namespace, Is.EqualTo("example.namespace"));
            Assert.That(result.Author, Is.EqualTo("Demo Author"));
            Assert.That(result.Version, Is.EqualTo("1.10.0"));
            Assert.That(result.Description, Is.EqualTo("Default description"));
            Assert.That(result.RawContent, Is.EqualTo(content));
            Assert.That(result.ContentByteLength, Is.EqualTo(Encoding.UTF8.GetByteCount(content)));
            Assert.That(result.HeaderPreview, Does.StartWith("// ==UserScript==").And.EndWith("// ==/UserScript=="));
            Assert.That(result.HeaderPreview, Does.Not.Contain("console.log"));
        });
    }

    [Test]
    public void NamespaceWithoutNameUsesFileNameAndKeepsNamespace()
    {
        var result = ScriptMetadataParser.Parse("""
            // ==UserScript==
            // @namespace demo.namespace
            // ==/UserScript==
            """, filePath: "custom.js");

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo("custom"));
            Assert.That(result.Namespace, Is.EqualTo("demo.namespace"));
        });
    }

    [TestCase("zh-CN", "简体名称", "简体描述")]
    [TestCase("zh-Hant", "中文名称", "中文描述")]
    [TestCase("en-US", "Demo Script", "Default description")]
    public void SelectsLocaleSpecificMetadataWithFallback(string locale, string name, string description)
    {
        var content = AddMetadata("""
            // @name:zh-CN 简体名称
            // @description:zh-CN 简体描述
            // @name:zh 中文名称
            // @description:zh 中文描述
            """);

        var result = ScriptMetadataParser.Parse(content, culture: CultureInfo.GetCultureInfo(locale));

        Assert.Multiple(() =>
        {
            Assert.That(result.Name, Is.EqualTo(name));
            Assert.That(result.Description, Is.EqualTo(description));
        });
    }

    [Test]
    public void CombinesMatchAndIncludeWithoutDuplicates()
    {
        var result = ScriptMetadataParser.Parse(AddMetadata("""
            // @match https://one.example/*
            // @match https://one.example/*
            // @match https://shared.example/*
            // @include https://shared.example/*
            // @include https://two.example/*
            """));

        var matches = new[] { "https://one.example/*", "https://shared.example/*", "https://two.example/*" };
        Assert.Multiple(() =>
        {
            Assert.That(result.MatchRules, Is.EqualTo(matches[..2]));
            Assert.That(result.IncludeRules, Is.EqualTo(matches[1..]));
            Assert.That(result.EffectiveMatchRules, Is.EqualTo(matches));
            Assert.That(result.ToScriptDto().MatchDomainNamesArray, Is.EqualTo(matches));
        });
    }

    [TestCase("icon")]
    [TestCase("icon64")]
    [TestCase("iconURL")]
    public void ParsesScriptDetailsAndExclusions(string iconTag)
    {
        var result = ScriptMetadataParser.Parse(AddMetadata($"""
            // @{iconTag} {IconUrl}
            // @homepageURL {HomepageUrl}
            // @downloadURL {DownloadUrl}
            // @updateURL {UpdateUrl}
            // @exclude https://blocked.example/*
            // @exclude-match https://private.example/*
            // @grant GM_xmlhttpRequest
            // @grant GM_addStyle
            // @connect api.example.test
            // @require https://example.test/lib.js
            // @run-at document-start
            """));

        var dto = result.ToScriptDto();
        Assert.Multiple(() =>
        {
            Assert.That(result.IconUrl, Is.EqualTo(IconUrl));
            Assert.That(result.HomepageUrl, Is.EqualTo(HomepageUrl));
            Assert.That(result.DownloadUrl, Is.EqualTo(DownloadUrl));
            Assert.That(result.UpdateUrl, Is.EqualTo(UpdateUrl));
            Assert.That(result.ExcludeRules, Is.EqualTo(new[] { "https://blocked.example/*", "https://private.example/*" }));
            Assert.That(result.Grants, Is.EqualTo(new[] { "GM_xmlhttpRequest", "GM_addStyle" }));
            Assert.That(result.Connects, Is.EqualTo(new[] { "api.example.test" }));
            Assert.That(result.Requires, Is.EqualTo(new[] { "https://example.test/lib.js" }));
            Assert.That(result.RunAt, Is.EqualTo("document-start"));
            Assert.That(dto.IconUrl, Is.EqualTo(IconUrl));
            Assert.That(dto.SourceLink, Is.EqualTo(HomepageUrl));
            Assert.That(dto.DownloadLink, Is.EqualTo(DownloadUrl));
            Assert.That(dto.UpdateLink, Is.EqualTo(UpdateUrl));
            Assert.That(dto.ExcludeDomainNamesArray, Is.EqualTo(result.ExcludeRules));
        });
    }

    [TestCase("console.log('no metadata');")]
    [TestCase("// ==UserScript==\n// @name Unclosed\n")]
    public void MissingOrUnterminatedHeaderIsCritical(string content)
    {
        var result = ScriptMetadataParser.Parse(content);

        Assert.Multiple(() =>
        {
            Assert.That(result.HasUserScriptHeader, Is.False);
            Assert.That(result.Warnings.Any(warning => warning.IsCritical &&
                warning.Title == Strings.Script_Inspect_Warn_NoHeaderTitle), Is.True);
        });
    }

    [Test]
    public void MissingMatchRulesAndRemoteLinksHaveDifferentSeverities()
    {
        var result = ScriptMetadataParser.Parse(CoreMetadata);

        Assert.Multiple(() =>
        {
            Assert.That(result.Warnings.Any(warning => warning.IsWarning &&
                warning.Title == Strings.Script_Inspect_Warn_NoMatchTitle), Is.True);
            Assert.That(result.Warnings.Any(warning => warning.IsInfo &&
                warning.Title == Strings.Script_Inspect_Warn_NoUpdateUrlTitle), Is.True);
            Assert.That(result.Warnings.Any(warning => warning.IsCritical), Is.False);
        });
    }

    [Test]
    public void WarnsAboutBroadMatchAndPrivilegedGrant()
    {
        var result = ScriptMetadataParser.Parse(AddMetadata($"""
            // @match *://*/*
            // @grant GM_xmlhttpRequest
            // @downloadURL {DownloadUrl}
            """));

        Assert.Multiple(() =>
        {
            Assert.That(result.Warnings.Where(warning => warning.IsWarning).Select(warning => warning.Title),
                Is.EquivalentTo(new[] { Strings.Script_Inspect_Warn_BroadMatchTitle, Strings.Script_Inspect_Warn_HighPrivGrantTitle }));
            Assert.That(result.Warnings.Any(warning => warning.Detail.Contains("GM_xmlhttpRequest", StringComparison.Ordinal)), Is.True);
            Assert.That(result.Warnings.Any(warning => warning.IsInfo), Is.False);
        });
    }

    [Test]
    public void PrioritizesPidOverUrlAndNameWhenFindingInstalledScript()
    {
        var pid = new Guid("6fe4a75b-7ea2-44f6-966f-2e3c86404058");
        var installed = new ScriptDTO { Id = pid, Name = "Previous name", Version = "1.2.0" };
        var other = new ScriptDTO { Name = "Demo Script", DownloadLink = DownloadUrl };
        var content = AddMetadata($"// @downloadURL {DownloadUrl}");

        var result = ScriptMetadataParser.Parse(content, existingScripts: [other, installed], pid: pid);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExistingScript, Is.SameAs(installed));
            Assert.That(result.ExistingVersion, Is.EqualTo("1.2.0"));
            Assert.That(result.IsUpgrade, Is.True);
        });
    }

    [TestCase("downloadURL", DownloadUrl)]
    [TestCase("updateURL", UpdateUrl)]
    [TestCase("homepageURL", HomepageUrl)]
    public void DetectsExistingScriptByUrlAfterRename(string tag, string url)
    {
        var installed = new ScriptDTO { Name = "Previous name", DownloadLink = url, Version = "v1.10.0" };

        var result = ScriptMetadataParser.Parse(AddMetadata($"// @{tag} {url}"), existingScripts: [installed]);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExistingScript, Is.SameAs(installed));
            Assert.That(result.IsUpgrade, Is.False);
        });
    }

    [Test]
    public void DetectsExistingScriptByNameIgnoringCase()
    {
        var installed = new ScriptDTO { Name = "demo script", Version = "1.10.1" };

        var result = ScriptMetadataParser.Parse(CoreMetadata, existingScripts: [installed]);

        Assert.Multiple(() =>
        {
            Assert.That(result.ExistingScript, Is.SameAs(installed));
            Assert.That(result.IsUpgrade, Is.False);
        });
    }

    static string AddMetadata(string lines)
        => CoreMetadata.Replace("// ==/UserScript==", lines + "\n// ==/UserScript==", StringComparison.Ordinal);
}

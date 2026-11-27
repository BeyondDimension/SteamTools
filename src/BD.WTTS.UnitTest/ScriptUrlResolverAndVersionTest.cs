using System.Net;
using System.Net.Http;
using BD.WTTS.Services.Implementation;

namespace BD.WTTS.UnitTest;

public sealed class ScriptUrlResolverAndVersionTest
{
    [TestCase("1.10.0", "1.2.0", 1)]
    [TestCase("v1.10.0", "V1.2.0", 1)]
    [TestCase("2026.11.27", "2026.2.1", 1)]
    [TestCase("1.0.0", "1.0.0-beta.1", 1)]
    [TestCase("1.0.0-beta.2", "1.0.0-beta.10", -1)]
    [TestCase("1.2.0+build.1", "V1.2.0+build.2", 0)]
    [TestCase("1.2", "1.2.0", 0)]
    [TestCase("", "", 0)]
    [TestCase(null, " ", 0)]
    [TestCase("", "1.0", -1)]
    public void ComparesUserscriptVersions(string? left, string? right, int expected)
        => Assert.That(Math.Sign(ScriptVersionComparer.Compare(left, right)), Is.EqualTo(expected));

    [TestCase("https://example.test/script.user.js", ScriptSourceKind.ExternalUrl)]
    [TestCase("http://example.test/script.js", ScriptSourceKind.ExternalUrl)]
    [TestCase("https://update.greasyfork.org/scripts/42/demo.meta.js?version=2", ScriptSourceKind.GreasyFork)]
    [TestCase("https://update.sleazyfork.org/scripts/42/demo.user.js", ScriptSourceKind.GreasyFork)]
    [TestCase("https://scriptcat.org/scripts/code/33/title.user.js", ScriptSourceKind.ScriptCat)]
    public async Task DirectScriptUrlsStayOnTheirDownloadEndpoint(string url, ScriptSourceKind kind)
    {
        using var client = new HttpClient(new FixtureHandler());
        var result = await new ScriptUrlResolver(client).ResolveAsync(url);

        Assert.Multiple(() =>
        {
            Assert.That(result.OriginalInputUrl, Is.EqualTo(url));
            Assert.That(result.DownloadUrl, Is.EqualTo(url));
            Assert.That(result.SourcePageUrl, Is.Null);
            Assert.That(result.SourceKind, Is.EqualTo(kind));
        });
    }

    [Test]
    public async Task GitHubBlobUrlBecomesRawDownloadUrl()
    {
        const string url = "https://github.com/owner/repo/blob/main/scripts/my%20script.user.js";
        using var client = new HttpClient(new FixtureHandler());
        var result = await new ScriptUrlResolver(client).ResolveAsync(url);

        Assert.Multiple(() =>
        {
            Assert.That(result.DownloadUrl, Is.EqualTo("https://raw.githubusercontent.com/owner/repo/main/scripts/my%20script.user.js"));
            Assert.That(result.SourcePageUrl, Is.EqualTo(url));
            Assert.That(result.SourceKind, Is.EqualTo(ScriptSourceKind.ExternalUrl));
        });
    }

    [TestCase("greasyfork.org")]
    [TestCase("sleazyfork.org")]
    public async Task ForkPagesResolveTheirJsonCodeUrl(string host)
    {
        var page = $"https://{host}/zh-CN/scripts/42-sample";
        var code = $"https://update.{host}/scripts/42/Sample.user.js";
        using var client = new HttpClient(new FixtureHandler($"https://{host}/scripts/42.json",
            HttpStatusCode.OK, $$"""{"code_url":"{{code}}"}"""));

        var result = await new ScriptUrlResolver(client).ResolveAsync(page);

        Assert.Multiple(() =>
        {
            Assert.That(result.DownloadUrl, Is.EqualTo(code));
            Assert.That(result.SourcePageUrl, Is.EqualTo(page));
            Assert.That(result.SourceKind, Is.EqualTo(ScriptSourceKind.GreasyFork));
        });
    }

    [TestCase("greasyfork.org", HttpStatusCode.NotFound, "")]
    [TestCase("sleazyfork.org", HttpStatusCode.OK, "{")]
    public async Task ForkPagesFallBackToTheDirectCodeEndpointWhenJsonFails(string host, HttpStatusCode status, string body)
    {
        using var client = new HttpClient(new FixtureHandler($"https://{host}/scripts/42.json", status, body));
        var result = await new ScriptUrlResolver(client).ResolveAsync($"https://{host}/en/scripts/42-sample");

        Assert.That(result.DownloadUrl, Is.EqualTo($"https://{host}/scripts/42/code/script.user.js"));
    }

    [TestCase("")]
    [TestCase("/code")]
    public async Task ScriptCatPagesResolveThePublishedInstallLink(string suffix)
    {
        const string page = "https://scriptcat.org/zh-CN/script-show-page/33";
        using var client = new HttpClient(new FixtureHandler(page, HttpStatusCode.OK, """
            <a href="https://unrelated.test/scripts/code/33/ignored.user.js">Ignore</a>
            <a href="/scripts/code/34/different.user.js">Other script</a>
            <a href='/scripts/code/33/My%20Script.user.js?foo=1&amp;bar=2'>Install</a>
            """));

        var result = await new ScriptUrlResolver(client).ResolveAsync(page + suffix);

        Assert.Multiple(() =>
        {
            Assert.That(result.DownloadUrl, Is.EqualTo("https://scriptcat.org/scripts/code/33/My%20Script.user.js?foo=1&bar=2"));
            Assert.That(result.SourcePageUrl, Is.EqualTo(page + suffix));
            Assert.That(result.SourceKind, Is.EqualTo(ScriptSourceKind.ScriptCat));
        });
    }

    [TestCase(HttpStatusCode.NotFound, "")]
    [TestCase(HttpStatusCode.OK, "<html>No install link available</html>")]
    public async Task ScriptCatFallsBackToItsGenericCodeEndpoint(HttpStatusCode status, string body)
    {
        const string page = "https://scriptcat.org/zh-CN/script-show-page/33";
        using var client = new HttpClient(new FixtureHandler(page, status, body));

        var result = await new ScriptUrlResolver(client).ResolveAsync(page);

        Assert.That(result.DownloadUrl, Is.EqualTo("https://scriptcat.org/scripts/code/33/script.user.js"));
    }

    [TestCase("")]
    [TestCase("not a URL")]
    [TestCase("/relative.js")]
    [TestCase("file:///tmp/script.user.js")]
    [TestCase("ftp://example.test/script.user.js")]
    public async Task RejectsInvalidAndNonHttpUrls(string url)
    {
        using var client = new HttpClient(new FixtureHandler());
        var resolver = new ScriptUrlResolver(client);

        var exception = await Assert.ThrowsAsync<ArgumentException>(() => resolver.ResolveAsync(url));

        Assert.That(exception!.ParamName, Is.EqualTo("url"));
    }

    [Test]
    public void ClassifiesBuiltInAndWorkshopScriptsByIdentity()
    {
        var basics = new ScriptDTO { Id = ScriptExternalLinks.BasicsScriptId };
        var workshop = new ScriptDTO { Id = new Guid("4fd77c63-e74b-45cb-bbfa-f76f16556ff9") };

        Assert.Multiple(() =>
        {
            Assert.That(ScriptUrlResolver.Classify(basics), Is.EqualTo(ScriptSourceKind.Basics));
            Assert.That(ScriptUrlResolver.Classify(workshop), Is.EqualTo(ScriptSourceKind.Workshop));
        });
    }

    [TestCase(null, ScriptSourceKind.LocalFile)]
    [TestCase("https://example.test/script.user.js", ScriptSourceKind.ExternalUrl)]
    [TestCase("https://scriptcat.org/zh-CN/script-show-page/33", ScriptSourceKind.ScriptCat)]
    [TestCase("https://greasyfork.org/en/scripts/42-demo", ScriptSourceKind.GreasyFork)]
    [TestCase("https://sleazyfork.org/en/scripts/42-demo", ScriptSourceKind.GreasyFork)]
    [TestCase("https://scriptcat.org.example.test/script.user.js", ScriptSourceKind.ExternalUrl)]
    public void ClassifiesExternalAndLocalScriptsBySourceLink(string? source, ScriptSourceKind expected)
    {
        var script = new ScriptDTO { SourceLink = source ?? string.Empty };

        Assert.That(ScriptUrlResolver.Classify(script), Is.EqualTo(expected));
    }

    sealed class FixtureHandler(string? expectedUrl = null, HttpStatusCode status = HttpStatusCode.OK, string body = "") : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Assert.That(request.Method, Is.EqualTo(HttpMethod.Get));
            Assert.That(request.RequestUri?.AbsoluteUri, Is.EqualTo(expectedUrl), "Unexpected HTTP request in fixture.");
            return Task.FromResult(new HttpResponseMessage(status) { Content = new StringContent(body) });
        }
    }
}

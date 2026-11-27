using System.Net;
using System.Net.Http;
using BD.Common.Services;
using BD.WTTS.Repositories.Abstractions;
using BD.WTTS.Services.Implementation;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using HttpClientFactory = System.Net.Http.Client.IHttpClientFactory;

namespace BD.WTTS.UnitTest;

public sealed class ScriptManagerHttpLifetimeTest
{
    const string ScriptUrl = "https://scripts.example.test/demo.user.js";
    const string EmptyDownloadUrl = "https://scripts.example.test/empty.user.js";
    const string WorkshopProbeUrl = "https://scripts.example.test/accelerator/scripts/1/100/";
    const string Script = """
        // ==UserScript==
        // @name Demo Script
        // @version 2.0.0
        // @match https://example.test/*
        // ==/UserScript==
        """;

    public enum ScriptOperation
    {
        Inspect,
        CheckSingle,
        CheckBatch,
        Download,
    }

    [TestCase(ScriptOperation.Inspect)]
    [TestCase(ScriptOperation.CheckSingle)]
    [TestCase(ScriptOperation.CheckBatch)]
    [TestCase(ScriptOperation.Download)]
    public async Task ScriptOperationKeepsFactoryClientAndHandlerAlive(ScriptOperation operation)
    {
        using var handler = new TrackingHandler();
        using var sharedClient = new HttpClient(handler);
        using var workshopClient = new HttpClient(handler, disposeHandler: false);
        var platform = new Mock<IHttpPlatformHelperService>();
        platform.SetupGet(service => service.UserAgent).Returns("WattToolkit/Test");
        var manager = new ScriptManager(new SharedClientFactory(sharedClient), Mock.Of<IScriptRepository>(),
            NullLoggerFactory.Instance, Mock.Of<IToast>(), platform.Object, Mock.Of<IMicroServiceClient>());
        var script = new ScriptDTO { Name = "Demo Script", Version = "1.0.0", DownloadLink = ScriptUrl };

        for (var attempt = 0; attempt < 2; attempt++)
        {
            switch (operation)
            {
                case ScriptOperation.Inspect:
                    var inspection = await manager.InspectScriptFromUrlAsync(ScriptUrl);
                    Assert.That(inspection.Content?.Version, Is.EqualTo("2.0.0"));
                    break;
                case ScriptOperation.CheckSingle:
                    var single = await manager.CheckScriptUpdateAsync(script);
                    Assert.That(single.Status, Is.EqualTo(ScriptUpdateCheckStatus.UpdateAvailable));
                    break;
                case ScriptOperation.CheckBatch:
                    var batch = await manager.CheckAllScriptUpdatesAsync([script, script]);
                    Assert.That(batch.UpdateAvailableCount, Is.EqualTo(2));
                    break;
                case ScriptOperation.Download:
                    // An HTTP 204 exercises the download client without writing to the plugin's cache.
                    var download = await manager.DownloadScriptAsync(EmptyDownloadUrl);
                    Assert.That(download.Code, Is.EqualTo(ApiRspCode.NoResponseContentValue));
                    break;
            }

            Assert.That(handler.Disposed, Is.False, $"The shared handler was disposed by {operation}.");
            Assert.That(await sharedClient.GetStringAsync(WorkshopProbeUrl), Is.EqualTo("Workshop OK"));
            Assert.That(await workshopClient.GetStringAsync(WorkshopProbeUrl), Is.EqualTo("Workshop OK"));
        }
    }

    sealed class SharedClientFactory(HttpClient client) : HttpClientFactory
    {
        public HttpClient CreateClient(string name, HttpHandlerCategory category = default) => client;
    }

    sealed class TrackingHandler : HttpMessageHandler
    {
        public bool Disposed { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            ObjectDisposedException.ThrowIf(Disposed, this);
            cancellationToken.ThrowIfCancellationRequested();
            var content = request.RequestUri?.AbsoluteUri switch
            {
                ScriptUrl => Script,
                EmptyDownloadUrl => null,
                WorkshopProbeUrl => "Workshop OK",
                _ => throw new InvalidOperationException($"Unexpected HTTP request to {request.RequestUri}."),
            };
            return Task.FromResult(content is null
                ? new HttpResponseMessage(HttpStatusCode.NoContent)
                : new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(content) });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
                Disposed = true;
            base.Dispose(disposing);
        }
    }
}

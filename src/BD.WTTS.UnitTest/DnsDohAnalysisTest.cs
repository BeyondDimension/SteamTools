using System.Collections.Concurrent;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Reflection;
using Ae.Dns.Client;
using BD.WTTS.Enums;
using DnsClient;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace BD.WTTS.UnitTest;

public sealed class DnsDohAnalysisTest
{
    [Test]
    public async Task FailedAndEmptyServersFallBackInOrderAndOnlySuccessfulAnswersAreCached()
    {
        var calls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://failed.invalid/dns-query", new DnsHandler("failed", calls, HttpStatusCode.ServiceUnavailable));
        AddServer(doh, "https://empty.invalid/dns-query", new DnsHandler("empty", calls, HttpStatusCode.OK, false));
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", calls, HttpStatusCode.OK, true));
        AddServer(doh, "https://unused.invalid/dns-query", new DnsHandler("unused", calls, HttpStatusCode.OK, true));

        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://failed.invalid/dns-query\nhttps://empty.invalid/dns-query\nhttps://working.invalid/dns-query\nhttps://unused.invalid/dns-query");
        settings.SetupGet(x => x.ProxyMode).Returns(ProxyMode.System);
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);

        var first = await resolver.AnalysisDomainIpAsync("example.test", null, false).ToArrayAsync();
        var second = await resolver.AnalysisDomainIpAsync("example.test", null, false).ToArrayAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
            Assert.That(second, Is.EqualTo(first));
            Assert.That(calls, Is.EqualTo(new[] { "failed", "empty", "working", "failed", "empty" }));
        });
    }

    [Test]
    public async Task ZeroTtlAnswerIsNotCached()
    {
        var calls = new List<string>();
        byte currentAddress = 17;
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", calls, HttpStatusCode.OK, true, ttl: 0, address: () => currentAddress));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);

        var first = await resolver.AnalysisDomainIpAsync("zero-ttl.example.test", null, false).ToArrayAsync();
        currentAddress = 18;
        var second = await resolver.AnalysisDomainIpAsync("zero-ttl.example.test", null, false).ToArrayAsync();

        Assert.That(first, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
        Assert.That(second, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.18") }));
    }

    [Test]
    public async Task RecoveredPreferredDnsIsNotMaskedByFallbackCache()
    {
        await using var preferred = new DnsUdpServer(false, 18);
        await using var fallback = new DnsUdpServer(true, 17);
        var preferredIp = IPAddress.Parse("192.0.2.1");
        var fallbackIp = IPAddress.Parse("192.0.2.2");
        var dns = new DnsAnalysisServiceImpl();
        var field = typeof(DnsAnalysisServiceImpl).GetField("serverClients", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var clients = (ConcurrentDictionary<IPAddress, LookupClient>)field.GetValue(dns)!;
        clients[preferredIp] = new LookupClient(new LookupClientOptions(preferred.EndPoint) { Retries = 0 });
        clients[fallbackIp] = new LookupClient(new LookupClientOptions(fallback.EndPoint) { Retries = 0 });
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("192.0.2.1\n192.0.2.2");
        settings.SetupGet(x => x.ProxyMode).Returns(ProxyMode.System);
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, dns, doh);

        var first = await resolver.AnalysisDomainIpAsync("recovery.example.test", null, false).ToArrayAsync();
        preferred.Success = true;
        var second = await resolver.AnalysisDomainIpAsync("recovery.example.test", null, false).ToArrayAsync();

        Assert.Multiple(() =>
        {
            Assert.That(first, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
            Assert.That(second, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.18") }));
        });
    }

    [Test]
    public async Task MixedServersFallBackAcrossProtocolsForAddressesAndTtl()
    {
        var calls = new ConcurrentQueue<string>();
        await using var emptyDns = new DnsUdpServer(false, 18, () => calls.Enqueue("dns"));
        var dns = new DnsAnalysisServiceImpl();
        var field = typeof(DnsAnalysisServiceImpl).GetField("serverClients", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var clients = (ConcurrentDictionary<IPAddress, LookupClient>)field.GetValue(dns)!;
        clients[IPAddress.Parse("192.0.2.1")] = new LookupClient(new LookupClientOptions(emptyDns.EndPoint) { Retries = 0 });
        var dohCalls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://failed.invalid/dns-query", new DnsHandler("failed", dohCalls, HttpStatusCode.ServiceUnavailable, onCall: () => calls.Enqueue("failed")));
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", dohCalls, HttpStatusCode.OK, true, () => calls.Enqueue("working")));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://failed.invalid/dns-query\n192.0.2.1\nhttps://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, dns, doh);

        var addresses = await resolver.AnalysisDomainIpAsync("mixed.example.test", null, false).ToArrayAsync();
        Assert.That(addresses, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
        Assert.That(calls.ToArray(), Is.EqualTo(new[] { "failed", "dns", "working" }));
        calls.Clear();
        var ttl = await resolver.AnalysisHostnameTimeAsync("ttl.example.test");
        Assert.That(ttl, Is.EqualTo(60));
        Assert.That(calls.ToArray(), Is.EqualTo(new[] { "failed", "dns", "working" }));
    }

    [Test]
    public async Task PreferredDnsWinsBeforeDohAndIsRetriedAfterFallback()
    {
        var calls = new ConcurrentQueue<string>();
        await using var preferred = new DnsUdpServer(false, 18, () => calls.Enqueue("dns"));
        var dns = new DnsAnalysisServiceImpl();
        var field = typeof(DnsAnalysisServiceImpl).GetField("serverClients", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var clients = (ConcurrentDictionary<IPAddress, LookupClient>)field.GetValue(dns)!;
        clients[IPAddress.Parse("192.0.2.1")] = new LookupClient(new LookupClientOptions(preferred.EndPoint) { Retries = 0 });
        var dohCalls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("doh", dohCalls, HttpStatusCode.OK, true, () => calls.Enqueue("doh")));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("192.0.2.1\nhttps://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, dns, doh);
        var first = await resolver.AnalysisDomainIpAsync("mixed-recovery.example.test", null, false).ToArrayAsync();
        preferred.Success = true;
        var second = await resolver.AnalysisDomainIpAsync("mixed-recovery.example.test", null, false).ToArrayAsync();
        Assert.That(first, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
        Assert.That(second, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.18") }));
        Assert.That(calls.ToArray(), Is.EqualTo(new[] { "dns", "doh", "dns" }));
    }

    [Test]
    public async Task LiteralAddressDoesNotQueryConfiguredServers()
    {
        var calls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", calls, HttpStatusCode.OK, true));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);
        var result = await resolver.AnalysisDomainIpAsync("2001:db8::1", null, true).ToArrayAsync();
        Assert.That(result, Is.EqualTo(new[] { IPAddress.Parse("2001:db8::1") }));
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public async Task EmptyIpv6AnswerFallsBackToIpv4()
    {
        var calls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", calls, HttpStatusCode.OK, true));

        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);
        var result = await resolver.AnalysisDomainIpAsync("example.test", null, true).ToArrayAsync();

        Assert.That(result, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
        Assert.That(calls, Is.EqualTo(new[] { "working", "working" }));
    }

    [Test]
    public void CancelledLookupDoesNotTryServers()
    {
        var calls = new List<string>();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://working.invalid/dns-query", new DnsHandler("working", calls, HttpStatusCode.OK, true));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://working.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);
        using var cancel = new CancellationTokenSource();
        cancel.Cancel();

        Assert.ThrowsAsync<OperationCanceledException>(async () =>
            await resolver.AnalysisDomainIpAsync("example.test", null, false, cancel.Token).ToArrayAsync(cancel.Token));
        Assert.That(calls, Is.Empty);
    }

    [Test]
    public void CancellationDuringLookupStopsBeforeFallback()
    {
        var calls = new List<string>();
        using var cancel = new CancellationTokenSource();
        using var doh = new DnsDohAnalysisService(NullLoggerFactory.Instance, null!, null!);
        AddServer(doh, "https://first.invalid/dns-query", new DnsHandler("first", calls, HttpStatusCode.ServiceUnavailable, onCall: cancel.Cancel));
        AddServer(doh, "https://second.invalid/dns-query", new DnsHandler("second", calls, HttpStatusCode.OK, true));
        var settings = new Mock<IReverseProxySettings>();
        settings.SetupGet(x => x.ProxyDNSAddresses).Returns("https://first.invalid/dns-query\nhttps://second.invalid/dns-query");
        var resolver = new DnsAnalysisServiceSwitchImpl(settings.Object, new DnsAnalysisServiceImpl(), doh);
        Assert.CatchAsync<OperationCanceledException>(async () =>
            await resolver.AnalysisDomainIpAsync("cancel.example.test", null, false, cancel.Token).ToArrayAsync(cancel.Token));
        Assert.That(calls, Is.EqualTo(new[] { "first" }));
    }

    [TestCase("https://doh.pub/resolve", "https://doh.pub/dns-query")]
    [TestCase("https://dns.alidns.com/resolve", "https://dns.alidns.com/dns-query")]
    [TestCase("https://custom.invalid/resolve?key=value", "https://custom.invalid/resolve?key=value")]
    public void MigrationChangesOnlyPreviouslyBundledEndpoints(string original, string expected)
        => Assert.That(IDnsAnalysisService.NormalizeLegacyDohEndpoint(original), Is.EqualTo(expected));

    static void AddServer(DnsDohAnalysisService doh, string address, HttpMessageHandler handler)
    {
        var uri = new Uri(address);
        var field = typeof(DnsDohAnalysisService).GetField("dnsClients", BindingFlags.Instance | BindingFlags.NonPublic)!;
        var clients = (ConcurrentDictionary<Uri, Lazy<DnsHttpClient>>)field.GetValue(doh)!;
        clients[uri] = new Lazy<DnsHttpClient>(() => new DnsHttpClient(
            new DnsHttpClientOptions { Path = uri.PathAndQuery }, new HttpClient(handler) { BaseAddress = uri }));
    }

    sealed class DnsUdpServer : IAsyncDisposable
    {
        readonly UdpClient udp = new(new IPEndPoint(IPAddress.Loopback, 0));
        readonly CancellationTokenSource stop = new();
        readonly Task worker;
        readonly byte lastOctet;
        readonly Action? onCall;

        public volatile bool Success;
        public IPEndPoint EndPoint => (IPEndPoint)udp.Client.LocalEndPoint!;

        public DnsUdpServer(bool success, byte lastOctet, Action? onCall = null)
        {
            Success = success;
            this.lastOctet = lastOctet;
            this.onCall = onCall;
            worker = RunAsync();
        }

        async Task RunAsync()
        {
            try
            {
                while (!stop.IsCancellationRequested)
                {
                    var packet = await udp.ReceiveAsync(stop.Token);
                    onCall?.Invoke();
                    var response = CreateResponse(packet.Buffer, Success, lastOctet);
                    await udp.SendAsync(response, packet.RemoteEndPoint, stop.Token);
                }
            }
            catch (OperationCanceledException) when (stop.IsCancellationRequested) { }
            catch (ObjectDisposedException) when (stop.IsCancellationRequested) { }
        }

        public async ValueTask DisposeAsync()
        {
            await stop.CancelAsync();
            udp.Dispose();
            await worker;
            stop.Dispose();
        }
    }

    static byte[] CreateResponse(byte[] query, bool answer, byte lastOctet = 17, byte ttl = 60)
    {
        var end = 12;
        while (query[end] != 0)
            end += query[end] + 1;
        end += 5;
        var isIpv4 = query[end - 3] == 1;
        var data = new byte[end + (answer && isIpv4 ? 16 : 0)];
        Array.Copy(query, data, end);
        data[2] = 0x81;
        data[3] = 0x80;
        data[6] = 0;
        data[7] = (byte)(answer && isIpv4 ? 1 : 0);
        data[8] = data[9] = data[10] = data[11] = 0;
        if (answer && isIpv4)
        {
            byte[] record = [0xc0, 0x0c, 0, 1, 0, 1, 0, 0, 0, ttl, 0, 4, 203, 0, 113, lastOctet];
            record.CopyTo(data, end);
        }
        return data;
    }

    sealed class DnsHandler(string name, List<string> calls, HttpStatusCode status, bool answer = false, Action? onCall = null, byte ttl = 60, Func<byte>? address = null) : HttpMessageHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            calls.Add(name);
            onCall?.Invoke();
            if (status != HttpStatusCode.OK)
                return new HttpResponseMessage(status);

            var query = await request.Content!.ReadAsByteArrayAsync(cancellationToken);
            var data = CreateResponse(query, answer, address?.Invoke() ?? 17, ttl);
            var response = new HttpResponseMessage(HttpStatusCode.OK) { Content = new ByteArrayContent(data) };
            response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/dns-message");
            return response;
        }
    }
}

using System.Net;
using System.Net.Sockets;
using System.Text;
using BD.WTTS.Models;
using Moq;

namespace BD.WTTS.UnitTest;

public sealed class AcceleratorProxyHandshakeTest
{
    [TestCase(1)]
    [TestCase(3)]
    [TestCase(4)]
    public async Task Socks5FragmentedReplyWithEachAddressTypeLeavesTunnelClean(byte addressType)
    {
        await RunProxyAsync("socks5", "http://example.test/", async (stream, token) =>
        {
            var hello = new byte[3];
            await stream.ReadExactlyAsync(hello, token);
            await stream.WriteAsync(new byte[] { 5 }, token);
            await Task.Delay(30, token);
            await stream.WriteAsync(new byte[] { 0 }, token);
            var header = new byte[4];
            await stream.ReadExactlyAsync(header, token);
            var addressLength = header[3] switch
            {
                1 => 4,
                4 => 16,
                3 => await ReadByteAsync(stream, token),
                _ => throw new InvalidDataException(),
            };
            await stream.ReadExactlyAsync(new byte[addressLength + 2], token);
            var boundAddress = addressType switch
            {
                1 => new byte[] { 127, 0, 0, 1 },
                4 => new byte[16],
                3 => new byte[] { 3, (byte)'d', (byte)'n', (byte)'s' },
                _ => throw new InvalidDataException(),
            };
            await stream.WriteAsync(new byte[] { 5, 0, 0, addressType }, token);
            await Task.Delay(30, token);
            await stream.WriteAsync(boundAddress, token);
            await stream.WriteAsync(new byte[] { 0, 80 }, token);
        });
    }

    [Test]
    public async Task Socks5FragmentedAuthenticationCanEstablishTunnel()
    {
        await RunProxyAsync("socks5", "http://example.test/", async (stream, token) =>
        {
            var hello = new byte[3];
            await stream.ReadExactlyAsync(hello, token);
            Assert.That(hello, Is.EqualTo(new byte[] { 5, 1, 2 }));
            await stream.WriteAsync(new byte[] { 5 }, token);
            await Task.Delay(30, token);
            await stream.WriteAsync(new byte[] { 2 }, token);
            var version = await ReadByteAsync(stream, token);
            var userLength = await ReadByteAsync(stream, token);
            var user = new byte[userLength];
            await stream.ReadExactlyAsync(user, token);
            var passwordLength = await ReadByteAsync(stream, token);
            var password = new byte[passwordLength];
            await stream.ReadExactlyAsync(password, token);
            Assert.That(version, Is.EqualTo(1));
            Assert.That(Encoding.ASCII.GetString(user), Is.EqualTo("user"));
            Assert.That(Encoding.ASCII.GetString(password), Is.EqualTo("secret"));
            await stream.WriteAsync(new byte[] { 1 }, token);
            await Task.Delay(30, token);
            await stream.WriteAsync(new byte[] { 0 }, token);
            var header = new byte[4];
            await stream.ReadExactlyAsync(header, token);
            var addressLength = header[3] == 3 ? await ReadByteAsync(stream, token) : 4;
            await stream.ReadExactlyAsync(new byte[addressLength + 2], token);
            await stream.WriteAsync(new byte[] { 5, 0, 0, 1, 127, 0, 0, 1, 0, 80 }, token);
        }, credentials: new NetworkCredential("user", "secret"));
    }

    [Test]
    public async Task Socks4FragmentedReplyLeavesTunnelClean()
    {
        await RunProxyAsync("socks4", "http://192.0.2.12/", async (stream, token) =>
        {
            await stream.ReadExactlyAsync(new byte[9], token);
            await stream.WriteAsync(new byte[] { 0, 0x5a }, token);
            await Task.Delay(30, token);
            await stream.WriteAsync(new byte[6], token);
        });
    }

    [Test]
    public async Task HttpConnectFragmentedHeadersLeaveTunnelClean()
    {
        await RunProxyAsync("http", "http://example.test:18400/", async (stream, token) =>
        {
            await ReadHeadersAsync(stream, token);
            await stream.WriteAsync("HTTP/1.1 200 Connection Established\r\n"u8.ToArray(), token);
            await Task.Delay(30, token);
            await stream.WriteAsync("Proxy-Agent: test\r\n\r\n"u8.ToArray(), token);
        }, "example.test:18400");
    }

    static async Task RunProxyAsync(string scheme, string destination, Func<NetworkStream, CancellationToken, Task> handshake, string? expectedHost = null, NetworkCredential? credentials = null)
    {
        using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(8));
        using var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        string? forwardedHeaders = null;
        var server = ServeAsync();
        var proxy = new WebProxy($"{scheme}://127.0.0.1:{port}") { Credentials = credentials };
        var handler = new ReverseProxyHttpClientHandler(new DomainConfig(), new Mock<IDomainResolver>().Object, proxy);
        using var client = new HttpClient(handler);
        try
        {
            var result = await client.GetStringAsync(destination, cancel.Token);
            Assert.That(result, Is.EqualTo("OK"));
            await server;
            if (expectedHost != null)
                Assert.That(forwardedHeaders, Does.Contain($"Host: {expectedHost}\r\n"));
        }
        finally
        {
            await cancel.CancelAsync();
        }

        async Task ServeAsync()
        {
            using var socket = await listener.AcceptTcpClientAsync(cancel.Token);
            using var stream = socket.GetStream();
            await handshake(stream, cancel.Token);
            forwardedHeaders = await ReadHeadersAsync(stream, cancel.Token);
            await stream.WriteAsync("HTTP/1.1 200 OK\r\nContent-Length: 2\r\nConnection: close\r\n\r\nOK"u8.ToArray(), cancel.Token);
        }
    }

    static async Task<int> ReadByteAsync(NetworkStream stream, CancellationToken token)
    {
        var data = new byte[1];
        await stream.ReadExactlyAsync(data, token);
        return data[0];
    }

    static async Task<string> ReadHeadersAsync(NetworkStream stream, CancellationToken token)
    {
        var recent = 0;
        using var bytes = new MemoryStream();
        for (var i = 0; i < 8192; i++)
        {
            var value = await ReadByteAsync(stream, token);
            bytes.WriteByte((byte)value);
            recent = (recent << 8) | value;
            if (recent == 0x0d0a0d0a)
                return Encoding.ASCII.GetString(bytes.ToArray());
        }
        throw new InvalidDataException("HTTP headers are too long.");
    }
}

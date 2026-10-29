using System.Net;
using System.Net.Sockets;
using System.Reflection;
using BD.WTTS.Settings;

namespace BD.WTTS.UnitTest;

public sealed class NetworkTestDnsTest
{
    [Test]
    public async Task AcceptsMatchingAnswerButNotAdditionalRecords()
    {
        var answer = await ProbeAsync(query => CreateResponse(query, answer: true));
        var additionalOnly = await ProbeAsync(query => CreateResponse(query, answer: false, additional: true));
        Assert.That(answer.Address, Is.EqualTo(new[] { IPAddress.Parse("203.0.113.17") }));
        Assert.That(additionalOnly.Address, Is.Empty);
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    public void RejectsDnsErrorsAndMismatchedTransactionIds(bool mismatch, bool dnsError)
        => Assert.CatchAsync<IOException>(async () => await ProbeAsync(query => CreateResponse(query, true, mismatch: mismatch, dnsError: dnsError)));

    [Test]
    public async Task CancellingCallerInterruptsAsyncUdpReceive()
    {
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        using var cancel = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var pending = CallProbe(server, cancel.Token);
        await server.ReceiveAsync(cancel.Token);
        await cancel.CancelAsync();
        Assert.CatchAsync<OperationCanceledException>(async () => await pending);
    }

    static async Task<(long DelayMs, IPAddress[] Address)> ProbeAsync(Func<byte[], byte[]> respond)
    {
        using var server = new UdpClient(new IPEndPoint(IPAddress.Loopback, 0));
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var pending = CallProbe(server, timeout.Token);
        var packet = await server.ReceiveAsync(timeout.Token);
        await server.SendAsync(respond(packet.Buffer), packet.RemoteEndPoint, timeout.Token);
        return await pending;
    }

    static Task<(long DelayMs, IPAddress[] Address)> CallProbe(UdpClient server, CancellationToken token)
    {
        var assembly = typeof(ProxySettings_).Assembly;
        var type = assembly.GetType("BD.WTTS.Services.Implementation.NetworkTestService", true)!;
        var recordType = assembly.GetType("BD.WTTS.Services.Implementation.NetworkTestService+DnsQueryAnswerRecord+DnsRecordType", true)!;
        var method = type.GetMethod("TestDNSAsync", BindingFlags.Public | BindingFlags.Instance)!;
        return (Task<(long DelayMs, IPAddress[] Address)>)method.Invoke(Activator.CreateInstance(type),
            ["example.test", "127.0.0.1", ((IPEndPoint)server.Client.LocalEndPoint!).Port, Enum.Parse(recordType, "A"), token])!;
    }

    static byte[] CreateResponse(byte[] query, bool answer, bool additional = false, bool mismatch = false, bool dnsError = false)
    {
        var end = 12;
        while (query[end] != 0)
            end += query[end] + 1;
        end += 5;
        var data = new byte[end + (answer || additional ? 16 : 0)];
        Array.Copy(query, data, end);
        if (mismatch)
            data[1] ^= 1;
        data[2] = 0x81;
        data[3] = (byte)(dnsError ? 0x83 : 0x80);
        data[6] = 0;
        data[7] = (byte)(answer ? 1 : 0);
        data[8] = data[9] = data[10] = 0;
        data[11] = (byte)(additional ? 1 : 0);
        if (answer || additional)
        {
            byte[] record = [0xc0, 0x0c, 0, 1, 0, 1, 0, 0, 0, 60, 0, 4, 203, 0, 113, 17];
            record.CopyTo(data, end);
        }
        return data;
    }
}

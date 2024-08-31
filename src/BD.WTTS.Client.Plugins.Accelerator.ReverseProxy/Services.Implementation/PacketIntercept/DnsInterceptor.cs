// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Dns/DnsInterceptor.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

using WinDivertSharp;
using DNS.Protocol;
using DNS.Protocol.ResourceRecords;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IDnsInterceptor"/>
sealed partial class DnsInterceptor : IDnsInterceptor, IDisposable
{
    const string DNS_FILTER = "udp.DstPort == 53";

    readonly IReverseProxyConfig reverseProxyConfig;
    readonly ILogger<DnsInterceptor> logger;

    readonly TimeSpan ttl = TimeSpan.FromMinutes(5d);
    bool disposedValue;

    /// <summary>
    /// 刷新 DNS 缓存
    /// </summary>
    [LibraryImport("dnsapi.dll", EntryPoint = "DnsFlushResolverCache", SetLastError = true)]
    static partial void DnsFlushResolverCache();

    static DnsInterceptor()
    {
        // 首次加载驱动往往有异常，所以要提前加载
        var handle = WinDivert.WinDivertOpen("false", WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
        WinDivert.WinDivertClose(handle);
    }

    public DnsInterceptor(
        IReverseProxyConfig reverseProxyConfig,
        ILogger<DnsInterceptor> logger)
    {
        this.reverseProxyConfig = reverseProxyConfig;
        this.logger = logger;
    }

    public async Task InterceptAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        var handle = WinDivert.WinDivertOpen(DNS_FILTER, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
        if (handle == new IntPtr(unchecked((long)ulong.MaxValue)))
        {
            throw new Win32Exception();
        }

        cancellationToken.Register(hwnd =>
        {
            WinDivert.WinDivertClose((IntPtr)hwnd!);
            DnsFlushResolverCache();
        }, handle);

        var packetLength = 0U;
        using WinDivertBuffer winDivertBuffer = new();
        WinDivertAddress winDivertAddress = default;

        DnsFlushResolverCache();
        while (!cancellationToken.IsCancellationRequested)
        {
            if (WinDivert.WinDivertRecv(handle, winDivertBuffer, ref winDivertAddress, ref packetLength) == false)
            {
                throw new Win32Exception();
            }

            try
            {
                ModifyDnsPacket(winDivertBuffer, ref winDivertAddress, ref packetLength);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex.Message);
            }
            finally
            {
                WinDivert.WinDivertSend(handle, winDivertBuffer, packetLength, ref winDivertAddress);
            }
        }
    }

    /// <summary>
    /// 修改 DNS 数据包
    /// </summary>
    /// <param name="winDivertBuffer"></param>
    /// <param name="winDivertAddress"></param>
    /// <param name="packetLength"></param>
    unsafe void ModifyDnsPacket(WinDivertBuffer winDivertBuffer, ref WinDivertAddress winDivertAddress, ref uint packetLength)
    {
        var packet = WinDivert.WinDivertHelperParsePacket(winDivertBuffer, packetLength);
        var requestPayload = new Span<byte>(packet.PacketPayload, (int)packet.PacketPayloadLength).ToArray();

        if (!TryParseRequest(requestPayload, out var request) ||
            request.OperationCode != OperationCode.Query ||
            request.Questions.Count == 0)
        {
            return;
        }

        var question = request.Questions.First();
        if (question.Type != RecordType.A && question.Type != RecordType.AAAA)
        {
            return;
        }

        var domain = question.Name;
        if (!reverseProxyConfig.IsMatch(question.Name.ToString()))
        {
            return;
        }

        // DNS 响应数据
        var response = Response.FromRequest(request);
        var loopback = question.Type == RecordType.A ? IPAddress.Loopback : IPAddress.IPv6Loopback;
        var record = new IPAddressResourceRecord(domain, loopback, ttl);
        response.AnswerRecords.Add(record);
        var responsePayload = response.ToArray();

        // 修改 payload 和包长 
        responsePayload.CopyTo(new Span<byte>(packet.PacketPayload, responsePayload.Length));
        packetLength = (uint)((int)packetLength + responsePayload.Length - requestPayload.Length);

        // 修改 IP 包
        IPAddress destAddress;
        if (packet.IPv4Header != null)
        {
            destAddress = packet.IPv4Header->DstAddr;
            packet.IPv4Header->DstAddr = packet.IPv4Header->SrcAddr;
            packet.IPv4Header->SrcAddr = destAddress;
            packet.IPv4Header->Length = (ushort)packetLength;
        }
        else
        {
            destAddress = packet.IPv6Header->DstAddr;
            packet.IPv6Header->DstAddr = packet.IPv6Header->SrcAddr;
            packet.IPv6Header->SrcAddr = destAddress;
            packet.IPv6Header->Length = (ushort)(packetLength - sizeof(IPv6Header));
        }

        // 修改 UDP 包
        (packet.UdpHeader->SrcPort, packet.UdpHeader->DstPort) = (packet.UdpHeader->DstPort, packet.UdpHeader->SrcPort);
        packet.UdpHeader->Length = (ushort)(sizeof(UdpHeader) + responsePayload.Length);

        winDivertAddress.Impostor = true;
        winDivertAddress.Direction = winDivertAddress.Loopback
            ? WinDivertDirection.Outbound
            : WinDivertDirection.Inbound;

        WinDivert.WinDivertHelperCalcChecksums(winDivertBuffer, packetLength, ref winDivertAddress, WinDivertChecksumHelperParam.All);

        logger.LogInformation($"{domain} -> {loopback}");
    }

    static bool TryParseRequest(byte[] payload, [NotNullWhen(true)] out Request? request)
    {
        try
        {
            request = Request.FromArray(payload);
            return true;
        }
        catch (Exception)
        {
            request = null;
            return false;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // TODO: 释放托管状态(托管对象)
                using var p = Process.Start(new ProcessStartInfo()
                {
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    FileName = "sc",
                    Arguments = "stop WinDivert1.4",
                });
            }

            // TODO: 释放未托管的资源(未托管的对象)并重写终结器
            // TODO: 将大型字段设置为 null
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}

#endif
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.PacketIntercept/Tcp/TcpInterceptor.cs

#if WINDOWS && !REMOVE_DNS_INTERCEPT

using WinDivertSharp;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="ITcpInterceptor"/>
abstract class TcpInterceptor : ITcpInterceptor
{
    readonly string filter;
    readonly ushort oldServerPort;
    readonly ushort newServerPort;
    readonly ILogger logger;

    public TcpInterceptor(int oldServerPort, int newServerPort, ILogger logger)
    {
        filter = $"loopback and (tcp.DstPort == {oldServerPort} or tcp.SrcPort == {newServerPort})";
        this.oldServerPort = (ushort)oldServerPort;
        this.newServerPort = (ushort)newServerPort;
        this.logger = logger;
    }

    /// <summary>
    /// 拦截指定端口的数据包
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="Win32Exception"></exception>
    public async Task InterceptAsync(CancellationToken cancellationToken)
    {
        if (oldServerPort == newServerPort)
        {
            return;
        }

        await Task.Yield();

        var handle = WinDivert.WinDivertOpen(filter, WinDivertLayer.Network, 0, WinDivertOpenFlags.None);
        if (handle == new IntPtr(unchecked((long)ulong.MaxValue)))
        {
            throw new Win32Exception();
        }

        if (Socket.OSSupportsIPv4)
        {
            logger.LogInformation($"{IPAddress.Loopback}:{oldServerPort} <=> {IPAddress.Loopback}:{newServerPort}");
        }
        if (Socket.OSSupportsIPv6)
        {
            logger.LogInformation($"{IPAddress.IPv6Loopback}:{oldServerPort} <=> {IPAddress.IPv6Loopback}:{newServerPort}");
        }
        cancellationToken.Register(hwnd => WinDivert.WinDivertClose((IntPtr)hwnd!), handle);

        var packetLength = 0U;
        using WinDivertBuffer winDivertBuffer = new();
        WinDivertAddress winDivertAddress = default;

        while (!cancellationToken.IsCancellationRequested)
        {
            winDivertAddress.Reset();
            if (WinDivert.WinDivertRecv(handle, winDivertBuffer, ref winDivertAddress, ref packetLength) == false)
            {
                throw new Win32Exception();
            }

            try
            {
                ModifyTcpPacket(winDivertBuffer, ref winDivertAddress, ref packetLength);
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
    /// 修改 TCP 数据端口的端口
    /// </summary>
    /// <param name="winDivertBuffer"></param>
    /// <param name="winDivertAddress"></param>
    /// <param name="packetLength"></param>
    unsafe void ModifyTcpPacket(WinDivertBuffer winDivertBuffer, ref WinDivertAddress winDivertAddress, ref uint packetLength)
    {
        var packet = WinDivert.WinDivertHelperParsePacket(winDivertBuffer, packetLength);
        if (packet.IPv4Header != null && packet.IPv4Header->SrcAddr.Equals(IPAddress.Loopback) == false)
        {
            return;
        }
        if (packet.IPv6Header != null && packet.IPv6Header->SrcAddr.Equals(IPAddress.IPv6Loopback) == false)
        {
            return;
        }

        if (packet.TcpHeader->DstPort == oldServerPort)
        {
            packet.TcpHeader->DstPort = newServerPort;
        }
        else
        {
            packet.TcpHeader->SrcPort = oldServerPort;
        }
        winDivertAddress.Impostor = true;
        WinDivert.WinDivertHelperCalcChecksums(winDivertBuffer, packetLength, ref winDivertAddress, WinDivertChecksumHelperParam.All);
    }
}

#endif
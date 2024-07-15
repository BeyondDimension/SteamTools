using STUN.StunResult;
using System.Net.NetworkInformation;
using static BD.WTTS.Services.Implementation.NetworkTestService;

namespace BD.WTTS.Services;

internal interface INetworkTestService : IHttpRequestTestService, IStunTestService
{
    Task<PingReply> TestPingAsync(string testHostNameOrAddress, TimeSpan timeout, CancellationToken cancellationToken = default);

    Task<(long delayMs, IPAddress[] address)> TestDNSAsync(
            string testDomain,
            string dnsServerIp,
            int dnsServerPort,
            DnsQueryAnswerRecord.DnsRecordType dnsRecordType = DnsQueryAnswerRecord.DnsRecordType.A,
            CancellationToken cancellationToken = default
        );
}

internal interface IHttpRequestTestService
{
    Task<(bool success, double? rate)> TestUploadSpeedAsync(
            string uploadServerUrl,
            byte[] uploadBytes,
            CancellationToken cancellationToken = default
        );

    Task<(bool success, double? rate)> TestDownloadSpeedAsync(string downloadUrl, CancellationToken cancellationToken = default);
}

internal interface IStunTestService
{
    /// <summary>
    /// 测试 STUN Client RFC3489
    /// </summary>
    /// <param name="testServerHostName">STUN 服务地址</param>
    /// <param name="testServerPort">STUN 服务端口</param>
    /// <param name="localIPEndPoint">本机IP</param>
    /// <param name="force">刷新测试 Client</param>
    /// <remarks></remarks>
    /// <returns></returns>
    Task<ClassicStunResult?> TestStunClient3489Async(
            string? testServerHostName = default,
            int? testServerPort = default,
            IPEndPoint? localIPEndPoint = default,
            bool force = false,
            CancellationToken cancellationToken = default
        );

    /// <summary>
    /// 测试 STUN Client RFC5389
    /// </summary>
    /// <param name="protocol">使用的传输协议 Tcp,Udp</param>
    /// <param name="testServerHostName">STUN 服务地址</param>
    /// <param name="testServerPort">STUN 服务端口</param>
    /// <param name="localIPEndPoint">本机IP</param>
    /// <param name="force"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<StunResult5389?> TestStunClient5389Async(
            TransportProtocol protocol,
            string? testServerHostName = default,
            int? testServerPort = default,
            IPEndPoint? localIPEndPoint = null,
            CancellationToken cancellationToken = default
        );
}
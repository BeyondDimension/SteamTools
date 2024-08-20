using STUN.StunResult;
using System.Net.NetworkInformation;
using static BD.WTTS.Services.Implementation.NetworkTestService;

namespace BD.WTTS.Services;

internal interface INetworkTestService : IHttpRequestTestService, IStunTestService
{
    static INetworkTestService Instance => Ioc.Get<INetworkTestService>();

    /// <summary>
    /// Ping
    /// </summary>
    /// <param name="testHostNameOrAddress"></param>
    /// <param name="timeout"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<PingReply> TestPingAsync(string testHostNameOrAddress, TimeSpan timeout, CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试 DNS UDP
    /// </summary>
    /// <param name="testDomain"></param>
    /// <param name="dnsServerIp"></param>
    /// <param name="dnsServerPort"></param>
    /// <param name="dnsRecordType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(long DelayMs, IPAddress[] Address)> TestDNSAsync(
            string testDomain,
            string dnsServerIp,
            int dnsServerPort,
            DnsQueryAnswerRecord.DnsRecordType dnsRecordType = DnsQueryAnswerRecord.DnsRecordType.A,
            CancellationToken cancellationToken = default
        );

    /// <summary>
    /// 测试 DNS Over Https
    /// </summary>
    /// <param name="testDomain"></param>
    /// <param name="dohServer"></param>
    /// <param name="dnsRecordType"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(long DelayMs, IPAddress[] Address)> TestDNSOverHttpsAsync(
            string testDomain,
            string dohServer,
            DnsQueryAnswerRecord.DnsRecordType dnsRecordType = DnsQueryAnswerRecord.DnsRecordType.A,
            CancellationToken cancellationToken = default
        );
}

internal interface IHttpRequestTestService
{
    /// <summary>
    /// 测试打开网址
    /// </summary>
    /// <param name="url"></param>
    /// <param name="httpClientFunc"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool Success, long? DelayMs)> TestOpenUrlAsync(string url, Func<HttpClient>? httpClientFunc = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// 测试上传速度
    /// </summary>
    /// <param name="uploadServerUrl"></param>
    /// <param name="uploadBytes"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool Success, double? Rate)> TestUploadSpeedAsync(
            string uploadServerUrl,
            byte[] uploadBytes,
            CancellationToken cancellationToken = default
        );

    /// <summary>
    /// 测试下载速度
    /// </summary>
    /// <param name="downloadUrl"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<(bool Success, double? Rate)> TestDownloadSpeedAsync(string downloadUrl, CancellationToken cancellationToken = default);
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
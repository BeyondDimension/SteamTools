using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// DNS 解析服务
    /// </summary>
    public interface IDnsDohAnalysisService
    {
        protected const string TAG = "DnsDohAnalysisService";

        static IDnsDohAnalysisService Instance => DI.Get<IDnsDohAnalysisService>();

        #region DNS常量
        const string DNS_Ali_DohAddres = "https://dns.alidns.com/dns-query";

        const string Dnspod_DohAddres = "https://1.12.12.12/dns-query";

        const string Google_DohAddres = "https://dns.google/dns-query";

        const string Cloudflare_DohAddres = "https://cloudflare-dns.com/dns-query";
        #endregion

        protected const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";

        /// <summary>
        /// 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="isIPv6"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, bool isIPv6, CancellationToken cancellationToken = default)
            => AnalysisDomainIpAsync(hostNameOrAddress, default, isIPv6, cancellationToken);

        /// <summary>
        /// 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, CancellationToken cancellationToken = default)
            => AnalysisDomainIpAsync(hostNameOrAddress, default, default, cancellationToken);

        /// <summary>
        /// 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, CancellationToken cancellationToken = default)
            => AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, cancellationToken);

        /// <summary>
        /// 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
        /// <param name="isIPv6"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, bool isIPv6, CancellationToken cancellationToken = default);

        Task<bool> GetIsIpv6Support() => Task.FromResult(false);
    }
}

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
    public interface IDnsAnalysisService
    {
        protected const string TAG = "DnsAnalysisService";

        static IDnsAnalysisService Instance => DI.Get<IDnsAnalysisService>();

        #region DNS常量
        const string DNS_Ali_DohAddres = "https://dns.alidns.com";
        const string Dnspod_DohAddres = "https://1.12.12.12";
        const string Google_DohAddres = "https://dns.google";
        const string Cloudflare_DohAddres = "https://cloudflare-dns.com";

        protected const string PrimaryDNS_IPV6_Ali = "2400:3200::1";

        const string PrimaryDNS_Ali = "223.5.5.5";
        const string SecondaryDNS_Ali = "223.6.6.6";

        const string PrimaryDNS_Dnspod = "119.29.29.29";
        const string SecondaryDNS_Dnspod = "182.254.116.116";

        const string PrimaryDNS_114 = "114.114.114.114";
        const string SecondaryDNS_114 = "114.114.115.115";

        const string PrimaryDNS_Google = "8.8.8.8";
        const string SecondaryDNS_Google = "8.8.4.4";

        const string PrimaryDNS_Cloudflare = "1.1.1.1";
        const string SecondaryDNS_Cloudflare = "1.0.0.1";

        const string PrimaryDNS_Baidu = "180.76.76.76";

        static readonly IPAddress[] DNS_Alis = new[] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) };
        static readonly IPAddress[] DNS_Dnspods = new[] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) };
        static readonly IPAddress[] DNS_114s = new[] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) };
        static readonly IPAddress[] DNS_Googles = new[] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address };
        static readonly IPAddress[] DNS_Cloudflares = new[] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address };

        private static class NameServer
        {
            public static readonly IPEndPoint GooglePublicDns = new(IPAddress.Parse(SecondaryDNS_Google), 53);

            public static readonly IPEndPoint GooglePublicDns2 = new(IPAddress.Parse(PrimaryDNS_Google), 53);

            public static readonly IPEndPoint Cloudflare = new(IPAddress.Parse("1.1.1.1"), 53);

            public static readonly IPEndPoint Cloudflare2 = new(IPAddress.Parse("1.0.0.1"), 53);
        }
        #endregion

        protected const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";
        protected const string IPV6_TESTDOMAIN_SUCCESS = PrimaryDNS_IPV6_Ali;

        async Task<long> PingHostname(string url)
        {
            var pin = new Ping();
            var r = await pin.SendPingAsync(url, 30);
            if (r.Status != IPStatus.Success)
            {
                return 0;
            }
            return r.RoundtripTime;
        }

        Task<int> AnalysisHostnameTime(string url, CancellationToken cancellationToken = default);

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
        IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, CancellationToken cancellationToken = default)
            => AnalysisDomainIpAsync(hostNameOrAddress, dnsServers, default, cancellationToken);

        /// <summary>
        /// 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
        /// <param name="isIPv6"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        async IAsyncEnumerable<IPAddress> AnalysisDomainIpAsync(string hostNameOrAddress, IPAddress[]? dnsServers, bool isIPv6, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var items = await
#if NET6_0_OR_GREATER
                Dns.GetHostAddressesAsync(hostNameOrAddress, cancellationToken);
#else
                Dns.GetHostAddressesAsync(hostNameOrAddress);
#endif
            foreach (var item in items)
            {
                yield return item;
            }
        }

        /// <summary>
        /// DOH 解析域名 IP 地址
        /// </summary>
        /// <param name="hostNameOrAddress">要解析的主机名或 IP 地址</param>
        /// <param name="dnsServers">自定义 DNS 服务器，可选的值有 <see cref="DNS_Alis"/>, <see cref="DNS_114s"/>, <see cref="DNS_Cloudflares"/>, <see cref="DNS_Dnspods"/>, <see cref="DNS_Googles"/></param>
        /// <param name="isIPv6"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        IAsyncEnumerable<IPAddress>? DohAnalysisDomainIpAsync(string hostNameOrAddress, string? dnsServers, bool isIPv6, CancellationToken cancellationToken = default)
        {
            return null;
        }

        async Task<string?> GetHostByIPAddress(IPAddress ip)
        {
            var hostEntry = await Dns.GetHostEntryAsync(ip);
            return hostEntry.HostName;
        }

        Task<bool> GetIsIpv6Support() => Task.FromResult(false);

        Task<IPAddress?> GetHostIpv6Addres() => Task.FromResult(default(IPAddress));
    }
}

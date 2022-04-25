using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// DNS 解析服务
    /// </summary>
    public interface IDnsAnalysisService
    {
        static IDnsAnalysisService Instance => DI.Get<IDnsAnalysisService>();

        #region DNS常量
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

        protected static readonly IPAddress[] DNS_Alis = new[] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) };
        protected static readonly IPAddress[] DNS_Dnspods = new[] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) };
        protected static readonly IPAddress[] DNS_114s = new[] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) };
        protected static readonly IPAddress[] DNS_Googles = new[] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address };
        protected static readonly IPAddress[] DNS_Cloudflares = new[] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address };

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

        int AnalysisHostnameTime(string url);

        Task<IPAddress[]?> AnalysisDomainIp(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, null, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpByGoogleDns(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, DNS_Googles, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpByCloudflare(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, DNS_Cloudflares, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpByDnspod(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, DNS_Dnspods, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpByAliDns(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, DNS_Alis, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpBy114Dns(string url, bool isIPv6 = false)
        {
            return AnalysisDomainIpByCustomDns(url, DNS_114s, isIPv6);
        }

        Task<IPAddress[]?> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
        {
            return Dns.GetHostAddressesAsync(url);
        }

        async Task<string?> GetHostByIPAddress(IPAddress ip)
        {
            var hostEntry = await Dns.GetHostEntryAsync(ip);
            return hostEntry.HostName;
        }

        Task<bool> GetIsIpv6Support() => Task.FromResult(false);
    }
}

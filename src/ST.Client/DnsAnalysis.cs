using DnsClient;
using DnsClient.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace System.Application
{
    /// <summary>
    /// DNS 解析
    /// </summary>
    public static class DnsAnalysis
    {
        #region DNS常量
        private const string PrimaryDNS_IPV6_Ali = "2400:3200::1";

        public const string PrimaryDNS_Ali = "223.5.5.5";
        public const string SecondaryDNS_Ali = "223.6.6.6";

        public const string PrimaryDNS_Dnspod = "119.29.29.29";
        public const string SecondaryDNS_Dnspod = "182.254.116.116";

        public const string PrimaryDNS_114 = "114.114.114.114";
        public const string SecondaryDNS_114 = "114.114.115.115";

        public const string PrimaryDNS_Google = "8.8.8.8";
        public const string SecondaryDNS_Google = "8.8.4.4";

        public const string PrimaryDNS_Cloudflare = "1.1.1.1";
        public const string SecondaryDNS_Cloudflare = "1.0.0.1";

        public const string PrimaryDNS_Baidu = "180.76.76.76";
        #endregion



        private const string IPV6_TESTDOMAIN = "ipv6.rmbgame.net";
        private const string IPV6_TESTDOMAIN_SUCCESS = PrimaryDNS_IPV6_Ali;

        private static readonly LookupClient lookupClient = new();

        public static async Task<long> PingHostname(string url)
        {
            var pin = new Ping();
            var r = await pin.SendPingAsync(url, 30);
            if (r.Status != IPStatus.Success)
            {
                return 0;
            }
            return r.RoundtripTime;
        }

        public static int AnalysisHostnameTime(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    var value = result.Answers.ARecords().FirstOrDefault()?.InitialTimeToLive;
                    if (value.HasValue) return value.Value;
                }
            }
            return 0;
        }

        public static IEnumerable<ARecord>? GetPingHostNameData(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    return result.Answers.ARecords();
                }
            }
            return null;
        }

        public static async Task<IPAddress[]?> AnalysisDomainIp(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, null, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByGoogleDns(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, new[] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address }, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByCloudflare(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, new[] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address }, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByDnspod(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, new[] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) }, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByAliDns(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, new[] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) }, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpBy114Dns(string url, bool isIPv6 = false)
        {
            return await AnalysisDomainIpByCustomDns(url, new[] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) }, isIPv6);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                DnsQuestion question;
                DnsQueryAndServerOptions? options = null;
                IDnsQueryResponse response;

                if (dnsServers != null)
                    options = new DnsQueryAndServerOptions(dnsServers);

                if (isIPv6)
                {
                    question = new DnsQuestion(url, QueryType.AAAA);

                    if (options != null)
                        response = await client.QueryAsync(question, options);
                    else
                        response = await client.QueryAsync(question);

                    if (!response.HasError && response.Answers.AaaaRecords().Any())
                    {
                        var aaaaRecord = response.Answers.AaaaRecords().Select(s => s.Address).ToArray();
                        if (aaaaRecord.Any_Nullable()) return aaaaRecord;
                    }
                }

                question = new DnsQuestion(url, QueryType.A);

                if (options != null)
                    response = await client.QueryAsync(question, options);
                else
                    response = await client.QueryAsync(question);

                if (!response.HasError && response.Answers.ARecords().Any())
                {
                    var aRecord = response.Answers.ARecords().Select(s => s.Address).ToArray();
                    if (aRecord.Any_Nullable()) return aRecord;
                }
            }
            return null;
        }

        public static async Task<string?> GetHostByIPAddress(IPAddress ip)
        {
            //var hostName = Dns.GetHostEntry(IPAddress2.Parse(ip)).HostName;
            var client = lookupClient;
            var result = await client.QueryReverseAsync(ip);
            var hostName = result.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName.Value;
            return hostName;
        }

        public static async Task<bool> GetIsIpv6Support()
        {
            try
            {
                var response = await lookupClient.QueryServerAsync(new[] { IPAddress.Parse(PrimaryDNS_IPV6_Ali) }, IPV6_TESTDOMAIN, QueryType.AAAA, QueryClass.IN);
                if (response.Answers.AaaaRecords().Any(s => s.Address.Equals(IPAddress.Parse(IPV6_TESTDOMAIN_SUCCESS))))
                {
                    return true;
                }
            }
            catch
            {
            }
            return false;
        }
    }
}
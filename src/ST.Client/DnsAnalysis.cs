using DnsClient;
using DnsClient.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace System.Application
{
    /// <summary>
    /// DNS 解析
    /// </summary>
    public static class DnsAnalysis
    {
        #region DNS常量
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

        private static readonly LookupClient lookupClient = new();

        public static int PingHostname(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    var value = result.Answers.ARecords().FirstOrDefault()?.TimeToLive;
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

        public static async Task<IPAddress[]?> AnalysisDomainIp(string url)
        {
            return await AnalysisDomainIpByCustomDns(url);
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                var question = new DnsQuestion(url, QueryType.AAAA);
                DnsQueryAndServerOptions? options = null;
                if (dnsServers != null)
                    options = new DnsQueryAndServerOptions(dnsServers);
                IDnsQueryResponse response;

                if (options != null)
                    response = await client.QueryAsync(question, options);
                else
                    response = await client.QueryAsync(question);

                if (response.Answers.AaaaRecords().Any())
                {
                    var aaaaRecord = response.Answers.AaaaRecords().Select(s => s.Address).ToArray();
                    if (aaaaRecord.Any_Nullable()) return aaaaRecord;
                }

                question = new DnsQuestion(url, QueryType.A);

                if (options != null)
                    response = await client.QueryAsync(question, options);
                else
                    response = await client.QueryAsync(question);

                if (response.Answers.ARecords().Any())
                {
                    var aRecord = response.Answers.ARecords().Select(s => s.Address).ToArray();
                    if (aRecord.Any_Nullable()) return aRecord;
                }
            }
            return null;
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByGoogleDns(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address });
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByCloudflare(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address });
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByDnspod(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { IPAddress.Parse(PrimaryDNS_Dnspod), IPAddress.Parse(SecondaryDNS_Dnspod) });
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpByAliDns(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { IPAddress.Parse(PrimaryDNS_Ali), IPAddress.Parse(SecondaryDNS_Ali) });
        }

        public static async Task<IPAddress[]?> AnalysisDomainIpBy114Dns(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { IPAddress.Parse(PrimaryDNS_114), IPAddress.Parse(SecondaryDNS_114) });
        }

        public static async Task<string?> GetHostByIPAddress(IPAddress ip)
        {
            //var hostName = Dns.GetHostEntry(IPAddress2.Parse(ip)).HostName;
            var client = lookupClient;
            var result = await client.QueryReverseAsync(ip);
            var hostName = result.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName.Value;
            return hostName;
        }
    }
}
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

        public static IEnumerable<ARecord> GetPingHostNameData(string url)
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
            return new List<ARecord>();
        }

        public static async Task<IPAddress> AnalysisDomainIp(string url)
        {
            return await AnalysisDomainIpByCustomDns(url);
        }

        public static async Task<IPAddress> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = lookupClient;
                var question = new DnsQuestion(url, QueryType.A);
                //var questionAAAA = new DnsQuestion(url, QueryType.AAAA);

                IDnsQueryResponse response;
                if (dnsServers != null)
                    response = await client.QueryAsync(question, new DnsQueryAndServerOptions(dnsServers));
                else
                    response = await client.QueryAsync(question);

                if (response.Answers.Count > 0)
                {
                    var aRecord = response.Answers.ARecords().FirstOrDefault();
                    if (aRecord != null) return aRecord.Address;
                }
            }
            return IPAddress.Any;
        }

        public static async Task<IPAddress> AnalysisDomainIpByGoogleDns(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { NameServer.GooglePublicDns.Address, NameServer.GooglePublicDns2.Address });
        }

        public static async Task<IPAddress> AnalysisDomainIpByCloudflare(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { NameServer.Cloudflare.Address, NameServer.Cloudflare2.Address });
        }

        public static async Task<IPAddress> AnalysisDomainIpByDnspod(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { IPAddress.Parse("119.29.29.29"), IPAddress.Parse("182.254.116.116") });
        }

        public static async Task<IPAddress> AnalysisDomainIpByAliDns(string url)
        {
            return await AnalysisDomainIpByCustomDns(url, new IPAddress[2] { IPAddress.Parse("223.5.5.5"), IPAddress.Parse("223.6.6.6") });
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
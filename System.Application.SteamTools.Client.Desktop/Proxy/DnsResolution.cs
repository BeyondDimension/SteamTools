using DnsClient;
using DnsClient.Protocol;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace System.Application.Proxy
{
    /// <summary>
    /// DNS 解析
    /// </summary>
    public static class DnsResolution
    {
        public static IPAddress ResolutionDomainIp(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = new LookupClient();
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    var address = result.Answers.ARecords().FirstOrDefault()?.Address;
                    if (address != null) return address;
                }
            }
            return IPAddress.Any;
        }

        public static int PingDomain(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = new LookupClient();
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    var value = result.Answers.ARecords().FirstOrDefault()?.TimeToLive;
                    if (value.HasValue) return value.Value;
                }
            }
            return 0;
        }

        public static IEnumerable<ARecord> GetPingDomainData(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = new LookupClient();
                var result = client.Query(url, QueryType.A);
                if (result.Answers.Count > 0)
                {
                    return result.Answers.ARecords();
                }
            }
            return new List<ARecord>();
        }

        public static IPAddress ResolutionDomainIpByGoogleDns(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = new LookupClient();
                var dns = new IPAddress[2] { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.8.4") };
                var result = client.Query(new DnsQuestion(url, QueryType.A), new DnsQueryAndServerOptions(dns));
                if (result.Answers.Count > 0)
                {
                    var address = result.Answers.ARecords().FirstOrDefault()?.Address;
                    if (address != null) return address;
                }
            }
            return IPAddress.Any;
        }

        public static string GetHostByIPAddress(string ip)
        {
            if (!string.IsNullOrEmpty(ip))
            {
                var result = Dns.GetHostEntry(IPAddress.Parse(ip)).HostName;
                return result;
            }
            return "";
        }
    }
}
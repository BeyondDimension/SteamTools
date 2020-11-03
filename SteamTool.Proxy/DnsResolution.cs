using DnsClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace SteamTool.Proxy
{
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
                    return result.Answers.ARecords().FirstOrDefault().Address;
                }
            }
            return IPAddress.Any;
        }

        public static IPAddress ResolutionDomainIpByGoogleDns(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                var client = new LookupClient();
                var dns = new IPAddress[2] { IPAddress.Parse("8.8.8.8"), IPAddress.Parse("8.8.8.4") };
                var result = client.Query(new DnsQuestion(url,QueryType.A), new DnsQueryAndServerOptions(dns));
                if (result.Answers.Count > 0)
                {
                    return result.Answers.ARecords().FirstOrDefault().Address;
                }
            }
            return IPAddress.Any;
        }
    }
}

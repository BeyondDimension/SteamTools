using DnsClient;
using DnsClient.Protocol;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using static System.Application.Services.IDnsAnalysisService;

namespace System.Application.Services.Implementation
{
    internal sealed class DnsAnalysisServiceImpl : IDnsAnalysisService
    {
        private static readonly LookupClient lookupClient = new();

        public int AnalysisHostnameTime(string url)
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

        public async Task<IPAddress[]?> AnalysisDomainIpByCustomDns(string url, IPAddress[]? dnsServers = null, bool isIPv6 = false)
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

        public async Task<string?> GetHostByIPAddress(IPAddress ip)
        {
            //var hostName = Dns.GetHostEntry(IPAddress2.Parse(ip)).HostName;
            var client = lookupClient;
            var result = await client.QueryReverseAsync(ip);
            var hostName = result.Answers.PtrRecords().FirstOrDefault()?.PtrDomainName.Value;
            return hostName;
        }

        public async Task<bool> GetIsIpv6Support()
        {
            try
            {
                var options = new LookupClientOptions
                {
                    Retries = 1,
                    Timeout = TimeSpan.FromSeconds(1),
                    UseCache = false
                };
                var client = new LookupClient(options);
                var response = await client.QueryServerAsync(new[] { IPAddress.Parse(PrimaryDNS_IPV6_Ali) }, IPV6_TESTDOMAIN, QueryType.AAAA, QueryClass.IN);
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

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDnsAnalysisService(this IServiceCollection services)
        {
            services.AddSingleton<IDnsAnalysisService, DnsAnalysisServiceImpl>();
            return services;
        }
    }
}
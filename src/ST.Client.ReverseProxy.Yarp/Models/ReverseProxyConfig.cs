// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/FastGithubConfig.cs

using System.Net;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Application.Services.Implementation;

namespace System.Application.Models;

sealed class ReverseProxyConfig : IReverseProxyConfig
{
    readonly SortedDictionary<DomainPattern, IDomainConfig> domainConfigs;
    readonly ConcurrentDictionary<string, IDomainConfig?> domainConfigCache;
    readonly YarpReverseProxyServiceImpl reverseProxyService;

    public ReverseProxyConfig(YarpReverseProxyServiceImpl reverseProxyService)
    {
        this.reverseProxyService = reverseProxyService;
        domainConfigs = new();
        AddDomainConfigs(domainConfigs, reverseProxyService.ProxyDomains);
        domainConfigCache = new();
    }

    YarpReverseProxyServiceImpl IReverseProxyConfig.Service => reverseProxyService;

    public int HttpProxyPort
    {
        get => reverseProxyService.ProxyPort;
        set => reverseProxyService.ProxyPort = value;
    }

    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains
    {
        get => reverseProxyService.ProxyDomains;
        set => reverseProxyService.ProxyDomains = value;
    }

    static void AddDomainConfigs(IDictionary<DomainPattern, IDomainConfig> dict, IEnumerable<AccelerateProjectDTO>? domainConfigs)
    {
        if (domainConfigs != null)
        {
            foreach (var item in domainConfigs)
            {
                foreach (var domainName in item.DomainNamesArray)
                {
                    dict.Add(new DomainPattern(domainName), item);
                }
            }
        }
    }

    //static void AddDomainConfigs(IDictionary<DomainPattern, IDomainConfig> dict, IReadOnlyDictionary<string, DomainConfig>? domainConfigs)
    //{
    //    if (domainConfigs != null)
    //    {
    //        foreach (var kv in domainConfigs)
    //        {
    //            dict.Add(new DomainPattern(kv.Key), kv.Value);
    //        }
    //    }
    //}

    ///// <summary>
    ///// 配置转换
    ///// </summary>
    ///// <param name="domainConfigs"></param>
    ///// <returns></returns>
    //[Obsolete("use AddDomainConfigs")]
    //static SortedDictionary<DomainPattern, IDomainConfig> ConvertDomainConfigs(IEnumerable<AccelerateProjectDTO>? domainConfigs)
    //{
    //    var result = new SortedDictionary<DomainPattern, IDomainConfig>();
    //    if (domainConfigs != null)
    //    {
    //        foreach (var item in domainConfigs)
    //        {
    //            foreach (var domainName in item.DomainNamesArray)
    //            {
    //                result.Add(new DomainPattern(domainName), item);
    //            }
    //        }
    //    }
    //    return result;
    //}

    public bool TryGetDomainConfig(string domain, [MaybeNullWhen(false)] out IDomainConfig value)
    {
        value = domainConfigCache.GetOrAdd(domain, GetDomainConfig);
        return value != null;

        IDomainConfig? GetDomainConfig(string domain)
        {
            var key = domainConfigs.Keys.FirstOrDefault(item => item.IsMatch(domain));
            return key == null ? null : domainConfigs[key];
        }
    }

    public DomainPattern[] GetDomainPatterns() => domainConfigs.Keys.ToArray();
}

// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/FastGithubConfig.cs

using System.Net;
using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Application.Services.Implementation;

namespace System.Application.Models;

sealed class ReverseProxyConfig : IReverseProxyConfig
{
    readonly SortedDictionary<DomainPattern, AccelerateProjectDTO> domainConfigs;
    readonly ConcurrentDictionary<string, AccelerateProjectDTO?> domainConfigCache;
    readonly YarpReverseProxyServiceImpl reverseProxyService;

    public ReverseProxyConfig(YarpReverseProxyServiceImpl reverseProxyService)
    {
        this.reverseProxyService = reverseProxyService;
        domainConfigs = ConvertDomainConfigs(reverseProxyService.ProxyDomains);
        domainConfigCache = new ConcurrentDictionary<string, AccelerateProjectDTO?>();
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

    /// <summary>
    /// 配置转换
    /// </summary>
    /// <param name="domainConfigs"></param>
    /// <returns></returns>
    static SortedDictionary<DomainPattern, AccelerateProjectDTO> ConvertDomainConfigs(IEnumerable<AccelerateProjectDTO>? domainConfigs)
    {
        var result = new SortedDictionary<DomainPattern, AccelerateProjectDTO>();
        if (domainConfigs != null)
        {
            foreach (var item in domainConfigs)
            {
                foreach (var domainName in item.DomainNamesArray)
                {
                    result.Add(new DomainPattern(domainName), item);
                }
            }
        }
        return result;
    }

    public bool TryGetDomainConfig(string domain, [MaybeNullWhen(false)] out AccelerateProjectDTO value)
    {
        value = domainConfigCache.GetOrAdd(domain, GetDomainConfig);
        return value != null;

        AccelerateProjectDTO? GetDomainConfig(string domain)
        {
            var key = domainConfigs.Keys.FirstOrDefault(item => item.IsMatch(domain));
            return key == null ? null : domainConfigs[key];
        }
    }

    public DomainPattern[] GetDomainPatterns() => domainConfigs.Keys.ToArray();
}

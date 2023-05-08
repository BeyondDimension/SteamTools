#if !DISABLE_ASPNET_CORE && (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/FastGithubConfig.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

sealed class ReverseProxyConfig : IReverseProxyConfig
{
    readonly SortedDictionary<DomainPattern, IDomainConfig> domainConfigs;
    readonly ICollection<KeyValuePair<DomainPattern, IScriptConfig>> scriptConfigs;
    readonly ConcurrentDictionary<string, IDomainConfig?> domainConfigCache;
    readonly YarpReverseProxyServiceImpl reverseProxyService;

    public ReverseProxyConfig(YarpReverseProxyServiceImpl reverseProxyService)
    {
        this.reverseProxyService = reverseProxyService;
        domainConfigs = new();
        scriptConfigs = new List<KeyValuePair<DomainPattern, IScriptConfig>>();
        AddDomainConfigs(domainConfigs, reverseProxyService.ProxyDomains);
        AddScriptConfigs(scriptConfigs, reverseProxyService.Scripts);
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

    public IReadOnlyCollection<ScriptDTO>? ProxyScripts
    {
        get => reverseProxyService.Scripts;
        set => reverseProxyService.Scripts = value;
    }

    static void AddDomainConfigs(IDictionary<DomainPattern, IDomainConfig> dict,
        IEnumerable<AccelerateProjectDTO>? accelerates)
    {
        if (accelerates != null)
        {
            foreach (var item in accelerates)
            {
                dict.Add(new DomainPattern(item.MatchDomainNames) { Order = item.Order }, item);
            }
        }
    }

    static void AddScriptConfigs(ICollection<KeyValuePair<DomainPattern, IScriptConfig>> dict,
    IEnumerable<ScriptDTO>? scripts)
    {
        if (IReverseProxyService.Constants.Instance.IsEnableScript && scripts != null)
        {
            foreach (var item in scripts)
            {
                var domainNames = string.Join(DomainPattern.GeneralSeparator, item.MatchDomainNamesArray.Select(GetDomainPatternString));
                //if (item.ExcludeDomainNamesArray.Any_Nullable())
                //{
                //    var domainNames2 = string.Join(DomainPattern.GeneralSeparator, item.ExcludeDomainNamesArray.Select(GetDomainPatternString));
                //}

                dict.Add(new KeyValuePair<DomainPattern, IScriptConfig>(new DomainPattern(domainNames) { Order = item.Order }, item));
            }

            static string GetDomainPatternString(string s)
            {
                if (s.IndexOf("/") != 0)
                {
                    return "/^" + Regex.Escape(s).Replace("\\*", ".*").Replace("\\?", ".") + "$";
                }
                return s;
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
        //value = domainConfigCache.GetOrAdd(domain.Host, GetDomainConfig);

        var uri = new UriBuilder(domain).Uri;

        domainConfigCache.TryGetValue(uri.Host, out value);
        if (value != null)
            return true;

        value = GetDomainConfig(domain);
        if (value == null)
            return false;

        domainConfigCache.TryAdd(uri.Host, value);
        return true;

        IDomainConfig? GetDomainConfig(string domain)
        {
            var key = domainConfigs.Keys.FirstOrDefault(item => item.IsMatch(domain));
            return key == null ? null : domainConfigs[key];
        }
    }

    public bool TryGetScriptConfig(string domain, [MaybeNullWhen(false)] out IEnumerable<IScriptConfig> value)
    {
        if (!IReverseProxyService.Constants.Instance.IsEnableScript)
        {
            value = null;
            return false;
        }

        value = scriptConfigs.Where(item => item.Key.IsMatch(domain))
                    .Where(item => item.Value.ExcludeDomainPattern?.IsMatch(domain) != true)
                    .Select(item => item.Value);

        return value.Any_Nullable();
    }

    public DomainPattern[] GetDomainPatterns() => domainConfigs.Keys.ToArray();
}
#endif
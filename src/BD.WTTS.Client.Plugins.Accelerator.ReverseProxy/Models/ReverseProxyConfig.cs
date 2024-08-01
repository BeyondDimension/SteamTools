// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/FastGithubConfig.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

sealed class ReverseProxyConfig : IReverseProxyConfig
{
    readonly SortedDictionary<DomainPattern, IDomainConfig> domainConfigs;
    readonly ICollection<KeyValuePair<DomainPattern, IScriptConfig>> scriptConfigs;
    readonly ConcurrentDictionary<string, IDomainConfig?> domainConfigCache;
    readonly YarpReverseProxyServiceImpl reverseProxyService;
    readonly ConcurrentDictionary<int, string> scriptsContent;

    public ReverseProxyConfig(YarpReverseProxyServiceImpl reverseProxyService)
    {
        this.reverseProxyService = reverseProxyService;
        domainConfigs = new();
        scriptsContent = new();
        scriptConfigs = new List<KeyValuePair<DomainPattern, IScriptConfig>>();
        AddDomainConfigs(domainConfigs, reverseProxyService.ProxyDomains);
        AddScriptConfigs(scriptConfigs, reverseProxyService.Scripts);
        LoadScriptContent(scriptsContent, reverseProxyService.Scripts);
        domainConfigCache = new();
    }

    YarpReverseProxyServiceImpl IReverseProxyConfig.Service => reverseProxyService;

    public ushort HttpProxyPort
    {
        get => reverseProxyService.ProxyPort;
        set => reverseProxyService.ProxyPort = value;
    }

    public IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains
    {
        get => reverseProxyService.ProxyDomains;
        set => reverseProxyService.ProxyDomains = value;
    }

    public IReadOnlyCollection<ScriptIPCDTO>? ProxyScripts
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
                var matchDomainNames = item.MatchDomainNames;
                if (string.IsNullOrWhiteSpace(matchDomainNames))
                    throw new ArgumentNullException(nameof(matchDomainNames));
                dict.Add(new DomainPattern(matchDomainNames) { Order = item.Order }, item);
            }
        }
    }

    void LoadScriptContent(ConcurrentDictionary<int, string> dict, IEnumerable<ScriptIPCDTO>? scripts)
    {
        if (IReverseProxyService.Constants.Instance.IsEnableScript && scripts != null)
        {
            foreach (var item in scripts)
            {
                if (!scriptsContent.ContainsKey(item.LocalId))
                {
                    if (LoadScriptFille(item.CachePath, out string? content))
                    {
                        if (!string.IsNullOrEmpty(content))
                            scriptsContent.TryAdd(item.LocalId, content);
                    }
                }
            }
        }
    }

    static bool LoadScriptFille(string cachePath, out string? content)
    {
        var cacheFilePath = Path.Combine(IOPath.CacheDirectory, cachePath);
        if (File.Exists(cacheFilePath))
        {
            content = File.ReadAllText(cacheFilePath);
            return true;
        }
        content = null;
        return false;
    }

    static void AddScriptConfigs(ICollection<KeyValuePair<DomainPattern, IScriptConfig>> dict,
    IEnumerable<ScriptIPCDTO>? scripts)
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

    public bool TryGetDomainConfig(string url, [MaybeNullWhen(false)] out IDomainConfig value)
    {
        //value = domainConfigCache.GetOrAdd(domain.Host, GetDomainConfig);

        var uri = new UriBuilder(url).Uri;

        domainConfigCache.TryGetValue(uri.Host, out value);
        if (value != null)
            return true;

        value = GetDomainConfig(url);
        if (value == null)
            return false;

        domainConfigCache.TryAdd(uri.Host, value);
        return true;

        IDomainConfig? GetDomainConfig(string url)
        {
            var key = domainConfigs.Keys.FirstOrDefault(item => item.IsMatch(url));
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

    public bool TryGetScriptContent(int lid, out string? content)
    {
        if (!IReverseProxyService.Constants.Instance.IsEnableScript)
        {
            content = null;
            return false;
        }
        if (scriptsContent.TryGetValue(lid, out string? content_))
        {
            if (string.IsNullOrEmpty(content_))
            {
                content = null;
                return false;
            }
            content = content_;
            return true;
        }
        content = null;
        return false;
    }

    public DomainPattern[] GetDomainPatterns() => domainConfigs.Keys.ToArray();
}
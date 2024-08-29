// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Configuration/FastGithubConfig.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models.Abstractions;

public partial interface IReverseProxyConfig
{
    internal YarpReverseProxyServiceImpl Service { get; }

    IDnsAnalysisService DnsAnalysis => Service.DnsAnalysis;

    ushort HttpProxyPort { get; set; }

    IReadOnlyCollection<AccelerateProjectDTO>? ProxyDomains { get; set; }

    IReadOnlyCollection<ScriptIPCDTO>? ProxyScripts { get; set; }

    /// <summary>
    /// 是否匹配指定的域名
    /// </summary>
    /// <param name="domain"></param>
    /// <returns></returns>
    bool IsMatch(string url) => TryGetDomainConfig(url, out _);

    /// <summary>
    /// 尝试获取域名配置
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryGetDomainConfig(string url, [MaybeNullWhen(false)] out IDomainConfig value);

    /// <summary>
    /// 尝试获取脚本配置
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    bool TryGetScriptConfig(string url, [MaybeNullWhen(false)] out IEnumerable<IScriptConfig> value);

    /// <summary>
    /// 尝试获取脚本内容
    /// </summary>
    /// <param name="lid"></param>
    /// <param name="content"></param>
    /// <returns></returns>
    bool TryGetScriptContent(int lid, out string? content);

    /// <summary>
    /// 获取所有域名表达式
    /// </summary>
    /// <returns></returns>
    DomainPattern[] GetDomainPatterns();
}
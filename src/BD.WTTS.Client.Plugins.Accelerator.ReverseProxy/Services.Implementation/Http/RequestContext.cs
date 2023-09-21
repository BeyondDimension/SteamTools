// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/RequestContext.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 请求上下文
/// </summary>
sealed class RequestContext
{
    /// <summary>
    /// 获取或设置是否为 HTTPS 请求
    /// </summary>
    public bool IsHttps { get; }

    /// <summary>
    /// 获取或设置 Sni 值
    /// </summary>
    public TlsSniPattern TlsSniValue { get; }

    public RequestContext(bool isHttps, TlsSniPattern tlsSniValue)
    {
        IsHttps = isHttps;
        TlsSniValue = tlsSniValue;
    }
}
// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/RequestContext.cs

using System.Net;

namespace System.Application.Services.Implementation.Http;

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
    /// 获取或设置Sni值
    /// </summary>
    public TlsSniPattern TlsSniValue { get; }

    public RequestContext(bool isHttps, TlsSniPattern tlsSniValue)
    {
        IsHttps = isHttps;
        TlsSniValue = tlsSniValue;
    }
}

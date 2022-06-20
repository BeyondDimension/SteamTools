// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.Http/IHttpClientFactory.cs

using System.Application.Models;

namespace System.Application.Services;

/// <summary>
/// 用于反向代理的 HttpClient 工厂
/// </summary>
public interface IReverseProxyHttpClientFactory
{
    /// <summary>
    /// 创建 HttpClient
    /// </summary>
    /// <param name="domain"></param>
    /// <param name="domainConfig"></param>
    /// <returns></returns>
    HttpClient CreateHttpClient(string domain, AccelerateProjectDTO domainConfig);
}

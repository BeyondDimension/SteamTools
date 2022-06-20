// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/KestrelServerOptionsExtensions.cs

using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Microsoft.AspNetCore.Hosting;

public static class KestrelServerOptionsExtensions
{
    /// <summary>
    /// 无限制
    /// </summary>
    /// <param name="options"></param>
    public static void NoLimit(this KestrelServerOptions options)
    {
        options.Limits.MaxRequestBodySize = null;
        options.Limits.MinResponseDataRate = null;
        options.Limits.MinRequestBodyDataRate = null;
    }

    /// <summary>
    /// 监听 Http 代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpProxy(this KestrelServerOptions options)
    {

    }

    /// <summary>
    /// 监听 SSH 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenSshReverseProxy(this KestrelServerOptions options)
    {

    }

    /// <summary>
    /// 监听 Git 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenGitReverseProxy(this KestrelServerOptions options)
    {

    }

    /// <summary>
    /// 监听 HTTP 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpReverseProxy(this KestrelServerOptions options)
    {

    }

    /// <summary>
    /// 监听 HTTPS 反向代理
    /// </summary>
    /// <param name="options"></param>
    public static void ListenHttpsReverseProxy(this KestrelServerOptions options)
    {

    }
}

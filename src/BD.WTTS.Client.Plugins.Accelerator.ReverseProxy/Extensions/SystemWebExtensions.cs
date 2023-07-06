using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;

// ReSharper disable once CheckNamespace
namespace System;

/// <summary>
/// 提供 ASP.NET 的兼容
/// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.web.httprequest</para>
/// </summary>
public static class SystemWebExtensions
{
    /// <summary>
    /// 获取所提供的客户端浏览器的原始用户代理字符串。 请注意此字符串可能为 <see langword="null"/>
    /// </summary>
    /// <param name="request"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? UserAgent(this HttpRequest request)
    {
        return request.Headers.TryGetValue("User-Agent", out var value) ? value.ToString() : null;
    }

    /// <summary>
    /// 获取输出流的 HTTP 字符集
    /// <para>https://docs.microsoft.com/zh-cn/dotnet/api/system.web.httpresponse.contentencoding?view=netframework-4.8</para>
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Encoding? ContentTextEncoding(this HttpResponse response)
    {
        if (MediaTypeHeaderValue.TryParse(response.ContentType, out var contentType))
        {
            // https://github.com/dotnet/aspnetcore/blob/v6.0.6/src/Http/Headers/src/MediaTypeHeaderValue.cs#L113
            try
            {
                return contentType.Encoding;
            }
            catch
            {

            }
        }
        // https://github.com/dotnet/runtime/blob/v6.0.6/src/libraries/System.Net.Http/src/System/Net/Http/HttpContent.cs#L24
        return null;
    }

    /// <summary>
    /// 获取当前请求的原始 URL
    /// </summary>
    /// <remarks>原始 URL 定义为后面包含域信息的 URL 部分。 在 URL 字符串 http://www.contoso.com/articles/recent.aspx中，原始 URL 为/articles/recent.aspx。 原始 URL 包括查询字符串（如果存在）</remarks>
    /// <param name="request"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string RawUrl(this HttpRequest request)
    {
        // 实现参考：https://github.com/dotnet/aspnetcore/blob/v3.1.5/src/Http/Http.Extensions/src/UriHelper.cs#L196
        var pathBase = request.PathBase.Value ?? string.Empty;
        var path = request.Path.Value ?? string.Empty;
        var queryString = request.QueryString.Value ?? string.Empty;
        var length = pathBase.Length + path.Length + queryString.Length;
        return new StringBuilder(length)
            .Append(pathBase)
            .Append(path)
            .Append(queryString)
            .ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string? Action(this IUrlHelper helper, string action, QueryString values)
    {
        var url = helper.Action(action);
        if (values.HasValue)
        {
            var length = url.Length + values.Value!.Length;
            return new StringBuilder(length)
               .Append(url)
               .Append(values.Value)
               .ToString();
        }
        else
        {
            return url;
        }
    }

    /// <summary>
    /// 获取远程客户端的 IP 主机地址
    /// </summary>
    /// <param name="request"></param>
    /// <param name="ipv4"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string UserHostAddress(this HttpRequest request, bool ipv4 = true)
    {
        var remoteIpAddress = request.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null) throw new ArgumentNullException(nameof(remoteIpAddress));
        try
        {
            return ipv4
                ? (remoteIpAddress.AddressFamily == AddressFamily.InterNetwork ? remoteIpAddress.ToString() : remoteIpAddress.MapToIPv4().ToString())
                : (remoteIpAddress.AddressFamily == AddressFamily.InterNetworkV6 ? remoteIpAddress.ToString() : remoteIpAddress.MapToIPv6().ToString());
        }
        catch
        {
            return remoteIpAddress.ToString();
        }
    }
}
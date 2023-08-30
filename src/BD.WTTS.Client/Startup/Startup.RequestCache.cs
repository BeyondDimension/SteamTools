using Fusillade;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup : IRequestCache
{
    protected virtual IRequestCache RequestCache => Ioc.Get<IRequestCacheRepository>();

    static string UniqueKeyForRequest(HttpRequestMessage request)
    {
        // https://github.com/reactiveui/Fusillade/blob/2.4.67/src/Fusillade/RateLimitedHttpMessageHandler.cs#L54-L89

        string originalRequestUri;
        if (request is ImageHttpClientService.ImageHttpRequestMessage request2)
        {
            // 对于一些重定向的 Url 使用原始 Url 进行唯一性的计算
            originalRequestUri = request2.OriginalRequestUri;
        }
        else
        {
            originalRequestUri = request.RequestUri?.ToString() ?? "";
        }
        using var s = new MemoryStream();
        s.Write(Encoding.UTF8.GetBytes(originalRequestUri));
        s.Write("\r\n"u8);
        s.Write(Encoding.UTF8.GetBytes(request.Method.Method));
        s.Write("\r\n"u8);
        static void Write(Stream s, IEnumerable<object> items)
        {
            foreach (var item in items)
            {
                var str = item.ToString();
                if (!string.IsNullOrEmpty(str))
                    s.Write(Encoding.UTF8.GetBytes(str));
                s.Write("|"u8);
            }
        }
        Write(s, request.Headers.Accept);
        s.Write("\r\n"u8);
        Write(s, request.Headers.AcceptEncoding);
        s.Write("\r\n"u8);
        var referrer = request.Headers.Referrer;
        if (referrer == default)
            s.Write("http://example"u8);
        else
            s.Write(Encoding.UTF8.GetBytes(referrer.ToString()));
        s.Write("\r\n"u8);
        Write(s, request.Headers.UserAgent);
        s.Write("\r\n"u8);
        if (request.Headers.Authorization != null)
        {
            var parameter = request.Headers.Authorization.Parameter;
            if (!string.IsNullOrEmpty(parameter))
                s.Write(Encoding.UTF8.GetBytes(parameter));
            s.Write(Encoding.UTF8.GetBytes(request.Headers.Authorization.Scheme));
            s.Write("\r\n"u8);
        }
        s.Position = 0;
        var bytes = SHA384.HashData(s);
        var str = bytes.ToHexString();
        return str;
    }

    Task IRequestCache.Save(
        HttpRequestMessage request,
        HttpResponseMessage response,
        string key,
        CancellationToken cancellationToken)
            => RequestCache.Save(request, response, UniqueKeyForRequest(request), cancellationToken);

    Task<byte[]> IRequestCache.Fetch(
        HttpRequestMessage request,
        string key,
        CancellationToken cancellationToken)
           => RequestCache.Fetch(request, UniqueKeyForRequest(request), cancellationToken);
}

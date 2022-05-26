namespace System.Net.Http;

partial class GeneralHttpClientFactory
{
    public static HttpWebRequest Create(string requestUriString)
    {
        var request = WebRequest.CreateHttp(requestUriString);
        ConfigHttpWebRequest(request);
        return request;
    }

    public static HttpWebRequest Create(Uri requestUri)
    {
        var request = WebRequest.CreateHttp(requestUri);
        ConfigHttpWebRequest(request);
        return request;
    }

    static void ConfigHttpWebRequest(HttpWebRequest request)
    {
        request.AllowAutoRedirect = true;
        request.MaximumAutomaticRedirections = 1000;
        request.Timeout = DefaultTimeoutMilliseconds;
#if NETSTANDARD
        // .NET Core 3+ 上已由 HttpClient.DefaultProxy 生效
        // https://docs.microsoft.com/zh-cn/dotnet/api/system.net.http.httpclient.defaultproxy?view=net-6.0
        var proxy = DefaultProxy;
        if (proxy != null)
        {
            request.Proxy = proxy;
        }
#endif
    }
}

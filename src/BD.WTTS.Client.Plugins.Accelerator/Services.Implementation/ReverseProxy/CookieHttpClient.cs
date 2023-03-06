#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

sealed class CookieHttpClient
{
    public static readonly CookieContainer CookieContainer = new();

    public const string HttpClientName = "CookieHttpClient";

    readonly IHttpClientFactory httpClientFactory;

    public CookieHttpClient(IHttpClientFactory httpClientFactory)
    {
        this.httpClientFactory = httpClientFactory;
    }

    public HttpClient HttpClient => httpClientFactory.CreateClient(HttpClientName);

    public static void AddHttpClient(IServiceCollection services)
    {
        services.AddSingleton<CookieHttpClient>();
        services.AddHttpClient(HttpClientName, (s, c) =>
        {
            c.Timeout = GeneralHttpClientFactory.DefaultTimeout;
        }).ConfigurePrimaryHttpMessageHandler(() => GeneralHttpClientFactory.CreateSocketsHttpHandler(new()
        {
            UseCookies = true,
            CookieContainer = CookieContainer,
        }));
    }
}
#endif
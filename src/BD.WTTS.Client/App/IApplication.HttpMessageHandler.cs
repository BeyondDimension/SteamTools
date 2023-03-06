using CreateHttpHandlerArgs = System.ValueTuple<
    bool,
    System.Net.DecompressionMethods,
    System.Net.CookieContainer,
    System.Net.IWebProxy?,
    int>;

// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    /// <summary>
    /// 用于请求 CloudService 的 <see cref="HttpMessageHandler"/>，不需要 Cookie
    /// </summary>
    /// <returns></returns>
    static Func<HttpMessageHandler>? ConfigureHandler()
    {
#if NETCOREAPP2_1_OR_GREATER
        //#if WINDOWS
        //        if (GeneralSettings.UseWinHttpHandler.Value)
        //        {
        //            return () => new WinHttpHandler
        //            {
        //                WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy,
        //                CookieUsePolicy = CookieUsePolicy.IgnoreCookies,
        //                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip,
        //            };
        //        }
        //        else
        //#endif
        {
            return () => GeneralHttpClientFactory.CreateSocketsHttpHandler(new()
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.Brotli | DecompressionMethods.GZip,
            });
        }
#elif ANDROID
            return () => PlatformHttpMessageHandlerBuilder.CreateAndroidClientHandler(new()
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip,
            });
#else
            return null;
#endif
    }

    /// <summary>
    /// 用于 <see cref="ArchiSteamFarm"/> 的 <see cref="HttpMessageHandler"/>
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    static HttpMessageHandler? CreateHttpHandler(CreateHttpHandlerArgs args)
    {
        var proxy = args.Item4;
        var useProxy = GeneralHttpClientFactory.UseWebProxy(proxy);
        var setMaxConnectionsPerServer = !(args.Item5 < 1);  // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/SocketsHttpHandler.cs#L157
#if NETCOREAPP2_1_OR_GREATER
        //#if WINDOWS
        //        if (GeneralSettings.UseWinHttpHandler.Value)
        //        {
        //            var handler = new WinHttpHandler
        //            {
        //                AutomaticRedirection = args.Item1,
        //                AutomaticDecompression = args.Item2,
        //                CookieContainer = args.Item3,
        //            };
        //            if (useProxy)
        //            {
        //                handler.Proxy = proxy;
        //                handler.WindowsProxyUsePolicy = WindowsProxyUsePolicy.UseCustomProxy;
        //            }
        //            else
        //            {
        //                handler.WindowsProxyUsePolicy = WindowsProxyUsePolicy.DoNotUseProxy;
        //            }
        //            if (setMaxConnectionsPerServer)
        //            {
        //                handler.MaxConnectionsPerServer = args.Item5;
        //            }
        //            return handler;
        //        }
        //        else
        //#endif
        {
            var handler = GeneralHttpClientFactory.CreateSocketsHttpHandler(new()
            {
                AllowAutoRedirect = args.Item1,
                AutomaticDecompression = args.Item2,
                CookieContainer = args.Item3,
            });
            if (useProxy)
            {
                handler.Proxy = proxy;
                handler.UseProxy = true;
            }
            if (setMaxConnectionsPerServer)
            {
                handler.MaxConnectionsPerServer = args.Item5;
            }
            return handler;
        }
#elif ANDROID
        var handler = PlatformHttpMessageHandlerBuilder.CreateAndroidClientHandler(new()
        {
            AllowAutoRedirect = args.Item1,
            AutomaticDecompression = args.Item2,
            CookieContainer = args.Item3,
        });
        if (useProxy)
        {
            handler.Proxy = proxy;
            handler.UseProxy = true;
        }
        if (setMaxConnectionsPerServer)
        {
            handler.MaxConnectionsPerServer = args.Item5;
        }
        return handler;
#else
        return null!;
#endif
    }
}
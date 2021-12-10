using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace System.Net.Http
{
    partial class GeneralHttpClientFactory
    {
#if NETSTANDARD
        static IWebProxy? mDefaultProxy;
#endif

        /// <summary>
        /// 获取或设置全局 HTTP 代理。
        /// </summary>
        public static IWebProxy? DefaultProxy
        {
            get
            {
                IWebProxy? proxy;
#if NETSTANDARD
                proxy = mDefaultProxy;
#else
                proxy = HttpClient.DefaultProxy;
#endif
                return UseWebProxy(proxy) ? proxy : null;
            }
            set
            {
#if NETSTANDARD
                mDefaultProxy = UseWebProxy(value) ? value : null;
                //RefreshWebProxyInDefaultHttpClientFactory(mDefaultProxy);
#else
                HttpClient.DefaultProxy = value ?? HttpNoProxy.Instance;
#endif
            }
        }

        //#if NETSTANDARD
        //        static void RefreshWebProxyInDefaultHttpClientFactory(IWebProxy? proxy)
        //        {
        //            // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/Microsoft.Extensions.Http/src/DefaultHttpClientFactory.cs
        //            // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientFactoryServiceCollectionExtensions.cs#L37
        //            var typeDefaultHttpClientFactory = Type.GetType("Microsoft.Extensions.Http.DefaultHttpClientFactory, Microsoft.Extensions.Http");
        //            var defaultHttpClientFactory = DI.Get_Nullable(typeDefaultHttpClientFactory);
        //            if (defaultHttpClientFactory != null)
        //            {
        //                RefreshWebProxyInDefaultHttpClientFactory(proxy, defaultHttpClientFactory, typeDefaultHttpClientFactory);
        //            }
        //        }

        //        static void RefreshWebProxyInDefaultHttpClientFactory(IWebProxy? proxy, object defaultHttpClientFactory, Type typeDefaultHttpClientFactory)
        //        {
        //            Lazy
        //            typeDefaultHttpClientFactory.GetField("")
        //        }
        //#endif

        /// <summary>
        /// https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/HttpNoProxy.cs
        /// </summary>
        sealed class HttpNoProxy : IWebProxy
        {
            public static readonly HttpNoProxy Instance = new();
            private HttpNoProxy() { }
            public ICredentials? Credentials { get; set; }
            public Uri? GetProxy(Uri destination) => null;
            public bool IsBypassed(Uri host) => true;
        }

        public static bool UseWebProxy([NotNullWhen(true)] IWebProxy? proxy)
            => proxy != null && proxy.GetType().Name != nameof(HttpNoProxy);

        static void SetProxyToHandler(HttpMessageHandler handler, IWebProxy? proxy, bool useProxy)
        {
            try
            {

#if !NETSTANDARD
                    if (handler is SocketsHttpHandler s)
                    {
                        s.Proxy = proxy;
                        if (useProxy && s.UseProxy != useProxy) s.UseProxy = useProxy; // 仅启用代理时修改开关
                    }
                    else
#endif
                if (handler is HttpClientHandler h)
                {
                    h.Proxy = proxy;
                    if (useProxy && h.UseProxy != useProxy) h.UseProxy = useProxy; // 仅启用代理时修改开关
                }
            }
            catch (InvalidOperationException)
            {
                // 如果 handler 被释放或者已启动会抛出此异常
                // https://github.com/dotnet/runtime/blob/v6.0.0/src/libraries/System.Net.Http/src/System/Net/Http/SocketsHttpHandler/SocketsHttpHandler.cs#L31
            }
        }

        public static void SetProxyToHandler(IWebProxy? proxy, HttpMessageHandler handler)
        {
            var useProxy = UseWebProxy(proxy);
            SetProxyToHandler(handler, proxy, useProxy);
        }

        //public static void SetProxyToHandler(IWebProxy? proxy, IEnumerable<HttpMessageHandler> handlers)
        //{
        //    if (!handlers.Any()) return;
        //    var useProxy = UseWebProxy(proxy);
        //    foreach (var handler in handlers)
        //    {
        //        SetProxyToHandler(handler, proxy, useProxy);
        //    }
        //}
    }
}

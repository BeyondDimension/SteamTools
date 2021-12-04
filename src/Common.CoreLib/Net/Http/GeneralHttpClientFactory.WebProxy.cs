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
#if NETSTANDARD
                return mDefaultProxy;
#else
                return HttpClient.DefaultProxy;
#endif
            }
            set
            {
#if NETSTANDARD
                mDefaultProxy = value;
#else
                HttpClient.DefaultProxy = value ?? HttpNoProxy.Instance;
#endif
            }
        }

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

        public static void SetProxyToHandler(HttpMessageHandler handler, IWebProxy? proxy)
        {
            bool useProxy;
            if (proxy == null)
            {
                useProxy = false;
            }
            else
            {
                if (proxy.GetType().Name == nameof(HttpNoProxy))
                {
                    proxy = null;
                    useProxy = false;
                }
                else
                {
                    useProxy = true;
                }
            }

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
    }
}

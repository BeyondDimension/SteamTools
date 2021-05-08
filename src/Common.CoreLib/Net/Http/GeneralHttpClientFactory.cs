using Microsoft.Extensions.Logging;

namespace System.Net.Http
{
    /// <summary>
    /// 通用 <see cref="HttpClient"/> 工厂
    /// </summary>
    public abstract class GeneralHttpClientFactory
    {
        protected readonly ILogger logger;
        protected readonly IHttpPlatformHelper http_helper;
        protected readonly IHttpClientFactory _clientFactory;

        public GeneralHttpClientFactory(
            ILogger logger,
            IHttpPlatformHelper http_helper,
            IHttpClientFactory clientFactory)
        {
            this.logger = logger;
            this.http_helper = http_helper;
            _clientFactory = clientFactory;
        }

        /// <summary>
        /// 用于 <see cref="IHttpClientFactory.CreateClient(string)"/> 中传递的 name
        /// <para>如果为 <see langword="null"/> 则调用 <see cref="HttpClientFactoryExtensions.CreateClient(IHttpClientFactory)"/></para>
        /// <para>默认值为 <see langword="null"/></para>
        /// </summary>
        protected virtual string? DefaultClientName { get; }

        /// <summary>
        /// 默认超时时间，16秒
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(11);

        static readonly Lazy<int> mDefaultTimeoutTotalMilliseconds = new(() => DefaultTimeout.TotalMilliseconds.ToInt32());

        /// <inheritdoc cref="DefaultTimeout"/>
        public static int DefaultTimeoutTotalMilliseconds => mDefaultTimeoutTotalMilliseconds.Value;

        /// <inheritdoc cref="DefaultTimeout"/>
        protected /*virtual*/ TimeSpan Timeout { get; } = DefaultTimeout;

        protected virtual HttpClient CreateClient(string? clientName = null)
        {
            var client = CreateClient_(clientName);
            client.Timeout = Timeout;
            return client;
        }

        HttpClient CreateClient_(string? clientName)
        {
            clientName ??= DefaultClientName;
#if DEBUG
            //logger.LogDebug("CreateClient, clientName: {0}", clientName);
#endif
            if (clientName == null)
            {
                return _clientFactory.CreateClient();
            }
            else
            {
                return _clientFactory.CreateClient(clientName);
            }
        }
    }
}
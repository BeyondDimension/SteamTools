using Microsoft.Extensions.Logging;

namespace System.Net.Http
{
    public abstract class HttpService
    {
        protected readonly ILogger logger;
        protected readonly IHttpPlatformHelper http_helper;
        readonly IHttpClientFactory _clientFactory;

        public HttpService(
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
        protected virtual string? ClientName { get; }

        protected virtual HttpClient CreateClient()
        {
#if DEBUG
            logger.LogDebug("CreateClient, name: {0}", ClientName);
#endif
            if (ClientName == null)
            {
                return _clientFactory.CreateClient();
            }
            else
            {
                return _clientFactory.CreateClient(ClientName);
            }
        }
    }
}
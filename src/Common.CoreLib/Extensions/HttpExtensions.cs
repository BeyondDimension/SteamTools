using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class HttpExtensions
    {
        #region UseDefaultSendX 使用此扩展代替 Send，将 DefaultRequestVersion 与 DefaultVersionPolicy 赋值给 HttpRequestMessage

#if NETSTANDARD
        public static Version DefaultRequestVersion { get; set; } = HttpVersion.Version20;
#endif

        static void UseDefault(HttpClient httpClient, HttpRequestMessage request)
        {
#if NETSTANDARD
            request.Version = DefaultRequestVersion;
#else
            request.Version = httpClient.DefaultRequestVersion;
            request.VersionPolicy = httpClient.DefaultVersionPolicy;
#endif
        }

        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage)"/>
        public static Task<HttpResponseMessage> UseDefaultSendAsync(this HttpClient httpClient, HttpRequestMessage request)
        {
            UseDefault(httpClient, request);
            return httpClient.SendAsync(request);
        }

        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption)"/>
        public static Task<HttpResponseMessage> UseDefaultSendAsync(this HttpClient httpClient, HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            UseDefault(httpClient, request);
            return httpClient.SendAsync(request, completionOption);
        }

        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/>
        public static Task<HttpResponseMessage> UseDefaultSendAsync(this HttpClient httpClient, HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            UseDefault(httpClient, request);
            return httpClient.SendAsync(request, completionOption, cancellationToken);
        }

        /// <inheritdoc cref="HttpClient.SendAsync(HttpRequestMessage, CancellationToken)"/>
        public static Task<HttpResponseMessage> UseDefaultSendAsync(this HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UseDefault(httpClient, request);
            return httpClient.SendAsync(request, cancellationToken);
        }

#if NET5_0_OR_GREATER
        /// <inheritdoc cref="HttpClient.Send(HttpRequestMessage)"/>
        public static HttpResponseMessage UseDefaultSend(this HttpClient httpClient, HttpRequestMessage request)
        {
            UseDefault(httpClient, request);
            return httpClient.Send(request);
        }

        /// <inheritdoc cref="HttpClient.Send(HttpRequestMessage, HttpCompletionOption)"/>
        public static HttpResponseMessage UseDefaultSend(this HttpClient httpClient, HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            UseDefault(httpClient, request);
            return httpClient.Send(request, completionOption);
        }

        /// <inheritdoc cref="HttpClient.Send(HttpRequestMessage, HttpCompletionOption, CancellationToken)"/>
        public static HttpResponseMessage UseDefaultSend(this HttpClient httpClient, HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            UseDefault(httpClient, request);
            return httpClient.Send(request, completionOption, cancellationToken);
        }

        /// <inheritdoc cref="HttpClient.Send(HttpRequestMessage, CancellationToken)"/>
        public static HttpResponseMessage UseDefaultSend(this HttpClient httpClient, HttpRequestMessage request, CancellationToken cancellationToken)
        {
            UseDefault(httpClient, request);
            return httpClient.Send(request, cancellationToken);
        }
#endif

        #endregion
    }
}

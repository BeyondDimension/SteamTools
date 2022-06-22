using System.Application.Models;
using System.Application.Services;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Internals.Http
{
    /// <summary>
    /// 表示http客户端
    /// </summary>
    public class ReverseProxyHttpClient : HttpMessageInvoker
    {
        const string Mark = "Steam++";

        /// <summary>
        /// 插入的UserAgent标记
        /// </summary>
        private static readonly ProductInfoHeaderValue userAgent = new(new ProductHeaderValue(Mark, "1.0"));

        /// <summary>
        /// http客户端
        /// </summary>
        /// <param name="domainConfig"></param>
        /// <param name="domainResolver"></param>
        public ReverseProxyHttpClient(IDomainConfig domainConfig, IDomainResolver domainResolver)
            : this(new ReverseProxyHttpClientHandler(domainConfig, domainResolver), disposeHandler: true)
        {
        }

        /// <summary>
        /// http客户端
        /// </summary> 
        /// <param name="handler"></param>
        /// <param name="disposeHandler"></param>
        public ReverseProxyHttpClient(HttpMessageHandler handler, bool disposeHandler)
            : base(handler, disposeHandler)
        {
        }

        /// <summary>
        /// 发送请求
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Headers.UserAgent.Contains(userAgent))
            {
                throw new Exception($"由于{request.RequestUri}实际指向了{Mark}自身，{Mark}已中断本次转发");
            }
            request.Headers.UserAgent.Add(userAgent);
            var response = await base.SendAsync(request, cancellationToken);
            response.Headers.Server.TryParseAdd(Mark);
            return response;
        }
    }
}
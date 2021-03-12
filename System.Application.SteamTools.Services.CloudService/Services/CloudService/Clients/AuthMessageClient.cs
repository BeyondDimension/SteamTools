using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AuthMessageClient : ApiClient, IAuthMessageClient
    {
        readonly IDictionary<string, DateTime> pairs;

        public AuthMessageClient(IApiConnection conn) : base(conn)
        {
            pairs = AuthMessageClientHelper.Create();
        }

        Task<IApiResponse> SendSmsCore(SendSmsRequest request)
            => conn.SendAsync(
                isAnonymous: false, // 发送短信验证码某些类型需要身份验证，某些则不要
                isSecurity: true,
                method: HttpMethod.Post,
                requestUri: "api/AuthMessage/SendSms",
                request: request,
                cancellationToken: default);

        public ValueTask<IApiResponse> SendSms(SendSmsRequest request)
            => AuthMessageClientHelper.SendSms(pairs, request, SendSmsCore);
    }
}
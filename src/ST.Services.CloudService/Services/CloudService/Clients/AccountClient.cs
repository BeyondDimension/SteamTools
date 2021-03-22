using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AccountClient : ApiClient, IAccountClient
    {
        public AccountClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<LoginOrRegisterResponse>> LoginOrRegister(LoginOrRegisterRequest request)
            => conn.SendAsync<LoginOrRegisterRequest, LoginOrRegisterResponse>(
                isAnonymous: true,
                isSecurity: true,
                method: HttpMethod.Post,
                requestUri: "api/Account/LoginOrRegister",
                request: request,
                cancellationToken: default,
                responseContentMaybeNull: false);

        public Task<IApiResponse<JWTEntity>> RefreshToken(string refresh_token)
            => conn.SendAsync<JWTEntity>(
                isAnonymous: true, // 刷新Token必须匿名身份，否则将递归死循环
                isSecurity: true,
                method: HttpMethod.Get,
                requestUri: $"api/Account/RefreshToken/{refresh_token}",
                cancellationToken: default,
                responseContentMaybeNull: true);
    }
}
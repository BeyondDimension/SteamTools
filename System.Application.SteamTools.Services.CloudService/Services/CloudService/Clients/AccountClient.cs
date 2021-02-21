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
                method: HttpMethod.Post,
                requestUri: "api/Account/LoginOrRegister",
                request: request,
                cancellationToken: default,
                responseContentMaybeNull: false);
    }
}
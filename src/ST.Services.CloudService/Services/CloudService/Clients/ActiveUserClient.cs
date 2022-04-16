using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class ActiveUserClient : ApiClient, IActiveUserClient
    {
        public ActiveUserClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse> Post(ActiveUserRecordDTO request)
            => conn.SendAsync<ActiveUserRecordDTO?>(
                isAnonymous: true, // 仅匿名收集
                isSecurity: true,
                method: HttpMethod.Post,
                requestUri: $"api/ActiveUser/",
                request: request,
                cancellationToken: default);
    }
}
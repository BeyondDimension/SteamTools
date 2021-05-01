using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class AccelerateClient : ApiClient, IAccelerateClient
    {
        public AccelerateClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<List<ScriptDTO>>> Scripts()
            => conn.SendAsync<List<ScriptDTO>>(
                isPolly: true,
                isAnonymous: true,
                isSecurity: false,
                method: HttpMethod.Get,
                requestUri: "api/Accelerate/Scripts",
                cancellationToken: default);

        public Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All()
            => conn.SendAsync<List<AccelerateProjectGroupDTO>>(
                isPolly: true,
                isAnonymous: true,
                isSecurity: false,
                method: HttpMethod.Get,
                requestUri: "api/Accelerate/All",
                cancellationToken: default);
    }
}
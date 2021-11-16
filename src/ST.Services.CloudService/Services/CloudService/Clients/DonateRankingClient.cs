using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;


namespace System.Application.Services.CloudService.Clients
{
    internal sealed class DonateRankingClient : ApiClient, IDonateRankingClient
    {
        public DonateRankingClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<PagedModel<RankingResponse>>> RangeQuery(PageQueryRequest<RankingRequest> model)
           => conn.SendAsync<PageQueryRequest<RankingRequest>, PagedModel<RankingResponse>>(
               isPolly: true,
               isAnonymous: true,
               isSecurity: false,
               method: HttpMethod.Post,
               requestUri: "api/ExternalTransaction/table",
               request: model,
               cancellationToken: default);
    }
}

using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface IDonateRankingClient
    {
        Task<IApiResponse<PagedModel<RankingResponse>>> Scripts(RankingRequest model);
    }
}

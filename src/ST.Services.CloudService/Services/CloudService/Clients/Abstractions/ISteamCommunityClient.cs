using System.Application.Models;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients.Abstractions
{
    public interface ISteamCommunityClient
    {
        Task<IApiResponse<SteamMiniProfile>> MiniProfile(int steamId32);
    }
}

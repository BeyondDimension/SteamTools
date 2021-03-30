using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class SteamCommunityClient : ApiClient, ISteamCommunityClient
    {
        public SteamCommunityClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<SteamMiniProfile>> MiniProfile(int steamId32)
            => conn.SendAsync<SteamMiniProfile>(
                isAnonymous: true,
                isSecurity: false,
                method: HttpMethod.Get,
                requestUri: string.Format("api/SteamCommunity/MiniProfile/{0}", steamId32),
                cancellationToken: default);
    }
}
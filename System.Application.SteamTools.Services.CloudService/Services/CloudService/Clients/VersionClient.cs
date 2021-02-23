using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class VersionClient : ApiClient, IVersionClient
    {
        public VersionClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<AppVersionInfoDTO?>> CheckUpdate(
            Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            CPUABI.Value supportedAbis,
            int major,
            int minor)
        {
            var url =
                $"/version/{id}/{(int)platform}/{(int)deviceIdiom}/{(int)supportedAbis}/{major}/{minor}";
            return conn.SendAsync<AppVersionInfoDTO?>(
                 method: HttpMethod.Get,
                 requestUri: url,
                 cancellationToken: default,
                 responseContentMaybeNull: true);
        }
    }
}
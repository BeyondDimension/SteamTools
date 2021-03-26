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

        public Task<IApiResponse<AppVersionDTO?>> CheckUpdate(
            Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            ArchitectureFlags supportedAbis,
            Version osVersion,
            ArchitectureFlags abi)
        {
            var url =
                $"api/version/checkupdate/{id}/{(int)platform}/{(int)deviceIdiom}" +
                $"/{(int)supportedAbis}/{osVersion.Major}/{osVersion.Minor}/{(int)abi}";
            return conn.SendAsync<AppVersionDTO?>(
                isAnonymous: true,
                method: HttpMethod.Get,
                requestUri: url,
                cancellationToken: default,
                responseContentMaybeNull: true);
        }
    }
}
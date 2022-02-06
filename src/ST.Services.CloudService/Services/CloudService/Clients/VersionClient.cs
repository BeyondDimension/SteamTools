using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService.Clients
{
    internal sealed class VersionClient : ApiClient, IVersionClient
    {
        public VersionClient(IApiConnection conn) : base(conn)
        {
        }

        public Task<IApiResponse<AppVersionDTO?>> CheckUpdate2(
            Guid id,
            Platform platform,
            DeviceIdiom deviceIdiom,
            Version osVersion,
            Architecture architecture,
            DeploymentMode deploymentMode)
        {
            var url =
                $"api/version/checkupdate3/{id}/{(int)platform}/{(int)deviceIdiom}/{(int)architecture}/{osVersion.Major}/{osVersion.Minor}/{osVersion.Build}/{(int)deploymentMode}";
            return conn.SendAsync<AppVersionDTO?>(
                isAnonymous: true,
                method: HttpMethod.Get,
                requestUri: url,
                cancellationToken: default,
                responseContentMaybeNull: true);
        }
    }
}
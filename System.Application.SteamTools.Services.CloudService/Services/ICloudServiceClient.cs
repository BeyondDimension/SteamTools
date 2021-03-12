using System.Application.Models;
using System.Application.Services.CloudService;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services
{
    public interface ICloudServiceClient
    {
        IAccountClient Account { get; }

        IManageClient Manage { get; }

        IAuthMessageClient AuthMessage { get; }

        IVersionClient Version { get; }

        /// <inheritdoc cref="IApiConnection.DownloadAsync(bool, CancellationToken, string, string, IProgress{float})"/>
        Task<IApiResponse> Download(bool isAnonymous, string requestUri, string cacheFilePath, IProgress<float> progress, CancellationToken cancellationToken = default);

        public static ICloudServiceClient Instance => DI.Get<ICloudServiceClient>();
    }
}
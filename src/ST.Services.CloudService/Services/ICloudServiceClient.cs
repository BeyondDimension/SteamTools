using System.Application.Models;
using System.Application.Services.CloudService;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using Const = System.Application.Services.CloudService.Constants;

namespace System.Application.Services
{
    public interface ICloudServiceClient
    {
        protected const string DefaultApiBaseUrl = ThisAssembly.Debuggable ?
            "https://steamtools-api.chinacloudsites.cn" :
            Const.Prefix_HTTPS + "api.steampp.net";

        string ApiBaseUrl { get; }
        IAccountClient Account { get; }
        IManageClient Manage { get; }
        IAuthMessageClient AuthMessage { get; }
        IVersionClient Version { get; }
        IActiveUserClient ActiveUser { get; }
        IAccelerateClient Accelerate { get; }
        ISteamCommunityClient SteamCommunity { get; }

        /// <inheritdoc cref="IApiConnection.DownloadAsync(bool, CancellationToken, string, string, IProgress{float})"/>
        Task<IApiResponse> Download(bool isAnonymous, string requestUri, string cacheFilePath, IProgress<float> progress, CancellationToken cancellationToken = default);

        /// <summary>
        /// 请求代理
        /// </summary>
        /// <param name="request"></param>
        /// <param name="completionOption"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        Task<HttpResponseMessage> Forward(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken = default);

        public static ICloudServiceClient Instance => DI.Get<ICloudServiceClient>();
    }
}
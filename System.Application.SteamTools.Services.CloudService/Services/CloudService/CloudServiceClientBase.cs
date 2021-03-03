using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Application.Columns;
using System.Application.Models;
using System.Application.Services.CloudService.Clients;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public abstract class CloudServiceClientBase : GeneralHttpClientFactory, ICloudServiceClient, IApiConnectionPlatformHelper
    {
        public const string ClientName_ = "CloudServiceClient";
        internal const string DefaultApiBaseUrl = Constants.Prefix_HTTPS + "api.steamtool.net";

        #region Clients

        public IAccountClient Account { get; }
        public IAuthMessageClient AuthMessage { get; }
        public IVersionClient Version { get; }

        #endregion

        readonly ApiConnection connection;
        protected readonly ICloudServiceSettings settings;
        protected readonly IAuthHelper authHelper;

        public string ApiBaseUrl { get; }

        internal ICloudServiceSettings Settings => settings;

        protected sealed override string? ClientName => ClientName_;

        public CloudServiceClientBase(
            ILogger logger,
            IHttpClientFactory clientFactory,
            IHttpPlatformHelper http_helper,
            IAuthHelper authHelper,
            IOptions<ICloudServiceSettings> options,
            IModelValidator validator) : base(logger, http_helper, clientFactory)
        {
            this.authHelper = authHelper;
            settings = options.Value;
            ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl)
                ? DefaultApiBaseUrl : settings.ApiBaseUrl;
            connection = new ApiConnection(logger, this, http_helper, validator);

            #region SetClients

            Account = new AccountClient(connection);
            AuthMessage = new AuthMessageClient(connection);
            Version = new VersionClient(connection);

            #endregion
        }

        /// <inheritdoc cref="IHttpPlatformHelper.UserAgent"/>
        internal string UserAgent => http_helper.UserAgent;

        IAuthHelper IApiConnectionPlatformHelper.Auth => authHelper;

        public abstract Task SaveAuthTokenAsync(string authToken);

        public abstract Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response);

        public abstract void ShowResponseErrorMessage(string message);

        HttpClient IApiConnectionPlatformHelper.CreateClient() => CreateClient();

        Task<IApiResponse> ICloudServiceClient.Download(string requestUri,
            string cacheFilePath, IProgress<float> progress, CancellationToken cancellationToken)
            => connection.DownloadAsync(cancellationToken, requestUri, cacheFilePath, progress);
    }
}
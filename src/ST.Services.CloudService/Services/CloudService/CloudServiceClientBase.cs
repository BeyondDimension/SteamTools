using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Application.Columns;
using System.Application.Models;
using System.Application.Security;
using System.Application.Services.CloudService.Clients;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public abstract class CloudServiceClientBase : GeneralHttpClientFactory, ICloudServiceClient, IApiConnectionPlatformHelper
    {
        protected internal const string ClientName_ = "CloudServiceClient";

        #region Clients

        public IScriptClient Script { get; }
        public IAccountClient Account { get; }
        public IManageClient Manage { get; }
        public IAuthMessageClient AuthMessage { get; }
        public IVersionClient Version { get; }
        public IActiveUserClient ActiveUser { get; }
        public IAccelerateClient Accelerate { get; }
        public IDonateRankingClient DonateRanking { get; }

        #endregion

        readonly IApiConnection connection;
        protected readonly ICloudServiceSettings settings;
        protected readonly IAuthHelper authHelper;
        protected readonly IToast toast;

        public string ApiBaseUrl { get; }

        public ICloudServiceSettings Settings => settings;

        RSA IApiConnectionPlatformHelper.RSA
        {
            get
            {
                try
                {
                    return Settings.RSA;
                }
                catch (IsNotOfficialChannelPackageException e)
                {
                    logger.LogError(e, nameof(ApiResponseCode.IsNotOfficialChannelPackage));
                    throw new ApiResponseCodeException(ApiResponseCode.IsNotOfficialChannelPackage);
                }
            }
        }

        protected sealed override string? DefaultClientName => ClientName_;

        public CloudServiceClientBase(
            ILogger logger,
            IHttpClientFactory clientFactory,
            IHttpPlatformHelperService http_helper,
            IToast toast,
            IAuthHelper authHelper,
            ICloudServiceSettings settings,
            IModelValidator validator) : base(logger, http_helper, clientFactory)
        {
            this.authHelper = authHelper;
            this.toast = toast;
            this.settings = settings;
            ApiBaseUrl = string.IsNullOrWhiteSpace(settings.ApiBaseUrl)
                ? throw new ArgumentNullException(nameof(ApiBaseUrl)) : settings.ApiBaseUrl;
            connection = new ApiConnection(logger, this, http_helper, validator);

            #region SetClients

            Account = new AccountClient(connection);
            Manage = new ManageClient(connection);
            AuthMessage = new AuthMessageClient(connection);
            Version = new VersionClient(connection);
            ActiveUser = new ActiveUserClient(connection);
            Accelerate = new AccelerateClient(connection);
            Script = new ScriptClient(connection);
            DonateRanking = new DonateRankingClient(connection);

            #endregion
        }

        /// <inheritdoc cref="IHttpPlatformHelperService.UserAgent"/>
        internal string UserAgent => http_helper.UserAgent;

        IAuthHelper IApiConnectionPlatformHelper.Auth => authHelper;

        public abstract Task SaveAuthTokenAsync(JWTEntity authToken);

        public abstract Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response);

        public virtual void ShowResponseErrorMessage(string message)
        {
            toast.Show(message);
        }

        HttpClient IApiConnectionPlatformHelper.CreateClient() => CreateClient();

        Task<IApiResponse> ICloudServiceClient.Download(bool isAnonymous, string requestUri,
            string cacheFilePath, IProgress<float>? progress, CancellationToken cancellationToken)
            => connection.DownloadAsync(cancellationToken, requestUri, cacheFilePath, progress, isAnonymous);

        Task<IApiResponse<JWTEntity>> IApiConnectionPlatformHelper.RefreshToken(JWTEntity jwt)
            => Account.RefreshToken(jwt);

        Task<HttpResponseMessage> ICloudServiceClient.Forward(
            HttpRequestMessage request,
            HttpCompletionOption completionOption,
            CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
            //request.RequestUri = new Uri(ForwardHelper.GetForwardRelativeUrl(request), UriKind.Relative);
            //return connection.SendAsync(request, completionOption, cancellationToken);
        }

        async Task<string> ICloudServiceClient.Info()
        {
            var api = await connection.GetHtml(default, "/info");
            var str = api.Content;
            if (!string.IsNullOrWhiteSpace(str))
            {
                try
                {
                    var jsonObj = JObject.Parse(str);
                    return Serializable.SJSON(Serializable.JsonImplType.NewtonsoftJson, jsonObj, writeIndented: true);
                }
                catch
                {
                    return str;
                }
            }
            else
            {
                return string.Empty;
            }
        }
    }
}
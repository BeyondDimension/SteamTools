using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Application.Columns;
using System.Application.Models;
using System.Application.Services.CloudService.Clients;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public abstract class CloudServiceClientBase : ICloudServiceClient, IApiConnectionPlatformHelper
    {
        //const string DefaultApiBaseUrl = Constants.Prefix_HTTPS + "api.待定.com";
        static string DefaultApiBaseUrl => throw new NotImplementedException();

        #region Clients

        public IAccountClient Account { get; }
        public IAuthMessageClient AuthMessage { get; }

        #endregion

        protected readonly ILogger logger;
        readonly ApiConnection connection;
        protected readonly IAuthHelper authHelper;

        public string ApiBaseUrl { get; }

        public CloudServiceClientBase(
            ILogger logger,
            IAuthHelper authHelper,
            IOptions<ICloudServiceSettings> options,
            IModelValidator validator)
        {
            this.logger = logger;
            this.authHelper = authHelper;
            var client = CreateHttpClient();
            ApiBaseUrl = string.IsNullOrWhiteSpace(options.Value.ApiBaseUrl) ? DefaultApiBaseUrl : options.Value.ApiBaseUrl;
            client.BaseAddress = new Uri(ApiBaseUrl);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
            client.DefaultRequestHeaders.Add(Constants.HeaderAppVersion,
                options.Value.AppVersion.ToStringN());
            connection = new ApiConnection(logger, client, this, validator);

            #region SetClients

            Account = new AccountClient(connection);
            AuthMessage = new AuthMessageClient(connection);

            #endregion
        }

        protected virtual HttpClient CreateHttpClient() => new HttpClient();

        /// <summary>
        /// 用户代理
        /// <para>https://developer.mozilla.org/zh-CN/docs/Web/HTTP/Headers/User-Agent</para>
        /// </summary>
        protected virtual string UserAgent => Constants.DefaultUserAgent;

        IAuthHelper IApiConnectionPlatformHelper.Auth => authHelper;

        public abstract Task SaveAuthTokenAsync(string authToken);

        public abstract Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response);

        public abstract void ShowResponseErrorMessage(string message);

        public abstract (string filePath, string mime)? TryHandleUploadFile(Stream imageFileStream, UploadFileType uploadFileType);
    }
}
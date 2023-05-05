using HttpVersion = System.Net.HttpVersion;

namespace BD.WTTS.Services.Implementation;

sealed class MicroServiceClient : MicroServiceClientBase
{
    readonly IUserManager userManager;

    public MicroServiceClient(
        ILoggerFactory loggerFactory,
        HttpClientFactory clientFactory,
        IHttpPlatformHelperService httpPlatformHelper,
        IUserManager userManager,
        IToast toast,
        IOptions<AppSettings> options,
        IModelValidator validator,
        IApplicationVersionService appVerService) : base(
            loggerFactory.CreateLogger<MicroServiceClient>(),
            loggerFactory,
            clientFactory,
            httpPlatformHelper,
            toast,
            userManager,
            options.Value,
            validator,
            appVerService)
    {
        this.userManager = userManager;
    }

    protected sealed override HttpClient CreateClient()
    {
        var client = base.CreateClient();
        client.BaseAddress = new Uri(ApiBaseUrl, UriKind.Absolute);
        client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
#if NETCOREAPP3_0_OR_GREATER
        client.DefaultRequestVersion = HttpVersion.Version20;
#endif
        return client;
    }

    public sealed override async Task SaveAuthTokenAsync(JWTEntity authToken)
    {
        var user = await userManager.GetCurrentUserAsync();
        if (user != null)
        {
            user.AuthToken = authToken;
            await userManager.SetCurrentUserAsync(user);
        }
    }

    public sealed override async Task OnLoginedAsync(
        IReadOnlyPhoneNumber? phoneNumber,
        ILoginResponse response)
    {
        if (response is LoginOrRegisterResponse loginOrRegisterResponse)
        {
            var user = loginOrRegisterResponse.User;
            if (user != null)
                await userManager.SetCurrentUserInfoAsync(user, true);
        }

        CurrentUser cUser = new()
        {
            UserId = response.UserId,
            AuthToken = response.AuthToken,
            PhoneNumber = phoneNumber?.PhoneNumber ?? string.Empty,
        };

        await userManager.SetCurrentUserAsync(cUser);
    }

    protected sealed override void SetDeviceId(IDeviceId deviceId)
    {
        var deviceIdG = DeviceIdHelper.DeviceIdG;
        var deviceIdR = DeviceIdHelper.DeviceIdR;
        var deviceIdN = DeviceIdHelper.DeviceIdN;

        deviceId.DeviceIdG = deviceIdG;
        deviceId.DeviceIdR = deviceIdR;
        deviceId.DeviceIdN = deviceIdN;
    }

    public sealed class HttpClientFactory : IHttpClientFactory, IDisposable
    {
        readonly Lazy<HttpClient> client;
        readonly Action<HttpClient>? config;
        readonly Func<HttpMessageHandler>? configureHandler;
        bool disposedValue;

        public HttpClientFactory(Action<HttpClient>? config = null, Func<HttpMessageHandler>? configureHandler = null)
        {
            this.config = config;
            this.configureHandler = configureHandler;
            client = new(CreateClient);
        }

        HttpClient CreateClient()
        {
            HttpMessageHandler handler = configureHandler?.Invoke() ?? new SocketsHttpHandler();
            var client = new HttpClient(handler);
            config?.Invoke(client);
            return client;
        }

        HttpClient IHttpClientFactory.CreateClient(string name) => client.Value;

        void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // 释放托管状态(托管对象)
                    if (client.IsValueCreated)
                    {
                        client.Value.Dispose();
                    }
                }

                // 释放未托管的资源(未托管的对象)并重写终结器
                // 将大型字段设置为 null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

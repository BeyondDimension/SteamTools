using HttpVersion = System.Net.HttpVersion;

namespace BD.WTTS.Services.Implementation;

sealed class MicroServiceClient : MicroServiceClientBase
{
    readonly IUserManager userManager;

    internal const string ClientName = ClientName_;

    public MicroServiceClient(
        ILoggerFactory loggerFactory,
        IHttpClientFactory clientFactory,
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

    protected sealed override HttpClient CreateClient(HttpHandlerCategory category)
    {
        category = HttpHandlerCategory.Default;
        var client = base.CreateClient(category);

        try
        {
            client.BaseAddress = new Uri(ApiBaseUrl, UriKind.Absolute);
            client.DefaultRequestHeaders.UserAgent.ParseAdd(UserAgent);
#if NETCOREAPP3_0_OR_GREATER
            client.DefaultRequestVersion = HttpVersion.Version20;
#endif
        }
        catch (InvalidOperationException)
        {

        }

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

    public sealed override async Task SaveShopAuthTokenAsync(JWTEntity authToken)
    {
        var user = await userManager.GetCurrentUserAsync();
        if (user != null)
        {
            user.ShopAuthToken = authToken;
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
}

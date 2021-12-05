using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Application.Columns;
using System.Application.Models;
using System.Application.Services.CloudService;
using System.Net.Http;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public class CloudServiceClient : CloudServiceClientBase
    {
        protected readonly IUserManager userManager;

        public CloudServiceClient(
            ILoggerFactory loggerFactory,
            IHttpClientFactory clientFactory,
            IHttpPlatformHelperService httpPlatformHelper,
            IUserManager userManager,
            IToast toast,
            IOptions<AppSettings> options,
            IModelValidator validator) : base(
                loggerFactory.CreateLogger(ClientName_),
                clientFactory,
                httpPlatformHelper,
                toast,
                userManager,
                options.Value,
                validator)
        {
            this.userManager = userManager;
        }

        public override async Task SaveAuthTokenAsync(JWTEntity authToken)
        {
            var user = await userManager.GetCurrentUserAsync();
            if (user != null)
            {
                user.AuthToken = authToken;
                await userManager.SetCurrentUserAsync(user);
            }
        }

        public override async Task OnLoginedAsync(IReadOnlyPhoneNumber? phoneNumber, ILoginResponse response)
        {
            await userManager.SetCurrentUserInfoAsync(response.User, true);

            var cUser = new CurrentUser
            {
                UserId = response.UserId,
                AuthToken = response.AuthToken,
                PhoneNumber = phoneNumber?.PhoneNumber ?? string.Empty,
            };

            await userManager.SetCurrentUserAsync(cUser);
        }
    }
}
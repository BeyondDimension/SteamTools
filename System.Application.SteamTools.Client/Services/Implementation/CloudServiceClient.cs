using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Application.Columns;
using System.Application.Models;
using System.Application.Services.CloudService;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    public abstract class CloudServiceClient : CloudServiceClientBase
    {
        public const string TAG = "ServerApiClient";

        protected readonly IUserManager userManager;
        protected readonly IToast toast;

        public CloudServiceClient(
            ILoggerFactory loggerFactory,
            IUserManager userManager,
            IToast toast,
            IOptions<AppSettings> options,
            IModelValidator validator) : base(loggerFactory.CreateLogger(TAG), userManager, options, validator)
        {
            this.userManager = userManager;
            this.toast = toast;
        }

        public override async Task SaveAuthTokenAsync(string authToken)
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
            var cUser = new CurrentUser
            {
                UserId = response.UserId,
                AuthToken = response.AuthToken,
                PhoneNumber = phoneNumber?.PhoneNumber ?? string.Empty,
            };
            await userManager.SetCurrentUserAsync(cUser);
        }

        public override void ShowResponseErrorMessage(string message)
        {
            toast.Show(message);
        }
    }
}
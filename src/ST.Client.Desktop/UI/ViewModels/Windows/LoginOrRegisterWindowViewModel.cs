using System.Application.Models;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.UI.Resx;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    partial class LoginOrRegisterWindowViewModel : WindowViewModel
    {
        internal static async Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
        {
            if (!AppHelper.IsSystemWebViewAvailable) return;
            var apiBaseUrl = ICloudServiceClient.Instance.ApiBaseUrl;
            var urlExternalLoginCallback = apiBaseUrl + "/ExternalLoginCallback";
            WebView3WindowViewModel? vm = null;
            vm = new WebView3WindowViewModel
            {
                Url = apiBaseUrl +
                    (channel == FastLoginChannel.Steam ?
                    "/ExternalLogin" :
                    $"/ExternalLogin/{(int)channel}") +
                    (isBind ?
                    "?isBind=true" :
                    string.Empty),
                StreamResponseFilterUrls = new[]
                {
                    urlExternalLoginCallback,
                },
                OnStreamResponseFilterResourceLoadComplete = _OnStreamResponseFilterResourceLoadComplete,
                FixedSinglePage = true,
                Title = AppResources.User_FastLogin_.Format(channel),
                TimeoutErrorMessage = channel == FastLoginChannel.Steam ? AppResources.User_SteamFastLoginTimeoutErrorMessage : null,
                IsSecurity = true,
                UseLoginUsingSteamClient = channel == FastLoginChannel.Steam,
                Close = close,
            };
            async void _OnStreamResponseFilterResourceLoadComplete(string url, Stream data)
            {
                if (url.StartsWith(urlExternalLoginCallback, StringComparison.OrdinalIgnoreCase))
                {
                    var response = await ApiResponse.DeserializeAsync<LoginOrRegisterResponse>(data, default);
                    if (response.IsSuccess && response.Content == null)
                    {
                        response.Code = ApiResponseCode.NoResponseContent;
                    }
                    var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
                    if (response.IsSuccess)
                    {
                        if (isBind)
                        {
                            await MainThread2.InvokeOnMainThreadAsync(async () =>
                            {
                                await UserService.Current.BindAccountAfterUpdateAsync(channel, response.Content!);
                                vm?.Close?.Invoke();
                                var msg = AppResources.Success_.Format(AppResources.User_AccountBind);
                                Toast.Show(msg);
                            });
                        }
                        else
                        {
                            await conn_helper.OnLoginedAsync(response.Content!, response.Content!);
                            await MainThread2.InvokeOnMainThreadAsync(async () =>
                            {
                                await SuccessAsync(response.Content!, vm?.Close);
                            });
                        }
                    }
                    else
                    {
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            vm?.Close?.Invoke();
                            conn_helper.ShowResponseErrorMessage(response);
                        });
                    }
                }
            }
            await IShowWindowService.Instance.Show(CustomWindow.WebView3, vm,
                resizeMode: ResizeModeCompat.CanResize,
                isDialog: true // 锁定父窗口，防止打开多个 WebViewWindow
                );
        }

        internal Task GoToLoginOrRegisterByPhoneNumberAsync()
        {
            LoginState = 1;
            return Task.CompletedTask;
        }
    }
}
using System.Application.Models;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.UI.Resx;
using System.Net;
using System.Net.WebSocket;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;
using static System.Application.UI.ViewModels.FastLRBHelper;
#if __MOBILE__
using LRViewModel = System.Application.UI.ViewModels.LoginOrRegisterPageViewModel;
using Microsoft.Identity.Client;
#else
using LRViewModel = System.Application.UI.ViewModels.LoginOrRegisterWindowViewModel;
#endif

namespace System.Application.UI.ViewModels
{
    public static class FastLRBHelper
    {
        public interface IViewModel : IDisposableHolder
        {
            /// <summary>
            /// 当前 WebSocket 服务
            /// </summary>
            ServerWebSocket? ServerWebSocket { get; set; }

            /// <summary>
            /// 当前 WebSocket 服务监听端口号
            /// </summary>
            int ServerWebSocketListenerPort { get; set; }

            /// <summary>
            /// 临时 Aes 对象
            /// </summary>
            Aes? TempAes { get; set; }

            /// <summary>
            /// 关闭当前窗口
            /// </summary>
            Action? Close { get; }

            /// <summary>
            /// 是否为绑定账号
            /// </summary>
            bool IsBind { get; }
        }

        /// <summary>
        /// 当接收到 WebSocket Client 发送的消息时
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static async Task OnClientReceived(this IViewModel vm, ReceivedEventArgs e)
        {
            if (vm.TempAes == null) return;
            var byteArray = e.Message.Base64UrlDecodeToByteArray();
            byteArray = vm.TempAes.Decrypt(byteArray);
            var response = ApiResponse.Deserialize<LoginOrRegisterResponse>(byteArray);
            var webResponse = new FastLRBWebResponse();
            if (response.IsSuccess && response.Content == null)
            {
                webResponse.Msg = ApiResponse.GetMessage(ApiResponseCode.NoResponseContent);
                await e.Client.WriteAsync(webResponse);
                return;
            }
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            webResponse.State = response.IsSuccess;
            await e.Client.WriteAsync(webResponse); // 仅可在 close 之前传递消息
            if (response.IsSuccess)
            {
                if (vm.IsBind)
                {
                    var channel = response.Content?.FastLRBChannel;
                    if (!channel.HasValue) return;
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        string msg;
                        if (channel.HasValue)
                        {
                            msg = AppResources.Success_.Format(AppResources.User_AccountBind);
                            await UserService.Current.BindAccountAfterUpdateAsync(channel.Value, response.Content!);
                        }
                        else
                        {
                            msg = "Account bind fail, unknown channel.";
                        }
                        vm?.Close?.Invoke();
                        Toast.Show(msg);
                    });
                }
                else
                {
                    await conn_helper.OnLoginedAsync(response.Content!, response.Content!);
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        await LRViewModel.SuccessAsync(response.Content!, vm.Close);
                    });
                }
            }
            else
            {
                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    vm?.Close?.Invoke();
                    conn_helper.ShowResponseErrorMessage(response);
                });
            }
        }

#if __MOBILE__
        /// <summary>
        /// 通过 Microsoft Authentication Library (MSAL) 实现 <see cref="FastLoginChannel.Microsoft"/> 的版本
        /// </summary>
        static async void MSAL()
        {
            var publicClientApp = DI.Get_Nullable<IPublicClientApplication>();
            if (publicClientApp == null) return;
            var uiHost = IMobilePlatformService.Instance.CurrentPlatformUIHost;
            // 页面后退不会结束等待，此处使用 ReactiveCommand.CreateFromTask，所以需要 async void
            var authResult = await publicClientApp
                .AcquireTokenInteractive(new string[] { "user.read" })
                .WithParentActivityOrWindow(uiHost)
                .ExecuteAsync();
            var dstr = Serializable.SJSON(authResult);
            Toast.Show(dstr);
        }

        /// <summary>
        /// 适用于移动端平台实现 <see cref="FastLoginChannel"/> 的版本
        /// </summary>
        /// <param name="action"></param>
        static void MobilePlatform(Action<IPlatformUIHost> action)
        {
            if (IMobilePlatformService.Instance.CurrentPlatformUIHost is IPlatformUIHost uiHost)
            {
                action(uiHost);
            }
        }

        /// <summary>
        /// 移动端平台实现定义接口
        /// </summary>
        public interface IPlatformUIHost
        {
            void QQLogin();
        }
#endif

        /// <summary>
        /// 开始第三方快速登录、注册、绑定
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static void StartLRB(this IViewModel vm, FastLoginChannel channel)
        {
#if __MOBILE__
            switch (channel)
            {
                case FastLoginChannel.Microsoft:
                    MSAL();
                    return;
                case FastLoginChannel.QQ:
                    MobilePlatform(uiHost => uiHost.QQLogin());
                    return;
            }
#endif
            if (vm.ServerWebSocket == null)
            {
                vm.ServerWebSocket = new(IPAddress.Loopback, out var port);
                vm.ServerWebSocket.AddTo(vm);
                vm.ServerWebSocketListenerPort = port;
                vm.ServerWebSocket.OnClientReceived += e => vm.OnClientReceived(e);
            }
            vm.ServerWebSocket.Start();
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            var apiBaseUrl =
//#if DEBUG
//           "https://127.0.0.1:28110";
//#else
ICloudServiceClient.Instance.ApiBaseUrl;
            //#endif

            vm.TempAes.RemoveTo(vm);
            vm.TempAes = AESUtils.Create();
            vm.TempAes.AddTo(vm);
            var skey_bytes = vm.TempAes.ToParamsByteArray();
            var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
            var csc = DI.Get<CloudServiceClientBase>();
            var padding = RSAUtils.DefaultPadding;
            var isBind = vm.IsBind;
            var auth = string.Empty;
            if (isBind)
            {
                Func<Task<JWTEntity?>> getAuthTokenAsync = () => conn_helper.Auth.GetAuthTokenAsync().AsTask();
                var authToken = getAuthTokenAsync.RunSync();
                var authHeaderValue = conn_helper.GetAuthenticationHeaderValue(authToken);
                if (authHeaderValue != null)
                {
                    var authHeaderValueStr = authHeaderValue.ToString();
                    auth = vm.TempAes.Encrypt(authHeaderValueStr);
                }
            }
            var url = apiBaseUrl +
                $"/ExternalLoginDetection/{(int)channel}" +
                $"?websocket=true&" +
                $"port={vm.ServerWebSocketListenerPort}&" +
                $"sKey={skey_str}&" +
                $"sKeyPadding={padding.OaepHashAlgorithm}&" +
                $"version={csc.Settings.AppVersionStr}&" +
                $"isBind={isBind}&" +
                $"auth={auth}";
            BrowserOpen(url);
        }
    }

    partial class
#if __MOBILE__
        LoginOrRegisterPageViewModel
#else
        LoginOrRegisterWindowViewModel
#endif
        : IViewModel
    {
        ServerWebSocket? IViewModel.ServerWebSocket { get; set; }

        int IViewModel.ServerWebSocketListenerPort { get; set; }

        Aes? IViewModel.TempAes { get; set; }

        bool IViewModel.IsBind => false;
    }

    partial class
#if __MOBILE__
        UserProfilePageViewModel
#else
        UserProfileWindowViewModel
#endif
        : IViewModel
    {
        ServerWebSocket? IViewModel.ServerWebSocket { get; set; }

        int IViewModel.ServerWebSocketListenerPort { get; set; }

        Aes? IViewModel.TempAes { get; set; }

        bool IViewModel.IsBind => true;
    }
}

//async Task HandleOnClientReceived(ReceivedEventArgs msg)
//{
//    var response = ApiResponse.Deserialize<LoginOrRegisterResponse>(aes.Decrypt(msg.Message.Base64UrlDecodeToByteArray()));
//    var retResponse = new FastLRBWebResponse();
//    if (response.IsSuccess && response.Content == null)
//    {
//        //retResponse.Msg = AppResources.UserChange_NoUserTip;
//        await msg.Client.WriteAsync(retResponse);
//        return;
//    }
//    var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
//    if (response.IsSuccess)
//    {
//        retResponse.State = true;
//        if (IsBind)
//        {
//            await MainThread2.InvokeOnMainThreadAsync(async () =>
//            {
//                await UserService.Current.BindAccountAfterUpdateAsync(Channel, response.Content!);
//                var msg = AppResources.Success_.Format(AppResources.User_AccountBind);
//                Toast.Show(msg);
//                Close?.Invoke();
//            });
//        }
//        else
//        {
//            await conn_helper.OnLoginedAsync(response.Content!, response.Content!);
//            await MainThread2.InvokeOnMainThreadAsync(async () =>
//            {
//                await SuccessAsync(response.Content!);
//                Close?.Invoke();
//            });
//        }
//        await msg.Client.WriteAsync(retResponse);
//        //登录完成释放
//        server!.Dispose();
//        return;
//    }
//    else
//    {
//        MainThread2.BeginInvokeOnMainThread(() =>
//        {
//            conn_helper.ShowResponseErrorMessage(response);
//        });
//    }
//    await msg.Client.WriteAsync(retResponse);
//}

//internal static Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
//{
//    var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
//    var apiBaseUrl = "https://127.0.0.1:28110"; //ICloudServiceClient.Instance.ApiBaseUrl;
//    if (aes == null)
//        aes = AESUtils.Create();
//    var skey_bytes = aes.ToParamsByteArray();
//    var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
//    var sc = DI.Get<CloudServiceClientBase>();
//    var url = apiBaseUrl +
//         (channel == FastLoginChannel.Steam ?
//         "/ExternalLoginDetection" : $"/ExternalLoginDetection/{(int)channel}") + $"?websocket=true&port={port}&sKey={skey_str}&Version={sc.Settings.AppVersionStr}" +
//         (isBind ? "&isBind=true" : string.Empty);
//    IsBind = isBind;
//    Channel = channel;
//    Services.CloudService.Constants.BrowserOpen(url);

//    //if (!AppHelper.IsSystemWebViewAvailable) return;
//    ////var apiBaseUrl = ICloudServiceClient.Instance.ApiBaseUrl;
//    //var urlExternalLoginCallback = apiBaseUrl + "/ExternalLoginCallback";
//    //WebView3WindowViewModel? vm = null;
//    //vm = new WebView3WindowViewModel
//    //{
//    //    Url = apiBaseUrl +
//    //        (channel == FastLoginChannel.Steam ?
//    //        "/ExternalLogin" :
//    //        $"/ExternalLogin/{(int)channel}") +
//    //        (isBind ?
//    //        "?isBind=true" :
//    //        string.Empty),
//    //    StreamResponseFilterUrls = new[]
//    //    {
//    //        urlExternalLoginCallback,
//    //    },
//    //    OnStreamResponseFilterResourceLoadComplete = _OnStreamResponseFilterResourceLoadComplete,
//    //    FixedSinglePage = true,
//    //    Title = AppResources.User_FastLogin_.Format(channel),
//    //    TimeoutErrorMessage = channel == FastLoginChannel.Steam ? AppResources.User_SteamFastLoginTimeoutErrorMessage : null,
//    //    IsSecurity = true,
//    //    UseLoginUsingSteamClient = channel == FastLoginChannel.Steam,
//    //    Close = close,
//    //};
//    //async void _OnStreamResponseFilterResourceLoadComplete(string url, Stream data)
//    //{
//    //    if (url.StartsWith(urlExternalLoginCallback, StringComparison.OrdinalIgnoreCase))
//    //    {
//    //        var response = await ApiResponse.DeserializeAsync<LoginOrRegisterResponse>(data, default);
//    //        if (response.IsSuccess && response.Content == null)
//    //        {
//    //            response.Code = ApiResponseCode.NoResponseContent;
//    //        }
//    //        var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
//    //        if (response.IsSuccess)
//    //        {
//    //            if (isBind)
//    //            {
//    //                await MainThread2.InvokeOnMainThreadAsync(async () =>
//    //                {
//    //                    await UserService.Current.BindAccountAfterUpdateAsync(channel, response.Content!);
//    //                    vm?.Close?.Invoke();
//    //                    var msg = AppResources.Success_.Format(AppResources.User_AccountBind);
//    //                    Toast.Show(msg);
//    //                });
//    //            }
//    //            else
//    //            {
//    //                await conn_helper.OnLoginedAsync(response.Content!, response.Content!);
//    //                await MainThread2.InvokeOnMainThreadAsync(async () =>
//    //                {
//    //                    await SuccessAsync(response.Content!, vm?.Close);
//    //                });
//    //            }
//    //        }
//    //        else
//    //        {
//    //            MainThread2.BeginInvokeOnMainThread(() =>
//    //            {
//    //                vm?.Close?.Invoke();
//    //                conn_helper.ShowResponseErrorMessage(response);
//    //            });
//    //        }
//    //    }
//    //}
//    //await IShowWindowService.Instance.Show(CustomWindow.WebView3, vm,
//    //    resizeMode: ResizeModeCompat.CanResize,
//    //    isDialog: true // 锁定父窗口，防止打开多个 WebViewWindow
//    //    );
//}
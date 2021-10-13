using Fleck;
using System.Application.Models;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static System.Application.UI.ViewModels.FastLRBHelper;

namespace System.Application.UI.ViewModels
{
    public static class FastLRBHelper
    {
#if DEBUG
        static readonly bool UseLoopbackTest = false;
#endif

        public interface IViewModel : IDisposableHolder
        {
            /// <summary>
            /// 当前 WebSocket 服务
            /// </summary>
            WebSocketServer? WebSocketServer { get; set; }

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

            void OnBindSuccessed() { }
        }

        public static async Task ManualLoginAsync(this IViewModel vm)
        {
            await MainThread2.InvokeOnMainThreadAsync(async () =>
            {
                TextBoxWindowViewModel modelvm = new()
                {
                    Title = AppResources.LoginInputManualLoginToken,
                    InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox
                };
                await TextBoxWindowViewModel.ShowDialogAsync(modelvm);
                if (string.IsNullOrWhiteSpace(modelvm.Value))
                {
                    if (modelvm.Value != null)
                    {
                        Toast.Show(AppResources.Login_ManualLoginEmpt);
                    }
                    return;
                }
                else
                {
                    await OnMessage(vm, modelvm.Value, null);
                }
            });
        }

        /// <summary>
        /// 当接收到 WebSocket Client 发送的消息时
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static async Task OnMessage(this IViewModel vm, string msg, IWebSocketConnection? socket)
        {
            if (vm.TempAes == null) return;
            var byteArray = msg.Base64UrlDecodeToByteArray();
            try
            {
                byteArray = vm.TempAes.Decrypt(byteArray);
            }
            catch
            {
                if (socket == null)
                    Toast.Show(AppResources.Login_WebSocketOnMessage);
                return;
            }
            var response = ApiResponse.Deserialize<LoginOrRegisterResponse>(byteArray);
            var webResponse = new FastLRBWebResponse();
            if (response.IsSuccess && response.Content == null)
            {
                webResponse.Msg = ApiResponse.GetMessage(ApiResponseCode.NoResponseContent);
                if (socket != null)
                    await socket.Send(JsonSerializer.Serialize(webResponse));
                else
                    Toast.Show(webResponse.Msg);
                return;
            }
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            webResponse.State = response.IsSuccess;
            if (socket != null)
                await socket.Send(JsonSerializer.Serialize(webResponse)); // 仅可在 close 之前传递消息
            else
                Toast.Show(webResponse.Msg);
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
                            vm.OnBindSuccessed();
                        }
                        else
                        {
                            msg = "Account bind fail, unknown channel.";
                        }
                        Toast.Show(msg);
                    });
                }
                else
                {
                    await conn_helper.OnLoginedAsync(response.Content!, response.Content!);
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        await LoginOrRegisterWindowViewModel.SuccessAsync(response.Content!, vm.Close);
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

        internal static bool IsPortOccupedFun(int port)
        {
            IPGlobalProperties iproperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = iproperties.GetActiveTcpListeners();
            return ipEndPoints.Any(x => x.Port == port);
        }

        /// <summary>
        /// 开始第三方快速登录、注册、绑定
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static async Task StartLRBAsync(this IViewModel vm, FastLoginChannel channel)
        {
            if (vm.WebSocketServer == null)
            {
                var random = new Random();
                int port = random.Next(10000, 25564);
                while (IsPortOccupedFun(port))
                {
                    port = random.Next(10000, 25564);
                }
                vm.WebSocketServer = new($"ws://{IPAddress.Loopback}:{port}");
                vm.WebSocketServer.AddTo(vm);
                vm.ServerWebSocketListenerPort = port;
                vm.WebSocketServer.Start(socket =>
                {
                    socket.OnMessage = async message => await vm.OnMessage(message, socket);
                });
            }
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            var apiBaseUrl = ICloudServiceClient.Instance.ApiBaseUrl;
#if DEBUG
            if (UseLoopbackTest) apiBaseUrl = "https://127.0.0.1:28110";
#endif

            vm.TempAes.RemoveTo(vm);
            vm.TempAes = AESUtils.Create();
            vm.TempAes.AddTo(vm);
            var skey_bytes = vm.TempAes.ToParamsByteArray();
            var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
            var csc = DI.Get<CloudServiceClientBase>();
            var padding = RSAUtils.DefaultPadding;
            var isBind = vm.IsBind;
            var access_token = string.Empty;
            var access_token_expires = string.Empty;
            if (isBind)
            {
                var authToken = await conn_helper.Auth.GetAuthTokenAsync();
                var authHeaderValue = conn_helper.GetAuthenticationHeaderValue(authToken);
                if (authHeaderValue != null)
                {
                    var authHeaderValueStr = authHeaderValue.ToString();
                    access_token = vm.TempAes.Encrypt(authHeaderValueStr);
                    var now = DateTime.UtcNow;
                    access_token_expires = vm.TempAes.Encrypt(now.ToString(DateTimeFormat.RFC1123));
                }
            }
            var url = $"{apiBaseUrl}/ExternalLoginDetection/{(int)channel}?port={vm.ServerWebSocketListenerPort}&sKey={skey_str}&sKeyPadding={padding.OaepHashAlgorithm}&version={csc.Settings.AppVersionStr}&isBind={isBind}&access_token_expires={access_token_expires}&access_token={access_token}";
            await Browser2.OpenAsync(url);
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    partial class LoginOrRegisterWindowViewModel : IViewModel
    {
        WebSocketServer? IViewModel.WebSocketServer { get; set; }

        int IViewModel.ServerWebSocketListenerPort { get; set; }

        Aes? IViewModel.TempAes { get; set; }

        bool IViewModel.IsBind => false;
    }

    partial class UserProfileWindowViewModel : IViewModel
    {
        WebSocketServer? IViewModel.WebSocketServer { get; set; }

        int IViewModel.ServerWebSocketListenerPort { get; set; }

        Aes? IViewModel.TempAes { get; set; }

        bool IViewModel.IsBind => true;

        void IViewModel.OnBindSuccessed()
        {
            HideFastLoginLoading();
            OnBindSuccessedRefreshUser();
        }
    }
}
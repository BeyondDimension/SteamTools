using Fleck;
using ReactiveUI;
using System.Application.Models;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Application.UI.ViewModels.ThirdPartyLoginHelper;
using _ThisAssembly = System.Properties.ThisAssembly;

namespace System.Application.UI.ViewModels
{
    /// <summary>
    /// 第三方快速登录助手类
    /// </summary>
    public static class ThirdPartyLoginHelper
    {
#if DEBUG
        static readonly bool UseLoopbackTest = false;
#endif

        public interface IBindWindowViewModel
        {
            void OnBindSuccessed();
        }

        public static ICommand ManualLogin { get; }
            = ReactiveCommand.CreateFromTask(ManualLoginAsync);

        /// <summary>
        /// 手动复制字符串登录
        /// </summary>
        /// <returns></returns>
        static async Task ManualLoginAsync()
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
                    await OnMessage(modelvm.Value, null);
                }
            });
        }

        /// <summary>
        /// 当接收到 WebSocket Client 发送的消息时
        /// </summary>
        /// <param name="host"></param>
        /// <param name="msg"></param>
        /// <param name="socket"></param>
        /// <returns></returns>
        static async Task OnMessage(string msg, IWebSocketConnection? socket)
        {
            if (tempAes == null) return;
            var byteArray = msg.Base64UrlDecodeToByteArray();
            try
            {
                byteArray = tempAes.Decrypt(byteArray);
            }
            catch
            {
                if (socket == null)
                    Toast.Show(AppResources.Login_WebSocketOnMessage);
                return;
            }
            var rsp = ApiResponse.Deserialize<LoginOrRegisterResponse>(byteArray);
            var webRsp = new WebResponseDTO();
            if (rsp.IsSuccess && rsp.Content == null)
            {
                webRsp.Msg = ApiResponse.GetMessage(ApiResponseCode.NoResponseContent);
                if (socket != null)
                    await socket.Send(JsonSerializer.Serialize(webRsp));
                else
                    Toast.Show(webRsp.Msg);
                return;
            }
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            webRsp.State = rsp.IsSuccess;
            if (socket != null)
                await socket.Send(JsonSerializer.Serialize(webRsp)); // 仅可在 close 之前传递消息
            else
                Toast.Show(webRsp.Msg);
            if (rsp.IsSuccess)
            {
                if (isBind)
                {
                    var chan = rsp.Content?.FastLRBChannel;
                    if (!chan.HasValue) return;
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        string msg;
                        if (chan.HasValue)
                        {
                            msg = AppResources.Success_.Format(AppResources.User_AccountBind);
                            await UserService.Current.
                                BindAccountAfterUpdateAsync(chan.Value, rsp.Content!);
                            if (vm is IBindWindowViewModel vm2)
                            {
                                vm2.OnBindSuccessed();
                            }
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
                    await conn_helper.OnLoginedAsync(rsp.Content!, rsp.Content!);
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        await LoginOrRegisterWindowViewModel.
                            SuccessAsync(rsp.Content!, () => vm?.Close());
                    });
                }
            }
            else
            {
                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    vm?.Close();
                    conn_helper.ShowResponseErrorMessage(rsp);
                });
            }
        }

        static void StartServer(IApplication app)
        {
            if (ws != null) return;
            var ip = IPAddress.Loopback;
            port = SocketHelper.GetRandomUnusedPort(ip);
            ws = new($"ws://{ip}:{port}");
            if (app is IDisposableHolder dh)
            {
                Disposable.Create(() =>
                {
                    ws?.Dispose();
                    ws = null;
                    tempAes?.Dispose();
                    tempAes = null;
                }).AddTo(dh);
            }
            ws.Start(socket =>
            {
                socket.OnMessage = async message => await OnMessage(message, socket);
            });
        }

        static Aes? tempAes;
        static bool isBind;
        static WindowViewModel? vm;

        /// <summary>
        /// 当前 WebSocket 服务，由应用程序托管生命周期，使用时检查是否创建，接收到回调后关闭，页面销毁时不释放此服务，仅程序退出时释放，因在 Android 上跳转网页，之前的活动可能被销毁
        /// </summary>
        static WebSocketServer? ws;

        /// <summary>
        /// 当前 WebSocket 服务监听端口号
        /// </summary>
        static int port;

        /// <summary>
        /// 开始第三方快速登录、注册、绑定
        /// </summary>
        /// <param name="vm"></param>
        /// <param name="channel"></param>
        /// <returns></returns>
        public static async Task StartAsync(WindowViewModel vm, FastLoginChannel channel, bool isBind)
        {
            var app = IApplication.Instance;
            StartServer(app);
            var conn_helper = DI.Get<IApiConnectionPlatformHelper>();
            var apiBaseUrl = ICloudServiceClient.Instance.ApiBaseUrl;
#if DEBUG
            if (UseLoopbackTest) apiBaseUrl = "https://127.0.0.1:28110";
#endif

            ThirdPartyLoginHelper.isBind = isBind;
            ThirdPartyLoginHelper.vm = vm;
            Disposable.Create(() =>
            {
                if (vm == ThirdPartyLoginHelper.vm)
                {
                    ThirdPartyLoginHelper.vm = null;
                }
            }).AddTo(vm);
            if (tempAes == null) tempAes = AESUtils.Create(); // 每次创建新的之前的会失效
            var skey_bytes = tempAes.ToParamsByteArray();
            var skey_str = conn_helper.RSA.EncryptToString(skey_bytes);
            var csc = DI.Get<CloudServiceClientBase>();
            var padding = RSAUtils.DefaultPadding;
            var access_token = string.Empty;
            var access_token_expires = string.Empty;
            if (isBind)
            {
                var authToken = await conn_helper.Auth.GetAuthTokenAsync();
                var authHeaderValue = conn_helper.GetAuthenticationHeaderValue(authToken);
                if (authHeaderValue != null)
                {
                    var authHeaderValueStr = authHeaderValue.ToString();
                    access_token = tempAes.Encrypt(authHeaderValueStr);
                    var now = DateTime.UtcNow;
                    access_token_expires = tempAes.Encrypt(now.ToString(DateTimeFormat.RFC1123));
                }
            }
            // &version={version}
            //var version = csc.Settings.AppVersionStr;
            var ver = _ThisAssembly.Version.Base64UrlEncode();
            var url = $"{apiBaseUrl}/ExternalLoginDetection/{(int)channel}?port={port}&sKey={skey_str}&sKeyPadding={padding.OaepHashAlgorithm}&ver={ver}&isBind={isBind}&access_token_expires={access_token_expires}&access_token={access_token}";
            await Browser2.OpenAsync(url);
        }

        /// <summary>
        /// 用于 WebSocket 传递给 Web 端的响应数据模型
        /// </summary>
        public sealed class WebResponseDTO
        {
            public bool State { get; set; }

            public string Msg { get; set; } = string.Empty;
        }

        public static FastLoginChannel[] FastLoginChannels = new[] // 更改此数组可控制UI列表的顺序
        {
            FastLoginChannel.QQ,
            FastLoginChannel.Steam,
            FastLoginChannel.Microsoft,
            FastLoginChannel.Apple,
        };
    }

    partial class UserProfileWindowViewModel : IBindWindowViewModel
    {
        void IBindWindowViewModel.OnBindSuccessed()
        {
            HideFastLoginLoading();
            OnBindSuccessedRefreshUser();
        }
    }
}
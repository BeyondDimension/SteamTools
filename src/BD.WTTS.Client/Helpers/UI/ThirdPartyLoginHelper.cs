using Fleck;
using AppResources = BD.WTTS.Client.Resources.Strings;
using JsonSerializer = System.Text.Json.JsonSerializer;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

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
                InputType = TextBoxWindowViewModel.TextBoxInputType.TextBox,
            };
            await TextBoxWindowViewModel.ShowDialogAsync(modelvm);
            if (string.IsNullOrWhiteSpace(modelvm.Value))
            {
                if (modelvm.Value != null)
                {
                    Toast.Show(ToastIcon.Warning, AppResources.Login_ManualLoginEmpt);
                }
                return;
            }
            else
            {
                await LoginForStr(modelvm.Value);
            }
        });
    }

    /// <summary>
    /// 当接收到 WebSocket Client 发送的消息时
    /// </summary>
    /// <param name="msg"></param>
    /// <param name="socket"></param>
    /// <returns></returns>
    public static async Task LoginForStr(string msg, IWebSocketConnection? socket = null)
    {
        if (tempAes == null) return;
        if (string.IsNullOrWhiteSpace(msg)) return;
        var conn_helper = Ioc.Get<IApiConnectionPlatformHelper>();
        try
        {
            var byteArray = msg.Base64UrlDecodeToByteArray();
            try
            {
                byteArray = tempAes.Decrypt(byteArray);
            }
            catch
            {
                Toast.Show(ToastIcon.Error, AppResources.Login_WebSocketOnMessage);
                return;
            }
            var rsp = ApiRspHelper.Deserialize<LoginOrRegisterResponse>(byteArray);
            var webRsp = new WebResponseDTO();
            if (rsp.IsSuccess && rsp.Content == null)
            {
                webRsp.Msg = ApiRspExtensions.GetMessage(ApiRspCode.NoResponseContent);
                if (socket != null)
                    await socket.Send(JsonSerializer.Serialize(webRsp));
                else
                    Toast.Show(ToastIcon.None, webRsp.Msg);
                return;
            }
            webRsp.State = rsp.IsSuccess;
            if (socket != null)
                await socket.Send(JsonSerializer.Serialize(webRsp)); // 仅可在 close 之前传递消息
            else
                Toast.Show(ToastIcon.None, webRsp.Msg);
            if (rsp.IsSuccess)
            {
                if (isBind)
                {
                    var chan = rsp.Content?.ExternalLoginChannel;
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
                        Toast.Show(ToastIcon.None, msg);
                    });
                }
                else
                {
                    await conn_helper.OnLoginedAsync(rsp.Content!, rsp.Content!);
                    await MainThread2.InvokeOnMainThreadAsync(async () =>
                    {
                        await LoginOrRegisterSuccessAsync(rsp.Content!, () => vm?.Close(false));
                    });
                }
            }
            else
            {
                await MainThread2.InvokeOnMainThreadAsync(() =>
                {
                    vm?.Close(false);
                    conn_helper.ShowResponseErrorMessage(null, rsp);
                });
            }
        }
        catch (Exception ex)
        {
            var rsp = ApiRspHelper.Exception(ex);
            await MainThread2.InvokeOnMainThreadAsync(() =>
            {
                vm?.Close(false);
                conn_helper.ShowResponseErrorMessage(null, rsp);
            });
        }
    }

    public static async Task LoginOrRegisterSuccessAsync(
        LoginOrRegisterResponse rsp,
        Action? close = null)
    {
        await UserService.Current.RefreshUserAsync();
        var msg = AppResources.Success_.Format((rsp?.IsLoginOrRegister ?? false) ?
            AppResources.User_Login :
            AppResources.User_Register);
        close?.Invoke();
        Toast.Show(ToastIcon.Success, msg);
        UserService.Current.RefreshShopToken();
    }

    static IDisposable? serverDisposable;

    static void StartServer(IApplication app)
    {
        if (ws != null) return;
        var ip = IPAddress.Loopback;
        port = SocketHelper.GetRandomUnusedPort(ip);
        ws = new($"ws://{ip}:{port}");

        serverDisposable?.Dispose();
        serverDisposable = Disposable.Create(() =>
        {
            ws?.Dispose();
            ws = null;
            tempAes?.Dispose();
            tempAes = null;
        });

        if (app is IDisposableHolder dh)
        {
            serverDisposable.AddTo(dh);
        }

        ws.Start(socket =>
        {
            socket.OnMessage = async message => await LoginForStr(message, socket);
        });
    }

    /// <summary>
    /// 释放 WebSocket Server 占用资源
    /// </summary>
    public static void DisposeServer()
    {
        serverDisposable?.Dispose();
        serverDisposable = null;
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
    /// <param name="isBind"></param>
    /// <returns></returns>
    public static async Task StartAsync(WindowViewModel vm, ExternalLoginChannel channel, bool isBind)
    {
        var app = IApplication.Instance;
        if (!OperatingSystem2.IsAndroid() && !OperatingSystem2.IsIOS() && !OperatingSystem2.IsMacOS())
        {
            // Android/iOS 使用 URL Scheme 回调
            StartServer(app);
        }
        var conn_helper = Ioc.Get<IApiConnectionPlatformHelper>();
        var apiBaseUrl = IMicroServiceClient.Instance.ApiBaseUrl;
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
        tempAes ??= AESUtils.Create(); // 每次创建新的之前的会失效
        var skey_bytes = tempAes.ToParamsByteArray();
        var skey_str = conn_helper.RSA.EncryptToHexString(skey_bytes);
        var csc = Ioc.Get<IMicroServiceClient>();
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
                access_token = tempAes.EncryptHex(authHeaderValueStr);
                var now = DateTime.UtcNow;
                access_token_expires = tempAes.EncryptHex(now.ToString(DateTimeFormat.RFC1123));
            }
        }
        // &version={version}
        //var version = csc.Settings.AppVersionStr;
        var ver = AssemblyInfo.Version.Base64UrlEncode();
        var qs = HttpUtility.ParseQueryString("");
        qs.Add("port", port.ToString());
        qs.Add("sKeyHex", skey_str);
        qs.Add("sKeyPadding", padding.OaepHashAlgorithm.ToString());
        qs.Add("ver", ver);
        qs.Add("isBind", isBind.ToString());
        qs.Add("access_token_expires_hex", access_token_expires);
        qs.Add("access_token_hex", access_token);
        qs.Add("dg", DeviceIdHelper.DeviceIdG.ToStringN());
        qs.Add("dr", DeviceIdHelper.DeviceIdR);
        qs.Add("dn", DeviceIdHelper.DeviceIdN);
        if (OperatingSystem2.IsMacOS())
        {
            qs.Add("isUS", "true");
        }
        var ub = new UriBuilder(apiBaseUrl)
        {
            Path = $"/identity/v1/externallogin/{(int)channel}",
            Query = qs.ToString(),
        };

        await Browser2.OpenAsync(ub.Uri, BrowserLaunchMode.External);
    }

    /// <summary>
    /// 用于 WebSocket 传递给 Web 端的响应数据模型
    /// </summary>
    public sealed class WebResponseDTO
    {
        public bool State { get; set; }

        public string Msg { get; set; } = string.Empty;
    }

    public static readonly ExternalLoginChannel[] ExternalLoginChannels = new[] // 更改此数组可控制UI列表的顺序
    {
            ExternalLoginChannel.QQ,
            ExternalLoginChannel.Steam,
            ExternalLoginChannel.Microsoft,
            ExternalLoginChannel.Apple,
    };
}
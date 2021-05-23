using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Application.UI.Resx;
using System.IO;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class LoginOrRegisterWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
    {
        public LoginOrRegisterWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
            FastLogin = ReactiveCommand.CreateFromTask<string>(async channel =>
            {
                if (Enum.TryParse<FastLoginChannel>(channel, out var channel_))
                    await FastLoginOrRegisterAsync(Close, channel_);
            });

            SteamConnectService.Current.WhenAnyValue(x => x.CurrentSteamUser)
                .Subscribe(_ => this.RaisePropertyChanged(nameof(SteamUser)));
        }

        private string? _PhoneNumber;
        public string? PhoneNumber
        {
            get => _PhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _PhoneNumber, value);
        }

        private string? _SmsCode;
        public string? SmsCode
        {
            get => _SmsCode;
            set => this.RaiseAndSetIfChanged(ref _SmsCode, value);
        }

        private int _TimeLimit = SMSInterval;
        public int TimeLimit
        {
            get => _TimeLimit;
            set
            {
                this.RaiseAndSetIfChanged(ref _TimeLimit, value);
                this.RaisePropertyChanged(nameof(IsUnTimeLimit));
            }
        }

        string _BtnSendSmsCodeText = AppResources.User_GetSMSCode;
        public string BtnSendSmsCodeText
        {
            get => _BtnSendSmsCodeText;
            set => this.RaiseAndSetIfChanged(ref _BtnSendSmsCodeText, value);
        }

        public bool IsUnTimeLimit
        {
            get => TimeLimit != SMSInterval;
        }

        public bool SendSmsCodeSuccess { get; set; }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        short _LoginState = 2;
        public short LoginState
        {
            get => _LoginState;
            set => this.RaiseAndSetIfChanged(ref _LoginState, value);
        }

        public SteamUser? SteamUser { get; } = SteamConnectService.Current.CurrentSteamUser;

        public async void Submit()
        {
            if (IsLoading || !this.CanSubmit()) return;

            var request = new LoginOrRegisterRequest
            {
                PhoneNumber = PhoneNumber,
                SmsCode = SmsCode
            };
            IsLoading = true;

            var response = await ICloudServiceClient.Instance.Account.LoginOrRegister(request);

            if (response.IsSuccess)
            {
                await SuccessAsync(response.Content!, Close);
                return;
            }

            IsLoading = false;
        }

        static async Task SuccessAsync(LoginOrRegisterResponse rsp, Action? close)
        {
            await UserService.Current.RefreshUserAsync();
            close?.Invoke();
            var msg = AppResources.Success_.Format((rsp?.IsLoginOrRegister ?? false) ? AppResources.User_Login : AppResources.User_Register);
            Toast.Show(msg);
        }

        public void ChangeLoginState(short state)
        {
            LoginState = state;
        }

        internal static async Task FastLoginOrRegisterAsync(Action? close = null, FastLoginChannel channel = FastLoginChannel.Steam, bool isBind = false)
        {
            var apiBaseUrl = ICloudServiceClient.Instance.ApiBaseUrl;
            var urlExternalLoginCallback = apiBaseUrl + "/ExternalLoginCallback";
            if (isBind) urlExternalLoginCallback += "?isBind=true";
            WebView3WindowViewModel? vm = null;
            vm = new WebView3WindowViewModel
            {
                Url = apiBaseUrl + (channel == FastLoginChannel.Steam ? "/ExternalLogin" : $"/ExternalLogin/{(int)channel}"),
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
                        await conn_helper.OnLoginedAsync(null, response.Content!);
                        await MainThreadDesktop.InvokeOnMainThreadAsync(async () =>
                        {
                            await SuccessAsync(response.Content!, vm?.Close);
                        });
                    }
                    else
                    {
                        MainThreadDesktop.BeginInvokeOnMainThread(() =>
                        {
                            close?.Invoke();
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

        public Action? Close { get; set; }

        public Action? TbPhoneNumberFocus { get; set; }

        public Action? TbSmsCodeFocus { get; set; }

        public CancellationTokenSource? CTS { get; set; }

        public async void SendSms()
        {
            if (this.TimeStart())
            {
                var request = new SendSmsRequest
                {
                    PhoneNumber = PhoneNumber,
                    Type = SmsCodeType.LoginOrRegister,
                };

#if DEBUG
                var response =
#endif
                await this.SendSms(request);
            }
        }

        public void OpenHyperlink(string parameter)
        {
            //BrowserOpen(parameter);
            IShowWindowService.Instance.Show(CustomWindow.WebView3, new WebView3WindowViewModel
            {
                Url = parameter,
            }, resizeMode: ResizeModeCompat.NoResize);
        }

        public ICommand FastLogin { get; }

        public FastLoginChannel[] FastLoginChannels { get; } = new[] {
            FastLoginChannel.Steam,
            FastLoginChannel.QQ,
            FastLoginChannel.Xbox,
            FastLoginChannel.Apple,
        };

        //int _FastLoginChannelsSelectedIndex = -1;
        //public int FastLoginChannelsSelectedIndex
        //{
        //    get => _FastLoginChannelsSelectedIndex;
        //    set
        //    {
        //        if (value > -1 && value < FastLoginChannels.Length)
        //        {
        //            var item = FastLoginChannels[value].ToString();
        //            if (FastLogin.CanExecute(item))
        //            {
        //                FastLogin.Execute(item);
        //            }
        //        }
        //        this.RaiseAndSetIfChanged(ref _FastLoginChannelsSelectedIndex, value);
        //        this.RaiseAndSetIfChanged(ref _FastLoginChannelsSelectedIndex, -1);
        //    }
        //}

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CTS?.Cancel();
            }
            base.Dispose(disposing);
        }
    }
}
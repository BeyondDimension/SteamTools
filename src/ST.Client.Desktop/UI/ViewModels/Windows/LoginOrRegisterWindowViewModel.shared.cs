using ReactiveUI;
using System.Application.Models;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xamarin.Essentials;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public partial class
#if __MOBILE__
        LoginOrRegisterPageViewModel
#else
        LoginOrRegisterWindowViewModel
#endif
        : SendSmsUIHelper.IViewModel
    {
        public
#if __MOBILE__
        LoginOrRegisterPageViewModel
#else
        LoginOrRegisterWindowViewModel
#endif
            () : base()
        {
            Title =
#if !__MOBILE__
                ThisAssembly.AssemblyTrademark + " | " +
#endif
                AppResources.LoginAndRegister;

            ChooseChannel = ReactiveCommand.CreateFromTask<string>(async channel =>
            {
                if (Enum.TryParse<FastLoginChannel>(channel, out var channel_))
                {
                    await FastLoginOrRegisterAsync(Close, channel_);
                }
                else
                {
                    switch (channel)
                    {
                        case FastLoginChannelViewModel.PhoneNumber:
                            await GoToLoginOrRegisterByPhoneNumberAsync();
                            break;
                    }
                }
            });
            SendSms = ReactiveCommand.CreateFromTask(SendSmsAsync);
            Submit = ReactiveCommand.CreateFromTask(SubmitAsync);
            OpenHyperlink = ReactiveCommand.Create<string>(OpenHyperlink_);

            //SteamConnectService.Current.WhenAnyValue(x => x.CurrentSteamUser)
            //    .Subscribe(_ => this.RaisePropertyChanged(nameof(SteamUser)));

            fastLoginChannels = new()
            {
                FastLoginChannelViewModel.Create(nameof(FastLoginChannel.Steam), this),
                FastLoginChannelViewModel.Create(nameof(FastLoginChannel.Xbox), this),
#if DEBUG
                FastLoginChannelViewModel.Create(nameof(FastLoginChannel.Apple), this),
                FastLoginChannelViewModel.Create(nameof(FastLoginChannel.QQ), this),
#endif
#if __MOBILE__
                FastLoginChannelViewModel.Create(FastLoginChannelViewModel.PhoneNumber, this),
#endif
            };
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
//#if __MOBILE__
//                OnIsUnTimeLimitChanged?.Invoke();
//#endif
            }
        }

        public static string DefaultBtnSendSmsCodeText => AppResources.User_GetSMSCode;

        string _BtnSendSmsCodeText = DefaultBtnSendSmsCodeText;
        public string BtnSendSmsCodeText
        {
            get => _BtnSendSmsCodeText;
            set => this.RaiseAndSetIfChanged(ref _BtnSendSmsCodeText, value);
        }

//#if __MOBILE__
//        public Action? OnIsUnTimeLimitChanged { get; set; }
//#endif

        public bool IsUnTimeLimit => TimeLimit != SMSInterval;

        public bool SendSmsCodeSuccess { get; set; }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        const short _LoginStateDefault = 2;
        short _LoginState = _LoginStateDefault;
        public short LoginState
        {
            get => _LoginState;
            set
            {
                //this.RaiseAndSetIfChanged(ref _LoginState, value);
                if (_LoginState == value) return;
                _LoginState = value;
                this.RaisePropertyChanged();
#if __MOBILE__
                Title = _LoginState == 1 ? AppResources.User_PhoneLogin : AppResources.LoginAndRegister;
#endif
            }
        }

        //public SteamUser? SteamUser { get; } = SteamConnectService.Current.CurrentSteamUser;

        public ICommand Submit { get; }

        async Task SubmitAsync()
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

#if !__MOBILE__
        public void ChangeLoginState(short state)
        {
            LoginState = state;
        }
#endif

        public new Action? Close { get; set; }

        public Action? TbPhoneNumberFocus { get; set; }

        public Action? TbSmsCodeFocus { get; set; }

        public CancellationTokenSource? CTS { get; set; }

        public ICommand SendSms { get; }

        async Task SendSmsAsync() => await this.SendSmsAsync(() => new()
        {
            PhoneNumber = PhoneNumber,
            Type = SmsCodeType.LoginOrRegister,
        });

        public ICommand OpenHyperlink { get; }

        public const string Agreement = "Agreement";
        public const string Privacy = "Privacy";

        void OpenHyperlink_(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter)) return;
            switch (parameter)
            {
                case Agreement:
                    parameter = UrlConstants.OfficialWebsite_Box_Agreement;
                    break;
                case Privacy:
                    parameter = UrlConstants.OfficialWebsite_Box_Privacy;
                    break;
            }
#if __MOBILE__
            BrowserOpen(parameter);
#else
            if (AppHelper.IsSystemWebViewAvailable)
            {
                IShowWindowService.Instance.Show(CustomWindow.WebView3, new WebView3WindowViewModel
                {
                    Url = parameter,
                }, resizeMode: ResizeModeCompat.NoResize);
            }
            else
            {
                BrowserOpen(parameter);
            }
#endif
        }

        /// <summary>
        /// 选择快速登录渠道点击命令，参数类型为 <see cref="FastLoginChannelViewModel"/>.Id
        /// </summary>
        public ICommand ChooseChannel { get; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CTS?.Cancel();
            }
            base.Dispose(disposing);
        }

        ObservableCollection<FastLoginChannelViewModel> fastLoginChannels;
        /// <summary>
        /// 快速登录渠道组
        /// </summary>
        public ObservableCollection<FastLoginChannelViewModel> FastLoginChannels
        {
            get => fastLoginChannels;
            set => this.RaiseAndSetIfChanged(ref fastLoginChannels, value);
        }

        public sealed class FastLoginChannelViewModel : RIdTitleIconViewModel<string, ResIcon>
        {
            FastLoginChannelViewModel()
            {
            }

            Color iconBgColor;
            public Color IconBgColor
            {
                get => iconBgColor;
                set => this.RaiseAndSetIfChanged(ref iconBgColor, value);
            }

            protected override void OnIdChanged(string? value)
            {
                IconBgColor = GetIconBgColorById(value);
            }

            public const string PhoneNumber = nameof(PhoneNumber);

            protected override ResIcon GetIconById(string? id)
            {
                return id switch
                {
                    nameof(FastLoginChannel.Steam) => ResIcon.Steam,
                    nameof(FastLoginChannel.Xbox) or
                    nameof(FastLoginChannel.Microsoft) => ResIcon.Xbox,
                    nameof(FastLoginChannel.Apple) => ResIcon.Apple,
                    nameof(FastLoginChannel.QQ) => ResIcon.QQ,
                    PhoneNumber => ResIcon.Phone,
                    _ => ResIcon.none,
                };
            }

            protected override string GetTitleById(string? id)
            {
                if (id == PhoneNumber)
                {
                    return AppResources.User_UsePhoneNumLoginChannel;
                }
                else if (!string.IsNullOrWhiteSpace(id))
                {
                    if (id == nameof(FastLoginChannel.Xbox) ||
                        id == nameof(FastLoginChannel.Microsoft))
                    {
                        id = "Xbox Live";
                    }
                    return AppResources.User_UseFastLoginChannel_.Format(id);
                }
                return string.Empty;
            }

            static Color GetIconBgColorById(string? id)
            {
                var hexColor = id switch
                {
                    nameof(FastLoginChannel.Steam) => "#145c8f",
                    nameof(FastLoginChannel.Xbox) or
                    nameof(FastLoginChannel.Microsoft) => "#027d00",
                    nameof(FastLoginChannel.Apple) => "#000000",
                    nameof(FastLoginChannel.QQ) => "#12B7F5",
                    PhoneNumber => "#2196F3",
                    _ => default,
                };
                return hexColor == default ? default : ColorConverters.FromHex(hexColor);
            }

            /// <summary>
            /// 创建实例
            /// </summary>
            /// <param name="id"></param>
            /// <param name="vm"></param>
            /// <returns></returns>
            public static FastLoginChannelViewModel Create(string id, IDisposableHolder vm)
            {
                FastLoginChannelViewModel r = new() { Id = id, };
                r.OnBind(vm);
                return r;
            }
        }

        /// <inheritdoc cref="SendSmsUIHelper.RemoveAllDelegate(SendSmsUIHelper.IViewModel)"/>
        public void RemoveAllDelegate()
        {
            Close = null;
            SendSmsUIHelper.IViewModel i = this;
            i.RemoveAllDelegate();
        }
    }
}
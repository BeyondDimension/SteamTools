using ReactiveUI;
using System.Application.Models;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using static System.Application.Services.CloudService.Constants;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class LoginOrRegisterWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
    {
        public static string DisplayName => AppResources.LoginAndRegister;

        public LoginOrRegisterWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);

            ChooseChannel = ReactiveCommand.CreateFromTask<string>(async channel_ =>
            {
                if (Enum.TryParse<FastLoginChannel>(channel_, out var channel))
                {
                    CurrentSelectChannel = channel_;
                    ChangeLoginState(3);
                    await ThirdPartyLoginHelper.StartAsync(this, channel, isBind: false);
                }
                else
                {
                    switch (channel_)
                    {
                        case FastLoginChannelViewModel.PhoneNumber:
                            ChangeLoginState(1);
                            break;
                    }
                }
            });
            ManualLogin = ThirdPartyLoginHelper.ManualLogin;
            SendSms = ReactiveCommand.CreateFromTask(SendSmsAsync);
            Submit = ReactiveCommand.CreateFromTask(SubmitAsync);
            OpenHyperlink = ReactiveCommand.Create<string>(OpenHyperlink_);

            //SteamConnectService.Current.WhenAnyValue(x => x.CurrentSteamUser)
            //    .Subscribe(_ => this.RaisePropertyChanged(nameof(SteamUser)));

            fastLoginChannels = new(GetFastLoginChannels());
        }

        IEnumerable<FastLoginChannelViewModel> GetFastLoginChannels()
        {
            foreach (var item in ThirdPartyLoginHelper.FastLoginChannels)
            {
                if (item.IsSupported())
                {
                    yield return FastLoginChannelViewModel.Create(item switch
                    {
                        FastLoginChannel.Microsoft => nameof(FastLoginChannel.Xbox),
                        _ => item.ToString(),
                    }, this);
                }
            }

            if (!IApplication.IsDesktopPlatform)
            {
                yield return FastLoginChannelViewModel.Create(FastLoginChannelViewModel.PhoneNumber, this);
            }
        }

        public ICommand ManualLogin { get; }

        private string? _CurrentSelectChannel;

        public string? CurrentSelectChannel
        {
            get => _CurrentSelectChannel;
            set => this.RaiseAndSetIfChanged(ref _CurrentSelectChannel, value);
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

        public static string DefaultBtnSendSmsCodeText => AppResources.User_GetSMSCode;

        string _BtnSendSmsCodeText = DefaultBtnSendSmsCodeText;

        public string BtnSendSmsCodeText
        {
            get => _BtnSendSmsCodeText;
            set => this.RaiseAndSetIfChanged(ref _BtnSendSmsCodeText, value);
        }

        public bool IsUnTimeLimit => TimeLimit != SMSInterval;

        public bool SendSmsCodeSuccess { get; set; }

        bool _IsLoading;

        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        bool _IsFastLogin;

        public bool IsFastLogin
        {
            get => _IsFastLogin;
            set => this.RaiseAndSetIfChanged(ref _IsFastLogin, value);
        }

        const short _LoginStateDefault = 2;
        short _LoginState = _LoginStateDefault;

        public short LoginState
        {
            get => _LoginState;
            set
            {
                if (_LoginState == value) return;
                _LoginState = value;
                this.RaisePropertyChanged();
                if (!IApplication.IsDesktopPlatform)
                {
                    Title = _LoginState == 1 ? AppResources.User_PhoneLogin : AppResources.LoginAndRegister;
                }
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

        internal static async Task SuccessAsync(LoginOrRegisterResponse rsp, Action? close = null)
        {
            await UserService.Current.RefreshUserAsync();
            var msg = AppResources.Success_.Format((rsp?.IsLoginOrRegister ?? false) ? AppResources.User_Login : AppResources.User_Register);
            close?.Invoke();
            Toast.Show(msg);
        }

        void ChangeLoginState(short state)
        {
            LoginState = state;
        }

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

        async void OpenHyperlink_(string parameter)
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
            await Browser2.OpenAsync(parameter);
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
                    _ => ResIcon.None,
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

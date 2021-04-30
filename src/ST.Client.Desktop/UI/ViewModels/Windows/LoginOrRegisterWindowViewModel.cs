using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;
using System.Threading;
using System.Windows;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class LoginOrRegisterWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
    {
        public LoginOrRegisterWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
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
                await UserService.Current.RefreshUserAsync();
                var msg = AppResources.Success_.Format((response.Content?.IsLoginOrRegister ?? false) ? AppResources.User_Login : AppResources.User_Register);
                Toast.Show(msg);
                Close?.Invoke();
                return;
            }

            IsLoading = false;
        }

        public Action? Close { private get; set; }

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

        public void SteamFastLogin()
        {
        }

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
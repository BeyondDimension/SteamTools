using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Diagnostics;
using System.Properties;
using System.Threading;

namespace System.Application.UI.ViewModels
{
    public class LoginUserWindowViewModel : WindowViewModel
    {
        private const int maxTimeLimit = 60;
        private Timer? timer;

        public LoginUserWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
            TimeLimit = maxTimeLimit;
            GetCodeButtonContent = AppResources.User_GetSMSCode;
        }

        void Timer_Elapsed()
        {
            if (TimeLimit == 0)
            {
                TimeLimit = maxTimeLimit;
                //this.RaisePropertyChanged(nameof(IsUnTimeLimit));
                GetCodeButtonContent = AppResources.User_GetSMSCode;
                timer?.Dispose();
            }
            else
            {
                GetCodeButtonContent = string.Format(AppResources.User_LoginCodeTimeLimitTip, TimeLimit);
            }
        }

        private void Timer_Elapsed(object _)
        {
            TimeLimit--;
            Timer_Elapsed();
        }

        private string? _Phone;
        public string? Phone
        {
            get => _Phone;
            set => this.RaiseAndSetIfChanged(ref _Phone, value);
        }

        private string? _SMSCode;
        public string? SMSCode
        {
            get => _SMSCode;
            set => this.RaiseAndSetIfChanged(ref _SMSCode, value);
        }

        private string? _RememberMe;
        public string? RememberMe
        {
            get => _RememberMe;
            set => this.RaiseAndSetIfChanged(ref _RememberMe, value);
        }

        private int _TimeLimit;
        public int TimeLimit
        {
            get => _TimeLimit;
            set
            {
                this.RaiseAndSetIfChanged(ref _TimeLimit, value);
                this.RaisePropertyChanged(nameof(IsUnTimeLimit));
            }
        }

        public string _GetCodeButtonContent = string.Empty;
        public string GetCodeButtonContent
        {
            get => _GetCodeButtonContent;
            set => this.RaiseAndSetIfChanged(ref _GetCodeButtonContent, value);
        }

        public bool IsUnTimeLimit
        {
            get => TimeLimit != maxTimeLimit;
        }

        public bool GetSmsCodeSuccess { get; set; }

        public async void LoginOrRegister()
        {
            if (!GetSmsCodeSuccess)
            {
                ToastService.Current.Notify(AppResources.User_SMSCode_Error);
                return;
            }
            var request = new LoginOrRegisterRequest
            {
                PhoneNumber = Phone,
                SmsCode = SMSCode
            };
#if DEBUG
            var response =
#endif
                await ICloudServiceClient.Instance.Account.LoginOrRegister(request);
        }

        public async void GetSMSCode()
        {
            if (IsUnTimeLimit)
            {
                return;
            }

            timer = new Timer(Timer_Elapsed, null, 0, 1000);
            var client = ICloudServiceClient.Instance;

            var request = new SendSmsRequest
            {
                PhoneNumber = Phone,
                Type = SmsCodeType.LoginOrRegister,
            };
            var response = await client.AuthMessage.SendSms(request);

            if (!response.IsSuccess)
            {
                timer.Dispose();
                timer = null;
                TimeLimit = 0;
                Timer_Elapsed();
                this.RaisePropertyChanged(nameof(IsUnTimeLimit));
                this.RaisePropertyChanged(nameof(GetCodeButtonContent));
            }

            if (!GetSmsCodeSuccess && response.IsSuccess) GetSmsCodeSuccess = true;
        }

        public void OpenHelpLink()
        {
            Process.Start(new ProcessStartInfo { FileName = "https://steampp.net/", UseShellExecute = true });
        }

        public void SteamFastLogin()
        {
        }
    }
}
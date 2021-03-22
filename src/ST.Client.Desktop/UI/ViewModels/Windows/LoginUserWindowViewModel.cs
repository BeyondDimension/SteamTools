using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Diagnostics;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI.ViewModels
{
    public class LoginUserWindowViewModel : WindowViewModel
    {
        private const int maxTimeLimit = 60;
        private Timer timer;

        public LoginUserWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
            TimeLimit = maxTimeLimit;
            GetCodeButtonContent = AppResources.User_GetSMSCode;
        }

        private void Timer_Elapsed(object state)
        {
            TimeLimit--;
            GetCodeButtonContent = string.Format(AppResources.User_LoginCodeTimeLimitTip, TimeLimit);
            if (TimeLimit == 0)
            {
                TimeLimit = maxTimeLimit;
                //this.RaisePropertyChanged(nameof(IsUnTimeLimit));
                GetCodeButtonContent = AppResources.User_GetSMSCode;
                timer.Dispose();
            }
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

        public string _GetCodeButtonContent;
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
            if (string.IsNullOrWhiteSpace(Phone) || string.IsNullOrWhiteSpace(SMSCode))
            {
                ToastService.Current.Notify(AppResources.User_PhoneCode_Error);
                return;
            }
            var request = new LoginOrRegisterRequest
            {
                PhoneNumber = Phone,
                SmsCode = SMSCode
            };
            var response = await ICloudServiceClient.Instance.Account.LoginOrRegister(request);


            if (!response.IsSuccess)
            {
                ToastService.Current.Notify("Fail:" + response.Code);
                return;
            }
        }

        public async void GetSMSCode()
        {
            if (string.IsNullOrWhiteSpace(Phone))
            {
                ToastService.Current.Notify(AppResources.User_Phone_Error);
                return;
            }
            if (IsUnTimeLimit)
            {
                return;
            }

            timer = new Timer(new TimerCallback(Timer_Elapsed), null, 0, 1000);
            var client = ICloudServiceClient.Instance;

            var request = new SendSmsRequest
            {
                PhoneNumber = Phone,
                Type = SmsCodeType.LoginOrRegister,
            };
            var response = await client.AuthMessage.SendSms(request);

            if (response.IsSuccess)
            {
                GetSmsCodeSuccess = true;
            }
            else
            {
                GetSmsCodeSuccess = false;
                ToastService.Current.Notify("Fail:" + response.Code);
                return;
            }

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
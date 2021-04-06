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

        public LoginUserWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
            TimeLimit = maxTimeLimit;
            GetCodeButtonContent = AppResources.User_GetSMSCode;
        }

        bool Timer_Elapsed()
        {
            if (TimeLimit <= 0)
            {
                TimeLimit = maxTimeLimit;
                GetCodeButtonContent = AppResources.User_GetSMSCode;
                return false;
            }
            else
            {
                GetCodeButtonContent = string.Format(AppResources.User_LoginCodeTimeLimitTip, TimeLimit);
                return true;
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

        CancellationTokenSource? cts;

        void TimeStart()
        {
            cts?.Cancel();
            cts = new();
            Task.Run(async () =>
            {
                bool b;
                do
                {
                    TimeLimit--;
                    b = Timer_Elapsed();
#if DEBUG
                    Toast.Show($"TimeLimit: {TimeLimit}");
#endif
                    try
                    {
                        if (b) await Task.Delay(1000, cts.Token);
                    }
                    catch (OperationCanceledException)
                    {
                        if (!_disposed)
                        {
                            TimeLimit = 0;
                            Timer_Elapsed();
                        }
                        break;
                    }
                } while (b);
            });
        }

        public async void GetSMSCode()
        {
            if (IsUnTimeLimit)
            {
                return;
            }

            TimeStart();

            var client = ICloudServiceClient.Instance;

            var request = new SendSmsRequest
            {
                PhoneNumber = Phone,
                Type = SmsCodeType.LoginOrRegister,
            };
            var response = await client.AuthMessage.SendSms(request);

            if (!response.IsSuccess)
            {
                cts?.Cancel();
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                cts?.Cancel();
            }
            base.Dispose(disposing);
        }
    }
}
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class LoginOrRegisterWindowViewModel : WindowViewModel
    {
        public LoginOrRegisterWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.LoginAndRegister;
            TimeLimit = SMSInterval;
            BtnSendSmsCodeText = AppResources.User_GetSMSCode;
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

        public string _BtnSendSmsCodeText = string.Empty;
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
            if (!SendSmsCodeSuccess)
            {
                Toast.Show(AppResources.User_SMSCode_Error);
                return;
            }
            var request = new LoginOrRegisterRequest
            {
                PhoneNumber = PhoneNumber,
                SmsCode = SmsCode
            };
            IsLoading = true;
#if DEBUG
            var response =
#endif
                await ICloudServiceClient.Instance.Account.LoginOrRegister(request);

            if (response.IsSuccess)
            {
                Toast.Show($"{((response.Content?.IsLoginOrRegister ?? false) ? "登录" : "注册")}成功");
                Close?.Invoke();
                return;
            }

            IsLoading = false;
        }

        public Action? Close { private get; set; }

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
                    bool SetBtnSendSmsCodeText()
                    {
                        if (TimeLimit <= 0)
                        {
                            TimeLimit = SMSInterval;
                            BtnSendSmsCodeText = AppResources.User_GetSMSCode;
                            return false;
                        }
                        else
                        {
                            BtnSendSmsCodeText = string.Format(AppResources.User_LoginCodeTimeLimitTip, TimeLimit);
                            return true;
                        }
                    }

                    TimeLimit--;
                    b = SetBtnSendSmsCodeText();
#if DEBUG
                    //Toast.Show($"TimeLimit: {TimeLimit}");
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
                            SetBtnSendSmsCodeText();
                        }
                        break;
                    }
                } while (b);
            });
        }

        public async void SendSms()
        {
            if (IsUnTimeLimit)
            {
                return;
            }

            TimeStart();

            var client = ICloudServiceClient.Instance;

            var request = new SendSmsRequest
            {
                PhoneNumber = PhoneNumber,
                Type = SmsCodeType.LoginOrRegister,
            };
            var response = await client.AuthMessage.SendSms(request);

            if (!response.IsSuccess)
            {
                cts?.Cancel();
            }

            if (!SendSmsCodeSuccess && response.IsSuccess) SendSmsCodeSuccess = true;
        }

        public void OpenHyperlink(string parameter) => BrowserOpen(parameter);

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
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels
{
    public class BindPhoneNumberWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
    {
        public BindPhoneNumberWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.User_BindPhoneNum;
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
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

        public async void Submit()
        {
            if (IsLoading || !this.CanSubmit()) return;

            var request = new BindPhoneNumberRequest
            {
                PhoneNumber = PhoneNumber,
                SmsCode = SmsCode
            };

            IsLoading = true;

            var response = await ICloudServiceClient.Instance.Manage.BindPhoneNumber(request);

            if (response.IsSuccess)
            {
                await UserService.Current.UpdateCurrentUserPhoneNumberAsync(request.PhoneNumber!);
                var msg = AppResources.Success_.Format(AppResources.User_BindPhoneNum);
                Toast.Show(msg);
                Close?.Invoke();
                return;
            }

            IsLoading = false;
        }

        public new Action? Close { get; set; }

        public Action? TbPhoneNumberFocus { get; set; }

        public Action? TbSmsCodeFocus { get; set; }

        public CancellationTokenSource? CTS { get; set; }

        public async void SendSms() => await this.SendSmsAsync(() => new()
        {
            PhoneNumber = PhoneNumber,
            Type = SmsCodeType.BindPhoneNumber,
        });

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                CTS?.Cancel();
            }
            base.Dispose(disposing);
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

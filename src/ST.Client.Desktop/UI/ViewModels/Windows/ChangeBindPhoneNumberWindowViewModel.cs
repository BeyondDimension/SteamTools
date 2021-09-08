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
    public class ChangeBindPhoneNumberWindowViewModel : WindowViewModel
    {
        readonly IReadOnlyDictionary<Step, SendSmsUIHelper.IViewModel> sendSmsUIViewModels;
        public ChangeBindPhoneNumberWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.User_ChangePhoneNum;
            sendSmsUIViewModels = new Dictionary<Step, SendSmsUIHelper.IViewModel>
            {
                { Step.Validation, new SendSmsUIViewModelValidation(this) },
                { Step.New, new SendSmsUIViewModelNew(this) },
            };
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        string? _SmsCodeValidation;
        public string? SmsCodeValidation
        {
            get => _SmsCodeValidation;
            set => this.RaiseAndSetIfChanged(ref _SmsCodeValidation, value);
        }

        int _TimeLimitValidation = SMSInterval;
        public int TimeLimitValidation
        {
            get => _TimeLimitValidation;
            set
            {
                this.RaiseAndSetIfChanged(ref _TimeLimitValidation, value);
                this.RaisePropertyChanged(nameof(IsUnTimeLimitValidation));
            }
        }

        public bool IsUnTimeLimitValidation
        {
            get => TimeLimitValidation != SMSInterval || _CurrentStep != Step.Validation;
        }

        string _BtnSendSmsCodeTextValidation = AppResources.User_GetSMSCode;
        public string BtnSendSmsCodeTextValidation
        {
            get => _BtnSendSmsCodeTextValidation;
            set => this.RaiseAndSetIfChanged(ref _BtnSendSmsCodeTextValidation, value);
        }

        public bool SendSmsCodeSuccessValidation { get; set; }

        string? _SmsCodeNew;
        public string? SmsCodeNew
        {
            get => _SmsCodeNew;
            set => this.RaiseAndSetIfChanged(ref _SmsCodeNew, value);
        }

        int _TimeLimitNew = SMSInterval;
        public int TimeLimitNew
        {
            get => _TimeLimitNew;
            set
            {
                this.RaiseAndSetIfChanged(ref _TimeLimitNew, value);
                this.RaisePropertyChanged(nameof(IsUnTimeLimitNew));
            }
        }

        public bool IsUnTimeLimitNew
        {
            get => TimeLimitNew != SMSInterval || _CurrentStep != Step.New;
        }

        string _BtnSendSmsCodeTextNew = AppResources.User_GetSMSCode;
        public string BtnSendSmsCodeTextNew
        {
            get => _BtnSendSmsCodeTextNew;
            set => this.RaiseAndSetIfChanged(ref _BtnSendSmsCodeTextNew, value);
        }

        public bool SendSmsCodeSuccessNew { get; set; }

        string? _PhoneNumber;
        public string? PhoneNumber
        {
            get => _PhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _PhoneNumber, value);
        }

        Step _CurrentStep;
        Step CurrentStep
        {
            set
            {
                if (value == _CurrentStep) return;
                _CurrentStep = value;
                this.RaisePropertyChanged(nameof(BtnSubmitText));
                this.RaisePropertyChanged(nameof(CurrentStepIsValidation));
                this.RaisePropertyChanged(nameof(CurrentStepIsNew));
                this.RaisePropertyChanged(nameof(IsUnTimeLimitValidation));
                this.RaisePropertyChanged(nameof(IsUnTimeLimitNew));
            }
        }

        public bool CurrentStepIsValidation => _CurrentStep == Step.Validation;

        public bool CurrentStepIsNew => _CurrentStep == Step.New;

        enum Step
        {
            /// <inheritdoc cref="ChangePhoneNumberRequest.Validation"/>
            Validation,
            /// <inheritdoc cref="ChangePhoneNumberRequest.New"/>
            New,
        }

        SendSmsUIHelper.IViewModel? SendSmsUIViewModel
        {
            get
            {
                if (!_CurrentStep.IsDefined() || !sendSmsUIViewModels.ContainsKey(_CurrentStep)) return null;
                return sendSmsUIViewModels[_CurrentStep];
            }
        }

        public async void SendSms()
        {
            var sendSmsUIViewModel = SendSmsUIViewModel;
            if (sendSmsUIViewModel == null) return;

            await sendSmsUIViewModel.SendSmsAsync(() => _CurrentStep switch
            {
                Step.Validation => new()
                {
                    PhoneNumber = PhoneNumberHelper.SimulatorDefaultValue,
                    Type = SmsCodeType.ChangePhoneNumberValidation,
                },
                Step.New => new()
                {
                    PhoneNumber = PhoneNumber,
                    Type = SmsCodeType.ChangePhoneNumberNew,
                },
                _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
            });
        }

        public bool IsComplete { get; set; }

        string? code;
        public async void Submit()
        {
            if (IsLoading) return;

            var sendSmsUIViewModel = SendSmsUIViewModel;
            if (sendSmsUIViewModel == null) return;

            if (!sendSmsUIViewModel.CanSubmit()) return;

            IsLoading = true;

            switch (_CurrentStep)
            {
                case Step.Validation:
                    await SubmitValidation();
                    break;
                case Step.New:
                    await SubmitNew();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null);
            }

            IsLoading = false;

            async Task SubmitValidation()
            {
                var request = new ChangePhoneNumberRequest.Validation
                {
                    SmsCode = SmsCodeValidation,
                    PhoneNumber = PhoneNumber,
                };
                var response = await ICloudServiceClient.Instance.Manage.ChangeBindPhoneNumber(request);
                if (response.IsSuccess)
                {
                    code = response.Content ?? throw new ArgumentNullException(nameof(code));
                    CTS?.Cancel();
                    CurrentStep = Step.New;

                    SendSms();
                }
            }

            async Task SubmitNew()
            {
                var request = new ChangePhoneNumberRequest.New
                {
                    SmsCode = SmsCodeNew,
                    PhoneNumber = PhoneNumber,
                    Code = code ?? throw new ArgumentNullException(nameof(code)),
                };
                var response = await ICloudServiceClient.Instance.Manage.ChangeBindPhoneNumber(request);
                if (response.IsSuccess)
                {
                    await UserService.Current.UpdateCurrentUserPhoneNumberAsync(request.PhoneNumber!);
                    IsComplete = true;
                    var msg = AppResources.Success_.Format(AppResources.User_ChangePhoneNum);
                    Toast.Show(msg);
                    Close?.Invoke();
                }
            }
        }

        public new Action? Close { private get; set; }

        public Action? TbSmsCodeFocusValidation { get; set; }

        public Action? TbSmsCodeFocusNew { get; set; }

        public CancellationTokenSource? CTS { get; set; }

        public string BtnSubmitText => _CurrentStep switch
        {
            Step.Validation => AppResources.Btn_Text_Continue,
            Step.New => AppResources.Btn_Text_Complete,
            _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
        };

        abstract class SendSmsUIViewModelBase : SendSmsUIHelper.IViewModel
        {
            protected readonly ChangeBindPhoneNumberWindowViewModel @this;

            protected SendSmsUIViewModelBase(ChangeBindPhoneNumberWindowViewModel @this) => this.@this = @this;

            public CancellationTokenSource? CTS
            {
                get => @this.CTS;
                set => @this.CTS = value;
            }

            public abstract int TimeLimit { get; set; }

            public abstract string BtnSendSmsCodeText { set; }

            public bool Disposed => @this.Disposed;

            public abstract bool SendSmsCodeSuccess { get; set; }

            public abstract bool IsUnTimeLimit { get; }

            public virtual Action? TbPhoneNumberFocus
            {
                get => null;
                set
                {

                }
            }

            public abstract Action? TbSmsCodeFocus { get; set; }
        }

        class SendSmsUIViewModelValidation : SendSmsUIViewModelBase
        {
            public SendSmsUIViewModelValidation(ChangeBindPhoneNumberWindowViewModel @this) : base(@this)
            {
            }

            public override int TimeLimit
            {
                get => @this.TimeLimitValidation;
                set => @this.TimeLimitValidation = value;
            }

            public override string BtnSendSmsCodeText
            {
                set => @this.BtnSendSmsCodeTextValidation = value;
            }

            public override bool SendSmsCodeSuccess
            {
                get => @this.SendSmsCodeSuccessValidation;
                set => @this.SendSmsCodeSuccessValidation = value;
            }

            public override bool IsUnTimeLimit => @this.IsUnTimeLimitValidation;

            public override Action? TbSmsCodeFocus
            {
                get => @this.TbSmsCodeFocusValidation;
                set => @this.TbSmsCodeFocusValidation = value;
            }
        }

        class SendSmsUIViewModelNew : SendSmsUIViewModelBase
        {
            public SendSmsUIViewModelNew(ChangeBindPhoneNumberWindowViewModel @this) : base(@this)
            {
            }

            public override int TimeLimit
            {
                get => @this.TimeLimitNew;
                set => @this.TimeLimitNew = value;
            }

            public override string BtnSendSmsCodeText
            {
                set => @this.BtnSendSmsCodeTextNew = value;
            }

            public override bool SendSmsCodeSuccess
            {
                get => @this.SendSmsCodeSuccessNew;
                set => @this.SendSmsCodeSuccessNew = value;
            }

            public override bool IsUnTimeLimit => @this.IsUnTimeLimitNew;

            public override Action? TbSmsCodeFocus
            {
                get => @this.TbSmsCodeFocusNew;
                set => @this.TbSmsCodeFocusNew = value;
            }
        }

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
            foreach (var item in sendSmsUIViewModels)
            {
                item.Value.RemoveAllDelegate();
            }
        }
    }
}
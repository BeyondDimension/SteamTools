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
    public class ChangeBindPhoneNumberWindowViewModel : WindowViewModel, SendSmsUIHelper.IViewModel
    {
        public ChangeBindPhoneNumberWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.User_ChangePhoneNum;
            Initialize();
        }

        public async void Initialize()
        {
            CurrentPhoneNumber = PhoneNumberHelper.ToStringHideMiddleFour((await DI.Get<IUserManager>().GetCurrentUserAsync())?.PhoneNumber);
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }

        string? _CurrentPhoneNumber;
        public string? CurrentPhoneNumber
        {
            get => _CurrentPhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _CurrentPhoneNumber, value);
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
            get => TimeLimitValidation != SMSInterval;
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
            get => TimeLimitValidation != SMSInterval || _CurrentStep != Step.New;
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
                this.RaisePropertyChanged(nameof(IsUnTimeLimitNew));
                this.RaisePropertyChanged(nameof(CurrentStepIsValidation));
                this.RaisePropertyChanged(nameof(CurrentStepIsNew));
                this.RaisePropertyChanged(nameof(BtnSubmitText));
            }
        }

        public bool CurrentStepIsValidation => _CurrentStep == Step.Validation;

        public bool CurrentStepIsNew => _CurrentStep == Step.New;

        enum Step
        {
            Validation,
            New,
        }

        public async void SendSms()
        {
            if (!_CurrentStep.IsDefined()) return;

            if (this.TimeStart())
            {
                var request = _CurrentStep switch
                {
                    Step.Validation => new SendSmsRequest
                    {
                        PhoneNumber = PhoneNumberHelper.SimulatorDefaultValue,
                        Type = SmsCodeType.ChangePhoneNumberValidation,
                    },
                    Step.New => new SendSmsRequest
                    {
                        PhoneNumber = PhoneNumber,
                        Type = SmsCodeType.ChangePhoneNumberNew,
                    },
                    _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
                };
#if DEBUG
                var response =
#endif
                await this.SendSms(request);
            }
        }

        string? code;
        public async void Submit()
        {
            if (!this.CanSubmit()) return;

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
                    CurrentStep = Step.Validation;
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
                    Toast.Show("换绑手机成功");
                    Close?.Invoke();
                }
            }
        }

        public Action? Close { private get; set; }

        public CancellationTokenSource? CTS { get; set; }

        public int TimeLimit
        {
            get => _CurrentStep switch
            {
                Step.Validation => TimeLimitValidation,
                Step.New => TimeLimitNew,
                _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
            };
            set
            {
                switch (_CurrentStep)
                {
                    case Step.Validation:
                        TimeLimitValidation = value;
                        break;
                    case Step.New:
                        TimeLimitNew = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null);
                }
            }
        }

        public string BtnSendSmsCodeText
        {
            get => _CurrentStep switch
            {
                Step.Validation => BtnSendSmsCodeTextValidation,
                Step.New => BtnSendSmsCodeTextNew,
                _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
            };
            set
            {
                switch (_CurrentStep)
                {
                    case Step.Validation:
                        BtnSendSmsCodeTextValidation = value;
                        break;
                    case Step.New:
                        BtnSendSmsCodeTextNew = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null);
                }
            }
        }

        public bool SendSmsCodeSuccess
        {
            get => _CurrentStep switch
            {
                Step.Validation => SendSmsCodeSuccessValidation,
                Step.New => SendSmsCodeSuccessNew,
                _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
            };
            set
            {
                switch (_CurrentStep)
                {
                    case Step.Validation:
                        SendSmsCodeSuccessValidation = value;
                        break;
                    case Step.New:
                        SendSmsCodeSuccessNew = value;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null);
                }
            }
        }

        public bool IsUnTimeLimit
        {
            get => _CurrentStep switch
            {
                Step.Validation => IsUnTimeLimitValidation,
                Step.New => IsUnTimeLimitNew,
                _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
            };
        }

        public string BtnSubmitText => _CurrentStep switch
        {
            Step.Validation => AppResources.Btn_Text_Continue,
            Step.New => AppResources.Btn_Text_Complete,
            _ => throw new ArgumentOutOfRangeException(nameof(_CurrentStep), _CurrentStep, null),
        };
    }
}
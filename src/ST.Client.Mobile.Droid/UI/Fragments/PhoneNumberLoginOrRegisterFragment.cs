using Android.Runtime;
using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Activities;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(PhoneNumberLoginOrRegisterFragment))]
    internal sealed class PhoneNumberLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_phone_number, LoginOrRegisterPageViewModel>, TextView.IOnEditorActionListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_phone_number;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.tbPhoneNumber.TextChanged += (_, _)
                => ViewModel!.PhoneNumber = binding!.tbPhoneNumber.Text;
            binding!.tbSmsCode.TextChanged += (_, _)
                => ViewModel!.SmsCode = binding!.tbSmsCode.Text;

            OnCreateViewAsync();

            binding!.tvAgreementAndPrivacy.SetLinkMovementMethod();

            // https://developer.android.google.cn/reference/android/widget/TextView.html#protected-methods
            binding!.tbPhoneNumber.SetRawInputType(InputTypes.ClassPhone);
            binding!.tbPhoneNumber.SetDigitsKeyListener();
            binding!.tbPhoneNumber.AddFilters(new InputFilterLengthFilter(InputLengthConstants.Current.PhoneNumber));
            binding!.tbPhoneNumber.ImeOptions = ImeAction.Next;
            binding!.tbPhoneNumber.SetOnEditorActionListener(this);

            binding!.tbSmsCode.SetRawInputType(InputTypes.ClassNumber | InputTypes.NumberVariationNormal);
            binding!.tbSmsCode.SetDigitsKeyListener();
            binding!.tbSmsCode.AddFilters(new InputFilterLengthFilter(InputLengthConstants.Current.SMS_CAPTCHA));
            binding!.tbSmsCode.ImeOptions = ImeAction.Done;
            binding!.tbSmsCode.SetOnEditorActionListener(this);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tbPhoneNumber.Hint = AppResources.User_Phone;
                binding!.tbPhoneNumber.SetImeActionLabel(AppResources.NextStep, binding!.tbPhoneNumber.ImeOptions);
                binding.tbSmsCode.Hint = AppResources.User_SMSCode;
                binding.tbSmsCode.SetImeActionLabel(AppResources.LoginAndRegister, binding!.tbSmsCode.ImeOptions);
                binding.btnSubmit.Text = AppResources.LoginAndRegister;
                binding.tvAgreementAndPrivacy.TextFormatted = LoginOrRegisterActivity.CreateAgreementAndPrivacy(ViewModel!);
                if (!ViewModel!.IsUnTimeLimit)
                {
                    binding.btnSendSms.Text = LoginOrRegisterPageViewModel.DefaultBtnSendSmsCodeText;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).Subscribe(value =>
            {
                if (binding == null) return;
                binding.loading.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                binding.content_group.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsUnTimeLimit).Subscribe(value =>
            {
                if (binding == null) return;
                //var p0 = ViewModel!.IsUnTimeLimit;
                //var p1 = ViewModel.TimeLimit;
                var enabled = !value;
                if (binding.btnSendSms.Enabled != enabled)
                {
                    binding.btnSendSms.Enabled = enabled;
                }
            }).AddTo(this);
            //ViewModel!.OnIsUnTimeLimitChanged = () =>
            //{
            //    if (binding == null) return;
            //    var enabled = !ViewModel.IsUnTimeLimit;
            //    if (binding.btnSendSms.Enabled != enabled)
            //    {
            //        binding.btnSendSms.Enabled = enabled;
            //    }
            //};
            ViewModel!.WhenAnyValue(x => x.BtnSendSmsCodeText).Subscribe(value =>
            {
                if (binding == null) return;
                binding.btnSendSms.Text = value;
            }).AddTo(this);

            SetOnClickListener(binding.btnSendSms, binding.btnSubmit);

            ViewModel!.TbPhoneNumberFocus = () =>
            {
                if (binding == null) return;
                binding.tbPhoneNumber.RequestFocus();
            };

            ViewModel!.TbSmsCodeFocus = () =>
            {
                if (binding == null) return;
                binding.tbSmsCode.RequestFocus();
            };
        }

        async void OnCreateViewAsync()
        {
            binding!.tbPhoneNumber.Text = await ITelephonyService.GetAutoFillPhoneNumberAsync(binding!.tbPhoneNumber.Text);
        }

        public override void OnDestroyView()
        {
            base.OnDestroyView();

            ViewModel?.RemoveAllDelegate();
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnSendSms)
            {
                ViewModel!.SendSms.Invoke();
                return true;
            }
            else if (view.Id == Resource.Id.btnSubmit)
            {
                ViewModel!.Submit.Invoke();
                return true;
            }

            return base.OnClick(view);
        }

        bool TextView.IOnEditorActionListener.OnEditorAction(TextView? view, ImeAction actionId, KeyEvent? e)
        {
            if (view != null && binding != null)
            {
                if (view.Id == Resource.Id.tbPhoneNumber)
                {
                    OnClick(binding.btnSendSms);
                    binding.tbSmsCode.RequestFocus();
                }
                else if (view.Id == Resource.Id.tbSmsCode)
                {
                    OnClick(binding.btnSubmit);
                }
            }
            return false;
        }
    }
}
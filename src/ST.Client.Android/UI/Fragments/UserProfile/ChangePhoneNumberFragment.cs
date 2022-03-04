using Android.Text;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using static System.Application.UI.ViewModels.UserProfileWindowViewModel;

namespace System.Application.UI.Fragments
{
    [Authorize]
    internal sealed class ChangePhoneNumberFragment : BaseFragment<fragment_change_bind_phone_number, UserProfileWindowViewModel>, TextView.IOnEditorActionListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_change_bind_phone_number;

        public new ChangeBindPhoneNumberWindowViewModel ViewModel => base.ViewModel!.ChangeBindPhoneNumberVM;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.tbPhoneNumber.Text = UserService.Current.PhoneNumber;

            binding!.tbNewPhoneNumber.TextChanged += (_, _)
                => ViewModel.PhoneNumber = binding!.tbNewPhoneNumber.Text;
            binding!.tbSmsCode.TextChanged += (_, _)
                => ViewModel.SmsCodeValidation = binding!.tbSmsCode.Text;
            binding!.tbNewSmsCode.TextChanged += (_, _)
                => ViewModel.SmsCodeNew = binding!.tbNewSmsCode.Text;

            OnCreateViewAsync();

            // https://developer.android.google.cn/reference/android/widget/TextView.html#protected-methods
            binding!.tbNewPhoneNumber.SetRawInputType(InputTypes.ClassPhone);
            binding!.tbNewPhoneNumber.SetDigitsKeyListener();
            binding!.tbNewPhoneNumber.SetMaxLength(ModelValidatorLengths.PhoneNumber);
            binding!.tbNewPhoneNumber.ImeOptions = ImeAction.Next;
            binding!.tbNewPhoneNumber.SetOnEditorActionListener(this);

            binding!.tbSmsCode.SetRawInputType(InputTypes.ClassNumber | InputTypes.NumberVariationNormal);
            binding!.tbSmsCode.SetDigitsKeyListener();
            binding!.tbSmsCode.SetMaxLength(ModelValidatorLengths.SMS_CAPTCHA);
            binding!.tbSmsCode.ImeOptions = ImeAction.Done;
            binding!.tbSmsCode.SetOnEditorActionListener(this);

            binding!.tbNewSmsCode.SetRawInputType(InputTypes.ClassNumber | InputTypes.NumberVariationNormal);
            binding!.tbNewSmsCode.SetDigitsKeyListener();
            binding!.tbNewSmsCode.SetMaxLength(ModelValidatorLengths.SMS_CAPTCHA);
            binding!.tbNewSmsCode.ImeOptions = ImeAction.Done;
            binding!.tbNewSmsCode.SetOnEditorActionListener(this);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.tbPhoneNumber.Hint = AppResources.User_Phone;

                binding.tbNewPhoneNumber.Hint = AppResources.User_NewPhone;
                binding.tbNewPhoneNumber.SetImeActionLabel(AppResources.NextStep, binding!.tbNewPhoneNumber.ImeOptions);

                binding.tbSmsCode.Hint = AppResources.User_SMSCode;
                binding.tbSmsCode.SetImeActionLabel(AppResources.NextStep, binding!.tbSmsCode.ImeOptions);

                binding.tbNewSmsCode.Hint = AppResources.User_SMSCode;
                binding.tbNewSmsCode.SetImeActionLabel(AppResources.Btn_Text_Complete, binding!.tbSmsCode.ImeOptions);
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsLoading).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.loading.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                binding.group_show_when_loading_false.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsUnTimeLimitValidation).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var enabled = !value;
                if (binding.btnSendSms.Enabled != enabled)
                {
                    binding.btnSendSms.Enabled = enabled;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.BtnSendSmsCodeTextValidation).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnSendSms.Text = value;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsUnTimeLimitNew).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var enabled = !value;
                if (binding.btnNewSendSms.Enabled != enabled)
                {
                    binding.btnNewSendSms.Enabled = enabled;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.BtnSendSmsCodeTextNew).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnNewSendSms.Text = value;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.BtnSubmitText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnDone.Text = value;
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.CurrentStepIsValidation).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tbSmsCode.Enabled = value;
                binding.tbNewPhoneNumber.Enabled = value;
            }).AddTo(this);

            SetOnClickListener(
                binding.btnSendSms,
                binding.btnNewSendSms,
                binding.btnDone);

            ViewModel.TbSmsCodeFocusValidation = () =>
            {
                if (binding == null) return;
                binding.tbSmsCode.RequestFocus();
            };

            ViewModel.TbSmsCodeFocusNew = () =>
            {
                if (binding == null) return;
                binding.tbNewSmsCode.RequestFocus();
            };
        }

        async void OnCreateViewAsync()
        {
            var value = await ITelephonyService.GetAutoFillPhoneNumberAsync(binding!.tbPhoneNumber.Text);
            if (value != IUserManager.Instance.GetCurrentUser()?.PhoneNumber)
            {
                binding!.tbNewPhoneNumber.Text = value;
            }
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnSendSms || view.Id == Resource.Id.btnNewSendSms)
            {
                ViewModel!.SendSms();
                return true;
            }
            else if (view.Id == Resource.Id.btnDone)
            {
                ViewModel!.Submit();
                return true;
            }

            return base.OnClick(view);
        }

        bool TextView.IOnEditorActionListener.OnEditorAction(TextView? view, ImeAction actionId, KeyEvent? e)
        {
            if (view != null && binding != null && ((e != null && e.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter) || e == null))
            {
                if (view.Id == Resource.Id.tbNewPhoneNumber || view.Id == Resource.Id.tbNewSmsCode)
                {
                    OnClick(binding.btnDone);
                    return true;
                }
                else if (view.Id == Resource.Id.tbSmsCode)
                {
                    OnClick(binding.btnSendSms);
                    binding.tbNewPhoneNumber.RequestFocus();
                    return true;
                }
            }
            return false;
        }
    }
}

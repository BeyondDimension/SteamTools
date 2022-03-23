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
    internal sealed class BindPhoneNumberFragment : BaseFragment<fragment_bind_phone_number, UserProfileWindowViewModel>, TextView.IOnEditorActionListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_bind_phone_number;

        public new BindPhoneNumberWindowViewModel ViewModel => base.ViewModel!.BindPhoneNumberVM;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.tbPhoneNumber.TextChanged += (_, _)
                => ViewModel.PhoneNumber = binding!.tbPhoneNumber.Text;
            binding!.tbSmsCode.TextChanged += (_, _)
                => ViewModel.SmsCode = binding!.tbSmsCode.Text;

            OnCreateViewAsync();

            // https://developer.android.google.cn/reference/android/widget/TextView.html#protected-methods
            binding!.tbPhoneNumber.SetRawInputType(InputTypes.ClassPhone);
            binding!.tbPhoneNumber.SetDigitsKeyListener();
            binding!.tbPhoneNumber.SetMaxLength(ModelValidatorLengths.PhoneNumber);
            binding!.tbPhoneNumber.ImeOptions = ImeAction.Next;
            binding!.tbPhoneNumber.SetOnEditorActionListener(this);

            binding!.tbSmsCode.SetRawInputType(InputTypes.ClassNumber | InputTypes.NumberVariationNormal);
            binding!.tbSmsCode.SetDigitsKeyListener();
            binding!.tbSmsCode.SetMaxLength(ModelValidatorLengths.SMS_CAPTCHA);
            binding!.tbSmsCode.ImeOptions = ImeAction.Done;
            binding!.tbSmsCode.SetOnEditorActionListener(this);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.tbPhoneNumber.Hint = AppResources.User_Phone;
                binding!.tbPhoneNumber.SetImeActionLabel(AppResources.NextStep, binding!.tbPhoneNumber.ImeOptions);
                binding.tbSmsCode.Hint = AppResources.User_SMSCode;
                binding.tbSmsCode.SetImeActionLabel(AppResources.Btn_Text_Complete, binding!.tbSmsCode.ImeOptions);
                binding.btnDone.Text = AppResources.Btn_Text_Complete;
                if (!ViewModel!.IsUnTimeLimit)
                {
                    binding.btnSendSms.Text = AppResources.User_GetSMSCode;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.loading.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                binding.group_show_when_loading_false.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsUnTimeLimit).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var enabled = !value;
                if (binding.btnSendSms.Enabled != enabled)
                {
                    binding.btnSendSms.Enabled = enabled;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.BtnSendSmsCodeText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnSendSms.Text = value;
            }).AddTo(this);

            SetOnClickListener(
                binding.btnSendSms,
                binding.btnDone);

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

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnSendSms)
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
                if (view.Id == Resource.Id.tbPhoneNumber)
                {
                    OnClick(binding.btnSendSms);
                    binding.tbSmsCode.RequestFocus();
                    return true;
                }
                else if (view.Id == Resource.Id.tbSmsCode)
                {
                    OnClick(binding.btnDone);
                    return true;
                }
            }
            return false;
        }
    }
}

using Android.Runtime;
using Android.Text.Method;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.UI.Activities;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(PhoneNumberLoginOrRegisterFragment))]
    internal sealed class PhoneNumberLoginOrRegisterFragment : BaseFragment<fragment_login_and_register_by_phone_number, LoginOrRegisterPageViewModel>, IDisposableHolder
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        protected override int? LayoutResource => Resource.Layout.fragment_login_and_register_by_phone_number;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.tvAgreementAndPrivacy.MovementMethod = LinkMovementMethod.Instance;

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tbPhoneNumber.Hint = AppResources.User_Phone;
                binding.tbSmsCode.Hint = AppResources.SMSCode;
                binding.btnSubmit.Text = AppResources.LoginAndRegister;
                binding.tvAgreementAndPrivacy.TextFormatted = LoginOrRegisterActivity.CreateAgreementAndPrivacy(ViewModel!);
                if (!ViewModel!.IsUnTimeLimit)
                {
                    binding.btnSendSms.Text = AppResources.User_GetSMSCode;
                }
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.IsLoading).Subscribe(value =>
            {
                if (binding == null) return;
                binding.loading.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                binding.content_group.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);

            SetOnClickListener(binding.btnSendSms, binding.btnSubmit);
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
    }
}
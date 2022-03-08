using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthSteamUserImportFragment : BaseFragment<fragment_local_auth_import_steam_user, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_user;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.layoutSteamUserName.Hint = Steam_User;
                binding.layoutSteamPassword.Hint = Steam_Password;
                binding.btnSubmit.Text = ViewModel!.RequiresLogin ? Login : Continue;
                binding.tvLoginTip.Text = Steam_UserLoginTip;
                binding.btnShowCaptchaImage.Text = Steam_ImageCodeShowError;
            }).AddTo(this);

            binding!.tbRevocationCode.SetReadOnly();
            binding!.tbSteamUserName.TextChanged += (_, _) =>
            {
                ViewModel!.UserName = binding.tbSteamUserName.Text;
            };
            binding!.tbSteamPassword.TextChanged += (_, _) =>
            {
                ViewModel!.Password = binding.tbSteamPassword.Text;
            };
            binding!.tbCaptcha.TextChanged += (_, _) =>
            {
                ViewModel!.CaptchaText = binding.tbCaptcha.Text;
            };
            binding!.tbEmailAuth.TextChanged += (_, _) =>
            {
                ViewModel!.EmailAuthText = binding.tbEmailAuth.Text;
            };
            binding!.tbSMSCode.TextChanged += (_, _) =>
            {
                ViewModel!.ActivationCode = binding.tbSMSCode.Text;
            };

            ViewModel!.WhenAnyValue(x => x.RevocationCode).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tbRevocationCode.Text = value;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.RequiresLogin).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.ivSteamLogo.Visibility = state;
                binding.layoutSteamUserName.Visibility = state;
                binding.layoutSteamPassword.Visibility = state;
                binding.btnSubmit.Text = Login;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.CaptchaImage).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = value == null ? ViewStates.Gone : ViewStates.Visible;
                binding.ivCaptchaImage.Visibility = state;
                binding.layoutCaptcha.Visibility = state;
                binding.ivCaptchaImage.SetImageSource(value, Resource.Dimension.img_size_captcha_w, Resource.Dimension.img_size_captcha_h);
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.EmailDomain).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var value_ = value == null;
                var state = value_ ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutEmailAuth.Visibility = state;
                binding.tvEmailDomain.Text = value;
                binding.tvEmailDomain.Visibility = state;
                binding.tvEmailCodeTip.Visibility = state;
                if (!value_) binding.btnSubmit.Text = Continue;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.RequiresActivation).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutSMSCode.Visibility = state;
                binding.layoutRevocationCode.Visibility = state;
                binding.cbRecoveryCodeRemember.Visibility = state;
                binding.tvSMSCodeTip.Visibility = state;
                if (value) binding.btnSubmit.Text = Continue;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.RequiresActivation).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var state = !value ? ViewStates.Gone : ViewStates.Visible;
                var state_reverse = value ? ViewStates.Gone : ViewStates.Visible;
                binding.ivSuccess.Visibility = state;
                binding.layoutRevocationCode.Visibility = state;
                binding.tvRecoveryCodeRememberTip.Visibility = state;
                binding.btnSubmit.Visibility = state_reverse;
            }).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.LoginSteamLoadingText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                var isNotLoading = string.IsNullOrWhiteSpace(value);
                var loadingState = isNotLoading ? ViewStates.Gone : ViewStates.Visible;
                var contentState = !isNotLoading ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutLoading.Visibility = loadingState;
                binding.layoutContent.Visibility = contentState;
            }).AddTo(this);

            SetOnClickListener(binding!.btnSubmit, binding.btnShowCaptchaImage);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnSubmit)
            {
                ViewModel!.LoginSteamImport();
                return true;
            }
            if (view.Id == Resource.Id.btnShowCaptchaImage)
            {
                ViewModel!.CaptchaUrlButton_Click();
                return true;
            }
            return base.OnClick(view);
        }
    }
}
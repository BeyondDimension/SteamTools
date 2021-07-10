using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Fragments
{
    internal sealed class LocalAuthSteamUserImportFragment : BaseFragment<fragment_local_auth_import_steam_user, AddAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_local_auth_import_steam_user;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.layoutSteamUserName.Hint = Steam_User;
                binding.layoutSteamPassword.Hint = Steam_Password;
                binding.btnLogin.Text = Login;
                binding.tvLoginTip.Text = Steam_UserLoginTip;
            }).AddTo(this);

            binding!.tbSteamUserName.TextChanged += (_, _) =>
            {
                ViewModel!.UserName = binding.tbSteamUserName.Text;
            };
            binding!.tbSteamPassword.TextChanged += (_, _) =>
            {
                ViewModel!.Password = binding.tbSteamPassword.Text;
            };

            ViewModel!.WhenAnyValue(x => x.LoginSteamLoadingText).Subscribe(value =>
            {
                if (binding == null) return;
                var isNotLoading = string.IsNullOrWhiteSpace(value);
                var loadingState = isNotLoading ? ViewStates.Gone : ViewStates.Visible;
                var contentState = !isNotLoading ? ViewStates.Gone : ViewStates.Visible;
                binding.layoutLoading.Visibility = loadingState;
                binding.layoutContent.Visibility = contentState;
            }).AddTo(this);

            SetOnClickListener(binding!.btnLogin);
        }

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnLogin)
            {
                ViewModel!.LoginSteamImport();
                return true;
            }
            return base.OnClick(view);
        }
    }
}
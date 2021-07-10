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
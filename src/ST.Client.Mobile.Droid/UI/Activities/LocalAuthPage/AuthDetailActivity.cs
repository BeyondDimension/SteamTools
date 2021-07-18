using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Threading;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AuthDetailActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AuthDetailActivity : BaseActivity<activity_detail_auth, MyAuthenticatorWrapper>, MyAuthenticator.IAutoRefreshCode, IReadOnlyViewFor<MyAuthenticator>
    {
        public CancellationTokenSource? AutoRefreshCode { get; set; }

        MyAuthenticator IReadOnlyViewFor<MyAuthenticator>.ViewModel => ViewModel!;

        protected override int? LayoutResource => Resource.Layout.activity_detail_auth;

        protected override MyAuthenticatorWrapper? OnCreateViewModel()
        {
            var vm = LocalAuthPageViewModel.Current.Authenticators?.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            return vm!;
        }

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            if (!ViewModel.HasValue())
            {
                Finish();
                return;
            }

            ViewModel!.Authenticator.WhenAnyValue(x => x.Name).Subscribe(value =>
            {
                Title = value;
            }).AddTo(this);

            SetOnClickListener(binding!.layoutValue,
                binding.layoutEditName,
                binding.layoutConfirmTrade,
                binding.layoutSeeData,
                binding.layoutDelete);
        }

        protected override void OnPause()
        {
            base.OnPause();
            MyAuthenticator.StopAutoRefreshCode(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AutoRefreshCode == null)
            {
                MyAuthenticator.StartAutoRefreshCode(this, noStop: true);
            }
        }

        protected override async void OnClick(View view)
        {
            base.OnClick(view);
            if (view.Id == Resource.Id.layoutValue)
            {
                ViewModel!.Authenticator.CopyCodeCilp();
            }
            else if (view.Id == Resource.Id.layoutEditName)
            {
                await ViewModel!.Authenticator.EditNameAsync();
            }
            else if (view.Id == Resource.Id.layoutConfirmTrade)
            {
                LocalAuthPageViewModel.Current.ShowSteamAuthTrade(ViewModel!.Authenticator);
            }
            else if (view.Id == Resource.Id.layoutSeeData)
            {
                LocalAuthPageViewModel.Current.ShowSteamAuthData(ViewModel!.Authenticator);
            }
            else if (view.Id == Resource.Id.layoutDelete)
            {
                LocalAuthPageViewModel.Current.DeleteAuth(ViewModel!.Authenticator);
            }
        }

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<AuthDetailActivity, ushort>(activity, authId);
        }
    }
}
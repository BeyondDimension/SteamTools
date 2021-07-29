using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Threading;
using static System.Application.UI.Resx.AppResources;
using TViewModel = System.Application.Models.MyAuthenticator;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AuthDetailActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AuthDetailActivity : BaseActivity<activity_detail_auth, MyAuthenticatorWrapper>, TViewModel.IAutoRefreshCode, IReadOnlyViewFor<TViewModel>
    {
        public CancellationTokenSource? AutoRefreshCode { get; set; }

        TViewModel IReadOnlyViewFor<TViewModel>.ViewModel => ViewModel!;

        protected override int? LayoutResource => Resource.Layout.activity_detail_auth;

        protected override MyAuthenticatorWrapper? OnCreateViewModel()
        {
            var vm = AuthService.Current.Authenticators.Items.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
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

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            ViewModel!.Authenticator.WhenAnyValue(x => x.Name).SubscribeInMainThread(value =>
            {
                Title = value;
            }).AddTo(this);
            ViewModel.Authenticator.WhenAnyValue(x => x.CurrentCode).SubscribeInMainThread(value =>
            {
                binding.tvValue.Text = TViewModel.CodeFormat(value);
            }).AddTo(this);
            ViewModel.Authenticator.WhenAnyValue(x => x.Period).SubscribeInMainThread(value =>
            {
                binding.progress.Max = value;
            }).AddTo(this);
            ViewModel.Authenticator.WhenAnyValue(x => x.AutoRefreshCodeTimingCurrent).SubscribeInMainThread(value =>
            {
                binding.progress.Progress = value;
                binding.tvProgress.Text = value.ToString();
            }).AddTo(this);

            R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
            {
                if (binding == null) return;
                binding.tvInfoTitle.Text = LocalAuth_Code_Title;
                binding.tvInfoDesc.Text = LocalAuth_Code_Desc;
                binding.tvValueTitle.Text = LocalAuth_Code;
                binding.tvTitleEditName.Text = EditName;
                binding.tvTitleConfirmTrade.Text = LocalAuth_AuthTrade;
                binding.tvTitleSeeData.Text = SeeData;
                binding.tvTitleDelete.Text = LocalAuth_Delete;
                binding.tvTitleExport.Text = LocalAuth_ExportAuth;
            }).AddTo(this);

            SetOnClickListener(binding!.layoutValue,
                binding.layoutEditName,
                binding.layoutConfirmTrade,
                binding.layoutSeeData,
                binding.layoutDelete,
                binding.layoutExport);
        }

        protected override void OnPause()
        {
            base.OnPause();
            TViewModel.StopAutoRefreshCode(this);
        }

        protected override void OnResume()
        {
            base.OnResume();
            if (AutoRefreshCode == null)
            {
                TViewModel.StartAutoRefreshCode(this, noStop: true);
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
                LocalAuthPageViewModel.Current.DeleteAuthCore(ViewModel!.Authenticator, () =>
                {
                    Finish();
                });
            }
            else if (view.Id == Resource.Id.layoutExport)
            {
                ExportAuthActivity.StartActivity(this, ViewModel!.Authenticator.Id);
            }
        }

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<AuthDetailActivity, ushort>(activity, authId);
        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Settings;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using static System.Application.UI.Resx.AppResources;
using TViewModel = System.Application.Models.MyAuthenticator;
using System.Application.UI.Fragments;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(AuthDetailActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class AuthDetailActivity : BaseActivity<activity_detail_auth, MyAuthenticatorWindowViewModel>, TViewModel.IAutoRefreshCodeHost, IReadOnlyViewFor<TViewModel>
    {
        TViewModel IReadOnlyViewFor<TViewModel>.ViewModel => ViewModel!.MyAuthenticator!;

        Timer? TViewModel.IAutoRefreshCodeHost.Timer { get; set; }

        IEnumerable<TViewModel> TViewModel.IAutoRefreshCodeHost.ViewModels
        {
            get
            {
                yield return ViewModel!.MyAuthenticator!;
            }
        }

        protected override int? LayoutResource => Resource.Layout.activity_detail_auth;

        protected override MyAuthenticatorWindowViewModel? OnCreateViewModel()
        {
            var vm = AuthService.Current.Authenticators.Items.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            if (vm == null) return null;
            return new(vm);
        }

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            if (!ViewModel.HasValue())
            {
                Finish();
                return;
            }

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            ViewModel!.MyAuthenticator.WhenAnyValue(x => x.Name).SubscribeInMainThread(value =>
            {
                Title = value;
            }).AddTo(this);
            ViewModel.MyAuthenticator.WhenAnyValue(x => x.CurrentCode).SubscribeInMainThread(value =>
            {
                binding.tvValue.Text = TViewModel.CodeFormat(value);
            }).AddTo(this);
            ViewModel.MyAuthenticator.WhenAnyValue(x => x.Period).SubscribeInMainThread(value =>
            {
                binding.progress.Max = value;
            }).AddTo(this);
            ViewModel.MyAuthenticator.WhenAnyValue(x => x.AutoRefreshCodeTimingCurrent).SubscribeInMainThread(value =>
            {
                binding.progress.Progress = value;
                binding.tvProgress.Text = value.ToString();
            }).AddTo(this);

            R.Subscribe(() =>
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

        protected override void OnResume()
        {
            base.OnResume();
            if (!GeneralSettings.CaptureScreen.Value)
            {
                this.SetWindowSecure(true);
            }
            TViewModel.IAutoRefreshCodeHost host = this;
            host.StartTimer();
        }

        protected override void OnStop()
        {
            base.OnStop();
            this.SetWindowSecure(false);
            TViewModel.IAutoRefreshCodeHost host = this;
            host.StopTimer();
        }

        protected override async void OnClick(View view)
        {
            base.OnClick(view);
            if (view.Id == Resource.Id.layoutValue)
            {
                ViewModel!.MyAuthenticator!.CopyCodeCilp();
            }
            else if (view.Id == Resource.Id.layoutEditName)
            {
                await ViewModel!.MyAuthenticator!.EditNameAsync();
            }
            else if (view.Id == Resource.Id.layoutConfirmTrade)
            {
                LocalAuthPageViewModel.Current.ShowSteamAuthTrade(ViewModel!.MyAuthenticator!);
            }
            else if (view.Id == Resource.Id.layoutSeeData)
            {
                LocalAuthPageViewModel.Current.ShowSteamAuthData(ViewModel!.MyAuthenticator!);
            }
            else if (view.Id == Resource.Id.layoutDelete)
            {
                LocalAuthPageViewModel.Current.DeleteAuthCore(ViewModel!.MyAuthenticator!, () =>
                {
                    Finish();
                });
            }
            else if (view.Id == Resource.Id.layoutExport)
            {
                ExportAuthActivity.StartActivity(this, ViewModel!.MyAuthenticator!.Id);
            }
        }

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<AuthDetailActivity, ushort>(activity, authId);
        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Linq;
using static System.Application.UI.Resx.AppResources;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(SteamAuthDataActivity))]
    [Activity(Theme = ManifestConstants.MainTheme_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class SteamAuthDataActivity : BaseActivity<activity_steam_auth_data, ShowAuthWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_steam_auth_data;

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var vm = MainActivity.Instance?.LocalAuthPageViewModel.Authenticators.FirstOrDefault(x => x.Id == this.GetViewModel<ushort>());
            if (vm == null)
            {
                Finish();
                return;
            }

            ViewModel = new(vm);
            ViewModel.AddTo(this);

            binding!.tvSteamGuardLabel.Text = "SteamGuard：";
            R.Current.WhenAnyValue(x => x.Res).Subscribe(_ =>
            {
                if (binding == null) return;
                binding.tvRecoveryCode.Text = RecoveryCode;
                binding.tvRecoveryCodeTip2.Text = LocalAuth_ShowSteamAuthTip2;
                binding.tvRecoveryCodeLabel.Text = RecoveryCode + "：";
                binding.tvRecoveryCodeTip1.Text = LocalAuth_ShowSteamAuthTip1;
                binding.tvUUIDLabel.Text = LocalAuth_SteamUUID;
            }).AddTo(this);

            binding.tbRecoveryCode.Text = ViewModel.RecoveryCode;
            binding.tbUUID.Text = ViewModel.DeviceId;
            binding.tbSteamGuard.Text = ViewModel.SteamDataIndented;
        }

        public static void StartActivity(Activity activity, ushort authId)
        {
            GoToPlatformPages.StartActivity<SteamAuthDataActivity, ushort>(activity, authId);
        }
    }
}
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Binding;
using System.Application.UI.Resx;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ProxySettingsActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ProxySettingsActivity : BaseActivity<activity_proxy_settings>
    {
        protected override int? LayoutResource => Resource.Layout.activity_proxy_settings;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.tvSystemProxyIp.Text = AppResources.Settings_Proxy_SystemProxyIp;
                binding.tvCustomDNS.Text = AppResources.Settings_Proxy_CustomDNS;
                binding.tvProgramStartupRunProxy.Text = AppResources.CommunityFix_AutoRunProxy;
                binding.tvOnlyEnableProxyScript.Text = AppResources.CommunityFix_ProxyOnlyOnScript;
                binding.tvProxySettingsSocks5.Text = AppResources.Settings_Proxy_Socks5Proxy;
                binding.tvSocks5ProxyPortId.Text = AppResources.Settings_Proxy_ProxyPort;
                binding.tvProxySettingsTwoLevelAgent.Text = AppResources.Settings_Proxy_TwoLevelAgent;
                binding.tvTwoLevelAgentProxyType.Text = AppResources.Settings_Proxy_ProxyType;
                binding.tvTwoLevelAgentIp.Text = AppResources.Settings_Proxy_IPAddress;
                binding.tvTwoLevelAgentPortId.Text = AppResources.Settings_Proxy_Port;
                binding.tvTwoLevelAgentUserName.Text = AppResources.Settings_Proxy_UserName;
                binding.tvTwoLevelAgentPassword.Text = AppResources.Settings_Proxy_Password;
            }).AddTo(this);
        }
    }
}

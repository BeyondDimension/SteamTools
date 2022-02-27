using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Fragments;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Linq;
using System.Net;
using Titanium.Web.Proxy.Models;
using static System.Application.Settings.ProxySettings;
using static System.Application.UI.ComboBoxHelper;

namespace System.Application.UI.Activities
{
    [Register(JavaPackageConstants.Activities + nameof(ProxySettingsActivity))]
    [Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
        LaunchMode = LaunchMode.SingleTask,
        ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
    internal sealed class ProxySettingsActivity : BaseActivity<activity_proxy_settings, ProxySettingsWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.activity_proxy_settings;

        protected override void OnCreate2(Bundle? savedInstanceState)
        {
            base.OnCreate2(savedInstanceState);

            this.SetSupportActionBarWithNavigationClick(binding!.toolbar, true);

            R.Subscribe(() =>
            {
                Title = ViewModel!.Title;
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
                binding.tvIsVpnMode.Text = AppResources.CommunityFix_IsVpnMode;
            }).AddTo(this);

            CreateArrayAdapter(binding.tbSystemProxyIp,
                items: ViewModel!.SystemProxyIps);
            CreateArrayAdapter(binding.tbCustomDNS,
                items: ViewModel!.ProxyDNSs);
            CreateArrayAdapter(binding.tbTwoLevelAgentProxyType,
                items: ViewModel.ProxyTypes.Select(x => x.ToString()));

            SetOnClickListener(
                binding.layoutRootProgramStartupRunProxy,
                binding.layoutRootOnlyEnableProxyScript,
                binding.layoutRootProxySettingsSocks5,
                binding.layoutRootProxySettingsTwoLevelAgent,
                binding.layoutRootIsVpnMode);

            SetProgramStartupRunProxy();
            SetOnlyEnableProxyScript();
            SetProxySettingsSocks5();
            SetProxySettingsTwoLevelAgent();
            SetIsVpnMode();

            #region Binding TextBox

            binding.tbSystemProxyIp.Text = SystemProxyIp.Value;
            binding.tbSystemProxyIp.TextChanged += (_, _) =>
            {
                var value = binding.tbSystemProxyIp.Text;
                if (IPAddress.TryParse(value, out var _))
                {
                    SystemProxyIp.Value = value;
                }
            };

            binding.tbCustomDNS.Text = ProxyMasterDns.Value;
            binding.tbCustomDNS.TextChanged += (_, _) =>
            {
                var value = binding.tbCustomDNS.Text;
                if (IPAddress.TryParse(value, out var _))
                {
                    ProxyMasterDns.Value = value;
                }
            };

            binding.tbSocks5ProxyPortId.Text = Socks5ProxyPortId.Value.ToString();
            binding.tbSocks5ProxyPortId.TextChanged += (_, _) =>
            {
                var value = binding.tbSocks5ProxyPortId.Text;
                if (ModelValidatorProvider.IsPortId(value, out var value2))
                {
                    Socks5ProxyPortId.Value = value2;
                }
            };

            binding.tbTwoLevelAgentIp.Text = TwoLevelAgentIp.Value;
            binding.tbTwoLevelAgentIp.TextChanged += (_, _) =>
            {
                var value = binding.tbTwoLevelAgentIp.Text;
                if (IPAddress.TryParse(value, out var _))
                {
                    TwoLevelAgentIp.Value = value;
                }
            };

            binding.tbTwoLevelAgentPortId.Text = TwoLevelAgentPortId.Value.ToString();
            binding.tbTwoLevelAgentPortId.TextChanged += (_, _) =>
            {
                var value = binding.tbTwoLevelAgentPortId.Text;
                if (ModelValidatorProvider.IsPortId(value, out var value2))
                {
                    TwoLevelAgentPortId.Value = value2;
                }
            };

            binding.tbTwoLevelAgentUserName.Text = TwoLevelAgentUserName.Value;
            binding.tbTwoLevelAgentUserName.TextChanged += (_, _) =>
            {
                var value = binding.tbTwoLevelAgentUserName.Text;
                TwoLevelAgentUserName.Value = value;
            };

            binding.tbTwoLevelAgentPassword.Text = TwoLevelAgentPassword.Value;
            binding.tbTwoLevelAgentPassword.TextChanged += (_, _) =>
            {
                var value = binding.tbTwoLevelAgentPassword.Text;
                TwoLevelAgentPassword.Value = value;
            };

            binding.tbTwoLevelAgentProxyType.Text =
                ((ExternalProxyType)TwoLevelAgentProxyType.Value).ToString();
            binding.tbTwoLevelAgentProxyType.TextChanged += (_, _) =>
            {
                var value = binding.tbTwoLevelAgentProxyType.Text;
                if (Enum.TryParse<ExternalProxyType>(value, true, out var value2))
                {
                    TwoLevelAgentProxyType.Value = (short)value2;
                }
            };

            #endregion
        }

        protected override void OnClick(View view)
        {
            if (view.Id == Resource.Id.layoutRootProgramStartupRunProxy)
            {
                ProgramStartupRunProxy.Value = !ProgramStartupRunProxy.Value;
                SetProgramStartupRunProxy();
                return;
            }
            else if (view.Id == Resource.Id.layoutRootOnlyEnableProxyScript)
            {
                OnlyEnableProxyScript.Value = !OnlyEnableProxyScript.Value;
                SetOnlyEnableProxyScript();
                return;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsSocks5)
            {
                Socks5ProxyEnable.Value = !Socks5ProxyEnable.Value;
                SetProxySettingsSocks5();
                return;
            }
            else if (view.Id == Resource.Id.layoutRootProxySettingsTwoLevelAgent)
            {
                TwoLevelAgentEnable.Value = !TwoLevelAgentEnable.Value;
                SetProxySettingsTwoLevelAgent();
                return;
            }
            else if (view.Id == Resource.Id.layoutRootIsVpnMode)
            {
                IsVpnMode.Value = !IsVpnMode.Value;
                SetIsVpnMode();
                return;
            }

            base.OnClick(view);
        }

        void SetProgramStartupRunProxy() => binding!.swProgramStartupRunProxy.Checked = ProgramStartupRunProxy.Value;
        void SetOnlyEnableProxyScript() => binding!.swOnlyEnableProxyScript.Checked = OnlyEnableProxyScript.Value;
        void SetProxySettingsSocks5() => binding!.swProxySettingsSocks5.Checked = Socks5ProxyEnable.Value;
        void SetProxySettingsTwoLevelAgent() => binding!.swProxySettingsTwoLevelAgent.Checked = TwoLevelAgentEnable.Value;
        void SetIsVpnMode() => binding!.swIsVpnMode.Checked = IsVpnMode.Value;
    }
}

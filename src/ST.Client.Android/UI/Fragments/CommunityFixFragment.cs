using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.View.Menu;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.Services.Native;
using System.Application.Settings;
using System.Application.UI.Adapters;
using System.Application.UI.Adapters.Internals;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using static System.Application.UI.ViewModels.CommunityProxyPageViewModel;
using System.Application.Services.Implementation;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(CommunityFixFragment))]
    internal sealed class CommunityFixFragment : BaseFragment<fragment_community_fix, CommunityProxyPageViewModel>, SwipeRefreshLayout.IOnRefreshListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_community_fix;

        protected sealed override CommunityProxyPageViewModel? OnCreateViewModel()
        {
            return IViewModelManager.Instance.GetMainPageViewModel<CommunityProxyPageViewModel>();
        }

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.btnStartProxyService.Text = AppResources.CommunityFix_StartProxy;
                binding.btnStopProxyService.Text = AppResources.CommunityFix_StopProxy;
                //binding.tvProxyMode.Text = AppResources.CommunityFix_ProxyModeTip + AppResources.CommunityFix_ProxyMode_WinSystem;
                binding.tvAccelerationsEnable.Text = AppResources.CommunityFix_AccelerationsEnable;
                binding.tvScriptsEnable.Text = AppResources.CommunityFix_ScriptsEnable;
                SetMenuTitle();
            }).AddTo(this);

            var proxyS = ProxyService.Current;
            proxyS.WhenAnyValue(x => x.AccelerateTime).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvAccelerateTime.Text = AppResources.CommunityFix_AlreadyProxy + value.ToString(@"hh\:mm\:ss");
            }).AddTo(this);
            proxyS.WhenAnyValue(x => x.ProxyStatus).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.layoutRootCommunityFixContentReady.Visibility = !value ? ViewStates.Visible : ViewStates.Gone;
                binding.layoutRootCommunityFixContentStarting.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
                if (value)
                {
                    binding.tvProxyMode.Text = $"IPEndPoint: {proxyS.ProxyIp}:{proxyS.ProxyPort}";
                    StringBuilder s = new();
                    var enableProxyDomains = proxyS.GetEnableProxyDomains();
                    if (enableProxyDomains != null)
                    {
                        foreach (var item in enableProxyDomains)
                        {
                            s.AppendLine(item.Name);
                        }
                    }
                    binding.tvAccelerationsEnableContent.Text = s.ToString();
                    if (proxyS.IsEnableScript)
                    {
                        s.Clear();
                        SetScriptsEnableContentText(s);
                    }
                }
                if (menuBuilder != null)
                {
                    var menu_settings_proxy = menuBuilder.FindItem(Resource.Id.menu_settings_proxy);
                    if (menu_settings_proxy != null)
                    {
                        menu_settings_proxy.SetVisible(!value);
                    }
                }
            }).AddTo(this);
            proxyS.WhenAnyValue(x => x.IsEnableScript).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                if (value)
                {
                    SetScriptsEnableContentText();
                }
                binding.cardScriptsEnable.Visibility = value ? ViewStates.Visible : ViewStates.Gone;
            }).AddTo(this);

            void SetScriptsEnableContentText(StringBuilder? s = null)
            {
                s ??= new();
                var enableProxyScripts = proxyS.GetEnableProxyScripts();
                if (enableProxyScripts != null)
                {
                    foreach (var item in enableProxyScripts)
                    {
                        s.AppendLine(item.Name);
                    }
                }
                binding!.tvScriptsEnableContent.Text = s.ToString();
            }

            var adapter = new AccelerateProjectGroupAdapter();
            adapter.ItemClick += (_, e) =>
            {
                //var value = e.Current.ThreeStateEnable;
                //e.Current.ThreeStateEnable = value == null || !value.Value;

                if (e is IPlatformItemClickEventArgs e2 && e2.ViewHolder is AccelerateProjectGroupViewHolder vh)
                {
                    vh.IsOpen = !vh.IsOpen;
                }
            };
            binding!.rvAccelerateProjectGroup.SetLinearLayoutManager();
            binding.rvAccelerateProjectGroup.AddVerticalItemDecorationRes(Resource.Dimension.activity_vertical_margin, Resource.Dimension.fab_height_with_margin_top_bottom);
            binding.rvAccelerateProjectGroup.SetAdapter(adapter);

            binding.swipeRefreshLayout.InitDefaultStyles();
            binding.swipeRefreshLayout.SetOnRefreshListener(this);

            SetOnClickListener(binding.btnStartProxyService, binding.btnStopProxyService);
        }

        public override void OnStop()
        {
            base.OnStop();

            if (ProxyService.IsChangeSupportProxyServicesStatus)
            {
                ProxyService.IsChangeSupportProxyServicesStatus = false;
                SettingsHost.Save();
#if DEBUG
                Toast.Show("已保存勾选状态");
#endif
            }
        }

        void StartProxyButton_Click(bool start)
            => AndroidPlatformServiceImpl.StartOrStopForegroundService(
                RequireActivity(), nameof(ProxyService), start);

        protected override bool OnClick(View view)
        {
            if (view.Id == Resource.Id.btnStartProxyService)
            {
                StartProxyButton_Click(true);
                return true;
            }
            else if (view.Id == Resource.Id.btnStopProxyService)
            {
                StartProxyButton_Click(false);
                return true;
            }
            return base.OnClick(view);
        }

        void SwipeRefreshLayout.IOnRefreshListener.OnRefresh()
        {
            binding!.swipeRefreshLayout.Refreshing = false;
            ViewModel!.RefreshButton_Click();
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            HasOptionsMenu = true;
        }

        MenuBuilder? menuBuilder;
        public override void OnCreateOptionsMenu(IMenu menu, MenuInflater inflater)
        {
            inflater.Inflate(Resource.Menu.community_fix_toolbar_menu, menu);
            menuBuilder = menu.SetOptionalIconsVisible();
            if (menuBuilder != null)
            {
                SetMenuTitle();
            }
        }

        void SetMenuTitle() => menuBuilder.SetMenuTitle(ToString2, MenuIdResToEnum);

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            var actionItem = MenuIdResToEnum(item.ItemId);
            if (actionItem.IsDefined())
            {
                switch (actionItem)
                {
                    case ActionItem.CertificateExport:
                        ViewModel!.ExportCertificateFile();
                        return true;
                    case ActionItem.GoToSystemSecuritySettings:
                        GoToPlatformPages.SystemSettingsSecurity(RequireContext());
                        return true;
                    case ActionItem.CertificateInstall:
                        InstallCertificate();
                        return true;
                    case ActionItem.CertificateUninstall:
                        //Test();
                        ViewModel!.UninstallCertificateShowTips();
                        return true;
                }
                ViewModel!.MenuItemClick(actionItem);
                return true;
            }
            return base.OnOptionsItemSelected(item);
        }

        static ActionItem MenuIdResToEnum(int resId)
        {
            if (resId == Resource.Id.menu_settings_proxy)
            {
                return ActionItem.ProxySettings;
            }
            else if (resId == Resource.Id.menu_export_certificate_file)
            {
                return ActionItem.CertificateExport;
            }
            else if (resId == Resource.Id.menu_settings_security)
            {
                return ActionItem.GoToSystemSecuritySettings;
            }
            else if (resId == Resource.Id.menu_install_certificate_file)
            {
                return ActionItem.CertificateInstall;
            }
            else if (resId == Resource.Id.menu_uninstall_certificate_file)
            {
                return ActionItem.CertificateUninstall;
            }
            return default;
        }

        void InstallCertificate() => ViewModel!.ExportCertificateFile(cefFilePath =>
        {
            GoToPlatformPages.InstallCertificate(RequireContext(), cefFilePath, IHttpProxyService.RootCertificateName);
        });

#if DEBUG
        async void Test()
        {
            if (!ProxyService.Current.ProxyStatus) return;
            var s = IHttpProxyService.Instance;
            Xamarin.Android.Net.AndroidClientHandler handler = new();
            handler.Proxy = new WebProxy(s.ProxyIp.ToString(), s.ProxyPort);
            var certFilePath = s.GetCerFilePathGeneratedWhenNoFileExists();
            if (certFilePath != null)
                handler.AddTrustedCert(File.OpenRead(certFilePath));
            using var client = new HttpClient(handler);
            using var rsp = await client.SendAsync(new HttpRequestMessage(HttpMethod.Get,
                "https://steamcommunity.com"));
            var html = await rsp.Content.ReadAsStringAsync();
            Toast.Show($"StatusCode: {rsp.StatusCode}, Length: {rsp.Content.Headers.ContentLength}");
        }
#endif
    }
}
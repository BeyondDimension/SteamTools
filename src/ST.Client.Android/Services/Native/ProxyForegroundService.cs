using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using System.Net;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.Services.Implementation;
using static AndroidX.Activity.Result.ActivityResultTask;
using static System.Properties.ThisAssembly;
using static System.Application.Services.Native.IServiceBase;
using System.Threading.Tasks;
using AndroidNetVpnService = Android.Net.VpnService;

// https://docs.microsoft.com/zh-cn/xamarin/android/app-fundamentals/services/creating-a-service/started-services

namespace System.Application.Services.Native
{
    static partial class ProxyForegroundService
    {
        public static async void StartOrStop(Activity activity, bool startOrStop)
        {
            if (ProxySettings.IsVpnMode.Value)
            {
                if (startOrStop)
                {
                    // 调用 VpnService.prepare() 以询问权限（需要时）
                    var intent = AndroidNetVpnService.Prepare(activity);
                    if (intent != null)
                    {
                        void OnResult(Intent intent)
                            => activity.StartOrStopForegroundService<VpnService>(true);
                        await IntermediateActivity.StartAsync(intent,
                            requestCodeVpnService, onResult: OnResult);
                        return;
                    }
                }
                // 不需要授权则直接启动
                activity.StartOrStopForegroundService<VpnService>(startOrStop);
            }
            else
            {
                activity.StartOrStopForegroundService<Service>(startOrStop);
            }
        }

        /// <summary>
        /// 用于本地加速的代理前台服务
        /// </summary>
        [Register(JavaPackageConstants.Services + TAG)]
        [Service]
        partial class Service : ForegroundService
        {
            const string TAG = nameof(ProxyForegroundService);

            public override void OnStart()
            {
                base.OnStart();
                ProxyService.Current.ProxyStatus = true;
            }

            public override void OnStop()
            {
                base.OnStop();
                ProxyService.Current.ProxyStatus = false;
            }
        }

        /// <summary>
        /// 使用 虚拟专用网 (VPN) 进行本地加速的代理前台服务
        /// <para>https://developer.android.google.cn/guide/topics/connectivity/vpn?hl=zh_cn</para>
        /// </summary>
        [Register(JavaPackageConstants.Services + TAG)]
        [Service(Permission = Manifest.Permission.BindVpnService, Exported = true)]
        [IntentFilter(new[] { "android.net.VpnService" })]
        sealed partial class VpnService /*: AndroidNetVpnService*/
        {
            const string TAG = nameof(ProxyForegroundService) + "_VPN";

            //bool isStart;
            //ParcelFileDescriptor? localTunnel;

            //public override void OnCreate()
            //{
            //    jni_init();
            //    base.OnCreate();
            //}

            //public void OnStart()
            //{
            //    if (isStart) return;
            //    isStart = true;

            //    IForegroundService f = this;
            //    AndroidNotificationServiceImpl.Instance.StartForeground(this, f.NotificationType, f.NotificationText, f.NotificationEntranceAction);

            //    ProxyService.Current.ProxyStatus = true;

            //    if (localTunnel != null) return;

            //    // Configure a new interface from our VpnService instance. This must be done
            //    // from inside a VpnService.
            //    var builder = new Builder(this).SetSession(AssemblyTrademark);

            //    // Create a local TUN interface using predetermined addresses. In your app,
            //    // you typically use values returned from the VPN gateway during handshaking.
            //    builder.AddAddress("10.1.10.1", 32);
            //    builder.AddAddress("fd00:1:fd00:1:fd00:1:fd00:1", 128);
            //    builder.AddRoute("0.0.0.0", 0);
            //    builder.AddRoute("0:0:0:0:0:0:0:0", 0);

            //    //var dnss = GetDefaultDNS();
            //    //Array.ForEach(dnss, x => builder.AddDnsServer(x));

            //    int mtu = jni_get_mtu();
            //    builder.SetMtu(mtu);

            //    var pkg = PackageName!;
            //    builder.AddDisallowedApplication(pkg);

            //    localTunnel = builder.Establish()!;

            //    var httpProxyService = IHttpProxyService.Instance;
            //    var address = httpProxyService.ProxyIp;
            //    var port = httpProxyService.ProxyPort;
            //    jni_start(localTunnel.Fd, false, 3, address!.ToString(), port);
            //}

            //public void OnStop()
            //{
            //    if (!isStart) return;
            //    isStart = false;

            //    StopForeground(true);

            //    if (localTunnel != null)
            //    {
            //        try
            //        {
            //            jni_stop(localTunnel.Fd);
            //        }
            //        catch (Java.Lang.Throwable ex)
            //        {
            //            Log.Error(TAG, ex, "jni_stop");
            //            jni_stop(-1);
            //        }
            //        try
            //        {
            //            localTunnel.Close();
            //        }
            //        catch (Java.IO.IOException ex)
            //        {
            //            Log.Error(TAG, ex, "localTunnel.Close");
            //        }
            //        localTunnel.Dispose();
            //        localTunnel = null;
            //    }

            //    ProxyService.Current.ProxyStatus = false;
            //}

            //[return: GeneratedEnum]
            //public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
            //{
            //    if (intent != null)
            //    {
            //        var action = intent.Action;
            //        switch (action)
            //        {
            //            case START:
            //                OnStart();
            //                return StartCommandResult.RedeliverIntent;
            //            case STOP:
            //            default:
            //                OnStop();
            //                StopSelf();
            //                break;
            //        }
            //    }
            //    return StartCommandResult.NotSticky;
            //}

            //public override void OnRevoke()
            //{
            //    OnStop();
            //    base.OnRevoke();
            //}

            //public override void OnDestroy()
            //{
            //    OnStop();
            //    base.OnDestroy();
            //}

            //string[] GetDefaultDNS()
            //{
            //    //return new[] {
            //    //    IDnsAnalysisService.PrimaryDNS_Ali,
            //    //    IDnsAnalysisService.SecondaryDNS_Ali,
            //    //};
            //    var context = Android.App.Application.Context;
            //    string? dns1 = null, dns2 = null;
            //    if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1)
            //    {
            //        var cm = context.GetSystemService<ConnectivityManager>();
            //        var an = cm.ActiveNetwork;
            //        if (an != null)
            //        {
            //            var lp = cm.GetLinkProperties(an);
            //            if (lp != null)
            //            {
            //                var dns = lp.DnsServers;
            //                if (dns != null)
            //                {
            //                    if (dns.Count > 0)
            //                        dns1 = dns[0].HostAddress;
            //                    if (dns.Count > 1)
            //                        dns2 = dns[1].HostAddress;
            //                }
            //            }
            //        }
            //    }
            //    else
            //    {
            //        dns1 = jni_getprop("net.dns1");
            //        dns2 = jni_getprop("net.dns2");
            //    }
            //    return new[]
            //    {
            //    string.IsNullOrEmpty(dns1) ? IDnsAnalysisService.PrimaryDNS_Ali : dns1!,
            //    string.IsNullOrEmpty(dns2) ? IDnsAnalysisService.SecondaryDNS_Ali : dns2!,
            //};
            //}
        }

        const NotificationType notificationType
            = NotificationType.ProxyForegroundService;
        static string NotificationText
            => AppResources.ProxyForegroundService_NotificationText;
        static string? NotificationEntranceAction
            => nameof(TabItemViewModel.TabItemId.CommunityProxy);

        partial class Service
        {
            public override NotificationType NotificationType
                => notificationType;

            public override string NotificationText
                => ProxyForegroundService.NotificationText;

            public override string? NotificationEntranceAction
                => ProxyForegroundService.NotificationEntranceAction;
        }

        partial class VpnService : IForegroundService
        {
            public NotificationType NotificationType => notificationType;

            public string NotificationText
                => ProxyForegroundService.NotificationText;

            public string? NotificationEntranceAction
                => ProxyForegroundService.NotificationEntranceAction;
        }
    }
}

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        bool IPlatformService.SetAsSystemProxy(bool state, IPAddress? ip, int port)
        {
#if DEBUG
            Toast.Show($"SystemProxy: {ip}:{port}");
#endif
            return true;
        }
    }
}
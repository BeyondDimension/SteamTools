using Android;
using Android.App;
using Android.Content;
using Android.Net;
using Android.OS;
using Android.Runtime;
using System.Application.Services.Native;
using System.Net;
using static AndroidX.Activity.Result.ActivityResultTask;
using XEPlatform = Xamarin.Essentials.Platform;
using static System.Properties.ThisAssembly;
using System.Application.Settings;

namespace System.Application.Services.Native
{
    /// <summary>
    /// 使用 虚拟专用网 (VPN) 进行网络加速
    /// <para>https://developer.android.google.cn/guide/topics/connectivity/vpn?hl=zh_cn</para>
    /// </summary>
    [Register(JavaPackageConstants.Services + nameof(CommunityFixVpnService))]
    [Service(Permission = Manifest.Permission.BindVpnService, Exported = true)]
    [IntentFilter(new[] { "android.net.VpnService" })]
    internal sealed partial class CommunityFixVpnService : VpnService
    {
        const string TAG = nameof(CommunityFixVpnService);

        /// <summary>
        /// 启动 VPN 服务
        /// </summary>
        /// <param name="activity"></param>
        /// <param name="startOrStop"></param>
        static void StartCore(Activity activity, bool startOrStop, IPAddress? ip, int port)
        {
            var intent = GetServiceIntent(activity)
                .SetAction(startOrStop ? ACTION_CONNECT : ACTION_DISCONNECT);
            if (startOrStop)
            {
                intent.PutExtra(nameof(IPEndPoint.Address), ip?.ToString());
                intent.PutExtra(nameof(IPEndPoint.Port), port);
            }
            activity.StartService(intent);
            static Intent GetServiceIntent(Context context)
                => new(context, typeof(CommunityFixVpnService));
        }

        /// <summary>
        /// 调用 VpnService.prepare() 以询问权限（需要时）与 启动 VPN 服务
        /// </summary>
        /// <param name="activity"></param>
        public static async void Start(Activity activity, bool startOrStop = true, IPAddress? ip = null, int port = default)
        {
            if (startOrStop)
            {
                // 调用 VpnService.prepare() 以询问权限（需要时）
                var intent = Prepare(activity);
                if (intent != null)
                {
                    void OnResult(Intent intent) => StartCore(activity, true, ip, port);
                    await IntermediateActivity.StartAsync(intent, requestCodeVpnService, onResult: OnResult);
                    return;
                }
            }
            // 不需要授权则直接启动
            StartCore(activity, startOrStop, ip, port);
        }

        const string ACTION_CONNECT = JavaPackageConstants.Services + nameof(CommunityFixVpnService) + ".START";
        const string ACTION_DISCONNECT = JavaPackageConstants.Services + nameof(CommunityFixVpnService) + ".STOP";

        ParcelFileDescriptor? localTunnel;
        string? address;
        int port;

        public override void OnCreate()
        {
            jni_init();
            base.OnCreate();
        }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            if (intent != null)
            {
                if (ACTION_DISCONNECT.Equals(intent.Action))
                {
                    Disconnect();
                }
                else
                {
                    address = intent.GetStringExtra(nameof(IPEndPoint.Address));
                    port = intent.GetIntExtra(nameof(IPEndPoint.Port), default);
                    Connect();
                }
            }
            return StartCommandResult.NotSticky; // 重启该服务还需启动代理服务
        }

        void Connect()
        {
            if (localTunnel != null) return;

            // Configure a new interface from our VpnService instance. This must be done
            // from inside a VpnService.
            var builder = new Builder(this).SetSession(AssemblyTrademark);

            // Create a local TUN interface using predetermined addresses. In your app,
            // you typically use values returned from the VPN gateway during handshaking.
            builder.AddAddress("10.1.10.1", 32);
            //builder.AddAddress("fd00:1:fd00:1:fd00:1:fd00:1", 128);
            builder.AddRoute("0.0.0.0", 0);
            //builder.AddRoute("0:0:0:0:0:0:0:0", 0);

            var dnss = GetDefaultDNS();
            Array.ForEach(dnss, x => builder.AddDnsServer(x));

            int mtu = jni_get_mtu();
            builder.SetMtu(mtu);

            var pkg = PackageName!;
            builder.AddDisallowedApplication(pkg);

            localTunnel = builder.Establish()!;

            jni_start(localTunnel.Fd, false, 3, address!, port);
        }

        void Disconnect()
        {
            if (localTunnel != null)
            {
                try
                {
                    jni_stop(localTunnel.Fd);
                }
                catch (Java.Lang.Throwable ex)
                {
                    Log.Error(TAG, ex, "jni_stop");
                    jni_stop(-1);
                }
                try
                {
                    localTunnel.Close();
                }
                catch (Java.IO.IOException ex)
                {
                    Log.Error(TAG, ex, "localTunnel.Close");
                }
                localTunnel.Dispose();
                localTunnel = null;
            }
            StopForeground(true);
        }

        public override void OnRevoke()
        {
            Disconnect();
            base.OnRevoke();
        }

        public override void OnDestroy()
        {
            Disconnect();
            jni_done();
            base.OnDestroy();
        }

        string[] GetDefaultDNS()
        {
            //return new[] {
            //    IDnsAnalysisService.PrimaryDNS_Ali,
            //    IDnsAnalysisService.SecondaryDNS_Ali,
            //};
            var context = Android.App.Application.Context;
            string? dns1 = null, dns2 = null;
            if (Build.VERSION.SdkInt > BuildVersionCodes.NMr1)
            {
                var cm = context.GetSystemService<ConnectivityManager>();
                var an = cm.ActiveNetwork;
                if (an != null)
                {
                    var lp = cm.GetLinkProperties(an);
                    if (lp != null)
                    {
                        var dns = lp.DnsServers;
                        if (dns != null)
                        {
                            if (dns.Count > 0)
                                dns1 = dns[0].HostAddress;
                            if (dns.Count > 1)
                                dns2 = dns[1].HostAddress;
                        }
                    }
                }
            }
            else
            {
                dns1 = jni_getprop("net.dns1");
                dns2 = jni_getprop("net.dns2");
            }
            return new[]
            {
                string.IsNullOrEmpty(dns1) ? IDnsAnalysisService.PrimaryDNS_Ali : dns1!,
                string.IsNullOrEmpty(dns2) ? IDnsAnalysisService.SecondaryDNS_Ali : dns2!,
            };
        }
    }
}

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        bool IPlatformService.SetAsSystemProxy(bool state, IPAddress? ip, int port)
        {
            if (ProxySettings.IsVpnMode.Value)
            {
                var activity = XEPlatform.CurrentActivity;
                CommunityFixVpnService.Start(activity, state, ip, port);
            }
#if DEBUG
            Toast.Show($"SystemProxy: {ip}:{port}");
#endif
            return true;
        }
    }
}
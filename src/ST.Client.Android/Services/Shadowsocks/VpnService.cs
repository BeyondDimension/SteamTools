#if SHADOWSOCKS
using Android.App;
using Android.Content;
using Android.Runtime;
using System.Application.Services.Implementation;
using static System.Application.Services.Native.IServiceBase;
using AndroidX.Core.App;
using static System.Properties.ThisAssembly;
using System.Collections.Generic;

namespace System.Application.Services.Native
{
    partial class ProxyForegroundService
    {
        partial class VpnService : Shadowsocks.VpnService
        {
            bool isStart;
            int notifyId;
            NotificationCompat.Builder? builder;
            NotificationManagerCompat? mNotificationManager;
            NotificationManagerCompat NotificationManager
            {
                get
                {
                    if (mNotificationManager == null)
                    {
                        mNotificationManager = NotificationManagerCompat.From(this);
                    }
                    return mNotificationManager;
                }
            }

            public override void OnStart()
            {
                if (isStart) return;
                isStart = true;

                // 先显示通知 UI
                IForegroundService f = this;
                (builder, mNotificationManager, notifyId) = AndroidNotificationServiceImpl.Instance.StartForeground(this, f.NotificationType, f.NotificationText, f.NotificationEntranceAction);

                // 启动本地代理服务
                ProxyService.Current.ProxyStatus = true;

                // 启动 VPN tun2socks
                base.OnStart();
            }

            public override void OnStop()
            {
                if (!isStart) return;
                isStart = false;

                // 停止通知栏 UI
                StopForeground(true);

                // 停止 VPN tun2socks
                base.OnStop();

                // 停止本地代理服务
                ProxyService.Current.ProxyStatus = false;
            }

            public override void OnRevoke()
            {
                OnStop();
                base.OnRevoke();
            }

            [return: GeneratedEnum]
            public override StartCommandResult OnStartCommand(Intent? intent, [GeneratedEnum] StartCommandFlags flags, int startId)
            {
                if (intent != null)
                {
                    var action = intent.Action;
                    switch (action)
                    {
                        case START:
                            OnStart();
                            return StartCommandResult.RedeliverIntent;
                        case STOP:
                        default:
                            OnStop();
                            StopSelf();
                            break;
                    }
                }
                return StartCommandResult.NotSticky;
            }

            public override string SocksServerAddress
            {
                get
                {
                    var s = ProxyService.Current;
                    return $"{s.ProxyIp}:{s.Socks5ProxyPortId}";
                }
            }

            const int VPN_MTU = 1500;
            const string VPN_MTU_STR = "1500";

            public override void OnConfigure(Builder builder)
            {
                builder.SetSession(AssemblyTrademark);
                //builder.AddDnsServer(IDnsAnalysisService.PrimaryDNS_Ali);
                // https://github.com/2dust/v2rayNG/blob/master/AndroidLibV2rayLite/CoreI/Status.go#L56
                builder.SetMtu(VPN_MTU);
                builder.AddAddress("26.26.26.1", 30);
                AddRouteByPassPrivateIPAddress(builder);
                builder.AddAddress("da26:2626::1", 126);
                builder.AddRoute("2000::", 3);
                builder.AddDisallowedApplication(PackageName!);
            }

            public override IList<string> Tun2socksArgs
            {
                get
                {
                    // https://github.com/2dust/v2rayNG/blob/master/AndroidLibV2rayLite/CoreI/Status.go#L28
                    return new List<string>
                    {
                        Tun2socksExecutablePath,
                        "--netif-ipaddr",
                        "26.26.26.2",
                        "--netif-netmask",
                        "255.255.255.252",
                        "--socks-server-addr",
                        SocksServerAddress,
                        "--tunmtu",
                        VPN_MTU_STR,
                        "--loglevel",
                        "notice",
                        "--enable-udprelay",
                        "--sock-path",
                        "sock_path",
                        "--netif-ip6addr",
                        "da26:2626::2"
                    };
                }
            }

#if DEBUG
            public override void OnError(string msg, Java.Lang.Throwable tr)
            {
                base.OnError(msg, tr);
            }

            public override string SockPath
            {
                get
                {
                    var value = base.SockPath;
                    Log.Info(nameof(VpnService), $"SockPath: {value}");
                    return value;
                }
            }
#endif
        }
    }
}
#endif

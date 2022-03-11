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

            public override string Session => AssemblyTrademark;

            public override string SocksServerAddress
            {
                get
                {
                    var s = ProxyService.Current;
                    return $"{s.ProxyIp}:{s.Socks5ProxyPortId}";
                }
            }

            public override void OnConfigure(Builder builder)
            {
                builder.AddDnsServer(IDnsAnalysisService.PrimaryDNS_Ali);
                builder.AddDisallowedApplication(PackageName!);
            }

#if DEBUG
            public override void OnError(string msg, Java.Lang.Throwable tr)
            {
                base.OnError(msg, tr);
            }

            public override IList<string> GetTun2socksArgs(int mtu, bool isSupportIpv6, string PRIVATE_VLAN4_ROUTER, string PRIVATE_VLAN6_ROUTER)
            {
                var args = base.GetTun2socksArgs(mtu, isSupportIpv6, PRIVATE_VLAN4_ROUTER, PRIVATE_VLAN6_ROUTER);
                var value = string.Join(' ', args);
                Log.Info(nameof(VpnService), $"Tun2socksArgs: {value}");
                return args;
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

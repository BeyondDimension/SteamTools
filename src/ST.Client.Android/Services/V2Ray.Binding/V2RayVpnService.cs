using V2RayNG;
using Android.App;
using Android.Content;
using Android.Runtime;
using System.Application.Services.Implementation;
using static System.Application.Services.Native.IServiceBase;
using SR = System.Application.Properties.SR;
using AndroidX.Core.App;
using static System.Properties.ThisAssembly;

namespace System.Application.Services.Native
{
    partial class ProxyForegroundService
    {
        partial class VpnService : V2RayVpnService, IV2RayServiceManager
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

            string IV2RayServiceManager.ConfigureFileContent
            {
                get
                {
                    var text = SR.v2rayPoint_configure;
                    var s = ProxyService.Current;
                    text = text.Replace("${ProxyIp}", s.ProxyIp.ToString());
                    text = text.Replace("${ProxyPort}", s.Socks5ProxyPortId.ToString());
                    return text;
                }
            }

            string IV2RayServiceManager.DomainName
            {
                get
                {
                    var s = ProxyService.Current;
                    return $"{s.ProxyIp}:{s.Socks5ProxyPortId}";
                }
            }

            bool IV2RayServiceManager.ForwardIpv6 => false;

            void IV2RayServiceManager.UpdateNotification(string? contentText, long proxyTraffic, long directTraffic)
            {
                if (builder == null) return;
                //if (proxyTraffic < NOTIFICATION_ICON_THRESHOLD && directTraffic < NOTIFICATION_ICON_THRESHOLD) {
                //    mBuilder?.setSmallIcon(R.drawable.ic_v)
                //} else if (proxyTraffic > directTraffic) {
                //    mBuilder?.setSmallIcon(R.drawable.ic_stat_proxy)
                //} else {
                //    mBuilder?.setSmallIcon(R.drawable.ic_stat_direct)
                //}
                builder.SetStyle(new NotificationCompat.BigTextStyle().BigText(contentText));
                builder.SetContentText(contentText);
                NotificationManager.Notify(notifyId, builder.Build());
            }
        }
    }
}

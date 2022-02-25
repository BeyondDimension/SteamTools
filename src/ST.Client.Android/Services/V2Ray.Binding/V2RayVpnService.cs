using V2RayNG;
using Android.App;
using Android.Content;
using Android.Runtime;
using System.Application.Services.Implementation;
using static System.Application.Services.Native.IServiceBase;
using SR = System.Application.Properties.SR;

namespace System.Application.Services.Native
{
    partial class ProxyForegroundService
    {
        partial class VpnService : V2RayVpnService, IV2RayServiceManager
        {
            bool isStart;

            public override void OnStart()
            {
                if (isStart) return;
                isStart = true;

                IForegroundService f = this;
                AndroidNotificationServiceImpl.Instance.StartForeground(this, f.NotificationType, f.NotificationText, f.NotificationEntranceAction);

                ProxyService.Current.ProxyStatus = true;

                base.OnStart();
            }

            public override void OnStop()
            {
                if (!isStart) return;
                isStart = false;

                StopForeground(true);

                base.OnStop();
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

            public string ConfigureFileContent
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

            public string DomainName
            {
                get
                {
                    var s = ProxyService.Current;
                    return $"{s.ProxyIp}:{s.Socks5ProxyPortId}";
                }
            }

            public bool ForwardIpv6 => false;

            public void CancelNotification()
            {
            }

            public void ShowNotification()
            {
            }

            public void StartSpeedNotification()
            {
            }
        }
    }
}

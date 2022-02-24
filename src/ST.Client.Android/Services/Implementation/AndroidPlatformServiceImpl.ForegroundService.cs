using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ASF_FS = System.Application.Services.Native.ArchiSteamFarmForegroundService;
using XEPlatform = Xamarin.Essentials.Platform;
using Android.App;
using System.Application.Services.Native;

namespace System.Application.Services.Implementation
{
    partial class AndroidPlatformServiceImpl
    {
        bool IPlatformService.UsePlatformForegroundService => true;

        public static void StartOrStopForegroundService(Activity activity, string serviceName, bool? startOrStop = null)
        {
            switch (serviceName)
            {
                case nameof(ASFService):
                    var s = ASFService.Current;
                    if (s.IsASFRunOrStoping) return;
                    var isASFRuning = s.IsASFRuning;
                    if (isASFRuning == startOrStop) return;
                    activity.StartOrStopForegroundService<ASF_FS>(!isASFRuning);
                    break;
                case nameof(ProxyService):
                    if (!startOrStop.HasValue) startOrStop = !ProxyService.Current.ProxyStatus;
                    ProxyForegroundService.StartOrStop(activity, startOrStop.Value);
                    break;
            }
        }

        async Task IPlatformService.StartOrStopForegroundServiceAsync(string serviceName, bool? startOrStop)
        {
            var activity = await XEPlatform.WaitForActivityAsync();
            StartOrStopForegroundService(activity, serviceName, startOrStop);
        }
    }
}

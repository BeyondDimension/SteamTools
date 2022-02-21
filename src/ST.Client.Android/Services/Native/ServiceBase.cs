using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using System.Application.Services.Implementation;
using static System.Application.Services.Native.IServiceBase;

namespace System.Application.Services.Native
{
    /// <summary>
    /// 平台原生服务接口
    /// </summary>
    public interface IServiceBase
    {
        /// <summary>
        /// 启动服务参数值
        /// </summary>
        public const string START = JavaPackageConstants.Services + nameof(IServiceBase) + ".START";

        /// <summary>
        /// 停止服务参数值
        /// </summary>
        public const string STOP = JavaPackageConstants.Services + nameof(IServiceBase) + ".STOP";

        /// <summary>
        /// 当服务启动时
        /// </summary>
        void OnStart();

        /// <summary>
        /// 当服务停止时
        /// </summary>
        void OnStop();
    }

    /// <summary>
    /// 平台原生服务基类
    /// </summary>
    public abstract class ServiceBase : Service, IServiceBase
    {
        public abstract void OnStart();

        public abstract void OnStop();

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
                        StopSelf();
                        break;
                }
            }
            return StartCommandResult.NotSticky;
        }

        public override void OnDestroy()
        {
            OnStop();
            base.OnDestroy();
        }

        public override IBinder? OnBind(Intent? intent) => default;
    }

    /// <summary>
    /// 平台原生前台服务接口
    /// </summary>
    public interface IForegroundService : IServiceBase
    {
        /// <summary>
        /// 通知栏显示的通知类型
        /// </summary>
        NotificationType NotificationType { get; }

        /// <summary>
        /// 通知栏显示的通知文本
        /// </summary>
        string NotificationText { get; }

        /// <summary>
        /// 通知栏点击启动活动的参数传递
        /// </summary>
        string? NotificationEntranceAction { get; }
    }

    public abstract class ForegroundService : ServiceBase, IForegroundService
    {
        public abstract NotificationType NotificationType { get; }

        public abstract string NotificationText { get; }

        public virtual string? NotificationEntranceAction => null;

        public override void OnStart()
        {
            AndroidNotificationServiceImpl.Instance.StartForeground(this, NotificationType, NotificationText, NotificationEntranceAction);
        }

        public override void OnStop()
        {
            StopForeground(true);
        }
    }
}

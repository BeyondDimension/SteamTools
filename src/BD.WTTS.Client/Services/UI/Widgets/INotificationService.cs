// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface INotificationService
{
    static INotificationService Instance => Ioc.Get<INotificationService>();

    protected const string DefaultTitle = AssemblyInfo.Product;

    /// <summary>
    /// 获取是否有通知权限
    /// </summary>
    /// <returns></returns>
    bool AreNotificationsEnabled() => true;

    ///// <summary>
    ///// 显示本地通知，使用 new <see cref="NotificationBuilder"/>() 构建参数
    ///// </summary>
    ///// <param name="builder"></param>
    //void Notify(NotificationBuilder.IInterface builder)
    //    => Notify(builder.Content, builder.Type, builder.AutoCancel,
    //        builder.Title, builder.Click?.Entrance ?? default,
    //        builder.Click?.RequestUri);

    /// <summary>
    /// 显示本地通知
    /// </summary>
    /// <param name="text">通知内容</param>
    /// <param name="notificationType">通知类型</param>
    /// <param name="autoCancel"></param>
    /// <param name="title">通知标题</param>
    /// <param name="entrance">点击通知的入口点</param>
    /// <param name="requestUri">入口点为 <see cref="Entrance.Browser"/> 时的 HttpUrl</param>
    void Notify(
        string text,
        NotificationType notificationType,
        bool autoCancel = true,
        string? title = default,
        Entrance entrance = default,
        string? requestUri = default);
    //{
    //    var builder = new NotificationBuilder
    //    {
    //        Title = string.IsNullOrWhiteSpace(title) ? DefaultTitle : title,
    //        Content = text,
    //        AutoCancel = autoCancel,
    //        Click = entrance switch
    //        {
    //            Entrance.Main => new NotificationBuilderClickAction
    //            {
    //                Entrance = Entrance.Main,
    //            },
    //            Entrance.Browser => new NotificationBuilderClickAction
    //            {
    //                Entrance = Entrance.Browser,
    //                RequestUri = requestUri,
    //            },
    //            _ => null,
    //        },
    //    };
    //    Notify(builder);
    //}

    /// <summary>
    /// 取消通知
    /// </summary>
    /// <param name="notificationType"></param>
    void Cancel(NotificationType notificationType);

    /// <summary>
    /// 取消所有通知
    /// </summary>
    void CancelAll();

    /// <summary>
    /// 当前平台是否支持下载进度通知
    /// </summary>
    bool IsSupportNotifyDownload => false;

    /// <summary>
    /// 下载进度通知
    /// </summary>
    Progress<float> NotifyDownload(
        Func<string> text,
        NotificationType notificationType,
        string? title = default) => throw new PlatformNotSupportedException();

    ///// <summary>
    ///// 显示从服务端获取到通知纪录
    ///// </summary> 
    //static async void Notify(ActiveUserType type)
    //{
    //    if (type == ActiveUserType.OnStartup)
    //        await NotificationService.Current.GetNewsAsync();
    //}

    /// <summary>
    /// NotifyIcon / TrayIcon 右下角托盘菜单助手类
    /// </summary>
    abstract class NotifyIconHelper
    {
        protected NotifyIconHelper() => throw new NotSupportedException();

        /// <summary>
        /// 托盘初始化是否完成
        /// <para>注意：在 Windows 上托盘初始化之前调用气泡消息会导致托盘不显示</para>
        /// </summary>
        public static bool IsInitialized { get; protected set; }
    }

    interface ILifeCycle
    {
        static ILifeCycle? Instance => INotificationService.Instance is ILifeCycle value ? value : null;

        void OnStartup();

        void OnShutdown();
    }
}

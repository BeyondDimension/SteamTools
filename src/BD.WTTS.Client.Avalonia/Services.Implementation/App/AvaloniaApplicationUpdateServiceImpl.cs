using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IAppUpdateService"/>
sealed class AvaloniaApplicationUpdateServiceImpl : AppUpdateServiceBaseImpl
{
    public AvaloniaApplicationUpdateServiceImpl(
        IApplication application,
        INotificationService notificationService,
        IToast toast,
        IMicroServiceClient client,
        IOptions<AppSettings> options) : base(application, notificationService, toast, client, options)
    {

    }

    protected override bool HasActiveWindow()
    {
        var hasActiveWindow = App.Instance.HasActiveWindow();
        return hasActiveWindow;
    }
}

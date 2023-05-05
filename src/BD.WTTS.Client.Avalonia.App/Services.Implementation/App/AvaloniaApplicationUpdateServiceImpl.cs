using AppResources = BD.WTTS.Client.Resources.Strings;

namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="IApplicationUpdateService"/>
sealed class AvaloniaApplicationUpdateServiceImpl : ApplicationUpdateServiceBaseImpl
{
    readonly IWindowManager windowManager;

    public AvaloniaApplicationUpdateServiceImpl(
        IApplication application,
        INotificationService notificationService,
        IWindowManager windowManager,
        IToast toast,
        IMicroServiceClient client,
        IOptions<AppSettings> options) : base(application, notificationService, toast, client, options)
    {
        this.windowManager = windowManager;
    }

    protected override async Task ShowNewVersionWindowAsync()
    {
        await windowManager.ShowAsync(typeof(object), AppEndPoint.NewVersion, isParent: false);
    }

    public override async void OnMainOpenTryShowNewVersionWindow()
    {
        if (ShowNewVersionWindowOnMainOpen)
        {
            ShowNewVersionWindowOnMainOpen = false;
            await ShowNewVersionWindowAsync();
        }
    }

    protected override async void OnExistNewVersion()
    {
        var hasActiveWindow = App.Instance.HasActiveWindow();
        if (hasActiveWindow)
        {
            await ShowNewVersionWindowAsync();
        }
        else
        {
            ShowNewVersionWindowOnMainOpen = true;
            //notification.Notify(AppResources.NewVersionUpdateNotifyText_.Format(NewVersionInfo?.Version), NotificationType.NewVersion);
        }
    }

    protected override void OnExit()
    {
        App.Instance.CompositeDisposable.Dispose();
    }
}

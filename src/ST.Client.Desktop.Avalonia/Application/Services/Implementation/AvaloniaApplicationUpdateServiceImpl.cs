using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.UI;
using System.Application.UI.Resx;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IApplicationUpdateService"/>
    internal sealed class AvaloniaApplicationUpdateServiceImpl : ApplicationUpdateServiceBaseImpl
    {
        readonly IAvaloniaApplication app;
        readonly IWindowManager windowManager;
        public AvaloniaApplicationUpdateServiceImpl(
            IAvaloniaApplication app,
            INotificationService notificationService,
            IWindowManager windowManager,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options) : base(app, notificationService, toast, client, options)
        {
            this.app = app;
            this.windowManager = windowManager;
        }

        protected override async Task ShowNewVersionWindowAsync()
        {
            await windowManager.Show(typeof(object), CustomWindow.NewVersion, isParent: false);
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
            var hasActiveWindow = app.HasActiveWindow();
            if (hasActiveWindow)
            {
                await ShowNewVersionWindowAsync();
            }
            else
            {
                ShowNewVersionWindowOnMainOpen = true;
                notification.Notify(AppResources.NewVersionUpdateNotifyText_.Format(NewVersionInfo?.Version), NotificationType.NewVersion);
            }
        }

        protected override void OnExit()
        {
            app.CompositeDisposable.Dispose();
        }
    }
}
using Microsoft.Extensions.Options;
using System.Application.Models;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaDesktopAppUpdateServiceImpl : PlatformApplicationUpdateServiceImpl
    {
        readonly IDesktopApplication app;
        public AvaloniaDesktopAppUpdateServiceImpl(
            IDesktopApplication app,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options) : base(toast, client, options)
        {
            this.app = app;
        }

        protected override async void OnExistNewVersion()
        {
            var hasActiveWindow = IDesktopApplication.Instance.HasActiveWindow();
            if (hasActiveWindow)
            {
                await IShowWindowService.Instance.Show(typeof(object), CustomWindow.NewVersion, isParent: false);
            }
        }

        protected override void OnExit()
        {
            app.CompositeDisposable.Dispose();
        }
    }
}
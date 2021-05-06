using Microsoft.Extensions.Options;
using System.Application.Models;

namespace System.Application.Services.Implementation
{
    internal sealed class AvaloniaDesktopAppUpdateServiceImpl : DesktopAppUpdateServiceImpl
    {
        readonly IDesktopAppService app;
        public AvaloniaDesktopAppUpdateServiceImpl(
            IDesktopAppService app,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options) : base(toast, client, options)
        {
            this.app = app;
        }

        protected override async void OnExistNewVersion()
        {
            var hasActiveWindow = IDesktopAppService.Instance.HasActiveWindow();
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
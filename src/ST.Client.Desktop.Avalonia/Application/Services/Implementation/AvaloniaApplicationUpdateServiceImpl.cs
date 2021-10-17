using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.UI;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IApplicationUpdateService"/>
    internal sealed class AvaloniaApplicationUpdateServiceImpl : ApplicationUpdateServiceBaseImpl
    {
        readonly IAvaloniaApplication app;
        readonly IWindowManager windowManager;
        public AvaloniaApplicationUpdateServiceImpl(
            IAvaloniaApplication app,
            IWindowManager windowManager,
            IToast toast,
            ICloudServiceClient client,
            IOptions<AppSettings> options) : base(toast, client, options)
        {
            this.app = app;
            this.windowManager = windowManager;
        }

        protected override async void OnExistNewVersion()
        {
            var hasActiveWindow = app.HasActiveWindow();
            if (hasActiveWindow)
            {
                await windowManager.Show(typeof(object), CustomWindow.NewVersion, isParent: false);
            }
        }

        protected override void OnExit()
        {
            app.CompositeDisposable.Dispose();
        }
    }
}
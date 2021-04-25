using Avalonia.Controls.ApplicationLifetimes;
using Microsoft.Extensions.Options;
using System.Application.Models;
using AvaloniaApplication = Avalonia.Application;

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
            if (AvaloniaApplication.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (desktop.Windows.Any_Nullable(x => x.IsActive))
                {
                    await IShowWindowService.Instance.Show(typeof(object), CustomWindow.NewVersion);
                }
            }
        }

        protected override void OnExit()
        {
            app.CompositeDisposable.Dispose();
        }
    }
}
using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam, bool hasGUI, bool hasNotifyIcon)
        {
            if (OperatingSystem2.IsLinux)
            {
                services.AddSingleton<IHttpPlatformHelper, PlatformHttpPlatformHelper>();
                services.AddSingleton<LinuxDesktopPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<LinuxDesktopPlatformServiceImpl>());
                services.AddSingleton<IDesktopPlatformService>(s => s.GetRequiredService<LinuxDesktopPlatformServiceImpl>());
                if (hasNotifyIcon) services.AddNotifyIcon();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
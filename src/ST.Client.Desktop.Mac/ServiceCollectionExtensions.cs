using System;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Net.Http;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
            if (OperatingSystem2.IsMacOS)
            {
                services.AddSingleton<IHttpPlatformHelperService, MacClientHttpPlatformHelperServiceImpl>();
                services.AddSingleton(AppDelegate.Instance!);
                services.AddSingleton<MacPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MacPlatformServiceImpl>());
                services.AddSingleton<IEmailPlatformService>(s => s.GetRequiredService<MacPlatformServiceImpl>());
                //if (hasNotifyIcon) services.AddNotifyIcon();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
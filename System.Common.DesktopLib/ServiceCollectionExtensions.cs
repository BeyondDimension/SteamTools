using System;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services)
        {
            if (DI.Platform == Platform.Windows)
            {
                services.AddSingleton<IDesktopPlatformService, WindowsDesktopPlatformServiceImpl>();
            }
            else
            {
                services.AddSingleton<IDesktopPlatformService, DesktopPlatformServiceImpl>();
            }
            return services;
        }
    }
}
using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam)
        {
            if (DI.Platform == Platform.Apple)
            {
                services.AddSingleton<AppDelegate>();
                services.AddSingleton<IDesktopPlatformService, MacDesktopPlatformServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
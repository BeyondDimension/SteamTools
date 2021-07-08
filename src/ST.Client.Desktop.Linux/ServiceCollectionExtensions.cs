using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam, bool hasGUI)
        {
            if (DI.Platform == Platform.Linux)
            {
                services.AddSingleton<IHttpPlatformHelper, PlatformHttpPlatformHelper>();
                services.AddSingleton<LinuxDesktopPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<LinuxDesktopPlatformServiceImpl>());
                services.AddSingleton<IDesktopPlatformService>(s => s.GetRequiredService<LinuxDesktopPlatformServiceImpl>());
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
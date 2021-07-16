using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam, bool hasGUI)
        {
            if (DI.Platform == Platform.Apple)
            {
                services.AddSingleton<IHttpPlatformHelper, PlatformHttpPlatformHelper>();
                services.AddSingleton(AppDelegateHelper.Instance!);
                services.AddSingleton<MacDesktopPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MacDesktopPlatformServiceImpl>());
                services.AddSingleton<IDesktopPlatformService>(s => s.GetRequiredService<MacDesktopPlatformServiceImpl>());
                services.AddSingleton<IEmailPlatformService>(s => s.GetRequiredService<MacDesktopPlatformServiceImpl>());
                services.AddSingleton<ISystemJumpListService, SystemJumpListServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
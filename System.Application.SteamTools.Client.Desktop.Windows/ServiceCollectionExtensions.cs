using System;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加桌面平台服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services)
        {
            if (DI.Platform == Platform.Windows)
            {
                services.AddSingleton<IDesktopPlatformService, WindowsDesktopPlatformServiceImpl>();
                services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
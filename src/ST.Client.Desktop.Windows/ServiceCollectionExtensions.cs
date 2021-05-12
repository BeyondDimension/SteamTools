using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Security.Cryptography;

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
                services.AddSingleton<WindowsProtectedData>();
                services.AddSingleton<IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                services.AddSingleton<ILocalDataProtectionProvider.IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                if (Environment.OSVersion.Version.Major >= 10)
                {
                    services.AddSingleton<ILocalDataProtectionProvider.IDataProtectionProvider, Windows10DataProtectionProvider>();
                }
                services.AddSingleton<ISystemWindowApiService, SystemWindowApiServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }
    }
}
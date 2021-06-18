using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加桌面平台服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam)
        {
            if (DI.Platform == Platform.Windows)
            {
                services.AddSingleton<IHttpPlatformHelper, PlatformHttpPlatformHelper>();
                services.AddSingleton<IDesktopPlatformService, WindowsDesktopPlatformServiceImpl>();
                if (hasSteam)
                {
                    services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
                }
                services.AddSingleton<WindowsProtectedData>();
                services.AddSingleton<IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                services.AddSingleton<ILocalDataProtectionProvider.IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                if (DI.IsWindows10OrLater)
                {
                    services.AddSingleton<ILocalDataProtectionProvider.IDataProtectionProvider, Windows10DataProtectionProvider>();
                }
                services.AddSingleton<ISystemWindowApiService, SystemWindowApiServiceImpl>();
                services.AddSingleton<ISystemJumpListService, SystemJumpListServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            services.AddMSAppCenterApplicationSettings();
            return services;
        }
    }
}
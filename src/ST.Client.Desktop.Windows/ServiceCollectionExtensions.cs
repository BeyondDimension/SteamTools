using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDesktopPlatformService(this IServiceCollection services, bool hasSteam, bool hasGUI, bool hasNotifyIcon)
        {
            if (OperatingSystem2.IsWindows)
            {
                services.AddSingleton<IHttpPlatformHelper, PlatformHttpPlatformHelper>();
                services.AddSingleton<WindowsDesktopPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<WindowsDesktopPlatformServiceImpl>());
                services.AddSingleton<IDesktopPlatformService>(s => s.GetRequiredService<WindowsDesktopPlatformServiceImpl>());
                services.AddSingleton<IEmailPlatformService>(s => s.GetRequiredService<WindowsDesktopPlatformServiceImpl>());
                if (hasSteam)
                {
                    services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
                }
                if (hasGUI)
                {
                    services.AddSingleton<IBiometricService, PlatformBiometricServiceImpl>();
                }
                services.AddSingleton<WindowsProtectedData>();
                services.AddSingleton<IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                services.AddSingleton<ILocalDataProtectionProvider.IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                if (OperatingSystem2.IsWindows10AtLeast)
                {
                    services.AddSingleton<ILocalDataProtectionProvider.IDataProtectionProvider, Windows10DataProtectionProvider>();
                }
                services.AddSingleton<INativeWindowApiService, NativeWindowApiServiceImpl>();
                services.AddSingleton<IJumpListService, JumpListServiceImpl>();
                if (hasNotifyIcon) services.AddNotifyIcon();
                //services.AddSingleton<AvaloniaFontManagerImpl, WindowsAvaloniaFontManagerImpl>();
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
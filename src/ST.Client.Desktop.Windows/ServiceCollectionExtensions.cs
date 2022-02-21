using System;
using System.Application;
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
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
#pragma warning disable CA1416 // 验证平台兼容性
            if (OperatingSystem2.IsWindows)
            {
                services.AddSingleton<IHttpPlatformHelperService, WindowsClientHttpPlatformHelperServiceImpl>();
                services.AddSingleton<WindowsPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<WindowsPlatformServiceImpl>());
                if (options.HasSteam)
                {
                    services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
                }
                if (options.HasGUI)
                {
                    services.AddSingleton<IBiometricService, PlatformBiometricServiceImpl>();
                }
                services.AddSingleton<WindowsProtectedData>();
                services.AddSingleton<IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                services.AddSingleton<ILocalDataProtectionProvider.IProtectedData>(s => s.GetRequiredService<WindowsProtectedData>());
                if (OperatingSystem2.IsWindows10AtLeast)
                {
                    services.AddSingleton<IEmailPlatformService>(s => s.GetRequiredService<WindowsPlatformServiceImpl>());
                    services.AddSingleton<ILocalDataProtectionProvider.IDataProtectionProvider, Windows10DataProtectionProvider>();
                }
                services.AddSingleton<INativeWindowApiService, NativeWindowApiServiceImpl>();
                if (Windows10JumpListServiceImpl.IsSupported)
                {
                    services.AddSingleton<IJumpListService, Windows10JumpListServiceImpl>();
                }
                else
                {
                    services.AddSingleton<IJumpListService, JumpListServiceImpl>();
                }
                if (options.HasMainProcessRequired)
                {
                    services.AddSingleton(typeof(NotifyIcon), NotifyIcon.ImplType);
                }
                //services.AddSingleton<AvaloniaFontManagerImpl, WindowsAvaloniaFontManagerImpl>();
                services.AddSingleton<ISevenZipHelper, SevenZipHelper>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            services.AddMSAppCenterApplicationSettings();
            return services;
#pragma warning restore CA1416 // 验证平台兼容性
        }
    }
}
#if WINDOWS
using System.Windows;
#endif

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddPlatformService(
        this IServiceCollection services,
        Startup options)
    {
#if LINUX
        services.AddSingleton<IHttpPlatformHelperService, LinuxClientHttpPlatformHelperServiceImpl>();
        services.AddSingleton<IPlatformService, LinuxPlatformServiceImpl>();
#elif MACOS || MACCATALYST || IOS
        services.AddSingleton<IHttpPlatformHelperService, MacCatalystClientHttpPlatformHelperServiceImpl>();
        services.AddSingleton<IPlatformService, MacCatalystPlatformServiceImpl>();
        services.AddSingleton<INotificationService, MacCatalystNotificationServiceImpl>();
#elif WINDOWS
        services.AddSingleton<IHttpPlatformHelperService, WindowsClientHttpPlatformHelperServiceImpl>();
        services.AddSingleton<WindowsPlatformServiceImpl>();
        services.AddSingleton<IPlatformService>(static s => s.GetRequiredService<WindowsPlatformServiceImpl>());
        services.AddSingleton<IRegistryService>(static s => s.GetRequiredService<WindowsPlatformServiceImpl>());
        services.AddSingleton<ILocalDataProtectionProvider.IProtectedData, WindowsProtectedData>();
        services.AddSingleton<ILocalDataProtectionProvider.IDataProtectionProvider, Windows10DataProtectionProvider>();
        //if (options.IsMainProcess)
        //{
        //    services.AddSingleton<NotifyIcon, WindowsNotifyIcon>();
        //}
        services.AddSingleton<ISevenZipHelper, SevenZipHelper>();
        if (!DesktopBridge.IsRunningAsUwp && OperatingSystem.IsWindowsVersionAtLeast(10, 0, 17763))
        {
            // 桌面 (MSIX) 中 无法处理通知点击事件
            services.AddSingleton<INotificationService, Windows10NotificationServiceImpl>();
        }
        else
        {
            services.AddSingleton<INotificationService, WindowsNotificationServiceImpl>();
        }
#else
        throw new PlatformNotSupportedException();
#endif
        return services;
    }
}
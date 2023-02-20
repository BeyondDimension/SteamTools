// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddPlatformService(
        this IServiceCollection services,
        StartupOptions options)
    {
#if LINUX
        services.AddSingleton<IHttpPlatformHelperService, LinuxClientHttpPlatformHelperServiceImpl>();
        services.AddSingleton<LinuxPlatformServiceImpl>();
        services.AddSingleton<IPlatformService>(s => s.GetRequiredService<LinuxPlatformServiceImpl>());
        if (options.HasSteam)
        {
            services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
        }
#elif MACOS || MACCATALYST || IOS
        services.AddSingleton<IHttpPlatformHelperService, MacCatalystClientHttpPlatformHelperServiceImpl>();
        services.AddSingleton<MacCatalystPlatformServiceImpl>();
        services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MacCatalystPlatformServiceImpl>());
        services.AddSingleton<INotificationService, MacCatalystNotificationServiceImpl>();
#else
        throw new PlatformNotSupportedException();
#endif
        if (options.HasSteam)
        {
#if WINDOWS || MACOS || MACCATALYST || IOS || LINUX
            services.AddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
#endif
        }
        return services;
    }
}
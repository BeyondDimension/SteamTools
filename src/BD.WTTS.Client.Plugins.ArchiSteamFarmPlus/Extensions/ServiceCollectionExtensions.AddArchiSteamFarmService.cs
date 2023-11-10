// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 添加 ArchiSteamFarm 服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddArchiSteamFarmService(this IServiceCollection services)
    {
        services.AddSingleton<IArchiSteamFarmWebApiService, ArchiSteamFarmWebApiServiceImpl>();
        services.AddSingleton<IArchiSteamFarmService, ArchiSteamFarmServiceImpl>();
        //services.AddSingleton<IArchiSteamFarmHelperService>(s => s.GetRequiredService<IArchiSteamFarmService>());

        return services;
    }

#endif
}
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    /// <summary>
    /// 尝试添加 Steamworks LocalApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddSteamworksLocalApiService(this IServiceCollection services)
    {
        services.TryAddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
        return services;
    }

#endif

    /// <summary>
    /// 添加 SteamDb WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamDbWebApiService(this IServiceCollection services)
    {
        services.AddSingleton<ISteamDbWebApiService, SteamDbWebApiServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 SteamGridDB WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamGridDBWebApiService(this IServiceCollection services)
    {
        services.AddSingleton<ISteamGridDBWebApiServiceImpl, SteamGridDBWebApiServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加 Steamworks WebApi Service
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamworksWebApiService(this IServiceCollection services)
    {
        services.AddSingleton<ISteamworksWebApiService, SteamworksWebApiServiceImpl>();
        return services;
    }
}
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    /// <summary>
    /// 添加 Steam 相关助手、工具类服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSteamService(this IServiceCollection services)
    {
        services.AddSingleton<ISteamService, SteamServiceImpl>();
        return services;
    }
#endif
}
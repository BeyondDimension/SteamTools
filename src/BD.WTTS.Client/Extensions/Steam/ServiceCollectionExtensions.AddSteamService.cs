// ReSharper disable once CheckNamespace
using BD.SteamClient.Services.Implementation;

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
    public static IServiceCollection AddSteamService2(this IServiceCollection services)
    {
        services.AddSingleton<ISteamService, SteamServiceImpl2>();
        return services;
    }
#endif
}
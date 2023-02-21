// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    /// <summary>
    /// 添加 hosts 文件助手服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddHostsFileService(this IServiceCollection services)
    {
        services.AddSingleton<IHostsFileService, HostsFileServiceImpl>();
        return services;
    }
#endif
}
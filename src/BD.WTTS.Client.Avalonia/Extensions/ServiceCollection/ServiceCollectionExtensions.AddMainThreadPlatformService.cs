// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加主线程助手类(MainThread)服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddMainThreadPlatformService(this IServiceCollection services)
    {
        services.AddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
        return services;
    }
}
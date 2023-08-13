#if AVALONIA
using WindowManagerImpl_ = BD.WTTS.Services.Implementation.AvaloniaWindowManagerImpl;
#endif

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加窗口管理服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddWindowManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IWindowManager, WindowManagerImpl_>();
        return services;
    }

    /// <summary>
    /// 添加内容导航服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddNavigationService(this IServiceCollection services)
    {
        services.TryAddSingleton<INavigationService, NavigationService>();
        return services;
    }
}
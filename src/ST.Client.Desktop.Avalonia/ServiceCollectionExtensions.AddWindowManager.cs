using System.Application.Services;
#if MAUI
using WindowManagerImpl = System.Application.Services.Implementation.MauiWindowManagerImpl;
#elif __ANDROID__
using WindowManagerImpl = System.Application.Services.Implementation.AndroidWindowManagerImpl;
#elif AVALONIA
using WindowManagerImpl = System.Application.Services.Implementation.AvaloniaWindowManagerImpl;
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
    public static IServiceCollection AddWindowManager(this IServiceCollection services)
    {
        services.AddSingleton<IWindowManager, WindowManagerImpl>();
        return services;
    }
}
#if AVALONIA
using AppUpdateServiceImpl = BD.WTTS.Services.Implementation.AvaloniaApplicationUpdateServiceImpl;
#else
using AppUpdateServiceImpl = BD.WTTS.Services.Implementation.ApplicationUpdateServiceImpl;
#endif

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加应用程序更新服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddApplicationUpdateService(this IServiceCollection services)
    {
        services.AddSingleton<IAppUpdateService, AppUpdateServiceImpl>();
        return services;
    }
}
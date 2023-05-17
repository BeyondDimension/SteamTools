#if AVALONIA
using Avalonia.Controls.Notifications;
using ToastServiceImpl_ = BD.WTTS.Services.Implementation.AvaloniaToastServiceImpl;
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
    public static IServiceCollection TryToastService(this IServiceCollection services)
    {
        services.TryAddSingleton<IToastService, ToastServiceImpl_>();
        return services;
    }
}
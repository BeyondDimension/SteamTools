// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 尝试添加由 NotifyIcon 实现的 <see cref="INotificationService"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddNotificationService(this IServiceCollection services)
    {
        services.TryAddSingleton<INotificationService, NotificationServiceImpl>();
        return services;
    }
}
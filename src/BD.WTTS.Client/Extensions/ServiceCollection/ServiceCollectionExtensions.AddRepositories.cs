// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [BD.Mobius(
"""
services.AddSingleton<IUserRepository, UserRepository>();
services.AddSingleton<IRequestCacheRepository, RequestCacheRepository>();
""", Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddSingleton<IUserRepository, UserRepository>();
        services.AddSingleton<INotificationRepository, NotificationRepository>();
        services.AddSingleton<IRequestCacheRepository, RequestCacheRepository>();
        return services;
    }
}
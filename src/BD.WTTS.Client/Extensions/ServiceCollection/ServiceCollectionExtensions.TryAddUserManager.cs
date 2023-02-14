// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddUserManager(this IServiceCollection services)
    {
        services.TryAddAreaResource<Area>();
        services.TryAddSingleton<IUserManager, UserManager>();
        return services;
    }
}
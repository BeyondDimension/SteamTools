// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddUserManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IUserManager, UserManager>();
        return services;
    }
}
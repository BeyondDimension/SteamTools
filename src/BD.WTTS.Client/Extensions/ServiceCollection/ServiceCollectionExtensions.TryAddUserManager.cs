// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [BD.Mobius(
"""
services.AddSingleton<IUserManager, BackendUserManager>();
""", Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddUserManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IUserManager, UserManager>();
        return services;
    }
}
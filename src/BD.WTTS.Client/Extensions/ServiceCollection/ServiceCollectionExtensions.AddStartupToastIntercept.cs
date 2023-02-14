// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    public static IServiceCollection AddStartupToastIntercept(this IServiceCollection services)
    {
        services.AddSingleton<StartupToastIntercept>();
        services.AddSingleton<IToastIntercept>(s => s.GetRequiredService<StartupToastIntercept>());
        return services;
    }
}
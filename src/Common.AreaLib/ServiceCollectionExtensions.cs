using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Entities;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddAreaResource<TArea>(this IServiceCollection services)
        where TArea : class, IArea
    {
        services.TryAddSingleton<AreaResourceImpl<TArea>>();
        services.TryAddSingleton<IAreaResourceHelper<TArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
        services.TryAddSingleton<IAreaResource<TArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
        services.TryAddSingleton<IAreaResource<IArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
        return services;
    }
}
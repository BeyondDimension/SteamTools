using System.Application.Entities;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAreaResource<TArea>(this IServiceCollection services)
            where TArea : class, IArea
        {
            services.AddSingleton<AreaResourceImpl<TArea>>();
            services.AddSingleton<IAreaResourceHelper<TArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
            services.AddSingleton<IAreaResource<TArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
            services.AddSingleton<IAreaResource<IArea>>(s => s.GetRequiredService<AreaResourceImpl<TArea>>());
            return services;
        }
    }
}
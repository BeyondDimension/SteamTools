using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddMainThreadPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
            return services;
        }
    }
}
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.CloudService;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加 CloudServiceClient
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddCloudServiceClient<T>(this IServiceCollection services)
            where T : CloudServiceClientBase
        {
            services.TryAddSingleton<T>();
            services.TryAddSingleton<ICloudServiceClient>(s => s.GetRequiredService<T>());
            services.TryAddSingleton<CloudServiceClientBase>(s => s.GetRequiredService<T>());
            return services;
        }
    }
}
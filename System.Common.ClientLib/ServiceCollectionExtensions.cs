using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Security;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加由 Xamarin.Essentials.SecureStorage 实现的 <see cref="IStorage"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddStorage(this IServiceCollection services)
        {
            services.TryAddSingleton<IStorage, ClientStorage>();
            return services;
        }

        /// <summary>
        /// 添加运行时权限
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddPermissions(this IServiceCollection services)
        {
            services.TryAddSingleton<IPermissions, PermissionsImpl>();
            return services;
        }
    }
}
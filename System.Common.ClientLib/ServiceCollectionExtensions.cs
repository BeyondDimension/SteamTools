using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Security;
using MSEXOptions = Microsoft.Extensions.Options.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加配置项
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddOptions<TOptions>(this IServiceCollection services, TOptions options) where TOptions : class, new()
        {
            services.TryAddSingleton(MSEXOptions.Create(options));
            return services;
        }

        /// <summary>
        /// 添加由 Xamarin.Essentials.SecureStorage 或 Repository 实现的 <see cref="IStorage"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddStorage(this IServiceCollection services)
        {
            if (DI.DeviceIdiom == DeviceIdiom.Desktop)
            {
                services.TryAddSingleton<IStorage, DesktopClientStorage>();
            }
            else
            {
                services.TryAddSingleton<IStorage, ClientStorage>();
            }
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
using Microsoft.Extensions.Http;
using System;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
            if (OperatingSystem2.IsIOS() /*|| OperatingSystem2.IsWatchOS() || OperatingSystem2.IsTvOS()*/)
            {
                services.AddSingleton<ApplePlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<ApplePlatformServiceImpl>());
                //services.AddPlatformNotificationService();
                services.TryAddAppleClientHttpPlatformHelperService();
                //PlatformToastImpl.TryAddToast(services);
                //if (options.HasGUI)
                //{
                //    services.AddSingleton<IBiometricService, PlatformBiometricServiceImpl>();
                //}
                //services.AddSingleton<IPlatformPageRouteService, AndroidPageRouteServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }

        /// <summary>
        /// 添加适用于 iOS 的原生 <see cref="IHttpClientFactory"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNativeHttpClient(this IServiceCollection services)
        {
            // https://github.com/dotnet/runtime/blob/v5.0.4/src/libraries/Microsoft.Extensions.Http/src/DependencyInjection/HttpClientFactoryServiceCollectionExtensions.cs#L36
            services.AddTransient<HttpMessageHandlerBuilder, PlatformHttpMessageHandlerBuilder>();
            services.AddHttpClient();
            return services;
        }

        /// <summary>
        /// 添加适用于 iOS 的 <see cref="IHttpPlatformHelperService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddAppleClientHttpPlatformHelperService(this IServiceCollection services)
        {
            services.AddSingleton<IHttpPlatformHelperService, AppleClientHttpPlatformHelperServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加平台权限
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPlatformPermissions(this IServiceCollection services)
        {
            return services;
        }

        ///// <summary>
        ///// 添加适用于 iOS 的 <see cref="INotificationService"/>
        ///// </summary>
        ///// <param name="services"></param>
        ///// <returns></returns>
        //public static IServiceCollection AddNotificationService(this IServiceCollection services)
        //{
        //    //services.AddSingleton<INotificationService, PlatformNotificationServiceImpl>();
        //    return services;
        //}
    }
}
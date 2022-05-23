using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Http;
using System;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddPlatformService(this IServiceCollection services, StartupOptions options)
        {
            if (OperatingSystem2.IsAndroid())
            {
                services.AddSingleton<AndroidPlatformServiceImpl>();
                services.AddSingleton<IPlatformService>(s => s.GetRequiredService<AndroidPlatformServiceImpl>());
                services.AddPlatformNotificationService();
                services.TryAddAndroidClientHttpPlatformHelperService();
                PlatformToastImpl.TryAddToast(services);
                services.AddSingleton<IBiometricService, PlatformBiometricServiceImpl>();
                services.AddSingleton<IFilePickerPlatformService.ISaveFileDialogService, FilePickerPlatformServiceImpl>();
#if __XAMARIN_FORMS__
                services.AddSingleton<IPlatformPageRouteService, AndroidPageRouteServiceImpl>();
#endif
                services.AddSingleton<IDeviceInfoPlatformService, AndroidDeviceInfoPlatformServiceImpl>();
            }
            else
            {
                throw new PlatformNotSupportedException();
            }
            return services;
        }

        /// <summary>
        /// 添加适用于安卓的原生 <see cref="IHttpClientFactory"/>
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
        /// 添加适用于安卓的 <see cref="IHttpPlatformHelperService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddAndroidClientHttpPlatformHelperService(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpPlatformHelperService, AndroidClientHttpPlatformHelperServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加适用于安卓的 <see cref="INotificationService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        static IServiceCollection AddPlatformNotificationService(this IServiceCollection services)
        {
            services.AddSingleton<INotificationService, AndroidNotificationServiceImpl>();
            return services;
        }
    }
}
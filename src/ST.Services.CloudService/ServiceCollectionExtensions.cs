using Microsoft.Extensions.DependencyInjection.Extensions;
using System;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Net.Http;
using System.Properties;

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
        /// <param name="config"></param>
        /// <param name="useMock"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddCloudServiceClient<T>(
            this IServiceCollection services,
            Action<HttpClient>? config = null,
            Func<HttpMessageHandler>? configureHandler = null,
            bool useMock = false)
            where T : CloudServiceClientBase
        {
            var b = services.AddHttpClient(CloudServiceClientBase.ClientName_, (s, c) =>
            {
                var sc = s.GetRequiredService<CloudServiceClientBase>();
                c.Timeout = GeneralHttpClientFactory.DefaultTimeout;
                c.BaseAddress = new Uri(sc.ApiBaseUrl);
                c.DefaultRequestHeaders.UserAgent.ParseAdd(sc.UserAgent);
                c.DefaultRequestHeaders.Add(Constants.Headers.Request.AppVersion, sc.Settings.AppVersionStr);
                config?.Invoke(c);
            });

            if (configureHandler != null)
            {
                b.ConfigurePrimaryHttpMessageHandler(configureHandler);
            }

            services.TryAddSingleton<T>();
            services.TryAddSingleton<CloudServiceClientBase>(s => s.GetRequiredService<T>());
            services.TryAddSingleton<IApiConnectionPlatformHelper>(s => s.GetRequiredService<T>());
            if (useMock && ThisAssembly.Debuggable)
            {
#if DEBUG
                services.AddSingleton<ICloudServiceClient, MockCloudServiceClient>();
#else
                throw new NotSupportedException();
#endif
            }
            else
            {
                services.TryAddSingleton<ICloudServiceClient>(s => s.GetRequiredService<T>());
            }
            return services;
        }
    }
}
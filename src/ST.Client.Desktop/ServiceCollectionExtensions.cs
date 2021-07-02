using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Net.Http;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加适用于Desktop的Toast
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddToast(this IServiceCollection services)
            => PlatformToastImpl.TryAddToast(services);

        /// <summary>
        /// 添加桌面端日志实现
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDesktopLogging(this IServiceCollection services)
        {
            var (minLevel, cfg) = AppHelper.Configure();
            services.AddLogging(cfg);
            services.Configure<LoggerFilterOptions>(o =>
            {
                o.MinLevel = minLevel;
            });
            return services;
        }

        /// <summary>
        /// 添加 hosts 文件助手服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostsFileService(this IServiceCollection services)
        {
            services.AddSingleton<IHostsFileService, HostsFileServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 Steam 相关助手、工具类服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSteamService(this IServiceCollection services)
        {
            services.AddSingleton<ISteamService, SteamServiceImpl>();
            return services;
        }

        /// <summary>
        /// 尝试添加 Steamworks LocalApi Service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddSteamworksLocalApiService(this IServiceCollection services)
        {
            services.TryAddSingleton<ISteamworksLocalApiService, SteamworksLocalApiServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 SteamDb WebApi Service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSteamDbWebApiService(this IServiceCollection services)
        {
            services.AddSingleton<ISteamDbWebApiService, SteamDbWebApiServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 Steamworks WebApi Service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSteamworksWebApiService(this IServiceCollection services)
        {
            services.AddSingleton<ISteamworksWebApiService, SteamworksWebApiServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加适用于桌面端的 <see cref="IHttpPlatformHelper"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddDesktopHttpPlatformHelper(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpPlatformHelper, DesktopHttpPlatformHelper>();
            return services;
        }

        /// <summary>
        /// 添加 Window 窗口viewmodel
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWindowService(this IServiceCollection services)
        {
            services.AddSingleton<IWindowService, WindowServiceImpl>();
            return services;
        }
    }
}
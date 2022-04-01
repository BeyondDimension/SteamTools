using ArchiSteamFarm;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application;
using System.Application.Entities;
using System.Application.Repositories;
using System.Application.Repositories.Implementation;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Net.Http;
using System.Windows;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddSingleton<IUserRepository, UserRepository>();
            services.AddSingleton<IGameAccountPlatformAuthenticatorRepository, GameAccountPlatformAuthenticatorRepository>();
            services.AddSingleton<IScriptRepository, ScriptRepository>();
            return services;
        }

        /// <summary>
        /// 添加 通用 Http 服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpService(this IServiceCollection services)
        {
            services.AddHttpClient(); // 添加 System.Net.Http.HttpClient / HttpClientFactory
            services.AddSingleton<IHttpService, HttpServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加JS脚本 管理
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddScriptManager(this IServiceCollection services)
        {
            services.TryAddSingleton<IScriptManager, ScriptManager>();
            return services;
        }

        public static IServiceCollection TryAddUserManager(this IServiceCollection services)
        {
            services.TryAddAreaResource<Area>();
            services.TryAddSingleton<IUserManager, UserManager>();
            return services;
        }

        /// <summary>
        /// 添加 HttpProxy 代理服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpProxyService(this IServiceCollection services)
        {
            services.AddSingleton<IHttpProxyService, HttpProxyServiceImpl>();
            return services;
        }

        public static IServiceCollection AddAutoMapper(this IServiceCollection services)
        {
            services.AddAutoMapper(cfg => cfg.AddProfile<AutoMapperProfile>());
            return services;
        }

        public static IServiceCollection AddStartupToastIntercept(this IServiceCollection services)
        {
            services.AddSingleton<StartupToastIntercept>();
            services.AddSingleton<IToastIntercept>(s => s.GetRequiredService<StartupToastIntercept>());
            return services;
        }

        /// <summary>
        /// 添加 ArchiSteamFarm 服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddArchiSteamFarmService(this IServiceCollection services)
        {
            services.AddSingleton<IArchiSteamFarmService, ArchiSteamFarmServiceImpl>();
            services.AddSingleton<IArchiSteamFarmHelperService>(s => s.GetRequiredService<IArchiSteamFarmService>());
            return services;
        }

        /// <summary>
        /// 尝试添加使用 <see cref="ToastService"/> 实现的 <see cref="IToast"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddToast(this IServiceCollection services)
            => ToastImpl.TryAddToast(services);

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
        /// 添加 SteamGridDB WebApi Service
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddSteamGridDBWebApiService(this IServiceCollection services)
        {
            services.AddSingleton<ISteamGridDBWebApiServiceImpl, SteamGridDBWebApiServiceImpl>();
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
        /// 添加适用于客户端的 <see cref="IHttpPlatformHelperService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddClientHttpPlatformHelperService(this IServiceCollection services)
        {
            services.TryAddSingleton<IHttpPlatformHelperService, ClientHttpPlatformHelperServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 <see cref="IViewModelManager"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddViewModelManager(this IServiceCollection services)
        {
            services.AddSingleton<IViewModelManager, ViewModelManager>();
            return services;
        }

        /// <summary>
        /// 尝试添加由 <see cref="NotifyIcon"/> 实现的 <see cref="INotificationService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddNotificationService(this IServiceCollection services)
        {
            services.TryAddSingleton<INotificationService, NotificationServiceImpl>();
            return services;
        }
    }
}
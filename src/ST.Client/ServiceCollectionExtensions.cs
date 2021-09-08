using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application;
using System.Application.Entities;
using System.Application.Repositories;
using System.Application.Repositories.Implementation;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class IdentityServiceCollectionExtensions
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
            services.TryAddSingleton<IScriptManagerService, ScriptManagerServiceImpl>();
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
        /// 添加 asf 功能
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddArchiSteamFarmService(this IServiceCollection services)
        {
            services.AddSingleton<IArchiSteamFarmService, ArchiSteamFarmServiceImpl>();
            return services;
        }
    }
}
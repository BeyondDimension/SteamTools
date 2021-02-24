using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加主线程助手类(MainThreadDesktop)服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMainThreadPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
            return services;
        }
    }
}
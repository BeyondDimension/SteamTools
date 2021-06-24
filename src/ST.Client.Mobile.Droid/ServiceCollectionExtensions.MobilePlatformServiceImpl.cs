using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加移动平台服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMobilePlatformService(this IServiceCollection services)
        {
            services.AddSingleton<MobilePlatformServiceImpl>();
            services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MobilePlatformServiceImpl>());
            services.AddSingleton<IMobilePlatformService>(s => s.GetRequiredService<MobilePlatformServiceImpl>());
            return services;
        }
    }
}
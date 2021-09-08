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
        /// <param name="hasGUI"></param>
        /// <returns></returns>
        public static IServiceCollection AddMobilePlatformService(this IServiceCollection services, bool hasGUI)
        {
            if (hasGUI)
            {
                services.AddSingleton<IBiometricService, PlatformBiometricServiceImpl>();
            }
            services.AddSingleton<MobilePlatformServiceImpl>();
            services.AddSingleton<IPlatformService>(s => s.GetRequiredService<MobilePlatformServiceImpl>());
            services.AddSingleton<IMobilePlatformService>(s => s.GetRequiredService<MobilePlatformServiceImpl>());
            return services;
        }
    }
}
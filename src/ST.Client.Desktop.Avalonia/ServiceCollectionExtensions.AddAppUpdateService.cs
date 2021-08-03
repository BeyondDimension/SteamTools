using System.Application.Services;
#if __MOBILE__
using _AppUpdateServiceImpl = System.Application.Services.Implementation.MobileAppUpdateServiceImpl;
#else
using _AppUpdateServiceImpl = System.Application.Services.Implementation.AvaloniaDesktopAppUpdateServiceImpl;
#endif

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加应用程序更新服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppUpdateService(this IServiceCollection services)
        {
            services.AddSingleton<IAppUpdateService, _AppUpdateServiceImpl>();
            return services;
        }
    }
}
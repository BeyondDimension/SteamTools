using System.Application.Services;
#if AVALONIA
using AppUpdateServiceImpl = System.Application.Services.Implementation.AvaloniaApplicationUpdateServiceImpl;
#else
using AppUpdateServiceImpl = System.Application.Services.Implementation.ApplicationUpdateServiceImpl;
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
        public static IServiceCollection AddApplicationUpdateService(this IServiceCollection services)
        {
            services.AddSingleton<IApplicationUpdateService, AppUpdateServiceImpl>();
            return services;
        }
    }
}
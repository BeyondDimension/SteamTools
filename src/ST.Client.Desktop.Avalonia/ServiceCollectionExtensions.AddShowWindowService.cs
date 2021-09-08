using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加显示窗口服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddShowWindowService(this IServiceCollection services)
        {
            services.AddSingleton<IShowWindowService, ShowWindowServiceImpl>();
            return services;
        }
    }
}
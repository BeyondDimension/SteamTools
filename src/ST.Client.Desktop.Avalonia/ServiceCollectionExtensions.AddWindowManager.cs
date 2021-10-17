using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加窗口管理服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddWindowManager(this IServiceCollection services)
        {
            services.AddSingleton<IWindowManager, AvaloniaWindowManagerImpl>();
            return services;
        }
    }
}
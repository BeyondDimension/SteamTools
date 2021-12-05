using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
#if __ANDROID__
using WindowManagerImpl_ = System.Application.Services.Implementation.AndroidWindowManagerImpl;
#elif AVALONIA
using WindowManagerImpl_ = System.Application.Services.Implementation.AvaloniaWindowManagerImpl;
#endif

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
        public static IServiceCollection TryAddWindowManager(this IServiceCollection services)
        {
            services.TryAddSingleton<IWindowManager, WindowManagerImpl_>();
            return services;
        }
    }
}
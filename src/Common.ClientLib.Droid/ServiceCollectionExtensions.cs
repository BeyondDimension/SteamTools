using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.Services.Implementation.Permissions;
using System.Diagnostics;
using System.Logging;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加适用于安卓平台的Toast
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddAndroidToast(this IServiceCollection services)
            => PlatformToastImpl.TryAddToast(services);

        /// <summary>
        /// 添加调试模式显示
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddDebuggerDisplay(this IServiceCollection services)
        {
            services.TryAddSingleton<IDebuggerDisplay, PlatformDebuggerDisplayImpl>();
            return services;
        }

        /// <summary>
        /// 添加电话服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddTelephonyService(this IServiceCollection services)
        {
            services.AddSingleton<ITelephonyService, PlatformTelephonyServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加平台权限
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPlatformPermissions(this IServiceCollection services)
        {
            services.AddTransient<IPermissions.IGetPhoneNumber, GetPhoneNumberPermission>();
            return services;
        }
    }
}
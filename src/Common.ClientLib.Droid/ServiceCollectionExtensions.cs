using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.Services.Implementation.Permissions;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
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
        /// 添加平台运行时权限
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddPlatformPermissions(this IServiceCollection services)
        {
            services.AddTransient<Permissions2.IGetPhoneNumber, GetPhoneNumberPermission>();
            return services;
        }
    }
}
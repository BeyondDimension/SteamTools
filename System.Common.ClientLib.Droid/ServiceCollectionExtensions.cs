using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.Services.Implementation.Permissions;
using System.Diagnostics;
using System.Logging;
using MSEXOptions = Microsoft.Extensions.Options.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 尝试添加配置项
        /// </summary>
        /// <typeparam name="TOptions"></typeparam>
        /// <param name="services"></param>
        /// <param name="options"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddOptions<TOptions>(this IServiceCollection services, TOptions options) where TOptions : class, new()
        {
            services.TryAddSingleton(MSEXOptions.Create(options));
            return services;
        }

        /// <summary>
        /// 尝试添加适用于安卓平台的Toast
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddToast(this IServiceCollection services)
            => PlatformToastImpl.TryAddToast(services);

        /// <summary>
        /// 添加客户端日志提供程序
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static ILoggingBuilder AddClientProvider(this ILoggingBuilder builder)
        {
            builder.AddProvider(PlatformLoggerProvider.Instance);
            return builder;
        }

        /// <summary>
        /// 添加客户端日志
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddClientLogging(this IServiceCollection services)
        {
            services.AddLogging(l => l.AddClientProvider());
            return services;
        }

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
            services.AddSingleton<IPermissions.IGetPhoneNumber, GetPhoneNumberPermission>();
            return services;
        }
    }
}
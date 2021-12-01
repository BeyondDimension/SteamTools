using Microsoft.Extensions.Logging;
using System.Application.UI;
using System.Logging;
using _ThisAssembly = System.Properties.ThisAssembly;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加通用日志实现
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddGeneralLogging(this IServiceCollection services)
        {
            var (minLevel, action) = IApplication.ConfigureLogging();
            services.AddLogging(b =>
            {
                action(b);
#if __ANDROID__
                if (_ThisAssembly.Debuggable)
                {
                    // Android Logcat Provider Impl
                    b.AddProvider(PlatformLoggerProvider.Instance);
                }
#elif MONO_MAC
                b.AddProvider(PlatformLoggerProvider.Instance);
#elif XAMARIN_MAC
                b.AddProvider(global::Uno.Extensions.Logging.OSLogLoggerProvider.Instance);
#endif
            });
            services.Configure<LoggerFilterOptions>(o =>
            {
                o.MinLevel = minLevel;
            });
            return services;
        }
    }
}
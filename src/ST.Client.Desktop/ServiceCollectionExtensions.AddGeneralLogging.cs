using System;
using System.Logging;
using System.Application.UI;
using Microsoft.Extensions.Logging;
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
            services.AddSingleton(new NLogDisposable());
            return services;
        }

        sealed class NLogDisposable : IDisposable
        {
            bool disposedValue;

            private void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                        NLog.LogManager.Shutdown();
                        ArchiSteamFarm.LogManager.Shutdown();
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
            // ~LogManagerShutdown()
            // {
            //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }
    }
}
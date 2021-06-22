using Avalonia.Controls;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加主线程助手类(MainThreadDesktop)服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMainThreadPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加应用程序更新服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddAppUpdateService(this IServiceCollection services)
        {
            services.AddSingleton<IAppUpdateService, AvaloniaDesktopAppUpdateServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加托盘图标
        /// </summary>
        /// <typeparam name="TNotifyIcon"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNotifyIcon<TNotifyIcon>(this IServiceCollection services) where TNotifyIcon : class, INotifyIcon
        {
            services.AddSingleton<INotifyIcon<ContextMenu>.IUIFrameworkHelper, INotifyIcon.UIFrameworkHelper>();
            services.AddSingleton<TNotifyIcon>();
            services.AddSingleton<INotifyIcon>(s => s.GetRequiredService<TNotifyIcon>());
            services.AddSingleton<INotifyIcon<ContextMenu>>(s => s.GetRequiredService<TNotifyIcon>());
            return services;
        }

        /// <summary>
        /// 添加适用于桌面端的 <see cref="INotificationService"/>
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddNotificationService(this IServiceCollection services)
        {
            services.AddSingleton<INotificationService, PlatformNotificationServiceImpl>();
            return services;
        }

        public static IServiceCollection AddFontManager(this IServiceCollection services, bool useGdiPlusFirst)
        {
            services.AddSingleton<IFontManager>(new AvaloniaFontManagerImpl(useGdiPlusFirst));
            return services;
        }
    }
}
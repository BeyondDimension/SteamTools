using System.Application.Services;
using System.Application.Services.Implementation;
using System.Application.UI;
using System.Application.UI.ViewModels;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加本地化、多语言服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddLocalizationService(this IServiceCollection services)
        {
            services.AddSingleton<ILocalizationService, LocalizationServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加视图模型组服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddViewModelCollectionService(this IServiceCollection services)
        {
            services.AddSingleton(new ViewModelCollectionServiceImpl(services));
            services.AddSingleton<IViewModelCollectionService>(s => s.GetRequiredService<ViewModelCollectionServiceImpl>());
            return services;
        }

        /// <summary>
        /// 添加视图模型
        /// </summary>
        /// <typeparam name="TViewModel"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddViewModel<TViewModel>(this IServiceCollection services) where TViewModel : ViewModelBase
        {
            services.AddSingleton<TViewModel>();
            return services;
        }

        /// <summary>
        /// 添加桌面端日志实现
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDesktopLogging(this IServiceCollection services)
        {
            services.AddLogging(AppHelper.Configure);
            return services;
        }

        /// <summary>
        /// 添加业务用户配置文件服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddConfigFileService(this IServiceCollection services)
        {
            services.AddSingleton<IConfigFileService>(new ConfigFileServiceImpl());
            return services;
        }

        /// <summary>
        /// 添加 hosts 文件助手服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHostsFileService(this IServiceCollection services)
        {
            services.AddSingleton<IHostsFileService, HostsFileServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 通用 Http 服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddHttpService(this IServiceCollection services)
        {
            services.AddHttpClient();
            services.AddSingleton<IHttpService, HttpServiceImpl>();
            return services;
        }
    }
}
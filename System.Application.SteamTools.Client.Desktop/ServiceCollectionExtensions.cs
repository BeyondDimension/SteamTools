using System.Application.Services;
using System.Application.Services.Implementation;
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
    }
}
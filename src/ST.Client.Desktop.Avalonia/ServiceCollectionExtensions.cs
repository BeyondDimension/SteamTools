using Avalonia.Controls;
using Microsoft.Extensions.DependencyInjection.Extensions;
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

        public static IServiceCollection TryAddFilePickerPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IFilePickerPlatformService, AvaloniaFilePickerPlatformService>();
            services.TryAddSingleton(s => s.GetRequiredService<IFilePickerPlatformService>().OpenFileDialogService);
            services.TryAddSingleton(s => s.GetRequiredService<IFilePickerPlatformService>().SaveFileDialogService);
            return services;
        }

        public static IServiceCollection AddFontManager(this IServiceCollection services, bool useGdiPlusFirst)
        {
            services.AddSingleton<IFontManager>(new AvaloniaFontManagerImpl(useGdiPlusFirst));
            return services;
        }
    }
}
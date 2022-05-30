using Avalonia.Platform;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        /// <summary>
        /// 添加主线程助手类(MainThread)服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddMainThreadPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
            return services;
        }

        /// <summary>
        /// 添加 Avalonia 实现的文件选择/保存框服务
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddAvaloniaFilePickerPlatformService(this IServiceCollection services)
        {
            services.AddSingleton<IFilePickerPlatformService, AvaloniaFilePickerPlatformService>();
            services.TryAddSingleton(s => s.GetRequiredService<IFilePickerPlatformService>().OpenFileDialogService);
            services.TryAddSaveFileDialogService(s => s.GetRequiredService<IFilePickerPlatformService>().SaveFileDialogService);
            return services;
        }

        /// <summary>
        /// 添加 Avalonia 实现的字体管理服务
        /// </summary>
        /// <param name="services"></param>
        /// <param name="useGdiPlusFirst"></param>
        /// <returns></returns>
        public static IServiceCollection TryAddAvaloniaFontManager(this IServiceCollection services, bool useGdiPlusFirst)
        {
            AvaloniaFontManagerImpl.UseGdiPlusFirst = useGdiPlusFirst;
            services.TryAddSingleton<AvaloniaFontManagerImpl>();
            services.AddSingleton<IFontManager>(s => s.GetRequiredService<AvaloniaFontManagerImpl>());
            services.AddSingleton<IFontManagerImpl>(s => s.GetRequiredService<AvaloniaFontManagerImpl>());
            return services;
        }
    }
}
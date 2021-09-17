using Avalonia.Controls;
using Avalonia.Platform;
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
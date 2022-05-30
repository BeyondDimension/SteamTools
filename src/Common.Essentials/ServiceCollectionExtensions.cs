using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加保存文件框服务
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSaveFileDialogService<T>(this IServiceCollection services) where T : class, IFilePickerPlatformService.ISaveFileDialogService
    {
        Essentials.IsSupportedSaveFileDialog = true;
        services.AddSingleton<IFilePickerPlatformService.ISaveFileDialogService, T>();
        return services;
    }

    /// <summary>
    /// 添加保存文件框服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="implementationFactory"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddSaveFileDialogService(this IServiceCollection services, Func<IServiceProvider, IFilePickerPlatformService.ISaveFileDialogService> implementationFactory)
    {
        Essentials.IsSupportedSaveFileDialog = true;
        services.TryAddSingleton(implementationFactory);
        return services;
    }
}
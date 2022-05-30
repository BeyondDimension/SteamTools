using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application;
using System.Application.Services;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSaveFileDialogService<T>(this IServiceCollection services) where T : class, IFilePickerPlatformService.ISaveFileDialogService
    {
        Essentials.IsSupportedSaveFileDialog = true;
        services.TryAddSingleton<IFilePickerPlatformService.ISaveFileDialogService, T>();
        return services;
    }
}
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Security;
using ISecureStorage = System.Security.ISecureStorage;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Essentials
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddEssentials(this IServiceCollection services)
    {
        services.TryAddSingleton<IBrowserPlatformService, BrowserPlatformServiceImpl>();
        services.TryAddSingleton<IClipboardPlatformService, ClipboardPlatformServiceImpl>();
        services.TryAddSingleton<IConnectivityPlatformService, ConnectivityPlatformServiceImpl>();
        services.TryAddSingleton<IDeviceInfoPlatformService, DeviceInfoPlatformServiceImpl>();
        services.TryAddSingleton<IEmailPlatformService, EmailPlatformServiceImpl>();
        services.TryAddSingleton<IFilePickerPlatformService, FilePickerPlatformServiceImpl>();
        services.TryAddSingleton<IMainThreadPlatformService, MainThreadPlatformServiceImpl>();
        services.TryAddSingleton<IPermissionsPlatformService, PermissionsPlatformServiceImpl>();
        services.TryAddSingleton<IPreferencesPlatformService, PreferencesPlatformServiceImpl>();
        return services;
    }

    /// <summary>
    /// 添加由 Xamarin.Essentials.SecureStorage 实现的 <see cref="ISecureStorage"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddEssentialsSecureStorage(this IServiceCollection services)
    {
        services.TryAddSingleton<ISecureStorage, EssentialsSecureStorage>();
        return services;
    }
}
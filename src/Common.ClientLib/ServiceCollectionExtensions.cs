using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Security;
using static System.Application.Services.ILocalDataProtectionProvider;
using static System.Application.Services.Implementation.LocalDataProtectionProviderBase;
using MSEXOptions = Microsoft.Extensions.Options.Options;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 尝试添加配置项
    /// </summary>
    /// <typeparam name="TOptions"></typeparam>
    /// <param name="services"></param>
    /// <param name="options"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddOptions<TOptions>(this IServiceCollection services, TOptions options) where TOptions : class, new()
    {
        services.TryAddSingleton(MSEXOptions.Create(options));
        return services;
    }

    /// <summary>
    /// 添加由 Repository 实现的 <see cref="ISecureStorage"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddRepositorySecureStorage(this IServiceCollection services)
    {
        services.TryAddSingleton<ISecureStorage, RepositorySecureStorage>();
        return services;
    }

    /// <summary>
    /// 添加安全服务
    /// </summary>
    /// <typeparam name="TEmbeddedAes"></typeparam>
    /// <typeparam name="TLocal"></typeparam>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddSecurityService<TEmbeddedAes, TLocal>(this IServiceCollection services)
        where TEmbeddedAes : EmbeddedAesDataProtectionProviderBase
        where TLocal : class, ILocalDataProtectionProvider
    {
        services.TryAddSingleton<IProtectedData, EmptyProtectedData>();
        services.TryAddSingleton<IDataProtectionProvider, EmptyDataProtectionProvider>();
        services.TryAddSingleton<ISecondaryPasswordDataProtectionProvider, SecondaryPasswordDataProtectionProvider>();
        services.AddSingleton<IEmbeddedAesDataProtectionProvider, TEmbeddedAes>();
        services.AddSingleton<ILocalDataProtectionProvider, TLocal>();
        services.AddSingleton<ISecurityService, SecurityService>();
        return services;
    }

    /// <summary>
    /// 添加由 <see cref="PreferencesPlatformServiceImpl"/> 实现的 <see cref="IPreferencesPlatformService"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddRepositoryPreferences(this IServiceCollection services)
    {
#if DBREEZE
        services.AddSingleton<IPreferencesPlatformService, PreferencesPlatformServiceImplV2>();
#else
        services.AddSingleton<IPreferencesPlatformService, PreferencesPlatformServiceImpl>();
#endif
        return services;
    }
}
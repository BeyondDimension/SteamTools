// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加安全服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddSecurityService(this IServiceCollection services)
    {
        services.AddSecurityService<EmbeddedAesDataProtectionProvider, LocalDataProtectionProvider>();
        return services;
    }
}
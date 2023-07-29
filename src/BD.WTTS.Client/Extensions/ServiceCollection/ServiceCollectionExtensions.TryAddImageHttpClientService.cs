// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加适用于客户端的 <see cref="IImageHttpClientService"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryImageHttpClientService(this IServiceCollection services)
    {
        services.TryAddSingleton<IImageHttpClientService, ImageHttpClientService>();
        return services;
    }
}
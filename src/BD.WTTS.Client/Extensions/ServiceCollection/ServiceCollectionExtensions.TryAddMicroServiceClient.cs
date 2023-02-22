using MSC_IMPL = BD.WTTS.Services.Implementation.MicroServiceClient;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 尝试添加 MicroServiceClient
    /// </summary>
    /// <param name="services"></param>
    /// <param name="config"></param>
    /// <param name="configureHandler"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddMicroServiceClient(
        this IServiceCollection services,
        Action<HttpClient>? config = null,
        Func<HttpMessageHandler>? configureHandler = null)
    {
        services.AddSingleton(new MSC_IMPL.HttpClientFactory(config, configureHandler));
        services.AddSingleton<MSC_IMPL>();

        services.TryAddSingleton<MicroServiceClientBase>(s => s.GetRequiredService<MSC_IMPL>());
        services.TryAddSingleton<IApiConnectionPlatformHelper>(s => s.GetRequiredService<MSC_IMPL>());
        services.TryAddSingleton<IMicroServiceClient>(s => s.GetRequiredService<MSC_IMPL>());
        return services;
    }
}
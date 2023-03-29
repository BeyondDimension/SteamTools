#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddDnsAnalysisService(this IServiceCollection services)
    {
        //services.AddSingleton<IDnsAnalysisService, DnsDohAnalysisService>();
        services.AddSingleton<IDnsAnalysisService, DnsAnalysisServiceImpl>();
        return services;
    }
}
#endif
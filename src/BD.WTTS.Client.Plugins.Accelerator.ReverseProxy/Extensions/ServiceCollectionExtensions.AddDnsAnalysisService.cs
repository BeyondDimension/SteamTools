// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddDnsAnalysisService(this IServiceCollection services)
    {
        services.AddSingleton<DnsDohAnalysisService>();
        services.AddSingleton<DnsAnalysisServiceImpl>();
        return services;
    }
}
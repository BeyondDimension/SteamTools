// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
#if WINDOWS
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddJumpListService(this IServiceCollection services)
    {
        services.AddSingleton<IJumpListService, JumpListServiceImpl>();
        return services;
    }
#endif
}
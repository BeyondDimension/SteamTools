// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddFontManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IFontManager, FontManagerImpl>();
        return services;
    }
}
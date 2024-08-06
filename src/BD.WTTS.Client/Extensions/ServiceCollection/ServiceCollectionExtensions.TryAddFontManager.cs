// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    [BD.Mobius(Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddFontManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IFontManager, FontManagerImpl>();
        return services;
    }
}
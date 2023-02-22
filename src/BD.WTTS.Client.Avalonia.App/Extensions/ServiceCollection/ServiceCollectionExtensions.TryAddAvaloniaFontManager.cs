// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Avalonia 实现的字体管理服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="useGdiPlusFirst"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddAvaloniaFontManager(this IServiceCollection services, bool useGdiPlusFirst)
    {
        AvaloniaFontManagerImpl.UseGdiPlusFirst = useGdiPlusFirst;
        services.TryAddSingleton<AvaloniaFontManagerImpl>();
        services.AddSingleton<IFontManager>(s => s.GetRequiredService<AvaloniaFontManagerImpl>());
        services.AddSingleton<IFontManagerImpl>(s => s.GetRequiredService<AvaloniaFontManagerImpl>());
        return services;
    }
}
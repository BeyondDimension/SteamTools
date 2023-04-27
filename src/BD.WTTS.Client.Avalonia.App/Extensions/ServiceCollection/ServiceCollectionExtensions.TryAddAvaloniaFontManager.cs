// ReSharper disable once CheckNamespace
using BD.WTTS.Services.Implementation;

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
        services.AddSingleton<IFontManager, FontManagerImpl>();
        return services;
    }
}
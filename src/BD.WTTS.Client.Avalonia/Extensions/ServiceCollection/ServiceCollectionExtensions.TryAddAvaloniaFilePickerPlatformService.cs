// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 Avalonia 实现的文件选择/保存框服务
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddAvaloniaFilePickerPlatformService(this IServiceCollection services)
    {
        services.AddSingleton<IFilePickerPlatformService, AvaloniaFilePickerPlatformService>();
        services.TryAddSingleton(s => s.GetRequiredService<IFilePickerPlatformService>().OpenFileDialogService);
        services.TryAddSaveFileDialogService(s => s.GetRequiredService<IFilePickerPlatformService>().SaveFileDialogService);
        return services;
    }
}
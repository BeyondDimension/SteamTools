// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 <see cref="IViewModelManager"/>
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection AddViewModelManager(this IServiceCollection services)
    {
        services.AddSingleton<IViewModelManager, ViewModelManager>();
        return services;
    }
}
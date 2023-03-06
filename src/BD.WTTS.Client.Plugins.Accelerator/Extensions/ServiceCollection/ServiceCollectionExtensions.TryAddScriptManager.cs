// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static partial class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 JS 脚本管理
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static IServiceCollection TryAddScriptManager(this IServiceCollection services)
    {
        services.TryAddSingleton<IScriptManager, ScriptManager>();
        return services;
    }
}
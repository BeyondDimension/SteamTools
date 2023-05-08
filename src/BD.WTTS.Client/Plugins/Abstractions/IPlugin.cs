namespace BD.WTTS.Plugins.Abstractions;

/// <summary>
/// 插件接口
/// </summary>
public interface IPlugin : IExplicitHasValue
{
    /// <summary>
    /// 插件的名称
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 插件的版本号
    /// </summary>
    string Version { get; }

    /// <summary>
    /// 插件加载完成时
    /// </summary>
    /// <returns></returns>
    ValueTask OnLoadedAsync();

    /// <summary>
    /// MainWindowViewModel.Initialize
    /// </summary>
    /// <returns></returns>
    ValueTask OnInitializeAsync();

    /// <summary>
    /// 配置按需使用的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <param name="isTrace"></param>
    void ConfigureDemandServices(
        IServiceCollection services,
        IApplication.IStartupArgs args,
        StartupOptions options);

    /// <summary>
    /// 配置任何进程都必要的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="args"></param>
    /// <param name="options"></param>
    /// <param name="isTrace"></param>
    void ConfigureRequiredServices(
        IServiceCollection services,
        IApplication.IStartupArgs args,
        StartupOptions options);

    /// <summary>
    /// 配置 AutoMapper
    /// </summary>
    /// <param name="cfg"></param>
    void OnAddAutoMapper(IMapperConfigurationExpression cfg);

    /// <summary>
    /// 未处理的异常
    /// </summary>
    /// <param name="ex"></param>
    /// <param name="name"></param>
    void OnUnhandledException(Exception ex, string name, bool? isTerminating = null);

    ValueTask OnExit();
}

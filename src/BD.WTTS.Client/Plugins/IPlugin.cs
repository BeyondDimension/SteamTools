namespace BD.WTTS.Plugins;

public interface IPlugin
{
    string Name { get; }

    Version Version { get; }

    ValueTask OnLoaded();

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
}

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
    /// 获取当前插件需要加载的菜单项视图模型
    /// </summary>
    /// <returns></returns>
    IEnumerable<TabItemViewModel>? GetMenuTabItems();

    /// <summary>
    /// 获取菜单项视图模型对应页面视图 MenuTabItemViewModel -> UserControl
    /// </summary>
    /// <returns></returns>
    IEnumerable<KeyValuePair<Type, Type>>? GetMenuTabItemToPages();

    /// <summary>
    /// 获取插件的配置项
    /// </summary>
    /// <param name="builder"></param>
    /// <param name="directoryExists"></param>
    /// <returns></returns>
    IEnumerable<Action<IConfiguration, IServiceCollection>>? GetConfiguration(
        ConfigurationBuilder builder,
        bool directoryExists);

    /// <summary>
    /// MainWindowViewModel.Initialize
    /// </summary>
    /// <returns></returns>
    ValueTask OnInitializeAsync();

    /// <summary>
    /// 配置按需使用的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="startup"></param>
    void ConfigureDemandServices(
        IServiceCollection services,
        Startup startup);

    /// <summary>
    /// 配置任何进程都必要的依赖注入服务
    /// </summary>
    /// <param name="services"></param>
    /// <param name="startup"></param>
    void ConfigureRequiredServices(
        IServiceCollection services,
        Startup startup);

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

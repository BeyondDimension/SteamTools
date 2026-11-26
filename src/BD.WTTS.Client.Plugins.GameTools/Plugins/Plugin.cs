using BD.WTTS.Properties;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.Plugins;

#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
[CompositionExport(typeof(IPlugin))]
#endif
public sealed class Plugin : PluginBase<Plugin>, IPlugin
{
    const string moduleName = AssemblyInfo.GameTools;

    public override Guid Id => Guid.Parse(AssemblyInfo.GameToolsId);

    public override string Name => Strings.GameRelated;

    public override string? NameResourceKey => nameof(Strings.GameRelated);

    public sealed override string UniqueEnglishName => moduleName;

    public sealed override string Description => Strings.L10nGames_ToolsDescription;

    public override string? DescriptionResourceKey => nameof(Strings.L10nGames_ToolsDescription);

    protected sealed override string? AuthorOriginalString => null;

    public sealed override object? Icon => Resources.toolbox; //"avares://BD.WTTS.Client.Plugins.GameTools/UI/Assets/toolbox.ico";

    public override IEnumerable<MenuTabItemViewModel>? GetMenuTabItems()
    {
        yield return new MenuTabItemViewModel(this, nameof(Strings.GameRelated))
        {
            PageType = typeof(GameToolsPage),
            IsResourceGet = true,
            IconKey = Icon,
        };
    }

    public override void ConfigureDemandServices(IServiceCollection services, Startup startup)
    {
    }

    public override void ConfigureRequiredServices(IServiceCollection services, Startup startup)
    {
        services.AddSingleton<BorderlessGamePageViewModel>();
    }

    public override void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

#if WINDOWS
<<<<<<< HEAD
    public override IEnumerable<(Action<IServiceCollection>? @delegate, bool isInvalid, string name)>? GetConfiguration(bool directoryExists)
    {
        yield return GetConfiguration<GameWindowToolsSettings_>(directoryExists);
    }

    public override ValueTask OnInitializeAsync()
    {
        if (GameWindowToolsSettings.Rules.Value is { Count: > 0 } rules)
            IPlatformService.Instance.SetWindowRules(rules);
        return ValueTask.CompletedTask;
    }

    public override ValueTask OnExit()
    {
        IPlatformService.Instance.ShutDownWindowTools();
        return ValueTask.CompletedTask;
    }
=======
    public override ValueTask OnInitializeAsync() => Startup.Instance.IsMainProcess
        ? new(Services.GameTools.CpuSchedulingRules.Instance.StartAsync())
        : ValueTask.CompletedTask;

    public override ValueTask OnExit() => new(Services.GameTools.CpuSchedulingRules.StopIfCreatedAsync());
>>>>>>> RMBGAME/gt-cpu-scheduler
#endif
}

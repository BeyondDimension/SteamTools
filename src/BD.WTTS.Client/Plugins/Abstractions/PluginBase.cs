namespace BD.WTTS.Plugins.Abstractions;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public abstract class PluginBase<TPlugin> : IPlugin where TPlugin : PluginBase<TPlugin>
{
    string DebuggerDisplay => Name;

    public abstract string Name { get; }

    readonly Lazy<string> mVersion = new(() =>
    {
        var assembly = typeof(TPlugin).Assembly;
        string? version = null;
        for (int i = 0; string.IsNullOrWhiteSpace(version); i++)
        {
            version = i switch
            {
                0 => assembly.GetCustomAttribute<AssemblyVersionAttribute>()?.Version ?? string.Empty,
                1 => assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? string.Empty,
                2 => assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? string.Empty,
                _ => null,
            };
            if (version == null) return string.Empty;
        }
        return version;
    });

    public virtual string Version => mVersion.Value;

    public virtual ValueTask OnInitializeAsync() => ValueTask.CompletedTask;

    public virtual void ConfigureDemandServices(
        IServiceCollection services,
        Startup startup)
    {

    }

    public virtual void ConfigureRequiredServices(
        IServiceCollection services,
        Startup startup)
    {

    }

    public virtual void OnAddAutoMapper(IMapperConfigurationExpression cfg)
    {

    }

    public virtual void OnUnhandledException(Exception ex, string name, bool? isTerminating = null)
    {

    }

    public virtual bool ExplicitHasValue()
    {
        return true;
    }

    public virtual ValueTask OnExit()
    {
        return ValueTask.CompletedTask;
    }
}

using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class MenuTabItemViewModel : TabItemViewModel, IExplicitHasValue
{
    string DebuggerDisplay => $"{Id} - {Name}";

    public MenuTabItemViewModel(IPlugin plugin, string resourceKeyOrName)
    {
        var pluginUniqueEnglishName = plugin.UniqueEnglishName;
        Id = $"{pluginUniqueEnglishName}-{resourceKeyOrName}";
        ResourceKeyOrName = resourceKeyOrName;
    }

    internal MenuTabItemViewModel(string resourceKeyOrName)
    {
        Id = $"{AssemblyInfo.Trademark}-{resourceKeyOrName}";
        ResourceKeyOrName = resourceKeyOrName;
    }

    public string Id { get; }

    public string ResourceKeyOrName { get; }

    public override Type? PageType { get; init; }

    public required bool IsResourceGet { get; init; }

    public override string Name => IsResourceGet ? GetString(ResourceKeyOrName) : ResourceKeyOrName;

    protected virtual string GetString(string resourceKey)
    {
        return AppResources.ResourceManager.GetString(resourceKey) ?? string.Empty;
    }

    bool IExplicitHasValue.ExplicitHasValue()
    {
        return !string.IsNullOrEmpty(Name);
    }
}
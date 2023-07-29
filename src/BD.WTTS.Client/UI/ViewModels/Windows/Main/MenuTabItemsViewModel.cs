using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public class MenuTabItemViewModel : TabItemViewModel, IExplicitHasValue
{
    public string ResourceKeyOrName { get; init; } = string.Empty;

    public Type? PageType { get; init; }

    public bool IsResourceGet { get; init; }

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
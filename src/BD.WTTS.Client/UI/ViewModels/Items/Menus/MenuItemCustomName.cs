using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public class MenuItemCustomName : MenuItemViewModel
{
    protected readonly string? toolTipName;

    public MenuItemCustomName(string? name = null, string? tipName = null)
    {
        Header = name;
        toolTipName = tipName;
    }

    public override string? Header { get; }

    public override string? ToolTip => string.IsNullOrEmpty(toolTipName) ? null : AppResources.ResourceManager.GetString(toolTipName, AppResources.Culture);
}
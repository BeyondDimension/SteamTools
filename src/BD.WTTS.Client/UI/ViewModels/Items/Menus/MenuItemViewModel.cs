using AppResources = BD.WTTS.Client.Resources.Strings;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public class MenuItemViewModel : ReactiveObject
{
    protected readonly string? name;

    public MenuItemViewModel(string? resourceName = null)
    {
        name = resourceName;
    }

    bool _IsVisible = true;

    public virtual bool IsVisible
    {
        get => _IsVisible;
        set => this.RaiseAndSetIfChanged(ref _IsVisible, value);
    }

    bool _IsEnabled = true;

    public virtual bool IsEnabled
    {
        get => _IsEnabled;
        set => this.RaiseAndSetIfChanged(ref _IsEnabled, value);
    }

    string? _IconKey;

    public virtual string? IconKey
    {
        get => _IconKey;
        set => this.RaiseAndSetIfChanged(ref _IconKey, value);
    }

    public virtual string? Header => string.IsNullOrEmpty(name) ? "-" : AppResources.ResourceManager.GetString(name, AppResources.Culture);

    public virtual string? ToolTip => string.IsNullOrEmpty(name) ? null : AppResources.ResourceManager.GetString(name + "Tip", AppResources.Culture);

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }

    protected IList<MenuItemViewModel>? _Items;

    public IList<MenuItemViewModel>? Items
    {
        get => _Items;
        set => this.RaiseAndSetIfChanged(ref _Items, value);
    }

    public void CheckmarkChange(bool isCheck)
    {
        if (isCheck)
            IconKey = "CheckmarkDrawing";
        else
            IconKey = null;
    }
}
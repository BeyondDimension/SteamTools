// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract partial class TabItemViewModelBase : ItemViewModel
{
    #region TaskBarSubMenu 托盘菜单

    bool _IsTaskBarSubMenu = true;

    public bool IsTaskBarSubMenu
    {
        get => _IsTaskBarSubMenu && MenuItems.Any_Nullable();
        protected set => this.RaiseAndSetIfChanged(ref _IsTaskBarSubMenu, value);
    }

    IList<MenuItemViewModel>? _MenuItems;

    public virtual IList<MenuItemViewModel>? MenuItems
    {
        get => _MenuItems;
        protected set => this.RaiseAndSetIfChanged(ref _MenuItems, value);
    }

    #endregion

    protected TabItemViewModelBase() : base()
    {
        var typeName = GetType().Name;
        IconKey = IApplication.Instance.GetIconKeyByTypeName(typeName);

        ResourceService.Subscribe(() =>
        {
            this.RaisePropertyChanged(nameof(Name));
        }).AddTo(this);
    }

    public virtual void Initialize()
    {
    }

    IEnumerable<ItemViewModel>? _Items;

    public IEnumerable<ItemViewModel>? Items
    {
        get => _Items;
        protected set => this.RaiseAndSetIfChanged(ref _Items, value);
    }
}
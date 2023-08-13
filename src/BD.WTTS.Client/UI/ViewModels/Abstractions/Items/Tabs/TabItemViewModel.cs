// ReSharper disable once CheckNamespace
using BD.WTTS.Client.Resources;
using System.Resources;

namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract partial class TabItemViewModel : ItemViewModel, IReadOnlyName
{
    #region TaskBarSubMenu 托盘菜单

    bool _IsTaskBarSubMenu = true;

    public bool IsTaskBarSubMenu
    {
        get => _IsTaskBarSubMenu && MenuItems.Any_Nullable();
        protected set => this.RaiseAndSetIfChanged(ref _IsTaskBarSubMenu, value);
    }

    IList<MenuItemViewModel>? _MenuItems;

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public virtual IList<MenuItemViewModel>? MenuItems
    {
        get => _MenuItems;
        protected set => this.RaiseAndSetIfChanged(ref _MenuItems, value);
    }

    #endregion

    public virtual Type? PageType { get; init; }

    protected TabItemViewModel() : base()
    {
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
using ReactiveUI;
using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public abstract partial class TabItemViewModel<TabItemId> : TabItemViewModelBase where TabItemId : struct, Enum
    {
        public virtual TabItemId Id { get; }
    }

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
            IconKey = OperatingSystem2.Application.UseMaui() ? $"{typeName.ToLowerInvariant()}.png" : typeName;
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
}

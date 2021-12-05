using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class MainWindowViewModel
    {
        readonly Dictionary<Type, Lazy<TabItemViewModel>> mTabItems = new();
        public IEnumerable<TabItemViewModel> TabItems => mTabItems.Values.Select(x => x.Value);

        public IReadOnlyList<TabItemViewModel>? FooterTabItems { get; private set; }

        public IEnumerable<TabItemViewModel> AllTabItems
        {
            get
            {
                if (FooterTabItems == null) return TabItems;
                else return TabItems.Concat(FooterTabItems);
            }
        }

        void AddTabItem<TabItemVM>() where TabItemVM : TabItemViewModel, new()
        {
            Lazy<TabItemViewModel> value = new(() => new TabItemVM()
#if !TRAY_INDEPENDENT_PROGRAM
            .AddTo(this)
#endif
            );
            mTabItems.Add(typeof(TabItemVM), value);
        }

        //void AddTabItem<TabItemVM>(Func<TabItemVM> func) where TabItemVM : TabItemViewModel
        //{
        //    Lazy<TabItemViewModel> value = new(func);
        //    mTabItems.Add(typeof(TabItemVM), value);
        //}

        internal TabItemVM GetTabItemVM<TabItemVM>() where TabItemVM : TabItemViewModel => (TabItemVM)mTabItems[typeof(TabItemVM)].Value;
    }
}

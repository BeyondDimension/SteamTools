using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class MainWindowViewModel
    {
        readonly object mTabItemsLock = new();
        static TabItemViewModel[]? mTabItems;

        public TabItemViewModel[] TabItems
        {
            get
            {
                lock (mTabItemsLock)
                {
                    if (mTabItems == null) mTabItems = GetTabItems().ToArray();
                    return mTabItems;
                }
                IEnumerable<TabItemViewModel> GetTabItems()
                {
                    foreach (var item in TabIdItems)
                    {
                        var type = TabItemViewModel.GetType(item);
                        yield return AllTabLazyItems[type].Value;
                    }
                }
            }
        }

        readonly object mFooterTabItemsLock = new();
        static TabItemViewModel[]? mFooterTabItems;

        public TabItemViewModel[] FooterTabItems
        {
            get
            {
                lock (mFooterTabItemsLock)
                {
                    if (mFooterTabItems == null) mFooterTabItems = GetFooterTabItems().ToArray();
                    return mFooterTabItems;
                }
                IEnumerable<TabItemViewModel> GetFooterTabItems()
                {
                    foreach (var item in FooterTabIdItems)
                    {
                        var type = TabItemViewModel.GetType(item);
                        yield return AllTabLazyItems[type].Value;
                    }
                }
            }
        }

        public TabItemViewModel.TabItemId[] TabIdItems { get; }

        public TabItemViewModel.TabItemId[] FooterTabIdItems { get; }

        public IReadOnlyCollection<TabItemViewModel.TabItemId> AllTabIdItems { get; }

        public Dictionary<Type, Lazy<TabItemViewModel>> AllTabLazyItems { get; }

        internal TabItemVM GetTabItemVM<TabItemVM>() where TabItemVM : TabItemViewModel
        {
            var type = typeof(TabItemVM);
            if (AllTabLazyItems.ContainsKey(type))
            {
                return (TabItemVM)AllTabLazyItems[type].Value;
            }

            throw new KeyNotFoundException($"type: {type} not found.");
        }
    }
}

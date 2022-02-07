using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class MainWindowViewModel
    {
        readonly Dictionary<Type, Lazy<TabItemViewModel>> mTabItems = new();
        public IEnumerable<TabItemViewModel> TabItems => mTabItems.Values.Select(x => x.Value);

        IReadOnlyList<TabItemViewModel>? _FooterTabItems;
        public IReadOnlyList<TabItemViewModel>? FooterTabItems
        {
            get
            {
                if (_FooterTabItems == null)
                {
                    var settingsPageVM = SettingsPageViewModel.Instance;
                    var aboutPageVM = AboutPageViewModel.Instance;
                    _FooterTabItems = new List<TabItemViewModel>
                    {
                        settingsPageVM,
                        aboutPageVM,
                    };
                }
                return _FooterTabItems;
            }
        }

        public IEnumerable<TabItemViewModel> AllTabItems
        {
            get
            {
                if (FooterTabItems == null) return TabItems;
                else return TabItems.Concat(FooterTabItems);
            }
        }

        /// <summary>
        /// 当前 <see cref="AllTabItems"/>，延时加载 <see cref="FooterTabItems"/>
        /// </summary>
        public IEnumerable<TabItemViewModel> CurrentAllTabItems
        {
            get
            {
                if (_FooterTabItems == null) return TabItems;
                else return TabItems.Concat(_FooterTabItems);
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

        internal TabItemVM GetTabItemVM<TabItemVM>() where TabItemVM : TabItemViewModel
        {
            var type = typeof(TabItemVM);
            if (mTabItems.ContainsKey(type))
            {
                return (TabItemVM)mTabItems[type].Value;
            }

            if (FooterTabItems != null)
            {
                foreach (var item in FooterTabItems)
                {
                    if (item is TabItemVM itemVM)
                    {
                        return itemVM;
                    }
                }
            }

            throw new KeyNotFoundException($"type: {type} not found.");
        }
    }
}

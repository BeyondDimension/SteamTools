using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using static System.Application.UI.ViewModels.TabItemViewModel;

namespace System.Application.UI
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        readonly IPlatformPageRouteService? pageRouteService = IPlatformPageRouteService.Instance;
        public AppShell()
        {
            InitializeComponent();

            if (IViewModelManager.Instance.MainWindow is MainWindowViewModel mainWindow)
            {
                BindingContext = mainWindow;

                InitTabItems(mainWindow, TabItemId.LocalAuth, TabItemId.ArchiSteamFarmPlus);
            }

            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        void InitTabItems(MainWindowViewModel mainWindow, params TabItemId[] topTabs)
        {
            IEnumerable<TabItemId> topTabs_ = topTabs;
            InitTabItems(mainWindow, topTabs_);
        }

        void InitTabItems(MainWindowViewModel mainWindow, IEnumerable<TabItemId>? topTabs = null)
        {
            var tabItems = mainWindow.TabItems.Concat(mainWindow.FooterTabItems);
            if (topTabs.Any_Nullable())
            {
                tabItems = tabItems.Where(x => topTabs.Contains(x.Id)).Concat(tabItems.Where(x => !topTabs.Contains(x.Id)));
            }
            foreach (var item in tabItems)
            {
                InitTabItem(item);
            }

            void InitTabItem(TabItemViewModel tab)
            {
                if (pageRouteService?.IsUseNativePage(tab.Id) ?? false)
                {
                    var tab_item = new MenuItem
                    {
                        IconImageSource = tab.IconKey,
                        BindingContext = tab,
                    };
                    tab_item.Clicked += (_, _) => pageRouteService!.GoToNativePage(tab.Id);
                    tab_item.SetBinding(MenuItem.TextProperty, nameof(tab.Name), BindingMode.OneWay);
                    Items.Add(tab_item);
                }
                else
                {
                    var tab_item = new FlyoutItem
                    {
                        Icon = tab.IconKey,
                        BindingContext = tab,
                    };
                    tab_item.SetBinding(BaseShellItem.TitleProperty, nameof(tab.Name), BindingMode.OneWay);
                    tab_item.Items.Add(new ShellContent
                    {
                        Route = tab.IconKey!.TrimEnd("ViewModel"),
                        ContentTemplate = new DataTemplate(GetPageType(tab)),
                    });
                    Items.Add(tab_item);
                }
            }
        }

        static Type GetPageType(TabItemViewModel tabItem)
        {
            if (OperatingSystem2.IsAndroid)
            {
                switch (tabItem.Id)
                {
                    case TabItemViewModel.TabItemId.LocalAuth:
                        return typeof(LocalAuthPage);
                    case TabItemViewModel.TabItemId.ArchiSteamFarmPlus:
                        return typeof(ArchiSteamFarmPlusPage);
                }
            }
            return typeof(UnderConstructionPage);
        }

        //private /*async*/ void OnMenuItemClicked(object sender, EventArgs e)
        //{
        //    //await Shell.Current.GoToAsync("//LoginPage");
        //}
    }
}

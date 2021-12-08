using ReactiveUI;
using System;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using n = System.Application.UI.Views.Native;
using static System.Application.UI.ViewModels.TabItemViewModel;
using System.Threading.Tasks;

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

            AddPage<n.MyPage, MyPageViewModel>(MyPageViewModel.Instance, isVisible: false);
#if DEBUG
            AddPage<LoginOrRegisterPage, LoginOrRegisterWindowViewModel>(new());
#endif

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
            var tabItems = mainWindow.AllTabItems.Where(x => x.Id != TabItemId.GameList);
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

        void AddPage<TPage, TPageViewModel>(TPageViewModel viewModel, bool isVisible = true, bool rSubscribe = true)
            where TPageViewModel : PageViewModel
            where TPage : Page
        {
            var tab_item = new FlyoutItem
            {
                BindingContext = viewModel,
                FlyoutItemIsVisible = isVisible,
            };
            tab_item.SetBinding(BaseShellItem.TitleProperty, nameof(PageViewModel.Title), BindingMode.OneWay);
            var pageType = typeof(TPage);
            tab_item.Items.Add(new ShellContent
            {
                Route = pageType.Name,
                ContentTemplate = new DataTemplate(pageType),
            });
            Items.Add(tab_item);
            if (rSubscribe)
            {
                R.Subscribe(() =>
                {
                    viewModel.RaisePropertyChanged(nameof(PageViewModel.Title));
                }).AddTo(viewModel);
            }
        }

        static Type GetPageType(TabItemViewModel tabItem)
        {
            if (OperatingSystem2.IsAndroid)
            {
                switch (tabItem.Id)
                {
                    case TabItemId.LocalAuth:
                        return typeof(n.LocalAuthPage);
                    case TabItemId.ArchiSteamFarmPlus:
                        return typeof(n.ArchiSteamFarmPlusPage);
                }
            }
            return typeof(UnderConstructionPage);
        }

        async void OnFlyoutHeaderTapped(object? sender, EventArgs e)
        {
            Current.FlyoutIsPresented = false;
            await Current.GoToAsync("//MyPage");
        }

        //private /*async*/ void OnMenuItemClicked(object sender, EventArgs e)
        //{
        //    //await Shell.Current.GoToAsync("//LoginPage");
        //}

        public static Task PopAsync()
        {
            // https://docs.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/shell/navigation#backwards-navigation
            return Current.GoToAsync("..");
        }

        public static async void Pop() => await PopAsync();
    }
}

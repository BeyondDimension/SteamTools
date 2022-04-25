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
    public partial class AppShell : Shell
    {
        /// <summary>
        /// 是否使用底部导航菜单
        /// </summary>
        public static bool IsUseBottomNav { get; private set; }

        readonly IPlatformPageRouteService? pageRouteService = IPlatformPageRouteService.Instance;

        public AppShell()
        {
            InitializeComponent();

            if (IViewModelManager.Instance.MainWindow is not MainWindowViewModel mainWindow) return;

            BindingContext = mainWindow;

            #region Flyout

            //            InitTabItems(mainWindow, TabItemId.LocalAuth, TabItemId.ArchiSteamFarmPlus);

            //            AddPage(MyPageViewModel.Instance, isVisible: false);
            //#if DEBUG
            //            AddPage(new LoginOrRegisterWindowViewModel());
            //#endif

            #endregion

            #region BottomNav

            IsUseBottomNav = true;
            InitButtomTabItems(mainWindow);
            //AddPage(SettingsPageViewModel.Instance, isVisible: false);
            //AddPage(AboutPageViewModel.Instance, isVisible: false);

            #endregion

            //Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            //Routing.RegisterRoute(nameof(NewItemPage), typeof(NewItemPage));
        }

        #region Flyout(左侧导航菜单)

        //void InitTabItems(MainWindowViewModel mainWindow, params TabItemId[] topTabs)
        //{
        //    IEnumerable<TabItemId> topTabs_ = topTabs;
        //    InitTabItems(mainWindow, topTabs_);
        //}

        //void InitTabItems(MainWindowViewModel mainWindow, IEnumerable<TabItemId>? topTabs = null)
        //{
        //    var tabItems = mainWindow.AllTabItems.Where(x => x.Id != TabItemId.GameList);
        //    if (topTabs.Any_Nullable())
        //    {
        //        tabItems = tabItems.Where(x => topTabs.Contains(x.Id)).Concat(tabItems.Where(x => !topTabs.Contains(x.Id)));
        //    }
        //    foreach (var item in tabItems)
        //    {
        //        InitTabItem(item);
        //    }

        //    void InitTabItem(TabItemViewModel tab)
        //    {
        //        if (pageRouteService?.IsUseNativePage(tab.Id) ?? false)
        //        {
        //            var tab_item = new MenuItem
        //            {
        //                IconImageSource = tab.IconKey,
        //                BindingContext = tab,
        //            };
        //            tab_item.Clicked += (_, _) => pageRouteService!.GoToNativePage(tab.Id);
        //            tab_item.SetBinding(MenuItem.TextProperty, nameof(tab.Name), BindingMode.OneWay);
        //            Items.Add(tab_item);
        //        }
        //        else
        //        {
        //            var tab_item = new FlyoutItem
        //            {
        //                Icon = tab.IconKey,
        //                BindingContext = tab,
        //            };
        //            tab_item.SetBinding(BaseShellItem.TitleProperty, nameof(tab.Name), BindingMode.OneWay);
        //            tab_item.Items.Add(new ShellContent
        //            {
        //                Route = tab.IconKey!.TrimEnd("ViewModel"),
        //                ContentTemplate = new DataTemplate(GetPageType(tab)),
        //            });
        //            Items.Add(tab_item);
        //        }
        //    }
        //}

        void AddPage(ViewModelBase vm, bool isVisible = true, bool rSubscribe = true)
        {
            var data = GetShellItemData(vm);
            if (data == default) return;
            var tab_item = new FlyoutItem
            {
                BindingContext = vm,
                FlyoutItemIsVisible = isVisible,
            };
            tab_item.SetBinding(BaseShellItem.TitleProperty, data.bindingPathName, BindingMode.OneWay);
            var pageType = GetPageType(vm);
            var content = new ShellContent
            {
                ContentTemplate = new DataTemplate(pageType),
            };
            if (data.route != null) content.Route = pageType.Name;
            tab_item.Items.Add(content);
            Items.Add(tab_item);
        }

        #endregion

        #region BottomNav(底部导航菜单)

        void InitButtomTabItems(MainWindowViewModel mainWindow)
        {
            var tabItems = new ViewModelBase[] {
                mainWindow.LocalAuthPage,
                mainWindow.ASFPage,
                MyPageViewModel.Instance,
            };
            TabBar tabBar = new();
            foreach (var tab in tabItems)
            {
                var data = GetShellItemData(tab);
                if (data == default) continue;
                var tab_item = new Tab
                {
                    BindingContext = tab,
                };
                tab_item.Icon = data.icon;
                tab_item.SetBinding(BaseShellItem.TitleProperty, data.bindingPathName, BindingMode.OneWay);
                var content = new ShellContent
                {
                    ContentTemplate = new DataTemplate(GetPageType(tab)),
                };
                if (data.route != null) content.Route = data.route;
                tab_item.Items.Add(content);
                tabBar.Items.Add(tab_item);
            }
            Items.Add(tabBar);
        }

        #endregion

        static (string icon, string bindingPathName, string route) GetShellItemData(ViewModelBase vm, bool rSubscribe = true)
        {
            string? icon = null;
            string? bindingPathName = null;
            string? route = null;
            if (vm is TabItemViewModel tabItemVM)
            {
                if (tabItemVM.IconKey == null) return default;
                if (IsUseBottomNav)
                {
                    if (tabItemVM is LocalAuthPageViewModel)
                    {
                        icon = "baseline_verified_user_black_24";
                    }
                    else if (tabItemVM is ArchiSteamFarmPlusPageViewModel)
                    {
                        icon = "icon_asf_24";
                    }
                    else
                    {
                        return default;
                    }
                }
                else
                {
                    icon = tabItemVM.IconKey;
                }
                bindingPathName = nameof(tabItemVM.Name);
                route = tabItemVM.IconKey.TrimEnd("ViewModel");
                if (tabItemVM is SettingsPageViewModel || tabItemVM is AboutPageViewModel)
                {
                    route = $"MyPage/{route}";
                    //route = null;
                }
                rSubscribe = false;
            }
            else if (vm is PageViewModel pageVM)
            {
                bindingPathName = nameof(pageVM.Title);
                route = pageVM.GetType().Name.TrimEnd("ViewModel");
                if (pageVM is MyPageViewModel)
                {
                    icon = "baseline_person_black_24";
                }
            }
            if (icon == null || bindingPathName == null || route == null) return default;
            if (rSubscribe)
            {
                R.Subscribe(() =>
                {
                    vm.RaisePropertyChanged(nameof(PageViewModel.Title));
                }).AddTo(vm);
            }
            return (icon, bindingPathName, route);
        }

        static Type GetPageType(ViewModelBase vm)
        {
            if (OperatingSystem2.IsAndroid)
            {
                if (vm is TabItemViewModel tabItem)
                {
                    switch (tabItem.Id)
                    {
                        case TabItemId.LocalAuth:
                            return typeof(n.LocalAuthPage);
                        case TabItemId.ArchiSteamFarmPlus:
                            return typeof(n.ArchiSteamFarmPlusPage);
                        case TabItemId.Settings:
                            return typeof(n.SettingsPage);
                        case TabItemId.About:
                            return typeof(n.AboutPage);
                    }
                }
                else if (vm is MyPageViewModel)
                {
                    return typeof(n.MyPage);
                }
                else if (vm is LoginOrRegisterWindowViewModel)
                {
                    return typeof(LoginOrRegisterPage);
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

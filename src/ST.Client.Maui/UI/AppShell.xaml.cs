using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using static System.Application.UI.ViewModels.TabItemViewModel;

namespace System.Application.UI;

public partial class AppShell : Shell, IViewFor<MainWindowViewModel>
{
    /// <summary>
    /// 是否使用底部导航菜单
    /// </summary>
    internal static bool IsUseBottomNav { private get; set; }

    readonly IPlatformPageRouteService? pageRouteService = IPlatformPageRouteService.Instance;

    MainWindowViewModel viewModel;

    public MainWindowViewModel ViewModel
    {
        get => viewModel;
        set => BindingContext = viewModel = value;
    }

    MainWindowViewModel? IViewFor<MainWindowViewModel>.ViewModel
    {
        get => (MainWindowViewModel)BindingContext;
        set => BindingContext = value;
    }

    object? IViewFor.ViewModel
    {
        get => BindingContext;
        set => BindingContext = value;
    }

    public AppShell(MainWindowViewModel viewModel)
    {
        BindingContext = this.viewModel = viewModel;

        InitializeComponent();

        //#if WINDOWS
        //        Microsoft.Maui.Controls.Handlers.ShellHandler.Mapper.AppendToMapping(nameof(AppShell), (handler, view) =>
        //        {
        //            handler.PlatformView.PaneDisplayMode = Microsoft.UI.Xaml.Controls.NavigationViewPaneDisplayMode.LeftCompact;
        //        });
        //#endif

        if (!IsUseBottomNav)
        {
            #region Flyout

            var topTabs = IApplication.IsDesktopPlatform ? Array.Empty<TabItemId>() : new[] { TabItemId.LocalAuth, TabItemId.ArchiSteamFarmPlus };
            InitTabItems(viewModel, topTabs);

            AddPage(MyPageViewModel.Instance, isVisible: false);
#if DEBUG
            AddPage(new LoginOrRegisterWindowViewModel());
#endif

            #endregion
        }
        else
        {
            #region BottomNav

            FlyoutBehavior = FlyoutBehavior.Disabled;
            InitButtomTabItems(viewModel);
            AddPage(SettingsPageViewModel.Instance, isVisible: false);
            AddPage(AboutPageViewModel.Instance, isVisible: false);

            #endregion
        }
    }

    #region Flyout(左侧导航菜单)

    void InitTabItems(MainWindowViewModel mainWindow, params TabItemId[] topTabs)
    {
        IEnumerable<TabItemId> topTabs_ = topTabs;
        InitTabItems(mainWindow, topTabs_);
    }

    void InitTabItems(MainWindowViewModel mainWindow, IEnumerable<TabItemId>? topTabs = null)
    {
        var tabItems = mainWindow.TabItems.Where(x => x.Id != TabItemId.GameList);
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

    void AddPage(ViewModelBase vm, bool isVisible = true)
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
                Icon = data.icon,
            };
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
        //if (OperatingSystem2.IsAndroid())
        //{
        //    if (vm is TabItemViewModel tabItem)
        //    {
        //        switch (tabItem.Id)
        //        {
        //            case TabItemId.LocalAuth:
        //                return typeof(n.LocalAuthPage);
        //            case TabItemId.ArchiSteamFarmPlus:
        //                return typeof(n.ArchiSteamFarmPlusPage);
        //            case TabItemId.Settings:
        //                return typeof(n.SettingsPage);
        //            case TabItemId.About:
        //                return typeof(n.AboutPage);
        //        }
        //    }
        //    else if (vm is MyPageViewModel)
        //    {
        //        return typeof(n.MyPage);
        //    }
        //    else if (vm is LoginOrRegisterWindowViewModel)
        //    {
        //        return typeof(LoginOrRegisterPage);
        //    }
        //}
        return typeof(UnderConstructionPage);
    }

    void OnFlyoutHeaderTapped(object? sender, EventArgs e)
    {
        Current.FlyoutIsPresented = false;
    }

    public static Task PopAsync()
    {
        // https://docs.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/shell/navigation#backwards-navigation
        return Current.GoToAsync("..");
    }

    public static async void Pop() => await PopAsync();
}
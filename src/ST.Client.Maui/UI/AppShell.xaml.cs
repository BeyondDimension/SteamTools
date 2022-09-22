using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views;
using static System.Application.UI.ViewModels.TabItemViewModel;

namespace System.Application.UI;

public partial class AppShell : Shell, IViewFor<MainWindowViewModel>
{
    //readonly IPlatformPageRouteService? pageRouteService = IPlatformPageRouteService.Instance;

    #region VM

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

    #endregion

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

        #region InitTabItems

        FlyoutItem flyoutItem = new()
        {
            FlyoutDisplayOptions = FlyoutDisplayOptions.AsMultipleItems,
        };
        AddPage(viewModel.LocalAuthPage);
        AddPage(viewModel.ASFPage);
        if (!OperatingSystem.IsIOS())
            AddPage(viewModel.CommunityProxyPage);
        AddPage(MyPageViewModel.Instance, flyoutItemIsVisible: false);
        Items.Add(flyoutItem);

        AddPage(SettingsPageViewModel.Instance, isSecondary: false);
        AddPage(AboutPageViewModel.Instance, isSecondary: false);

        void AddPage(ViewModelBase vm, bool flyoutItemIsVisible = true, bool isSecondary = true)
        {
            var shellContent = CreateShellContent(vm);
            if (shellContent == null) return;
            shellContent.FlyoutItemIsVisible = flyoutItemIsVisible;
            if (isSecondary) flyoutItem.Items.Add(shellContent);
            else Items.Add(shellContent);
        }

        #endregion
    }

    #region TabItems

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

    static ShellContent? CreateShellContent(ViewModelBase vm)
    {
        var pageType = GetPageType(vm);
        var content = new ShellContent
        {
            ContentTemplate = new DataTemplate(pageType),
            BindingContext = vm,
        };
        string? bindingPathName;
        if (vm is TabItemViewModel tabVM)
        {
            content.Icon = tabVM.IconKey;
            content.Route = tabVM.Id.ToString();
            bindingPathName = nameof(TabItemViewModel.Name);
        }
        else if (vm is PageViewModel)
        {
            bindingPathName = nameof(PageViewModel.Title);
            content.Icon = "baseline_person_black_24";
        }
        else
        {
            bindingPathName = default;
        }
        if (bindingPathName != default)
        {
            content.SetBinding(BaseShellItem.TitleProperty, bindingPathName, BindingMode.OneWay);
        }
        return content;
    }

    #endregion

    public static Task PopAsync()
    {
        // https://docs.microsoft.com/zh-cn/xamarin/xamarin-forms/app-fundamentals/shell/navigation#backwards-navigation
        return Current.GoToAsync("..");
    }

    public static async void Pop() => await PopAsync();
}
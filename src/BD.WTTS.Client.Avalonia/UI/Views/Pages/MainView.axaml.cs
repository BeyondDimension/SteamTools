using Avalonia.Controls;
using BD.WTTS.Enums;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Pages;

public sealed partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    public MainView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, IViewModelManager.Instance.MainWindow!);
#endif

        NavView.ItemInvoked += NavView_ItemInvoked;
        FrameView.Navigated += OnFrameViewNavigated;
        NavView.BackRequested += OnNavigationViewBackRequested;
    }

    private void NavView_ItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            if (nvi.DataContext is MenuTabItemViewModel { PageType: { } pageType })
                NavigationService.Instance.Navigate(pageType);
            else
                NavigationService.Instance.Navigate(typeof(ErrorPage), NavigationTransitionEffect.Entrance);
        }
    }

    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        if (e.Content is not Control page)
            return;

        UpdateBackButton();
        if (page is ErrorPage || MatchesPage(NavView.SelectedItem))
            return;

        NavView.SelectedItem = FindMenuItem(NavView.MenuItemsSource) ?? FindMenuItem(NavView.FooterMenuItemsSource);

        bool MatchesPage(object? item) => item is TabItemViewModel tab &&
            (ReferenceEquals(tab, page.DataContext) || tab is MenuTabItemViewModel menu && menu.PageType == page.GetType());

        object? FindMenuItem(System.Collections.IEnumerable? items)
        {
            if (items != null)
                foreach (var item in items)
                    if (MatchesPage(item))
                        return item;
            return null;
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        var top = TopLevel.GetTopLevel(this);
        if (top != null)
            AvaloniaToastServiceImpl.Instance.SetSnackbarManager(top);
        NavigationService.Instance.SetFrame(FrameView);
        UpdateBackButton();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        NavigationService.Instance.DetachFrame(FrameView);
        base.OnDetachedFromVisualTree(e);
    }

    private void OnNavigationViewBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private void UpdateBackButton()
    {
        var canGoBack = FrameView.CanGoBack;
        NavView.IsBackButtonVisible = canGoBack;
        TitleBarHost.WindowIcon.Margin = new Thickness(canGoBack ? 48 : 18, 4, 10, 4);
    }
}

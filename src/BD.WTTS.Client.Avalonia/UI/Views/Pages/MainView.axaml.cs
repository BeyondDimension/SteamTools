using Avalonia.Controls;
using BD.WTTS.Enums;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Media.Animation;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Pages;

public sealed partial class MainView : ReactiveUserControl<MainWindowViewModel>
{
    public MainView()
    {
        InitializeComponent();

#if DEBUG
        if (Design.IsDesignMode)
            Design.SetDataContext(this, MainWindow.GetMainWinodwViewModel());
#endif

        NavView.ItemInvoked += NavView_ItemInvoked;

        FrameView.Navigated += OnFrameViewNavigated;
        NavView.BackRequested += OnNavigationViewBackRequested;
    }

    private void NavView_ItemInvoked(object? sender, NavigationViewItemInvokedEventArgs e)
    {
        if (e.InvokedItemContainer is NavigationViewItem nvi)
        {
            if (nvi.DataContext is MenuTabItemViewModel menu && menu.PageType != null)
            {
                if (menu.PageType == FrameView?.Content?.GetType())
                    return;
                NavigationService.Instance.Navigate(menu.PageType, NavigationTransitionEffect.FromBottom);
                return;
            }

            NavigationService.Instance.Navigate(typeof(ErrorPage), NavigationTransitionEffect.Entrance);
        }
    }

    //private void NavView_SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    //{
    //    if (e.SelectedItem is MenuTabItemViewModel menu && menu.PageType != null)
    //    {
    //        if (menu.PageType == FrameView?.Content?.GetType())
    //            return;
    //        NavigationService.Instance.Navigate(menu.PageType, NavigationTransitionEffect.FromBottom);
    //        return;
    //    }

    //    NavigationService.Instance.Navigate(typeof(ErrorPage), NavigationTransitionEffect.Entrance);
    //    //FrameView?.Navigate(typeof(ErrorPage), info);
    //}

    private void OnFrameViewNavigated(object? sender, NavigationEventArgs e)
    {
        var page = e.Content as Control;
        if (page == null)
            return;
        if (page.GetType() == typeof(ErrorPage))
            return;

        var dc = page.DataContext;

        ViewModelBase? mainPage = dc switch
        {
            MenuTabItemViewModel mtvm => mtvm,
            TabItemViewModel tvm => tvm,
            ViewModelBase b => b,
            _ => null,
        };

        NavView.SelectedItem = null;

        foreach (var nvi in NavView.MenuItemsSource)
        {
            SetSelectedItem(nvi);
        }

        foreach (var nvi in NavView.FooterMenuItemsSource)
        {
            SetSelectedItem(nvi);
        }

        if (FrameView.BackStackDepth > 0 && !NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(true);
        }
        else if (FrameView.BackStackDepth == 0 && NavView.IsBackButtonVisible)
        {
            AnimateContentForBackButton(false);
        }

        void SetSelectedItem(object? nvi)
        {
            if (nvi == mainPage)
            {
                NavView.SelectedItem = nvi;
            }
            else if (nvi is MenuTabItemViewModel menu && menu.PageType == page.GetType())
            {
                NavView.SelectedItem = nvi;
            }
        }
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        NavigationService.Instance.SetFrame(FrameView);
    }

    private void OnNavigationViewBackRequested(object? sender, NavigationViewBackRequestedEventArgs e)
    {
        NavigationService.Instance.GoBack();
    }

    private async void AnimateContentForBackButton(bool show)
    {
        if (!TitleBarHost.WindowIcon.IsVisible)
            return;

        if (show)
        {
            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(18, 4, 10, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 10, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(TitleBarHost.WindowIcon);

            NavView.IsBackButtonVisible = true;
        }
        else
        {
            NavView.IsBackButtonVisible = false;

            var ani = new Animation
            {
                Duration = TimeSpan.FromMilliseconds(250),
                FillMode = FillMode.Forward,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0d),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(48, 4, 10, 4))
                        }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1d),
                        KeySpline = new KeySpline(0, 0, 0, 1),
                        Setters =
                        {
                            new Setter(MarginProperty, new Thickness(18, 4, 10, 4))
                        }
                    }
                }
            };

            await ani.RunAsync(TitleBarHost.WindowIcon);
        }
    }
}

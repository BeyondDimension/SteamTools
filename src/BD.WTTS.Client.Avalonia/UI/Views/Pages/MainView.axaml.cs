using FluentAvalonia.Core;
using FluentAvalonia.UI.Media.Animation;

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

        NavView.SelectionChanged += (_, e) =>
        {
            if (e.SelectedItem is MenuTabItemViewModel menu && menu.PageType != null)
            {
                FrameView?.Navigate(menu.PageType, new FluentAvalonia.UI.Navigation.FrameNavigationOptions
                {
                    IsNavigationStackEnabled = true,
                    TransitionInfoOverride = new SuppressNavigationTransitionInfo()
                });
                return;
            }
            FrameView?.Navigate(typeof(ErrorPage));
        };
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        NavigationService.Instance.SetFrame(FrameView);

    }
}

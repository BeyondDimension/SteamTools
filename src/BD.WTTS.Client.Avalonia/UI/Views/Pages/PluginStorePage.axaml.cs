using FluentAvalonia.UI.Controls.Experimental;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Pages;

public partial class PluginStorePage : UserControl
{
    public PluginStorePage()
    {
        InitializeComponent();
        this.SetViewModel<PluginStorePageViewModel>(useCache: true);

        // Use the frame events here to ensure ConnectedAnimations still work with
        // Back/Forward navigation and not just explicit page invokes
        //AddHandler(Frame.NavigatingFromEvent, OnNavigatingFrom, RoutingStrategies.Direct);
        //AddHandler(Frame.NavigatedToEvent, OnNavigatedTo, RoutingStrategies.Direct);

    }

    //private void PluginStorePage_Click(object? sender, RoutedEventArgs e)
    //{
    //    if (sender != null && sender is Button b && b.DataContext is TabItemViewModel fci)
    //    {
    //        _animationPage = fci;
    //        //NavigationService.Instance.NavigateFromContext(fci);
    //    }
    //}

    //private void OnNavigatedTo(object? sender, NavigationEventArgs e)
    //{
    //    if (_animationPage == null)
    //        return;

    //    var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //    var anim = svc.GetAnimation("BackAnimation");

    //    if (anim == null)
    //        return;

    //    var item = this.GetVisualDescendants()
    //                 ?.Where(x => x is Button && x.DataContext == _animationPage)
    //                 ?.FirstOrDefault()
    //                 ?.GetVisualDescendants()
    //                 .Where(x => x is Border && x.Name == "IconHost")
    //                 .FirstOrDefault();

    //    var presenter = item;// GetAnimationSource();

    //    // In WinUI, ConnectedAnimation is somehow exempt from all clipping behaviors
    //    // Here, we are not, so disable ClipToBounds on all elements in the SettingsExpander
    //    // The rest are taken care of in the xaml.
    //    // NOTE: The ScrollViewer is not changed here as that's important for scrolling - thus
    //    // the animation will be cut off, but the back animation is pretty fast and mostly is
    //    // only visible closer to the element so we're ok, I think
    //    var x = presenter?.GetVisualParent();

    //    while (!(x is ScrollContentPresenter) && x != null)
    //    {
    //        x.ClipToBounds = false;
    //        x = x.GetVisualParent();
    //    }

    //    anim.Configuration = new DirectConnectedAnimationConfiguration();
    //    anim.TryStart(presenter);
    //}

    //private void OnNavigatingFrom(object? sender, NavigatingCancelEventArgs e)
    //{
    //    if (_animationPage == null)
    //        return;

    //    // We're not navigating to a control page, don't set up the animation & clear
    //    // the previous animation source
    //    if (!e.SourcePageType.Name.Equals(nameof(TabItemViewModel)))
    //    {
    //        _animationPage = null;
    //        _animationPage = null;
    //        return;
    //    }

    //    var item = this.GetVisualDescendants()
    //                ?.Where(x => x is ListBoxItem && x.DataContext == _animationPage)
    //                ?.FirstOrDefault()
    //                ?.GetVisualDescendants()
    //                .Where(x => x is Viewbox && x.Name == "IconHost")
    //                .FirstOrDefault();
    //    var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //    svc.PrepareToAnimate("ForwardAnimation", item);
    //}

    //private TabItemViewModel? _animationPage;
}

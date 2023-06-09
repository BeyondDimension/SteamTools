using Avalonia.Rendering.Composition;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Pages;

public class PageBase : PageBase<ViewModelBase>
{
    public PageBase() : base()
    {

    }
}

public class PageBase<TViewModel> : ReactiveUserControl<TViewModel> where TViewModel : ViewModelBase
{
    public PageBase()
    {
        SizeChanged += PageBaseSizeChanged;
        //AddHandler(Frame.NavigatingFromEvent, FrameNavigatingFrom, RoutingStrategies.Direct);
        //AddHandler(Frame.NavigatedToEvent, FrameNavigatedTo, RoutingStrategies.Direct);
    }

    public static readonly StyledProperty<string> ControlNameProperty =
        AvaloniaProperty.Register<PageBase, string>(nameof(ControlName));

    public static readonly StyledProperty<string> ControlNamespaceProperty =
        AvaloniaProperty.Register<PageBase, string>(nameof(ControlNamespace));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<PageBase, string>(nameof(Description));

    public static readonly StyledProperty<IconSource> PreviewImageProperty =
        AvaloniaProperty.Register<PageBase, IconSource>(nameof(PreviewImage));

    public static readonly StyledProperty<string> WinUINamespaceProperty =
        AvaloniaProperty.Register<PageBase, string>(nameof(WinUINamespace));

    public static readonly StyledProperty<Uri> WinUIDocsLinkProperty =
        AvaloniaProperty.Register<PageBase, Uri>(nameof(WinUIDocsLink));

    public static readonly StyledProperty<Uri> WinUIGuidelinesLinkProperty =
        AvaloniaProperty.Register<PageBase, Uri>(nameof(WinUIGuidelinesLink));

    public static readonly StyledProperty<Uri> PageXamlSourceLinkProperty =
        AvaloniaProperty.Register<PageBase, Uri>(nameof(PageXamlSourceLink));

    public static readonly StyledProperty<Uri> PageCSharpSourceLinkProperty =
        AvaloniaProperty.Register<PageBase, Uri>(nameof(PageCSharpSourceLink));

    public static readonly StyledProperty<bool> ShowToggleThemeButtonProperty =
        AvaloniaProperty.Register<PageBase, bool>(nameof(ShowToggleThemeButton),
            defaultValue: true);

    public string ControlName
    {
        get => GetValue(ControlNameProperty);
        set => SetValue(ControlNameProperty, value);
    }

    public string ControlNamespace
    {
        get => GetValue(ControlNamespaceProperty);
        set => SetValue(ControlNamespaceProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconSource PreviewImage
    {
        get => GetValue(PreviewImageProperty);
        set => SetValue(PreviewImageProperty, value);
    }

    public string WinUINamespace
    {
        get => GetValue(WinUINamespaceProperty);
        set => SetValue(WinUINamespaceProperty, value);
    }

    public Uri WinUIDocsLink
    {
        get => GetValue(WinUIDocsLinkProperty);
        set => SetValue(WinUIDocsLinkProperty, value);
    }

    public Uri WinUIGuidelinesLink
    {
        get => GetValue(WinUIGuidelinesLinkProperty);
        set => SetValue(WinUIGuidelinesLinkProperty, value);
    }

    public Uri PageXamlSourceLink
    {
        get => GetValue(PageXamlSourceLinkProperty);
        set => SetValue(PageXamlSourceLinkProperty, value);
    }

    public Uri PageCSharpSourceLink
    {
        get => GetValue(PageCSharpSourceLinkProperty);
        set => SetValue(PageCSharpSourceLinkProperty, value);
    }

    public string GithubPrefixString
    {
        set
        {
            PageXamlSourceLink = new Uri($"{value}/{GetType().Name}.axaml");
            PageCSharpSourceLink = new Uri($"{value}/{GetType().Name}.axaml.cs");
        }
    }

    public bool ShowToggleThemeButton
    {
        get => GetValue(ShowToggleThemeButtonProperty);
        set => SetValue(ShowToggleThemeButtonProperty, value);
    }

    protected override Type StyleKeyOverride => typeof(PageBase);

    protected ThemeVariantScope? ThemeScopeProvider { get; private set; }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        PseudoClasses.Set(":namespace", ControlNamespace != null);
        PseudoClasses.Set(":winuiNamespace", WinUINamespace != null);

        ThemeScopeProvider = e.NameScope.Find<ThemeVariantScope>("ThemeScopeProvider");

        _previewImageHost = e.NameScope.Find<IconSourceElement>("PreviewImageElement");
        _detailsHost = e.NameScope.Find<StackPanel>("DetailsTextHost");
        _optionsHost = e.NameScope.Find<StackPanel>("OptionsRegion");
        _detailsPanel = e.NameScope.Find<Panel>("PageDetails");
        _scroller = e.NameScope.Find<ScrollViewer>("PageScroller");

        _toggleThemeButton = e.NameScope.Find<Button>("ToggleThemeButton");
        if (_toggleThemeButton != null)
            _toggleThemeButton.Click += ToggleThemeButtonClick;

        _winUIDocsItem = e.NameScope.Find<MenuFlyoutItem>("WinUIDocsItem");
        _winUIGuidelinesItem = e.NameScope.Find<MenuFlyoutItem>("WinUIGuidelinesItem");
        _xamlSourceItem = e.NameScope.Find<MenuFlyoutItem>("XamlSourceItem");
        _cSharpSourceItem = e.NameScope.Find<MenuFlyoutItem>("CSharpSourceItem");
        _showDefItem = e.NameScope.Find<MenuFlyoutItem>("ShowDefItem");
        _sep1 = e.NameScope.Find<MenuFlyoutSeparator>("Sep1");
        _sep2 = e.NameScope.Find<MenuFlyoutSeparator>("Sep2");

        var winUIDocs = WinUIDocsLink;
        var winUIGuidelines = WinUIGuidelinesLink;

        if (_winUIDocsItem != null)
        {
            if (winUIDocs == null)
                _winUIDocsItem.IsVisible = false;
            else
                _winUIDocsItem.Click += MoreOptionsItemClick;
        }

        if (_winUIGuidelinesItem != null)
        {
            if (winUIGuidelines == null)
                _winUIGuidelinesItem.IsVisible = false;
            else
                _winUIGuidelinesItem.Click += MoreOptionsItemClick;
        }

        if (_showDefItem != null)
            _showDefItem.IsVisible = false;
        if (_xamlSourceItem != null)
            _xamlSourceItem.Click += MoreOptionsItemClick;
        if (_cSharpSourceItem != null)
            _cSharpSourceItem.Click += MoreOptionsItemClick;

        if (_sep1 != null && _winUIDocsItem != null && _winUIGuidelinesItem != null)
            _sep1.IsVisible = _winUIDocsItem.IsVisible && _winUIGuidelinesItem.IsVisible;

        if (_sep2 != null)
            _sep2.IsVisible = _showDefItem?.IsVisible ?? false;
    }

    private void MoreOptionsItemClick(object? sender, RoutedEventArgs e)
    {
        //if (sender is MenuFlyoutItem mfi)
        //{
        //    switch (mfi.Name)
        //    {
        //        case "WinUIDocsItem":
        //            LaunchLink(WinUIDocsLink);
        //            break;

        //        case "WinUIGuidelinesItem":
        //            LaunchLink(WinUIGuidelinesLink);
        //            break;

        //        case "XamlSourceItem":
        //            LaunchLink(PageXamlSourceLink);
        //            break;

        //        case "CSharpSourceItem":
        //            LaunchLink(PageCSharpSourceLink);
        //            break;

        //        case "ShowDefItem":
        //            //NavigationService.Instance.ShowControlDefinitionOverlay(TargetType);
        //            break;
        //    }
        //}
    }

    protected override void OnLoaded()
    {
        base.OnLoaded();
        _hasLoaded = true;
        SetDetailsAnimation();
    }

    protected override void OnUnloaded()
    {
        base.OnUnloaded();
        _hasLoaded = false;
    }

    private void PageBaseSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var sz = e.NewSize.Width;

        bool isSmallWidth2 = sz < 580;

        PseudoClasses.Set(":smallWidth", sz < 710);
        PseudoClasses.Set(":smallWidth2", isSmallWidth2);

        if (isSmallWidth2 && !_isSmallWidth2)
        {
            AnimateOptions(true);
            _isSmallWidth2 = true;
        }
        else if (!isSmallWidth2 && _isSmallWidth2)
        {
            AnimateOptions(false);
            _isSmallWidth2 = false;
        }
    }

    private async void AnimateOptions(bool toSmall)
    {
        if (!_hasLoaded || _optionsHost == null)
            return;

        _cts?.Cancel();

        _cts = new CancellationTokenSource();
        double x = toSmall ? 70 : -70;
        double y = toSmall ? -30 : 30;
        var ani = new Animation
        {
            Duration = TimeSpan.FromSeconds(0.25),
            Children =
            {
                new KeyFrame
                {
                    Cue = new Cue(0d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, x),
                        new Setter(TranslateTransform.YProperty, y),
                        new Setter(OpacityProperty, 0d)
                    }
                },
                new KeyFrame
                {
                    Cue = new Cue(1d),
                    Setters =
                    {
                        new Setter(TranslateTransform.XProperty, 0d),
                        new Setter(TranslateTransform.YProperty, 0d),
                        new Setter(OpacityProperty, 1d)
                    },
                    KeySpline = new KeySpline(0, 0, 0, 1)
                }
            }
        };

        await ani.RunAsync(_optionsHost, _cts.Token);

        _cts = null;
    }

    private void SetDetailsAnimation()
    {
        if (_detailsPanel == null)
            return;

        var ec = ElementComposition.GetElementVisual(_detailsPanel);
        var compositor = ec.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

        var ani = compositor.CreateImplicitAnimationCollection();
        ani["Offset"] = offsetAnimation;

        ec.ImplicitAnimations = ani;
    }

    protected virtual void ToggleThemeButtonClick(object? sender, RoutedEventArgs e)
    {
        //var examples = this.GetVisualDescendants()
        //    .OfType<ControlExample>();

        //foreach (var ex in examples)
        //{
        //    ex.SetExampleTheme();
        //}
    }

    private void LaunchLink(Uri link)
    {
        try
        {
            Process.Start(new ProcessStartInfo(link.ToString()) { UseShellExecute = true, Verb = "open" });
        }
        catch
        {

        }
    }

    //private void FrameNavigatingFrom(object sender, NavigatingCancelEventArgs e)
    //{
    //    // If TargetType is not set, we know we're currently on a CoreControls page since those
    //    // are grouped pages - whereas, FA controls only display one control per page and
    //    // set all the extra properties
    //    bool isFAControlPage = TargetType != null;

    //    // Only setup the ConnectedAnimation if it makes sense
    //    if ((!isFAControlPage && e.SourcePageType == typeof(CoreControlsPageViewModel)) ||
    //        (isFAControlPage && e.SourcePageType == typeof(FAControlsOverviewPageViewModel)))
    //    {
    //        // Only setup the Back connected animation if we're going back to the
    //        // controls list pages
    //        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //        svc.PrepareToAnimate("BackAnimation", (Control)_previewImageHost.Parent);
    //        NavigationService.Instance.PreviousPage = this;
    //    }
    //}

    //private void FrameNavigatedTo(object sender, NavigationEventArgs e)
    //{
    //    var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //    var animation = svc.GetAnimation("ForwardAnimation");

    //    if (animation != null)
    //    {
    //        var coordinated = new List<Visual>
    //        {
    //            _optionsHost,
    //            _detailsHost,
    //            _scroller
    //        };

    //        // PreviewImageHost is inside a Viewbox which can really mess with the Composition 
    //        // animation - use the viewbox directly for the animation to ensure it works correctly
    //        animation.TryStart((Control)_previewImageHost.Parent, coordinated);
    //    }
    //}

    private bool _isSmallWidth2;
    private CancellationTokenSource? _cts;
    private bool _hasLoaded;

    private Button? _toggleThemeButton;
    private Panel? _detailsPanel;
    private StackPanel? _optionsHost;
    private IconSourceElement? _previewImageHost;
    private StackPanel? _detailsHost;
    private ScrollViewer? _scroller;

    private MenuFlyoutItem? _winUIDocsItem;
    private MenuFlyoutItem? _winUIGuidelinesItem;
    private MenuFlyoutItem? _xamlSourceItem;
    private MenuFlyoutItem? _cSharpSourceItem;
    private MenuFlyoutItem? _showDefItem;
    private MenuFlyoutSeparator? _sep1;
    private MenuFlyoutSeparator? _sep2;
}

using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Rendering.Composition;
using FluentAvalonia.UI.Controls.Experimental;
using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Pages;

public class PageBase : UserControl
{
    public PageBase() : base()
    {
        _disposables = new();

        SizeChanged += PageBaseSizeChanged;

        if (HeaderBackground == null)
        {
            this[!HeaderBackgroundProperty] = new DynamicResourceExtension("PageHeaderBackgroundBrush");
        }

        _disposables.Add(UISettings.WindowBackgroundCustomImage.Subscribe(x => IsShowBackgroundImage = !x));

        //AddHandler(Frame.NavigatingFromEvent, FrameNavigatingFrom, RoutingStrategies.Direct);
        //AddHandler(Frame.NavigatedToEvent, FrameNavigatedTo, RoutingStrategies.Direct);
    }

    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<PageBase, string>(nameof(Title));

    public static readonly StyledProperty<string?> SubtitleProperty =
        AvaloniaProperty.Register<PageBase, string?>(nameof(Subtitle));

    public static readonly StyledProperty<string?> DescriptionProperty =
        AvaloniaProperty.Register<PageBase, string?>(nameof(Description));

    public static readonly StyledProperty<IconSource?> PreviewImageProperty =
        AvaloniaProperty.Register<PageBase, IconSource?>(nameof(PreviewImage));

    public static readonly StyledProperty<string?> ContentDescriptionProperty =
        AvaloniaProperty.Register<PageBase, string?>(nameof(ContentDescription));

    public static readonly StyledProperty<object?> TabContentProperty =
        AvaloniaProperty.Register<PageBase, object?>(nameof(TabContent));

    public static readonly StyledProperty<object?> ActionContentProperty =
        AvaloniaProperty.Register<PageBase, object?>(nameof(ActionContent));

    public static readonly StyledProperty<bool> IsShowBackgroundImageProperty =
        AvaloniaProperty.Register<PageBase, bool>(nameof(IsShowBackgroundImage), defaultValue: true);

    public static readonly StyledProperty<IBrush?> HeaderBackgroundProperty =
       AvaloniaProperty.Register<PageBase, IBrush?>(nameof(HeaderBackground));

    public static readonly StyledProperty<object?> PaneContentProperty =
        AvaloniaProperty.Register<PageBase, object?>(nameof(PaneContent));

    public static readonly StyledProperty<bool> IsPaneOpenProperty =
        SplitView.IsPaneOpenProperty.AddOwner<PageBase>();

    public static readonly StyledProperty<double> OpenPaneLengthProperty =
        SplitView.OpenPaneLengthProperty.AddOwner<PageBase>();

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string? Subtitle
    {
        get => GetValue(SubtitleProperty);
        set => SetValue(SubtitleProperty, value);
    }

    public string? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public IconSource? PreviewImage
    {
        get => GetValue(PreviewImageProperty);
        set => SetValue(PreviewImageProperty, value);
    }

    public string? ContentDescription
    {
        get => GetValue(ContentDescriptionProperty);
        set => SetValue(ContentDescriptionProperty, value);
    }

    public object? TabContent
    {
        get => GetValue(TabContentProperty);
        set => SetValue(TabContentProperty, value);
    }

    public object? ActionContent
    {
        get => GetValue(ActionContentProperty);
        set => SetValue(ActionContentProperty, value);
    }

    public object? PaneContent
    {
        get => GetValue(PaneContentProperty);
        set => SetValue(PaneContentProperty, value);
    }

    public bool IsShowBackgroundImage
    {
        get => GetValue(IsShowBackgroundImageProperty);
        set => SetValue(IsShowBackgroundImageProperty, value);
    }

    public bool IsPaneOpen
    {
        get => GetValue(IsPaneOpenProperty);
        set => SetValue(IsPaneOpenProperty, value);
    }

    public double OpenPaneLength
    {
        get => GetValue(OpenPaneLengthProperty);
        set => SetValue(OpenPaneLengthProperty, value);
    }

    public IBrush? HeaderBackground
    {
        get => GetValue(HeaderBackgroundProperty);
        set => SetValue(HeaderBackgroundProperty, value);
    }

    public ScrollViewer? PageScroller { get => _scroller; set => _scroller = value; }

    protected override Type StyleKeyOverride => typeof(PageBase);

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        PseudoClasses.Set(":namespace", Subtitle != null);
        PseudoClasses.Set(":winuiNamespace", Description != null);

        //_previewImageHost = e.NameScope.Find<IconSourceElement>("PreviewImageElement");
        //_detailsHost = e.NameScope.Find<StackPanel>("DetailsTextHost");

        _optionsHost = e.NameScope.Find<Panel>("OptionsRegion");
        _tabsHost = e.NameScope.Find<Border>("TabRegion");
        _detailsPanel = e.NameScope.Find<Panel>("PageDetails");
        _scroller = e.NameScope.Find<ScrollViewer>("PageScroller");
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        _hasLoaded = true;
        SetDetailsAnimation();
        SetTabsAnimation();
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);
        _hasLoaded = false;
        _disposables.Dispose();
    }

    private void PageBaseSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        var sz = e.NewSize.Width;

        bool isSmallWidth2 = sz < 640;

        PseudoClasses.Set(":smallWidth", sz < 800);
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

    private void SetTabsAnimation()
    {
        if (_tabsHost == null)
            return;

        var ec = ElementComposition.GetElementVisual(_tabsHost);
        if (ec == null)
            return;
        var compositor = ec.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

        var ani = compositor.CreateImplicitAnimationCollection();
        ani["Offset"] = offsetAnimation;

        ec.ImplicitAnimations = ani;
    }

    private void SetDetailsAnimation()
    {
        if (_detailsPanel == null)
            return;

        var ec = ElementComposition.GetElementVisual(_detailsPanel);
        if (ec == null)
            return;
        var compositor = ec.Compositor;

        var offsetAnimation = compositor.CreateVector3KeyFrameAnimation();
        offsetAnimation.Target = "Offset";
        offsetAnimation.InsertExpressionKeyFrame(1.0f, "this.FinalValue");
        offsetAnimation.Duration = TimeSpan.FromMilliseconds(250);

        var ani = compositor.CreateImplicitAnimationCollection();
        ani["Offset"] = offsetAnimation;

        ec.ImplicitAnimations = ani;
    }

    //private void FrameNavigatingFrom(object? sender, NavigatingCancelEventArgs e)
    //{
    //    // Only setup the ConnectedAnimation if it makes sense
    //    if ((e.SourcePageType == typeof(HomePageViewModel)) ||
    //        (e.SourcePageType == typeof(PluginStorePageViewModel)))
    //    {
    //        // Only setup the Back connected animation if we're going back to the
    //        // controls list pages
    //        var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //        svc.PrepareToAnimate("BackAnimation", (Control)_previewImageHost.Parent);
    //        //NavigationService.Instance.PreviousPage = this;
    //    }
    //}

    //private void FrameNavigatedTo(object? sender, NavigationEventArgs e)
    //{
    //    var svc = ConnectedAnimationService.GetForView(TopLevel.GetTopLevel(this));
    //    var animation = svc.GetAnimation("ForwardAnimation");

    //    if (animation != null)
    //    {
    //        var coordinated = new List<Visual?>
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

    private Panel? _detailsPanel;
    private Panel? _optionsHost;
    private Border? _tabsHost;
    //private IconSourceElement? _previewImageHost;
    //private StackPanel? _detailsHost;
    private ScrollViewer? _scroller;
    private CompositeDisposable _disposables;
}

public class PageBase<TViewModel> : PageBase, IViewFor<TViewModel> where TViewModel : class
{
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty.Register<PageBase<TViewModel>, TViewModel?>(nameof(ViewModel));

    /// <summary>
    /// Initializes a new instance of the <see cref="PageBase{TViewModel}"/> class.
    /// </summary>
    public PageBase() : base()
    {
        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.

        this.WhenActivated(disposables =>
        {
            //disposables.Add(UISettings.WindowBackgroundCustomImage.Subscribe(x => IsShowBackgroundImage = !x));
        });
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);
    }

    /// <summary>
    /// The ViewModel.
    /// </summary>
    public TViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        ViewModel = DataContext as TViewModel;
    }

    private void OnViewModelChanged(object? value)
    {
        if (value == null)
        {
            ClearValue(DataContextProperty);
        }
        else if (DataContext != value)
        {
            DataContext = value;
        }
    }
}

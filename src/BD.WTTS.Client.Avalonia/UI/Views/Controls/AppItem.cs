namespace BD.WTTS.UI.Views.Controls;

public class AppItem : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Title));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Description));

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Status));

    public static readonly StyledProperty<FAIconElement> IconProperty =
        AvaloniaProperty.Register<AppItem, FAIconElement>(nameof(Icon));

    public static readonly StyledProperty<bool> ExpandsProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Expands));

    public static readonly StyledProperty<object?> TagsProperty =
        AvaloniaProperty.Register<AppItem, object?>(nameof(Tags));

    public static readonly StyledProperty<bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<AppItem>();

    public static readonly StyledProperty<ICommand> ClickCommandProperty =
        AvaloniaProperty.Register<AppItem, ICommand>(nameof(ClickCommand));

    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public string Status
    {
        get => GetValue(StatusProperty);
        set => SetValue(StatusProperty, value);
    }

    public FAIconElement Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool Expands
    {
        get => GetValue(ExpandsProperty);
        set => SetValue(ExpandsProperty, value);
    }

    public object? Tags
    {
        get => GetValue(TagsProperty);
        set => SetValue(TagsProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ICommand ClickCommand
    {
        get => GetValue(ClickCommandProperty);
        set => SetValue(ClickCommandProperty, value);
    }

    public static readonly RoutedEvent<RoutedEventArgs> NavigationRequestedEvent =
        RoutedEvent.Register<OptionsDisplayItem, RoutedEventArgs>(nameof(NavigationRequested), RoutingStrategies.Bubble);

    public event EventHandler<RoutedEventArgs> NavigationRequested
    {
        add => AddHandler(NavigationRequestedEvent, value);
        remove => RemoveHandler(NavigationRequestedEvent, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ExpandsProperty)
        {
            PseudoClasses.Set(":expands", change.GetNewValue<bool>());
        }
        else if (change.Property == DescriptionProperty)
        {
            PseudoClasses.Set(":desc", !string.IsNullOrEmpty(change.GetNewValue<string>()));
        }
        else if (change.Property == IsExpandedProperty)
            PseudoClasses.Set(":expanded", change.GetNewValue<bool>());
        else if (change.Property == IconProperty)
            PseudoClasses.Set(":icon", change.NewValue != null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        _layoutRoot = e.NameScope.Find<Border>("LayoutRoot");
        if (_layoutRoot != null)
        {
            _layoutRoot.PointerPressed += OnLayoutRootPointerPressed;
            _layoutRoot.PointerReleased += OnLayoutRootPointerReleased;
            _layoutRoot.PointerCaptureLost += OnLayoutRootPointerCaptureLost;
        }
    }

    private void OnLayoutRootPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(this).Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonPressed)
        {
            _isPressed = true;
            PseudoClasses.Set(":pressed", true);
        }
    }

    private void OnLayoutRootPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        var pt = e.GetCurrentPoint(this);
        if (_isPressed && pt.Properties.PointerUpdateKind == PointerUpdateKind.LeftButtonReleased)
        {
            _isPressed = false;

            PseudoClasses.Set(":pressed", false);

            if (Expands)
                IsExpanded = !IsExpanded;

            if (ClickCommand != null)
            {
                RaiseEvent(new RoutedEventArgs(NavigationRequestedEvent, this));
                ClickCommand.Execute(null);
            }
        }
    }

    private void OnLayoutRootPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        _isPressed = false;
        PseudoClasses.Set(":pressed", false);
    }

    private bool _isPressed;
    private bool _isExpanded;
    private Border? _layoutRoot;
}

using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Controls;

public class OptionsDisplayItem : TemplatedControl
{
    public static readonly StyledProperty<object?> HeaderProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, object?>(nameof(Header));

    public static readonly StyledProperty<object?> DescriptionProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, object?>(nameof(Description));

    public static readonly StyledProperty<object?> IconProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, object?>(nameof(Icon));

    public static readonly StyledProperty<bool> NavigatesProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Navigates));

    public static readonly StyledProperty<Control> ActionButtonProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, Control>(nameof(ActionButton));

    public static readonly StyledProperty<bool> ExpandsProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, bool>(nameof(Expands));

    public static readonly StyledProperty<object?> ContentProperty =
        ContentControl.ContentProperty.AddOwner<OptionsDisplayItem>();

    public static readonly StyledProperty<bool> IsExpandedProperty =
        Expander.IsExpandedProperty.AddOwner<OptionsDisplayItem>();

    public static readonly StyledProperty<ICommand> NavigationCommandProperty =
        AvaloniaProperty.Register<OptionsDisplayItem, ICommand>(nameof(NavigationCommand));

    public object? Header
    {
        get => GetValue(HeaderProperty);
        set => SetValue(HeaderProperty, value);
    }

    public object? Description
    {
        get => GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }

    public object? Icon
    {
        get => GetValue(IconProperty);
        set => SetValue(IconProperty, value);
    }

    public bool Navigates
    {
        get => GetValue(NavigatesProperty);
        set => SetValue(NavigatesProperty, value);
    }

    public Control ActionButton
    {
        get => GetValue(ActionButtonProperty);
        set => SetValue(ActionButtonProperty, value);
    }

    public bool Expands
    {
        get => GetValue(ExpandsProperty);
        set => SetValue(ExpandsProperty, value);
    }

    public object? Content
    {
        get => GetValue(ContentProperty);
        set => SetValue(ContentProperty, value);
    }

    public bool IsExpanded
    {
        get => GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    public ICommand NavigationCommand
    {
        get => GetValue(NavigationCommandProperty);
        set => SetValue(NavigationCommandProperty, value);
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

        if (change.Property == NavigatesProperty)
        {
            if (Expands)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":navigates", change.GetNewValue<bool>());
        }
        else if (change.Property == ExpandsProperty)
        {
            if (Navigates)
                throw new InvalidOperationException("Control cannot both Navigate and Expand");

            PseudoClasses.Set(":expands", change.GetNewValue<bool>());
        }
        else if (change.Property == DescriptionProperty)
        {
            PseudoClasses.Set(":desc", change.GetNewValue<object?>() != null);
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

            if (Navigates)
            {
                RaiseEvent(new RoutedEventArgs(NavigationRequestedEvent, this));
                NavigationCommand?.Execute(null);
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

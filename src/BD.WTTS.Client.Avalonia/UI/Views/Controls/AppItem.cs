using Avalonia.Rendering.Composition;
using Avalonia.Rendering.Composition.Animations;
using System.Numerics;

namespace BD.WTTS.UI.Views.Controls;

public class AppItem : TemplatedControl
{
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Title));

    public static readonly StyledProperty<string> DescriptionProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Description));

    public static readonly StyledProperty<string> StatusProperty =
        AvaloniaProperty.Register<AppItem, string>(nameof(Status));

    public static readonly StyledProperty<Control> ImageProperty =
        AvaloniaProperty.Register<AppItem, Control>(nameof(Image));

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

    public Control Image
    {
        get => GetValue(ImageProperty);
        set => SetValue(ImageProperty, value);
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
        else if (change.Property == ImageProperty)
            PseudoClasses.Set(":icon", change.NewValue != null);

    }

    protected override void OnPointerEntered(PointerEventArgs e)
    {
        base.OnPointerEntered(e);
        var ec = ElementComposition.GetElementVisual(this);
        if (ec == null) return;
        //_initOffset = ec.Offset;

        var comp = ec.Compositor;

        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(200);
        ani.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        ani.InsertKeyFrame(0f, ec.Offset);
        ani.InsertKeyFrame(1f, ec.Offset + new Vector3(0, -5, 0));
        ani.Target = "Offset";

        ec.StartAnimation("Offset", ani);
    }

    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);
        var ec = ElementComposition.GetElementVisual(this);
        if (ec == null) return;
        var comp = ec.Compositor;

        var ani = comp.CreateVector3KeyFrameAnimation();
        ani.Duration = TimeSpan.FromMilliseconds(100);
        ani.StopBehavior = AnimationStopBehavior.SetToFinalValue;
        ani.InsertKeyFrame(0f, ec.Offset);
        ani.InsertKeyFrame(1f, ec.Offset + new Vector3(0, 5, 0));
        ani.Target = "Offset";

        ec.StartAnimation("Offset", ani);
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

    private Vector3 _initOffset;
    private bool _isPressed;
    private bool _isExpanded;
    private Border? _layoutRoot;
}

using Avalonia.Automation.Peers;
using MouseButton = Avalonia.Input.MouseButton;
using Avalonia.Controls.Mixins;

namespace BD.WTTS.UI.Views.Controls;

[PseudoClasses(":pressed", ":selected")]
public class StepperItem : ContentControl, ISelectable
{
    /// <summary>
    /// Defines the <see cref="IsSelected"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsSelectedProperty =
        SelectingItemsControl.IsSelectedProperty.AddOwner<StepperItem>();

    private static readonly Point s_invalidPoint = new Point(double.NaN, double.NaN);
    private Point _pointerDownPoint = s_invalidPoint;

    /// <summary>
    /// Gets or sets the selection state of the item.
    /// </summary>
    public bool IsSelected
    {
        get { return GetValue(IsSelectedProperty); }
        set { SetValue(IsSelectedProperty, value); }
    }

    /// <summary>
    /// Initializes static members of the <see cref="StepperItem"/> class.
    /// </summary>
    static StepperItem()
    {
        SelectableMixin.Attach<StepperItem>(IsSelectedProperty);
        PressedMixin.Attach<StepperItem>();
        FocusableProperty.OverrideDefaultValue<StepperItem>(true);
    }

    public static readonly DirectProperty<StepperItem, bool> IsFirstProperty =
        AvaloniaProperty.RegisterDirect<StepperItem, bool>(nameof(IsFirst),
            o => o.IsFirst);

    public static readonly DirectProperty<StepperItem, bool> IsLastProperty =
        AvaloniaProperty.RegisterDirect<StepperItem, bool>(nameof(IsLast),
            o => o.IsLast);

    public bool IsFirst { get; internal set; }

    public bool IsLast { get; internal set; }

    public static readonly DirectProperty<StepperItem, int> IndexProperty =
        AvaloniaProperty.RegisterDirect<StepperItem, int>(nameof(Index)
           , o => o._index);

    int _index = 0;

    public int Index
    {
        get => _index;
        internal set => SetAndRaise(IndexProperty, ref _index, value);
    }

    public static readonly DirectProperty<StepperItem, StepStatus> StatusProperty =
        AvaloniaProperty.RegisterDirect<StepperItem, StepStatus>(nameof(Status)
            , o => o._status);

    private StepStatus _status = StepStatus.Waiting;

    public StepStatus Status
    {
        get => _status;
        internal set => SetAndRaise(StatusProperty, ref _status, value);
    }

    protected override AutomationPeer OnCreateAutomationPeer()
    {
        return base.OnCreateAutomationPeer();
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);

        _pointerDownPoint = s_invalidPoint;

        if (e.Handled)
            return;

        if (!e.Handled && ItemsControl.ItemsControlFromItemContaner(this) is Stepper owner)
        {
            var p = e.GetCurrentPoint(this);

            if (p.Properties.PointerUpdateKind is PointerUpdateKind.LeftButtonPressed or
                PointerUpdateKind.RightButtonPressed)
            {
                if (p.Pointer.Type == PointerType.Mouse)
                {
                    // If the pressed point comes from a mouse, perform the selection immediately.
                    e.Handled = owner.IsMouseSelectable
                        && owner.UpdateSelectionFromPointerEvent(this, e);
                }
                else
                {
                    // Otherwise perform the selection when the pointer is released as to not
                    // interfere with gestures.
                    _pointerDownPoint = p.Position;

                    // Ideally we'd set handled here, but that would prevent the scroll gesture
                    // recognizer from working.
                    ////e.Handled = true;
                }
            }
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);

        if (!e.Handled &&
            !double.IsNaN(_pointerDownPoint.X) &&
            e.InitialPressMouseButton is MouseButton.Left or MouseButton.Right)
        {
            var point = e.GetCurrentPoint(this);
            var settings = TopLevel.GetTopLevel(e.Source as Visual)?.PlatformSettings;
            var tapSize = settings?.GetTapSize(point.Pointer.Type) ?? new Size(4, 4);
            var tapRect = new Rect(_pointerDownPoint, new Size())
                .Inflate(new Thickness(tapSize.Width, tapSize.Height));

            if (new Rect(Bounds.Size).ContainsExclusive(point.Position) &&
                tapRect.ContainsExclusive(point.Position) &&
                ItemsControl.ItemsControlFromItemContaner(this) is Stepper owner)
            {
                if (owner.UpdateSelectionFromPointerEvent(this, e))
                    e.Handled = true;
            }
        }

        _pointerDownPoint = s_invalidPoint;
    }

}

using Avalonia.Controls.Shapes;

namespace BD.WTTS.UI.Views.Controls;

public class RippleEffect : ContentControl
{
    // ReSharper disable once InconsistentNaming
    private Canvas? PART_RippleCanvasRoot;

    private Ripple? _last;
    private byte _pointers;

    public RippleEffect()
    {
        AddHandler(LostFocusEvent, LostFocusHandler);
        AddHandler(PointerReleasedEvent, PointerReleasedHandler);
        AddHandler(PointerPressedEvent, PointerPressedHandler);
        AddHandler(PointerCaptureLostEvent, PointerCaptureLostHandler);
    }

    private void PointerPressedHandler(object? sender, PointerPressedEventArgs e)
    {
        if (!IsAllowedRaiseRipple)
            return;

        if (_pointers != 0)
            return;

        // Only first pointer can arrive a ripple
        _pointers++;
        var r = CreateRipple(e, RaiseRippleCenter);
        _last = r;

        // Attach ripple instance to canvas
        PART_RippleCanvasRoot?.Children.Add(r);
        r.RunFirstStep();
    }

    private void LostFocusHandler(object? sender, RoutedEventArgs e)
    {
        RemoveLastRipple();
    }

    private void PointerReleasedHandler(object? sender, PointerReleasedEventArgs e)
    {
        RemoveLastRipple();
    }

    private void PointerCaptureLostHandler(object? sender, PointerCaptureLostEventArgs e)
    {
        RemoveLastRipple();
    }

    private void RemoveLastRipple()
    {
        if (_last == null)
            return;

        _pointers--;

        // This way to handle pointer released is pretty tricky
        // could have more better way to improve
        OnReleaseHandler(_last);
        _last = null;
    }

    private void OnReleaseHandler(Ripple r)
    {
        // Fade out ripple
        r.RunSecondStep();

        void RemoveRippleTask(Task arg1, object? arg2)
        {
            Dispatcher.UIThread.InvokeAsync(() => { PART_RippleCanvasRoot?.Children.Remove(r); });
        }

        // Remove ripple from canvas to finalize ripple instance
        Task.Delay(Ripple.Duration).ContinueWith(RemoveRippleTask, null);
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        // Find canvas host
        PART_RippleCanvasRoot = e.NameScope.Find<Canvas>(nameof(PART_RippleCanvasRoot));
    }

    private Ripple CreateRipple(PointerPressedEventArgs e, bool center)
    {
        var w = Bounds.Width;
        var h = Bounds.Height;

        var r = new Ripple(w, h)
        {
            Fill = RippleFill
        };

        if (center) r.Margin = new Thickness(w / 2, h / 2, 0, 0);
        else r.SetupInitialValues(e, this);

        return r;
    }

    #region Styled properties

    public static readonly StyledProperty<IBrush> RippleFillProperty =
        AvaloniaProperty.Register<RippleEffect, IBrush>(nameof(RippleFill), inherits: true);

    public IBrush RippleFill
    {
        get => GetValue(RippleFillProperty);
        set => SetValue(RippleFillProperty, value);
    }

    public static readonly StyledProperty<double> RippleOpacityProperty =
        AvaloniaProperty.Register<RippleEffect, double>(nameof(RippleOpacity), inherits: true);

    public double RippleOpacity
    {
        get => GetValue(RippleOpacityProperty);
        set => SetValue(RippleOpacityProperty, value);
    }

    public static readonly StyledProperty<bool> RaiseRippleCenterProperty =
        AvaloniaProperty.Register<RippleEffect, bool>(nameof(RaiseRippleCenter));

    public bool RaiseRippleCenter
    {
        get => GetValue(RaiseRippleCenterProperty);
        set => SetValue(RaiseRippleCenterProperty, value);
    }

    public static readonly StyledProperty<bool> IsAllowedRaiseRippleProperty =
        AvaloniaProperty.Register<RippleEffect, bool>(nameof(IsAllowedRaiseRipple));

    public bool IsAllowedRaiseRipple
    {
        get => GetValue(IsAllowedRaiseRippleProperty);
        set => SetValue(IsAllowedRaiseRippleProperty, value);
    }

    #endregion Styled properties
}
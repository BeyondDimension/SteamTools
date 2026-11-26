using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;

namespace BD.WTTS.UI.Views.Controls;

public partial class WindowPicker : UserControl
{
    public static readonly StyledProperty<ICommand?> PickCommandProperty =
        AvaloniaProperty.Register<WindowPicker, ICommand?>(nameof(PickCommand));

    public static readonly StyledProperty<string?> InstructionProperty =
        AvaloniaProperty.Register<WindowPicker, string?>(nameof(Instruction));

    IPointer? pickerPointer;

    public WindowPicker() => InitializeComponent();

    public ICommand? PickCommand
    {
        get => GetValue(PickCommandProperty);
        set => SetValue(PickCommandProperty, value);
    }

    public string? Instruction
    {
        get => GetValue(InstructionProperty);
        set => SetValue(InstructionProperty, value);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        base.OnPointerPressed(e);
        if (pickerPointer is not null || e.Pointer.Type != PointerType.Mouse ||
            e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed ||
            PickCommand is not { } command || !command.CanExecute(null))
            return;

        e.Pointer.Capture(this);
        if (e.Pointer.Captured != this)
            return;

        pickerPointer = e.Pointer;
        Crosshair.Opacity = 0;
        e.Handled = true;
        try
        {
            command.Execute(null);
        }
        catch (Exception ex)
        {
            ResetPicker();
            ex.LogAndShowT(nameof(WindowPicker));
        }
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        if (e.Pointer != pickerPointer ||
            e.GetCurrentPoint(this).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased)
            return;

        ResetPicker();
        e.Handled = true;
    }

    protected override void OnPointerCaptureLost(PointerCaptureLostEventArgs e)
    {
        base.OnPointerCaptureLost(e);
        if (e.Pointer == pickerPointer)
            ResetPicker();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        ResetPicker();
        base.OnDetachedFromVisualTree(e);
    }

    void ResetPicker()
    {
        var pointer = pickerPointer;
        pickerPointer = null;
        Crosshair.Opacity = 1;
        if (pointer?.Captured == this)
            pointer.Capture(null);
    }
}

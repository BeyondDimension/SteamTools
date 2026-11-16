#if WTTS_NATIVE_AOT
namespace BD.WTTS.UI;

// Our generic page/window bases expose IViewFor directly. Use that contract rather
// than ReactiveUI's reflection-based view-model property observer in Native AOT.
internal sealed class NativeViewModelActivation
{
    readonly IViewFor view;
    IActivatableViewModel? activeViewModel;
    IDisposable? activation;
    bool attached;

    NativeViewModelActivation(Control control)
    {
        view = (IViewFor)control;
        control.AttachedToVisualTree += OnAttached;
        control.DetachedFromVisualTree += OnDetached;
        control.PropertyChanged += OnPropertyChanged;
        if (control is Window window)
        {
            window.Opened += OnOpened;
            window.Closed += OnClosed;
        }
    }

    internal static void Attach(Control control) => _ = new NativeViewModelActivation(control);

    void OnAttached(object? sender, VisualTreeAttachmentEventArgs args) => SetAttached(true);
    void OnDetached(object? sender, VisualTreeAttachmentEventArgs args) => SetAttached(false);
    void OnOpened(object? sender, EventArgs args) => SetAttached(true);
    void OnClosed(object? sender, EventArgs args) => SetAttached(false);

    void SetAttached(bool value)
    {
        attached = value;
        Update();
    }

    void OnPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs args)
    {
        if (args.Property.Name == nameof(IViewFor.ViewModel))
            Update();
    }

    void Update()
    {
        var next = attached ? view.ViewModel as IActivatableViewModel : null;
        if (ReferenceEquals(next, activeViewModel)) return;
        activation?.Dispose();
        activation = null;
        activeViewModel = null;
        if (next == null) return;
        activation = next.Activator.Activate();
        activeViewModel = next;
    }
}
#endif

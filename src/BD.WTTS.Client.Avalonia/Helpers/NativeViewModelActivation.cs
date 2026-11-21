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

    Window? ownerWindow;
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

    void OnAttached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        ownerWindow = TopLevel.GetTopLevel((Control)sender!) as Window;
        SetAttached(true);
    }

    void OnDetached(object? sender, VisualTreeAttachmentEventArgs args)
    {
        attached = false;
        Update(deferDeactivation: true);
        ownerWindow = null;
    }

    void OnOpened(object? sender, EventArgs args)
    {
        ownerWindow = (Window)sender!;
        SetAttached(true);
    }

    void OnClosed(object? sender, EventArgs args)
    {
        SetAttached(false);
        ownerWindow = null;
    }

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

    void Update(bool deferDeactivation = false)
    {
        var next = attached ? view.ViewModel as IActivatableViewModel : null;
        if (ReferenceEquals(next, activeViewModel)) return;
        var previous = activation;
        var previousViewModel = activeViewModel;
        activation = null;
        activeViewModel = null;
        if (previous != null)
        {
            if (deferDeactivation) ReleaseAfterDetach(previousViewModel, previous);
            else ReleaseActivation(previousViewModel, previous);
        }
        next = attached ? view.ViewModel as IActivatableViewModel : null;
        if (next == null || ReferenceEquals(next, activeViewModel)) return;
        activeViewModel = next;
        activation = next.Activator.Activate();
    }

    static void ReleaseActivation(IActivatableViewModel? viewModel, IDisposable lease)
    {
        // Disposing a ViewModelBase already tears down its activator; a late lease would notify its disposed subjects.
        if (viewModel is not ViewModelBase { Disposed: true })
            lease.Dispose();
    }

    void ReleaseAfterDetach(IActivatableViewModel? viewModel, IDisposable lease)
    {
        var window = ownerWindow;
        IDisposable? pending = lease;
        EventHandler? closed = null;
        void Release()
        {
            var retired = pending;
            if (retired == null) return;
            pending = null;
            if (window != null) window.Closed -= closed;
            ReleaseActivation(viewModel, retired);
        }

        // Deactivation may remove a sibling while Avalonia is walking a cached child count.
        // Window.Closed also drains the old lease if application shutdown stops the queue.
        closed = (_, _) => Release();
        if (window != null) window.Closed += closed;
        Dispatcher.UIThread.Post(Release, DispatcherPriority.Background);
    }
}
#endif

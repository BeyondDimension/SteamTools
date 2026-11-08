using FluentAvalonia.UI.Navigation;

namespace BD.WTTS.UI.Views.Windows;

/// <summary>State needed to rebuild a main window without retaining its controls.</summary>
internal sealed class MainWindowViewState
{
    internal Type? CurrentPage { get; init; }
    internal object? CurrentParameter { get; init; }
    internal NavigationEntry[] BackStack { get; init; } = [];
    internal NavigationEntry[] ForwardStack { get; init; } = [];
    internal Dictionary<Type, PageState> Pages { get; set; } = new();
    internal PixelPoint Position { get; set; }
    internal double Width { get; set; }
    internal double Height { get; set; }
    internal WindowState WindowState { get; set; }

    internal readonly record struct NavigationEntry(Type PageType, object? Parameter)
    {
        internal FAPageStackEntry ToEntry() => new(PageType, Parameter!, null!);
    }

    internal sealed record PageState(ViewModelBase? ViewModel, Vector? Offset, bool? IsPaneOpen, Dictionary<string, int> Tabs);
}

/// <summary>Temporary page state consumed by newly constructed views.</summary>
internal static class TrayPageState
{
    static Dictionary<Type, MainWindowViewState.PageState>? pages;

    internal static void Set(Dictionary<Type, MainWindowViewState.PageState> value) => pages = value;
    internal static MainWindowViewState.PageState Capture(Control page)
    {
        var viewModel = (page as IViewFor)?.ViewModel as ViewModelBase;
        var offset = (page as PageBase)?.PageScroller?.Offset;
        var tabs = new Dictionary<string, int>();
        var nameScope = page.FindNameScope();
        foreach (var name in new[] { "Tabs", "AcceleratorTabs", "TabSelect" })
        {
            if (nameScope?.Find<TabStrip>(name) is { SelectedIndex: >= 0 } tab)
                tabs[name] = tab.SelectedIndex;
        }
        return new(viewModel, offset, (page as PageBase)?.IsPaneOpen, tabs);
    }

    internal static void Restore(Control page)
    {
        var state = Take(page.GetType());
        if (state == null)
            return;

        var nameScope = page.FindNameScope();
        foreach (var (name, selectedIndex) in state.Tabs)
        {
            if (nameScope?.Find<TabStrip>(name) is { } tab && selectedIndex < tab.ItemCount)
                tab.SelectedIndex = selectedIndex;
        }
        if (state.IsPaneOpen is { } isPaneOpen && page is PageBase panePage)
            panePage.IsPaneOpen = isPaneOpen;


        if (state.Offset is { } offset && page is PageBase pageBase)
            RestoreOffset(pageBase, offset);
    }

    static void RestoreOffset(PageBase page, Vector offset)
    {
        EventHandler? layout = null;
        EventHandler<VisualTreeAttachmentEventArgs>? detached = null;
        EventHandler<PointerPressedEventArgs>? pointer = null;
        EventHandler<PointerWheelEventArgs>? wheel = null;
        EventHandler<Avalonia.Input.KeyEventArgs>? key = null;
        IDisposable? timeout = null;
        IDisposable? settle = null;
        var stopped = false;

        void Stop()
        {
            if (stopped) return;
            stopped = true;
            page.LayoutUpdated -= layout;
            page.DetachedFromVisualTree -= detached;
            page.RemoveHandler(InputElement.PointerPressedEvent, pointer!);
            page.RemoveHandler(InputElement.PointerWheelChangedEvent, wheel!);
            page.RemoveHandler(InputElement.KeyDownEvent, key!);
            settle?.Dispose();
            settle = null;
            timeout?.Dispose();
            timeout = null;
        }

        void Apply(bool final)
        {
            if (stopped) return;
            if (page.PageScroller is not { } scroller)
            {
                if (final) Stop();
                return;
            }
            var maxX = Math.Max(0, scroller.Extent.Width - scroller.Viewport.Width);
            var maxY = Math.Max(0, scroller.Extent.Height - scroller.Viewport.Height);
            if (!final && (offset.X > maxX || offset.Y > maxY))
                return;
            scroller.Offset = new Vector(Math.Min(offset.X, maxX), Math.Min(offset.Y, maxY));
            Stop();
        }

        void ScheduleWhenReady()
        {
            if (stopped || settle != null || page.PageScroller is not { } scroller)
                return;
            var maxX = Math.Max(0, scroller.Extent.Width - scroller.Viewport.Width);
            var maxY = Math.Max(0, scroller.Extent.Height - scroller.Viewport.Height);
            if (offset.X <= maxX && offset.Y <= maxY)
                settle = DispatcherTimer.RunOnce(() =>
                {
                    settle = null;
                    Apply(false);
                }, TimeSpan.FromMilliseconds(500), DispatcherPriority.Background);
        }

        layout = (_, _) => ScheduleWhenReady();
        detached = (_, _) => Stop();
        pointer = (_, _) => Stop();
        wheel = (_, _) => Stop();
        key = (_, _) => Stop();
        page.AddHandler(InputElement.PointerPressedEvent, pointer, RoutingStrategies.Tunnel, handledEventsToo: true);
        page.AddHandler(InputElement.PointerWheelChangedEvent, wheel, RoutingStrategies.Tunnel, handledEventsToo: true);
        page.AddHandler(InputElement.KeyDownEvent, key, RoutingStrategies.Tunnel, handledEventsToo: true);
        page.LayoutUpdated += layout;
        page.DetachedFromVisualTree += detached;
        timeout = DispatcherTimer.RunOnce(() => Apply(true), TimeSpan.FromSeconds(2), DispatcherPriority.Background);
        Dispatcher.UIThread.Post(ScheduleWhenReady, DispatcherPriority.Loaded);
    }



    internal static T? GetViewModel<T>(Type pageType) where T : class
        => pages != null && pages.TryGetValue(pageType, out var state) ? state.ViewModel as T : null;

    internal static MainWindowViewState.PageState? Take(Type pageType)
    {
        if (pages == null || !pages.Remove(pageType, out var state))
            return null;
        if (pages.Count == 0)
            pages = null;
        return state;
    }

    internal static void Clear() => pages = null;
}

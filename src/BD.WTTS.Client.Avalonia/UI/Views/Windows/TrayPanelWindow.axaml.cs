using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace BD.WTTS.UI.Views.Windows;

sealed partial class TrayPanelWindow : Window
{
    readonly PixelPoint? anchor;
    readonly Action? restoreTrayFocus;
    bool closed;

    public TrayPanelWindow()
    {
        InitializeComponent();
        Deactivated += (_, _) => Dismiss();
    }

    internal TrayPanelWindow(TrayPanelViewModel viewModel, PixelPoint? anchor, Action? restoreTrayFocus) : this()
    {
        DataContext = viewModel;
        this.anchor = anchor;
        this.restoreTrayFocus = restoreTrayFocus;
        var screen = anchor is { } point ? Screens.ScreenFromPoint(point) ?? Screens.Primary : Screens.Primary;
        if (screen != null)
        {
            Width = Math.Min(Width, screen.WorkingArea.Width / screen.Scaling);
            MaxHeight = Math.Min(MaxHeight, screen.WorkingArea.Height / screen.Scaling);
        }
        if (anchor is { } position)
            Position = position;
        else
            WindowStartupLocation = WindowStartupLocation.CenterScreen;
    }

    internal TrayPanelViewModel ViewModel => (TrayPanelViewModel)DataContext!;

    internal void SetOtherActions(IEnumerable<TrayMenuItem> items)
    {
        var flyout = (MenuFlyout?)PluginActionsButton.Flyout;
        flyout?.Items.Clear();
        foreach (var item in items)
        {
            if (item.Command == null && item.Items is not { Count: > 0 })
                continue;
            flyout ??= new MenuFlyout { Placement = PlacementMode.TopEdgeAlignedRight };
            flyout.Items.Add(CreateMenuItem(item));
        }
        PluginActionsButton.Flyout = flyout;
        PluginActionsButton.IsVisible = flyout?.Items.Count > 0;
    }

    MenuItem CreateMenuItem(TrayMenuItem item)
    {
        var action = new MenuItem
        {
            Header = App.GetTrayItemName(item),
            CommandParameter = item.CommandParameter,
            Command = item.Command,
        };
        if (item.IsVisible is BindingBase visible)
            action[!IsVisibleProperty] = visible;
        if (item.IsEnabled is BindingBase enabled)
            action[!IsEnabledProperty] = enabled;
        if (item.Items is { Count: > 0 } children)
        {
            foreach (var child in children)
                action.Items.Add(CreateMenuItem(child));
        }
        else
        {
            action.AddHandler(MenuItem.ClickEvent, DismissAfterAction, handledEventsToo: true);
        }
        return action;
    }

    void DismissAfterAction(object? sender, RoutedEventArgs e)
    {
        // Let the click-bound command execute before closing its menu.
        if (ReferenceEquals(sender, e.Source))
            Dispatcher.UIThread.Post(() => Dismiss());
    }

    protected override void OnOpened(EventArgs e)
    {
        base.OnOpened(e);
        UpdatePosition(ClientSize);
        MainPanelButton.Focus();
    }

    protected override void OnSizeChanged(SizeChangedEventArgs e)
    {
        base.OnSizeChanged(e);
        UpdatePosition(e.NewSize);
    }

    void UpdatePosition(Size size)
    {
        if (anchor is not { } point || size.Width <= 0 || size.Height <= 0 ||
            (Screens.ScreenFromPoint(point) ?? Screens.Primary) is not { } screen)
            return;

        var area = screen.WorkingArea;
        var pixels = PixelSize.FromSize(size, screen.Scaling);
        var gap = (int)Math.Ceiling(6 * screen.Scaling);
        var x = Math.Clamp(point.X - pixels.Width / 2, area.X, Math.Max(area.X, area.Right - pixels.Width));
        var above = point.Y - gap - pixels.Height;
        var below = point.Y + gap;
        var top = above >= area.Y || below + pixels.Height > area.Bottom ? above : below;
        var y = Math.Clamp(top, area.Y, Math.Max(area.Y, area.Bottom - pixels.Height));
        var position = new PixelPoint(x, y);
        if (Position != position)
            Position = position;
    }

    protected override void OnKeyDown(KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (!e.Handled && e.Key == Key.Escape)
        {
            e.Handled = true;
            Dismiss(returnFocus: true);
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        closed = true;
        (DataContext as TrayPanelViewModel)?.Dispose();
        base.OnClosed(e);
    }

    void Dismiss(bool returnFocus = false)
    {
        if (closed) return;
        Close();
        if (returnFocus)
            restoreTrayFocus?.Invoke();
    }

    void Restore_OnClick(object? sender, RoutedEventArgs e)
    {
        Dismiss();
        App.Instance.RestoreMainWindow();
    }

    void Exit_OnClick(object? sender, RoutedEventArgs e) => App.Instance.Shutdown();
}

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class AuthenticatorHomePage : PageBase<AuthenticatorHomePageViewModel>
{
    AuthenticatorItemModel? dragItem;
    AuthenticatorItem? dragSource;
    AuthenticatorItem? dragTarget;
    AuthenticatorItemModel? dragTargetItem;
    IPointer? dragPointer;
    TopLevel? dragRoot;
    Point dragPosition;
    Point dragStart;
    Point dragOffset;
    bool dragAfter;
    bool dragging;
    bool dragMoved;
    readonly DispatcherTimer dragTimer = new() { Interval = TimeSpan.FromMilliseconds(40) };

    public AuthenticatorHomePage()
    {
        InitializeComponent();
        DataContext ??= IViewModelManager.Instance.Get<AuthenticatorHomePageViewModel>();
        PointerCaptureLost += TokenPointerCaptureLost;
        KeyDown += (_, e) =>
        {
            if (e.Key == Avalonia.Input.Key.Escape && dragPointer != null)
            {
                CancelTokenDrag();
                e.Handled = true;
            }
        };
        DetachedFromVisualTree += (_, _) => CancelTokenDrag();
        dragTimer.Tick += (_, _) => UpdateTokenDrag();
    }

    void FocusForReorder()
    {
        // ItemsRepeater pins focused cards; keeping focus on a recycled menu owner
        // can leave stale cards after a move followed by filtering.
        Focusable = true;
        Focus(NavigationMethod.Pointer);
    }

    void OnMoveAuthenticatorUp(object? sender, RoutedEventArgs e) => MoveFromMenu(sender, up: true);

    void OnMoveAuthenticatorDown(object? sender, RoutedEventArgs e) => MoveFromMenu(sender, up: false);

    void MoveFromMenu(object? sender, bool up)
    {
        if (sender is not MenuItem { DataContext: AuthenticatorItemModel item } ||
            DataContext is not AuthenticatorHomePageViewModel vm) return;
        // Wait until the flyout restores focus, then move it away before changing indices.
        Dispatcher.UIThread.Post(async () =>
        {
            if (DataContext != vm || vm.Disposed) return;
            FocusForReorder();
            if (up) await vm.AuthenticatorIndexMoveUp(item);
            else await vm.AuthenticatorIndexMoveDown(item);
        }, DispatcherPriority.Background);
    }

    internal void BeginTokenPress(AuthenticatorItem source, AuthenticatorItemModel item, PointerPressedEventArgs e)
    {
        if (PageScroller == null || DataContext is not AuthenticatorHomePageViewModel { Disposed: false, IsLoading: false } ||
            TopLevel.GetTopLevel(this) is not { } root) return;

        CancelTokenDrag();
        dragSource = source;
        dragItem = item;
        dragPointer = e.Pointer;
        dragRoot = root;
        dragStart = dragPosition = e.GetPosition(PageScroller);
        dragOffset = e.GetPosition(source);
        root.AddHandler(PointerMovedEvent, TokenPointerMoved, RoutingStrategies.Tunnel, handledEventsToo: true);
        root.AddHandler(PointerReleasedEvent, TokenPointerReleased, RoutingStrategies.Tunnel, handledEventsToo: true);
        root.PointerExited += TokenRootPointerExited;
    }

    void StartTokenDrag()
    {
        if (dragPointer is not { } pointer || dragSource is not { } source || dragItem is not { } item ||
            !ReferenceEquals(source.DataContext, item) || PageScroller == null ||
            DataContext is not AuthenticatorHomePageViewModel { Disposed: false, IsLoading: false })
        {
            CancelTokenDrag();
            return;
        }

        if (pointer.Captured is Visual captured && !ReferenceEquals(captured, source) &&
            !captured.GetVisualAncestors().Contains(source))
        {
            CancelTokenDrag();
            return;
        }

        dragging = true;
        pointer.Capture(this);
        if (!ReferenceEquals(pointer.Captured, this))
        {
            CancelTokenDrag();
            return;
        }
        FocusForReorder();
        source.Classes.Add("Dragging");
        DragPreview.DataContext = item;
        DragPreviewIcon.Data = source.IconGeometry;
        DragPreview.Width = source.Bounds.Width;
        DragPreview.Height = source.Bounds.Height;
        DragPreview.IsVisible = true;
        MoveDragPreview();
    }

    void TokenPointerMoved(object? sender, PointerEventArgs e)
    {
        if (e.Pointer != dragPointer || PageScroller == null) return;
        dragPosition = e.GetPosition(PageScroller);
        var dx = Math.Abs(dragPosition.X - dragStart.X);
        var dy = Math.Abs(dragPosition.Y - dragStart.Y);
        if (!dragging)
        {
            if (dx <= 8 && dy <= 8) return;
            StartTokenDrag();
            if (!dragging) return;
        }
        if (!dragMoved && (dx > 4 || dy > 4))
        {
            dragMoved = true;
            dragTimer.Start();
        }
        MoveDragPreview();
        if (dragMoved) UpdateTokenDrag();
        e.Handled = true;
    }

    void MoveDragPreview()
    {
        if (PageScroller?.TranslatePoint(dragPosition, DragOverlay) is not { } point) return;
        Canvas.SetLeft(DragPreview, point.X - dragOffset.X);
        Canvas.SetTop(DragPreview, point.Y - dragOffset.Y);
    }

    void TokenRootPointerExited(object? sender, PointerEventArgs e)
    {
        if (!dragging && e.Pointer == dragPointer && ReferenceEquals(e.Source, dragRoot)) CancelTokenDrag();
    }

    void TokenPointerCaptureLost(object? sender, PointerCaptureLostEventArgs e)
    {
        if (e.Pointer == dragPointer && !ReferenceEquals(e.Pointer.Captured, this)) CancelTokenDrag();
    }

    void UpdateTokenDrag()
    {
        if (!dragging || PageScroller == null || DataContext is not AuthenticatorHomePageViewModel vm) return;
        var viewport = PageScroller.Viewport;
        var y = dragPosition.Y;
        if (y < 45 || y > viewport.Height - 45)
        {
            var delta = y < 45 ? -16 : 16;
            var max = Math.Max(0, PageScroller.Extent.Height - viewport.Height);
            PageScroller.Offset = new Vector(PageScroller.Offset.X, Math.Clamp(PageScroller.Offset.Y + delta, 0, max));
        }
        MoveDragPreview();

        AuthenticatorItem? nearest = null;
        var bestDistance = double.MaxValue;
        var after = false;
        foreach (var candidate in AuthRepeater.GetVisualDescendants().OfType<AuthenticatorItem>())
        {
            if (candidate.DataContext is not AuthenticatorItemModel model || !vm.Auths.Contains(model)) continue;
            var origin = candidate.TranslatePoint(new Point(), PageScroller);
            if (origin == null) continue;
            var center = origin.Value + new Vector(candidate.Bounds.Width / 2, candidate.Bounds.Height / 2);
            var dx = dragPosition.X - center.X;
            var dy = dragPosition.Y - center.Y;
            var distance = dx * dx + dy * dy;
            if (distance < bestDistance)
            {
                bestDistance = distance;
                nearest = candidate;
                after = dragPosition.X >= center.X;
            }
        }
        dragTarget?.Classes.Remove("DropBefore");
        dragTarget?.Classes.Remove("DropAfter");
        dragTarget = nearest;
        dragTargetItem = nearest?.DataContext as AuthenticatorItemModel;
        dragAfter = after;
        dragTarget?.Classes.Add(after ? "DropAfter" : "DropBefore");
    }

    async void TokenPointerReleased(object? sender, PointerReleasedEventArgs e)
    {
        if (e.Pointer != dragPointer) return;
        var item = dragItem;
        var target = dragTargetItem;
        var after = dragAfter;
        var wasDragging = dragging;
        var valid = dragging && dragMoved && PageScroller != null && new Rect(PageScroller.Viewport).Contains(e.GetPosition(PageScroller));
        CancelTokenDrag();
        if (!wasDragging) return;
        e.Handled = true;
        if (valid && item != null && target != null && DataContext is AuthenticatorHomePageViewModel vm)
            await vm.ReorderAuthenticator(item, target, after);
    }

    void CancelTokenDrag()
    {
        dragTimer.Stop();
        dragTarget?.Classes.Remove("DropBefore");
        dragTarget?.Classes.Remove("DropAfter");
        dragSource?.Classes.Remove("Dragging");
        DragPreview.IsVisible = false;
        DragPreview.DataContext = null;
        DragPreviewIcon.Data = null;
        dragTarget = null;
        dragSource = null;
        dragItem = null;
        dragTargetItem = null;
        dragging = false;
        dragMoved = false;
        var root = dragRoot;
        dragRoot = null;
        if (root != null)
        {
            root.RemoveHandler(PointerMovedEvent, TokenPointerMoved);
            root.RemoveHandler(PointerReleasedEvent, TokenPointerReleased);
            root.PointerExited -= TokenRootPointerExited;
        }
        var pointer = dragPointer;
        dragPointer = null;
        if (ReferenceEquals(pointer?.Captured, this)) pointer.Capture(null);
    }
}
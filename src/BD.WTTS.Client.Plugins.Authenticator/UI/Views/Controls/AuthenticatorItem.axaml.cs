using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.VisualTree;
using BD.WTTS.UI.Views.Pages;

namespace BD.WTTS.UI.Views.Controls;

public partial class AuthenticatorItem : UserControl
{
    AuthenticatorItemModel? currentItem;
    bool inViewport;

    internal Geometry? IconGeometry => PlatformIcon.Data;

    void UpdatePlatformIcon()
    {
        var key = (DataContext as AuthenticatorItemModel)?.DisplayPlatform switch
        {
            AuthenticatorPlatform.Steam => "Steam",
            AuthenticatorPlatform.BattleNet => "BattleNet",
            AuthenticatorPlatform.Google => "Google",
            AuthenticatorPlatform.Microsoft => "Microsoft",
            AuthenticatorPlatform.HOTP => "AuthenticatorHOTP",
            AuthenticatorPlatform.TOTP => "AuthenticatorTOTP",
            _ => "AuthenticatorUnknown",
        };
        PlatformIcon.Data = this.TryFindResource(key, ActualThemeVariant, out var resource) ? resource as Geometry : null;
    }

    void UpdateCodeVisibility() => currentItem?.SetIsVisible(
        inViewport && IsEffectivelyVisible && TopLevel.GetTopLevel(this) != null);

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        currentItem = DataContext as AuthenticatorItemModel;
        UpdatePlatformIcon();
        UpdateCodeVisibility();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        inViewport = false;
        currentItem?.SetIsVisible(false);
        currentItem = null;
        base.OnDetachedFromVisualTree(e);
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        Classes.Remove("DropBefore");
        Classes.Remove("DropAfter");
        Classes.Remove("Dragging");
        currentItem?.SetIsVisible(false);
        base.OnDataContextChanged(e);
        currentItem = DataContext as AuthenticatorItemModel;
        UpdatePlatformIcon();
        UpdateCodeVisibility();
    }
    public AuthenticatorItem()
    {
        InitializeComponent();

        EffectiveViewportChanged += (_, e) =>
        {
            inViewport = e.EffectiveViewport.Intersects(new Rect(Bounds.Size));
            UpdateCodeVisibility();
        };
        ToggleEye.Click += AuthenticatorItem_Click;
        AddHandler(PointerPressedEvent, CardPointerPressed, RoutingStrategies.Tunnel, handledEventsToo: true);
        this.DoubleTapped += AuthenticatorItem_DoubleTapped;
    }

    void CardPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (DataContext is AuthenticatorItemModel item &&
            (e.Pointer.Type == PointerType.Touch || e.GetCurrentPoint(this).Properties.IsLeftButtonPressed))
            this.GetVisualAncestors().OfType<AuthenticatorHomePage>().FirstOrDefault()?.BeginTokenPress(this, item, e);
    }

    private async void AuthenticatorItem_DoubleTapped(object? sender, TappedEventArgs e)
    {
        if (DataContext is AuthenticatorItemModel authenticatorItemModel)
        {
            await authenticatorItemModel.CopyCode();
        }
    }

    private void AuthenticatorItem_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (DataContext is AuthenticatorItemModel authenticatorItemModel)
        {
            authenticatorItemModel.Tapped();
            e.Handled = true;
        }
    }

}

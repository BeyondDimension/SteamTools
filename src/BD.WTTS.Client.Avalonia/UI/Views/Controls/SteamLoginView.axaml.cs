using Avalonia.VisualTree;
using ReactiveUI.Avalonia;

namespace BD.WTTS.UI.Views.Controls;

public partial class SteamLoginView : ReactiveUserControl<SteamLoginViewModel>
{
    public static readonly StyledProperty<bool> ShowRememberLoginProperty =
        AvaloniaProperty.Register<SteamLoginView, bool>(nameof(ShowRememberLogin));
    public static readonly StyledProperty<bool> RememberLoginProperty =
        AvaloniaProperty.Register<SteamLoginView, bool>(nameof(RememberLogin));

    public bool ShowRememberLogin
    {
        get => GetValue(ShowRememberLoginProperty);
        set => SetValue(ShowRememberLoginProperty, value);
    }

    public bool RememberLogin
    {
        get => GetValue(RememberLoginProperty);
        set => SetValue(RememberLoginProperty, value);
    }

    bool cookieMode;
    readonly List<Visual> visibilityAncestors = new();
    SteamLoginViewModel? currentModel;
    bool isAttached;
    bool inViewport;
    bool qrVisible;
    bool? narrowLayout;

    public SteamLoginView()
    {
        InitializeComponent();
        EffectiveViewportChanged += (_, e) =>
        {
            inViewport = e.EffectiveViewport.Intersects(new Rect(Bounds.Size));
            UpdateQrVisibility();
        };
        FormsLayout.SizeChanged += (_, e) => UpdateLayout(e.NewSize.Width);
    }

    void UpdateLayout(double width)
    {
        // Two equal columns must each fit the QR image and its padding.
        var narrow = width < 488;
        if (narrowLayout == narrow) return;
        narrowLayout = narrow;
        Grid.SetColumnSpan(AccountPanel, narrow ? 2 : 1);
        Grid.SetColumnSpan(QrPanel, narrow ? 2 : 1);
        Grid.SetColumn(QrPanel, narrow ? 0 : 1);
        Grid.SetRow(QrPanel, narrow ? 1 : 0);
    }

    void SetCurrentModel(SteamLoginViewModel? model)
    {
        if (ReferenceEquals(currentModel, model)) return;
        currentModel?.SetQrLoginVisible(false);
        if (cookieMode)
        {
            cookieMode = false;
            FormsLayout.IsVisible = true;
            CookiePanel.IsVisible = false;
            CookieSwitch.IsVisible = true;
            AccountSwitch.IsVisible = false;
        }
        currentModel = model;
        qrVisible = false;
        UpdateQrVisibility();
    }

    void UpdateQrVisibility()
    {
        var visible = !cookieMode && isAttached && inViewport && IsVisible && visibilityAncestors.All(x =>
            x.IsVisible && (x is not Avalonia.Controls.Window window || window.WindowState != Avalonia.Controls.WindowState.Minimized));
        if (qrVisible == visible) return;
        qrVisible = visible;
        currentModel?.SetQrLoginVisible(visible);
    }

    void OnAncestorPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.IsVisibleProperty || e.Property == Avalonia.Controls.Window.WindowStateProperty)
            UpdateQrVisibility();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        SetCurrentModel(DataContext as SteamLoginViewModel);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);
        if (change.Property == Visual.IsVisibleProperty) UpdateQrVisibility();
        if (change.Property.Name == nameof(ViewModel))
            SetCurrentModel(ViewModel);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        isAttached = true;
        foreach (var ancestor in this.GetVisualAncestors())
        {
            visibilityAncestors.Add(ancestor);
            ancestor.PropertyChanged += OnAncestorPropertyChanged;
        }
        SetCurrentModel(DataContext as SteamLoginViewModel);
        UpdateQrVisibility();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        isAttached = false;
        inViewport = false;
        foreach (var ancestor in visibilityAncestors)
            ancestor.PropertyChanged -= OnAncestorPropertyChanged;
        visibilityAncestors.Clear();
        UpdateQrVisibility();
        base.OnDetachedFromVisualTree(e);
    }

    void OnSwitchLoginMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        cookieMode = !cookieMode;
        if (cookieMode) UpdateQrVisibility();
        currentModel?.Cancel();
        FormsLayout.IsVisible = !cookieMode;
        CookiePanel.IsVisible = cookieMode;
        CookieSwitch.IsVisible = !cookieMode;
        AccountSwitch.IsVisible = cookieMode;
        if (!cookieMode) UpdateQrVisibility();
    }

    async void OnCredentialKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.Return || currentModel == null || currentModel.IsBusy) return;
        e.Handled = true;
        await currentModel.LoginAsync();
    }

    async void OnCookieKeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
    {
        if (e.Key != Avalonia.Input.Key.Return || currentModel == null || currentModel.IsBusy) return;
        e.Handled = true;
        await currentModel.CookieLoginAsync();
    }
}

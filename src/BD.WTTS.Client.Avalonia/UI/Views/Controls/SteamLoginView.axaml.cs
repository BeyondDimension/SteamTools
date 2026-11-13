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

    int loginMode;
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
        currentModel?.Cancel();
        currentModel?.ClearSavedAccounts();
        currentModel = null;
        if (loginMode != 0) SetLoginMode(0);
        currentModel = model;
        SavedAccountSwitch.IsVisible = model?.AllowSavedAccountLogin == true;
        SavedAccountSeparator.IsVisible = model?.AllowSavedAccountLogin == true;
        qrVisible = false;
        UpdateQrVisibility();
    }

    void UpdateQrVisibility()
    {
        var visible = loginMode == 0 && isAttached && inViewport && IsVisible && visibilityAncestors.All(x =>
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

    void SetLoginMode(int mode)
    {
        if (loginMode == mode) return;
        loginMode = mode;
        UpdateQrVisibility();
        currentModel?.Cancel();
        if (mode != 2) currentModel?.ClearSavedAccounts();
        FormsLayout.IsVisible = mode == 0;
        CookiePanel.IsVisible = mode == 1;
        SavedAccountPanel.IsVisible = mode == 2;
        AccountSwitch.Classes.Set("loginModeSelected", mode == 0);
        CookieSwitch.Classes.Set("loginModeSelected", mode == 1);
        SavedAccountSwitch.Classes.Set("loginModeSelected", mode == 2);
        UpdateQrVisibility();
    }

    void OnCookieMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => SetLoginMode(1);
    void OnAccountMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => SetLoginMode(0);
    async void OnSavedAccountMode(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (loginMode == 2) return;
        SetLoginMode(2);
        if (currentModel != null) await currentModel.LoadSavedAccountsAsync();
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

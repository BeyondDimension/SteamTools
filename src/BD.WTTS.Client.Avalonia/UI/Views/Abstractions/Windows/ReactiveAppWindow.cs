namespace BD.WTTS.UI.Views.Windows;

/// <summary>
/// A ReactiveUI <see cref="Window"/> that implements the <see cref="IViewFor{TViewModel}"/> interface and will
/// activate your ViewModel automatically if the view model implements <see cref="IActivatableViewModel"/>. When
/// the DataContext property changes, this class will update the ViewModel property with the new DataContext value,
/// and vice versa.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
public class ReactiveAppWindow<TViewModel> : AppWindow, IViewFor<TViewModel>, IViewFor, IActivatableView where TViewModel : class
{
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    public static readonly StyledProperty<bool> IsSaveWindowSizeProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, bool>(nameof(IsSaveWindowSize), true);

    private bool startupPlacementChecked;

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveAppWindow{TViewModel}"/> class.
    /// </summary>
    public ReactiveAppWindow() : base()
    {
#if WINDOWS
        if (OperatingSystem2.IsWindows() && !Design.IsDesignMode)
        {
            if (!GeneralSettings.GPU.Value)
            {
                var hWND = this.TryGetPlatformHandle()?.Handle;
                if (hWND != null)
                    IPlatformService.Instance.BeautifyTheWindow(hWND.Value);
            }
            else if (UISettings.WindowBackgroundMaterial.Value is WindowBackgroundMaterial.None or WindowBackgroundMaterial.Blur)
            {
                var hWND = this.TryGetPlatformHandle()?.Handle;
                if (hWND != null)
                    IPlatformService.Instance.BeautifyTheWindow(hWND.Value);
            }
        }
#endif

#if WTTS_NATIVE_AOT
        NativeViewModelActivation.Attach(this);
#else
        this.WhenActivated();
#endif
        this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);

#if WINDOWS
        WindowDecorations = WindowDecorations.BorderOnly;
#endif
        this.TryFindResource("TitleBarHeight", App.Instance.RequestedThemeVariant, out object? titleBarHeight);
        TitleBar.Height = (double?)titleBarHeight ?? 60;
        TitleBar.ExtendsContentIntoTitleBar = true;

        Background = null;
        TransparencyBackgroundFallback = OperatingSystem2.IsMacOS() ? Brushes.Black : Brushes.Transparent;
        TransparencyLevelHint = new WindowTransparencyLevel[] { UISettings.WindowBackgroundMaterial.Value.ToWindowTransparencyLevel() };

        if (IsSaveWindowSize)
        {
            var windowName = GetType().Name;
            if (windowName != null && UISettings.WindowSizePositions.TryGetValue(windowName, out var sizePosition))
            {
                SizePosition = sizePosition;
            }
        }

        //UISettings.EnableCustomBackgroundImage.ValueChanged += (sender, e) =>
        //{
        //    PseudoClasses.Set(":image", e.NewValue);
        //};
        //UISettings.EnableCustomBackgroundImage.RaiseValueChanged();
    }

    SizePosition? SizePosition { get; set; }

    public bool IsSaveWindowSize
    {
        get => GetValue(IsSaveWindowSizeProperty);
        set => SetValue(IsSaveWindowSizeProperty, value);
    }

    /// <summary>
    /// The ViewModel.
    /// </summary>
    public TViewModel? ViewModel
    {
        get => GetValue(ViewModelProperty);
        set => SetValue(ViewModelProperty, value);
    }

    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (TViewModel?)value;
    }

    private void OnDataContextChanged(object? value)
    {
        if (value is TViewModel viewModel)
        {
            ViewModel = viewModel;
        }
        else
        {
            ViewModel = null;
        }
    }

    private void OnViewModelChanged(object? value)
    {
        if (value == null)
        {
            ClearValue(DataContextProperty);
        }
        else if (DataContext != value)
        {
            DataContext = value;
        }
    }

    public override void Show()
    {
        if (!startupPlacementChecked && !IsVisible)
        {
            startupPlacementChecked = true;
            if (IsSaveWindowSize && CanResize && SizePosition is { } saved)
                RestoreWindowPlacement(saved);
        }
        base.Show();
    }

    private void RestoreWindowPlacement(SizePosition saved)
    {
        var savedPosition = new PixelPoint(saved.X, saved.Y);
        var screen = Screens.ScreenFromPoint(savedPosition);
        if (screen is null || !screen.Bounds.Contains(savedPosition))
            return;

        var frame = PlatformImpl is { FrameSize: { } frameSize } platform
            ? frameSize - platform.ClientSize
            : default;
        var placement = WindowPlacement.Restore(savedPosition, new Size(saved.Width, saved.Height),
            screen.WorkingArea, screen.Scaling, frame, new Size(MinWidth, MinHeight), new Size(MaxWidth, MaxHeight),
            Screens.All.Where(other => other.Bounds != screen.Bounds).Select(other => other.WorkingArea));
        if (placement is not { } restored)
            return;

        // Moving across monitors can change native DPI before Avalonia performs the first layout.
        Position = restored.Position;
        Width = restored.ClientSize.Width;
        Height = restored.ClientSize.Height;
        WindowStartupLocation = WindowStartupLocation.Manual;
    }

    //protected override void OnOpened(EventArgs e)
    //{
    //    base.OnOpened(e);
    //}

    protected override void OnClosing(WindowClosingEventArgs e)
    {
        base.OnClosing(e);

        if (IsSaveWindowSize && WindowState == WindowState.Normal)
        {
            SizePosition ??= new SizePosition();
            SizePosition.X = Position.X;
            SizePosition.Y = Position.Y;
            SizePosition.Width = ClientSize.Width;
            SizePosition.Height = ClientSize.Height;

            var typeName = GetType().Name;
            if (typeName != null)
            {
                UISettings.WindowSizePositions.Add(typeName, SizePosition);
            }
        }
    }
    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}

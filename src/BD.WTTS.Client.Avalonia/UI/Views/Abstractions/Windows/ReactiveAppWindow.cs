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
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = ReactiveWindow<TViewModel>.ViewModelProperty.AddOwner<ReactiveAppWindow<TViewModel>>();

    public static readonly StyledProperty<bool> IsSaveWindowSizeProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, bool>(nameof(IsSaveWindowSize), true);

    private bool _isInitialize;

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

        // This WhenActivated block calls ViewModel's WhenActivated
        // block if the ViewModel implements IActivatableViewModel.
        this.WhenActivated(disposables => { });
        this.GetObservable(DataContextProperty).Subscribe(OnDataContextChanged);
        this.GetObservable(ViewModelProperty).Subscribe(OnViewModelChanged);

        //ExtendClientAreaToDecorationsHint = true;
        //ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
        this.TryFindResource("TitleBarHeight", App.Instance.RequestedThemeVariant, out object? titleBarHeight);
        TitleBar.Height = (double?)titleBarHeight ?? 60;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        //SystemDecorations = SystemDecorations.BorderOnly;
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
        if (IsSaveWindowSize && SizePosition != null)
        {
            var screen = Screens.ScreenFromPoint(Position);
            if (screen != null)
            {
                var primaryScreenBounds = screen.Bounds;

                if (CanResize && !_isInitialize && !IsVisible)
                {
                    WindowStartupLocation = WindowStartupLocation.Manual;
                    var scalingOffset = 31 - ((screen.Scaling - 1) / 0.5); // appwindow偏移量

                    if (SizePosition.Width > 0 &&
                        primaryScreenBounds.Width >= SizePosition.Width)
                        Width = SizePosition.Width;

                    if (SizePosition.Height > 0 &&
                        primaryScreenBounds.Height >= SizePosition.Height)
                        Height = SizePosition.Height - scalingOffset;

                    if (SizePosition.X > 0 && SizePosition.Y > 0)
                    {
                        var offsetY = (int)Math.Round(scalingOffset * screen.Scaling);
                        //var offsetY = scalingOffset;
                        var leftTopPoint = new PixelPoint(SizePosition.X, SizePosition.Y + offsetY);
                        var rightBottomPoint = new PixelPoint(SizePosition.X + (int)Width, SizePosition.Y + (int)Height);
                        if (primaryScreenBounds.Contains(leftTopPoint) &&
                           primaryScreenBounds.Contains(rightBottomPoint))
                        {
                            Position = leftTopPoint;
                        }
                    }
                    _isInitialize = true;
                }
            }
        }
        base.Show();
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

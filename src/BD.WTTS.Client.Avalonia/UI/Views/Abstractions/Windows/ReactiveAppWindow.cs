using Avalonia;
using Avalonia.Controls;
using Avalonia.Platform;

namespace BD.WTTS.UI.Views.Windows;

/// <summary>
/// A ReactiveUI <see cref="Window"/> that implements the <see cref="IViewFor{TViewModel}"/> interface and will
/// activate your ViewModel automatically if the view model implements <see cref="IActivatableViewModel"/>. When
/// the DataContext property changes, this class will update the ViewModel property with the new DataContext value,
/// and vice versa.
/// </summary>
/// <typeparam name="TViewModel">ViewModel type.</typeparam>
public class ReactiveAppWindow<TViewModel> : AppWindow, IViewFor<TViewModel> where TViewModel : class
{
    public static readonly StyledProperty<TViewModel?> ViewModelProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, TViewModel?>(nameof(ViewModel));

    public static readonly StyledProperty<bool> IsSaveWindowSizeProperty = AvaloniaProperty
        .Register<ReactiveAppWindow<TViewModel>, bool>(nameof(IsSaveWindowSize), true);

    /// <summary>
    /// Initializes a new instance of the <see cref="ReactiveAppWindow{TViewModel}"/> class.
    /// </summary>
    public ReactiveAppWindow() : base()
    {
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
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = (WindowTransparencyLevel)UISettings.WindowBackgroundMaterial.Value;

        if (IsSaveWindowSize)
        {
            var windowName = GetType().Name;
            if (UISettings.WindowSizePositions.ActualValue?.ContainsKey(windowName) == true)
            {
                SizePosition = UISettings.WindowSizePositions.ActualValue[windowName];
            }
        }

        //this.GetObservable(TransparencyLevelHintProperty)
        //    .Subscribe(x =>
        //    {
        //        PseudoClasses.Set(":transparent", TransparencyLevelHint == WindowTransparencyLevel.Mica ||
        //            TransparencyLevelHint == WindowTransparencyLevel.Blur ||
        //            TransparencyLevelHint == WindowTransparencyLevel.AcrylicBlur);
        //    });

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

                if (CanResize)
                {
                    if (SizePosition.Width > 0 &&
                        primaryScreenBounds.Width >= SizePosition.Width)
                        Width = SizePosition.Width;

                    if (SizePosition.Height > 0 &&
                        primaryScreenBounds.Height >= SizePosition.Height)
                        Height = SizePosition.Height;

                    if (SizePosition.X > 0 && SizePosition.Y > 0)
                    {
                        var leftTopPoint = new PixelPoint(SizePosition.X, SizePosition.Y);
                        var rightBottomPoint = new PixelPoint(SizePosition.X + (int)Width, SizePosition.Y + (int)Height);
                        if (primaryScreenBounds.Contains(leftTopPoint) &&
                           primaryScreenBounds.Contains(rightBottomPoint))
                        {
                            Position = leftTopPoint;
                            WindowStartupLocation = WindowStartupLocation.Manual;
                        }
                    }
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

            UISettings.WindowSizePositions.ActualValue!.AddOrUpdate(GetType().Name, SizePosition, (s, e) => SizePosition);
            UISettings.WindowSizePositions.RaiseValueChanged();
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DataContext is IDisposable disposable)
            disposable.Dispose();
    }
}

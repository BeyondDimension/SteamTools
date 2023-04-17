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
        TitleBar.Height = (double?)titleBarHeight ?? 70;
        TitleBar.ExtendsContentIntoTitleBar = true;
        TitleBar.TitleBarHitTestType = TitleBarHitTestType.Complex;

        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint = WindowTransparencyLevel.Mica;
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
}

using MinMaxCloseControl = BD.WTTS.UI.Views.Controls.MinMaxCloseControl;

namespace BD.WTTS.UI.Views.Windows;

public partial class CoreWindow : Window, IStyleable
{
    private IDisposable? _windowStateObservable;

    private Border? _templateRoot;
    private MinMaxCloseControl? _captionButtons;
    private Panel? _defaultTitleBar;

    public CoreWindow() : base()
    {
        this.SystemDecorations = SystemDecorations.Full;
        this.CanResize = true;
        //this.ExtendClientAreaTitleBarHeightHint = -1;
        this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.NoChrome;
        this.ExtendClientAreaToDecorationsHint = true;

        Width = 1080;
        Height = 660;
        WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (OperatingSystem2.IsWindows() && !Design.IsDesignMode)
        {
            PseudoClasses.Add(":windows");
        }

        this.Opened += Window_Opened;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (OperatingSystem2.IsWindows() && !Design.IsDesignMode)
        {
            _templateRoot = e.NameScope.Find<Border>("RootBorder");

            _captionButtons = e.NameScope.Find<MinMaxCloseControl>("SystemCaptionButtons");
            _defaultTitleBar = e.NameScope.Find<Panel>("DefaultTitleBar");

            if (_defaultTitleBar != null)
            {
                _defaultTitleBar.PointerPressed += (i, e) =>
                {
                    PlatformImpl?.BeginMoveDrag(e);
                };
            }
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);

        _windowStateObservable?.Dispose();
        _windowStateObservable = null;
    }

    void Window_Opened(object? sender, EventArgs e)
    {
        _windowStateObservable = new CompositeDisposable();
    }
}

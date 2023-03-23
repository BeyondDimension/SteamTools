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
        if (OperatingSystem.IsWindows())
        {
            this.SystemDecorations = SystemDecorations.BorderOnly;
            //this.ExtendClientAreaTitleBarHeightHint = -1;
            this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
            //this.ExtendClientAreaToDecorationsHint = true;
        }
        else if (OperatingSystem.IsMacOS())
        {
            this.SystemDecorations = SystemDecorations.Full;
            //this.ExtendClientAreaTitleBarHeightHint = -1;
            this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.PreferSystemChrome;
            this.ExtendClientAreaToDecorationsHint = true;
        }
        else
        {
            this.SystemDecorations = SystemDecorations.Full;
            this.ExtendClientAreaChromeHints = ExtendClientAreaChromeHints.SystemChrome;
        }

        this.CanResize = true;
        this.Width = 1080;
        this.Height = 660;
        this.WindowStartupLocation = WindowStartupLocation.CenterScreen;

        if (OperatingSystem.IsWindows() && !Design.IsDesignMode)
        {
            PseudoClasses.Add(":windows");
        }

        this.Opened += Window_Opened;
    }

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        if (OperatingSystem.IsWindows() && !Design.IsDesignMode)
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

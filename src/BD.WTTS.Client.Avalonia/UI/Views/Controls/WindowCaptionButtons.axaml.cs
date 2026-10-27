namespace BD.WTTS.UI.Views.Controls;

public sealed partial class WindowCaptionButtons : UserControl
{
    readonly CompositeDisposable subscriptions = new();

    public WindowCaptionButtons()
    {
        InitializeComponent();
        MinimizeButton.Click += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window)
                window.WindowState = WindowState.Minimized;
        };
        MaximizeButton.Click += (_, _) =>
        {
            if (TopLevel.GetTopLevel(this) is Window window && window.CanMaximize)
                window.WindowState = window.WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        };
        CloseButton.Click += (_, _) => (TopLevel.GetTopLevel(this) as Window)?.Close();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        if (TopLevel.GetTopLevel(this) is Window window)
        {
            subscriptions.Add(window.GetObservable(Window.CanMinimizeProperty).Subscribe(value => MinimizeButton.IsEnabled = value));
            subscriptions.Add(window.GetObservable(Window.CanMaximizeProperty).Subscribe(value => MaximizeButton.IsEnabled = value));
            subscriptions.Add(window.GetObservable(Window.WindowStateProperty).Subscribe(value =>
            {
                MaximizeButton.Content = value == WindowState.Maximized ? "\uE923" : "\uE922";
                MaximizeButton[!ToolTip.TipProperty] = new ReflectionBinding(value == WindowState.Maximized ? "Res.Restore" : "Res.Maximize")
                {
                    Source = ResourceService.Current,
                };
                IsVisible = value != WindowState.FullScreen;
            }));
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        subscriptions.Clear();
        base.OnDetachedFromVisualTree(e);
    }
}

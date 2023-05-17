using Avalonia.Controls.Documents;
using Avalonia.Controls.Notifications;

namespace BD.WTTS.UI.Views.Controls;

public class Snackbar : InfoBar
{
    private bool _isClosing;

    /// <summary>
    /// Determines if the notification is already closing.
    /// </summary>
    public bool IsClosing
    {
        get { return _isClosing; }
        private set { SetAndRaise(IsClosingProperty, ref _isClosing, value); }
    }

    /// <summary>
    /// Defines the <see cref="IsClosing"/> property.
    /// </summary>
    public static readonly DirectProperty<Snackbar, bool> IsClosingProperty =
        AvaloniaProperty.RegisterDirect<Snackbar, bool>(nameof(IsClosing), o => o.IsClosing);

    public Snackbar() : base()
    {
        Closing += Snackbar_Closing;
        Closed += Snackbar_Closed;
    }

    private void Snackbar_Closed(InfoBar sender, InfoBarClosedEventArgs args)
    {
        IsClosing = false;
    }

    private void Snackbar_Closing(InfoBar sender, InfoBarClosingEventArgs args)
    {
        if (args.Reason == InfoBarCloseReason.CloseButton)
        {
            args.Cancel = true;
            IsClosing = true;
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        Closing -= Snackbar_Closing;
        Closed -= Snackbar_Closed;
        base.OnDetachedFromVisualTree(e);
    }

    public void Close()
    {
        if (IsClosing)
        {
            return;
        }

        IsClosing = true;
    }
}

namespace BD.WTTS.UI.Views.Windows;

public class CoreWindow : Window
{
    public CoreWindow() : base()
    {
        this.SystemDecorations = SystemDecorations.BorderOnly;
        this.CanResize = true;
        //this.ExtendClientAreaTitleBarHeightHint = true;
    }
}

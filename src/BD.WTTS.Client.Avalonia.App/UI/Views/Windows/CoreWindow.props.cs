namespace BD.WTTS.UI.Views.Windows;

public partial class CoreWindow : Window, IStyleable
{
    private bool _hideSizeButtons;

    Type IStyleable.StyleKey => typeof(CoreWindow);

    /// <summary>
    /// Gets or sets a value whether the AppWindow should hide its minimize/maximize buttons like 
    /// a dialog window. This property is only respected on Windows.
    /// </summary>
    public bool ShowAsDialog
    {
        get => _hideSizeButtons;
        set
        {
            _hideSizeButtons = value;
            PseudoClasses.Set(":dialog", value);
        }
    }
}

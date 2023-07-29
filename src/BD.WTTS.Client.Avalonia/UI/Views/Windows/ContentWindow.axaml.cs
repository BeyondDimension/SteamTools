namespace BD.WTTS.UI.Views.Windows;

public partial class ContentWindow : ReactiveAppWindow<ContentWindowViewModel>
{
    public ContentWindow()
    {
        InitializeComponent();
    }

    public ContentWindow(Control page) : this()
    {
        ViewContent.Content = page;
    }
}

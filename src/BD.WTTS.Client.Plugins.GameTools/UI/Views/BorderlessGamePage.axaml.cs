namespace BD.WTTS.UI.Views.Pages;

public partial class BorderlessGamePage : PageBase<BorderlessGamePageViewModel>
{
    public BorderlessGamePage()
    {
        InitializeComponent();
        DataContext ??= new BorderlessGamePageViewModel();
    }
}

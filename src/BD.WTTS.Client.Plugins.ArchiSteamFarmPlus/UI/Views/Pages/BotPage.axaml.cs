namespace BD.WTTS.UI.Views.Pages;

public partial class BotPage : PageBase<BotPageViewModel>
{
    public BotPage()
    {
        InitializeComponent();
        DataContext ??= new BotPageViewModel();
    }
}

namespace BD.WTTS.UI.Views.Pages;

public partial class ASFSettingsPage : PageBase<ASFSettingsPageViewModel>
{
    public ASFSettingsPage()
    {
        InitializeComponent();
        DataContext ??= new ASFSettingsPageViewModel();
    }
}

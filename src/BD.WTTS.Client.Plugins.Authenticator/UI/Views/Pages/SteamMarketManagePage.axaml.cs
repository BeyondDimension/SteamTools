using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Pages;

public partial class SteamMarketManagePage : PageBase<SteamMarketManagePageViewModel>
{
    public SteamMarketManagePage()
    {
        InitializeComponent();
        DataContext ??= new SteamMarketManagePageViewModel();
    }
}

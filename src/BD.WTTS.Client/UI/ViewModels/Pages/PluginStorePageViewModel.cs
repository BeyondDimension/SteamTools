using BD.WTTS.Client.Resources;
using DynamicData;
using static BD.WTTS.Services.IMicroServiceClient;

namespace BD.WTTS.UI.ViewModels;

public sealed class PluginStorePageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico";

    public void NavgationToMenuPage(TabItemViewModel tabItem)
    {
        INavigationService.Instance.Navigate(tabItem.PageType, NavigationTransitionEffect.FromBottom);
    }
}

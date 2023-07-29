using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed class HomePageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    public override string IconKey => "avares://BD.WTTS.Client.Avalonia/UI/Assets/Icons/home.ico";

    [Reactive]
    public ObservableCollection<string> Navigations { get; set; }

    public HomePageViewModel()
    {
        Navigations = new ObservableCollection<string>()
        {
            "https://media.st.dl.eccdnx.com/steam/apps/2239550/capsule_616x353.jpg?t=1674755684",
            "https://media.st.dl.eccdnx.com/steam/apps/1938090/capsule_616x353_alt_assets_3_schinese.jpg?t=1684446457",
            "https://media.st.dl.eccdnx.com/steam/apps/1868180/capsule_616x353_schinese.jpg?t=1685635527",
            "https://media.st.dl.eccdnx.com/steam/apps/271590/capsule_616x353.jpg?t=1678296348",
            "https://media.st.dl.eccdnx.com/steam/apps/1865680/capsule_616x353_schinese.jpg?t=1643602186",
        };
    }
}

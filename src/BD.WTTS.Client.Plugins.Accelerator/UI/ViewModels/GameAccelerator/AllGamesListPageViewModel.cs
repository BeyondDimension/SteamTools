namespace BD.WTTS.UI.ViewModels;

public sealed partial class AllGamesListPageViewModel : WindowViewModel
{
    [Reactive]
    public partial ObservableCollection<XunYouGameViewModel> XunYouGames { get; set; } = [];

    public AllGamesListPageViewModel()
    {
        XunYouGames = new ObservableCollection<XunYouGameViewModel>();

        if (XunYouSDK.IsSupported)
        {
            Task2.InBackground(() =>
            {
                var games = XunYouSDK.GetAllGames();
                if (games != null)
                {
                    XunYouGames.Clear();
                    foreach (var game in games)
                    {
                        XunYouGames.Add(new XunYouGameViewModel(game));
                    }
                }
            });
        }
    }
}

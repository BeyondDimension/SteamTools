namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAcceleratorPageViewModel
{
    public GameAcceleratorPageViewModel()
    {
        //GameAcceleratorService.Current.LoadGames();

        GameAcceleratorService.Current.Games
              .Connect()
              .Sort(new LastAccelerateTimeComparer())
              .ObserveOn(RxApp.MainThreadScheduler)
              .Bind(out _Games)
              .Subscribe();
    }
}

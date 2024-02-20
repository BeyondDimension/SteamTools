namespace BD.WTTS.UI.ViewModels;

/// <summary>
/// 游戏加速视图模型
/// </summary>
public sealed partial class GameAcceleratorViewModel
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GameAcceleratorViewModel"/> class.
    /// </summary>
    public GameAcceleratorViewModel()
    {
        GameAcceleratorService.Current.Games
            .Connect()
            .Sort(new LastAccelerateTimeComparer())
            .ObserveOn(RxApp.MainThreadScheduler)
            .Bind(out _Games)
            .Subscribe();

        GameAcceleratorCommand = ReactiveCommand.Create<XunYouGameViewModel>(app =>
        {
            app.IsAccelerated = !app.IsAccelerated;
        });
    }
}

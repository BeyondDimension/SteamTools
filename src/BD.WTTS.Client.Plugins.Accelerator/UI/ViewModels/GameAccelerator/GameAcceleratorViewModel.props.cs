namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAcceleratorViewModel : ViewModelBase
{
    /// <summary>
    /// 迅游游戏数据
    /// </summary>
    private readonly ReadOnlyObservableCollection<XunYouGameViewModel>? _Games;

    public ReadOnlyObservableCollection<XunYouGameViewModel>? Games => _Games;

    /// <summary>
    /// 游戏加速点击 <see cref="ICommand"/>
    /// </summary>
    public ICommand GameAcceleratorCommand { get; }

    [Reactive]
    public string? SearchText { get; set; }
}

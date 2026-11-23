namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAcceleratorPageViewModel : TabItemViewModel
{
    public override string Name => Strings.GameAccelerator;

    private readonly ReadOnlyObservableCollection<XunYouGameViewModel>? _Games;

    public ReadOnlyObservableCollection<XunYouGameViewModel>? Games => _Games;

    [Reactive]
    public partial string? SearchText { get; set; }
}

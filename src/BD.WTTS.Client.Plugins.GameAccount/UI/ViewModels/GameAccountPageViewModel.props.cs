using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class GameAccountPageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    [Reactive]
    public ObservableCollection<PlatformAccount>? GamePlatforms { get; set; }

    [Reactive]
    public ObservableCollection<PlatformAccount>? AddGamePlatforms { get; set; }

    public ICommand AddPlatformCommand { get; set; }
}

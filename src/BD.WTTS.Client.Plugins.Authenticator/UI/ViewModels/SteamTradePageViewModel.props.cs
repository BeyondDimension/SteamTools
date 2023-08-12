using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel : ViewModelBase
{
    [Reactive]
    public ObservableCollection<SteamTradeConfirmationModel> Confirmations { get; set; } = new();

    [Reactive]
    public string? UserNameText { get; set; }

    [Reactive]
    public string? PasswordText { get; set; }

    [Reactive]
    public bool IsLogged { get; set; }

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public bool SelectedAll { get; set; }

    public bool IsConfirmationsAny => Confirmations.Any_Nullable();
}
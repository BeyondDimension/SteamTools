using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel
{
    [Reactive]
    public ObservableCollection<SteamTradeConfirmationModel> Confirmations { get; set; } = new();

    public SteamLoginViewModel Login { get; }

    [Reactive]
    public bool RememberLogin { get; set; } = true;

    [Reactive]
    public string? LoginStatus { get; set; }

    [Reactive]
    public bool IsLogged { get; set; }

    [Reactive]
    public bool IsLoading { get; set; }

    [Reactive]
    public bool? SelectedAll { get; set; } = false;

    public ICommand ConfirmTradeCommand { get; }

    public ICommand CancelTradeCommand { get; }
}
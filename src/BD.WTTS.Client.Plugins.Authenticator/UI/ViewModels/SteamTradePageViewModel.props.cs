using Avalonia.Media.Imaging;
using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class SteamTradePageViewModel
{
    [Reactive]
    public partial ObservableCollection<SteamTradeConfirmationModel> Confirmations { get; set; } = new();

    public SteamLoginViewModel Login { get; }

    public int LoginModeIndex
    {
        get => Login.RequireMobileSession ? 1 : 0;
        set
        {
            if (value is not (0 or 1) || Login.IsBusy || value == LoginModeIndex) return;
            Login.RequireMobileSession = value == 1;
            this.RaisePropertyChanged(nameof(LoginModeIndex));
        }
    }

    [Reactive]
    public partial bool RememberLogin { get; set; } = true;

    [Reactive]
    public partial string? LoginStatus { get; set; }

    [Reactive]
    public partial bool IsLogged { get; set; }

    [Reactive]
    public partial bool IsLoading { get; set; }

    [Reactive]
    public partial string? ConfirmationLoadError { get; private set; }

    [Reactive]
    public partial bool HasLoadedConfirmations { get; private set; }

    [Reactive]
    public partial bool IsOperatingConfirmations { get; private set; }

    public bool CanChangeConfirmations => IsLogged && HasLoadedConfirmations &&
        ConfirmationLoadError is null && !IsLoading && !IsOperatingConfirmations;

    [Reactive]
    public partial bool? SelectedAll { get; set; } = false;

    public ICommand ConfirmTradeCommand { get; }

    public ICommand CancelTradeCommand { get; }
}

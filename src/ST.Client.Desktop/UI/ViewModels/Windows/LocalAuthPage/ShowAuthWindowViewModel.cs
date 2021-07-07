namespace System.Application.UI.ViewModels
{
    partial class ShowAuthWindowViewModel : WindowViewModel
    {
        public string? RecoveryCode => _Authenticator?.RecoveryCode;

        public string? SteamData => _Authenticator?.SteamData;

        public string? DeviceId => _Authenticator?.DeviceId;
    }
}
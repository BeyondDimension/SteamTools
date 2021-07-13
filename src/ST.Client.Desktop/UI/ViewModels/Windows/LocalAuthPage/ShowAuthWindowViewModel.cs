namespace System.Application.UI.ViewModels
{
    partial class ShowAuthWindowViewModel : WindowViewModel
    {
#if DEBUG
        [Obsolete("use SteamDataIndented")]
        public string? SteamData => _Authenticator?.SteamData;
#endif
    }
}
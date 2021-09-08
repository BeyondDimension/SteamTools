using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    partial class AddAuthWindowViewModel : PageViewModel
    {
        private string? _LoginSteamLoadingText;
        public string? LoginSteamLoadingText
        {
            get => _LoginSteamLoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoginSteamLoadingText, value);
        }
    }
}
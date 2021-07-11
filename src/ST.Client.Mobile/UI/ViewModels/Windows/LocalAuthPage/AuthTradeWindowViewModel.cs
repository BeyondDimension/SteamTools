using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    partial class AuthTradeWindowViewModel : PageViewModel
    {
        private string? _LoadingText;
        public string? LoadingText
        {
            get => _LoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoadingText, value);
        }
    }
}
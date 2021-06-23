using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public class MessageBoxWindowViewModel : DialogWindowViewModel
    {
        private string? _Content;
        public string? Content
        {
            get => _Content;
            set => this.RaiseAndSetIfChanged(ref _Content, value);
        }

        private bool _IsCancelcBtn;
        public bool IsCancelcBtn
        {
            get => _IsCancelcBtn;
            set => this.RaiseAndSetIfChanged(ref _IsCancelcBtn, value);
        }
    }
}
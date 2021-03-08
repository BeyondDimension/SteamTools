using ReactiveUI;
using System.Application.Services;

namespace System.Application.UI.ViewModels.Windows
{
    public class MessageWindowViewModel<T> : WindowViewModel
    {
        private readonly T Window;
        public MessageWindowViewModel(T window) 
        {
            Window = window;
        }

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

        public void OK()
        {
            DialogResult = true;
            DI.Get<IMessageWindowService>().CloseWindow(Window);
        }

        public void Cancel()
        {
            DialogResult = false;
            DI.Get<IMessageWindowService>().CloseWindow(Window);
        }
    }
}

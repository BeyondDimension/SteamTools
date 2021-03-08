using ReactiveUI;

namespace System.Application.UI.ViewModels.Windows
{
    public class MessageWindowViewModel : WindowViewModel
    {
        public MessageWindowViewModel() 
        {

        }

        private string _Content;
        public string Content
        {
            get => _Content;
            set => this.RaiseAndSetIfChanged(ref _Content, value);
        }

    }
}

using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public class WindowViewModel : ViewModelBase
    {
        string title = string.Empty;
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        ViewModelBase? _Toast;
        public ViewModelBase? Toast
        {
            get => _Toast;
            set => this.RaiseAndSetIfChanged(ref _Toast, value);
        }

        public bool IsInitialized { get; set; }

        protected void InvokeOnUIDispatcher(Action action)
        {
            MainThreadDesktop.BeginInvokeOnMainThread(action);
        }
    }
}
using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public class WebView3WindowViewModel : WindowViewModel
    {
        public const string AboutBlank = "about:blank";

        string _Url = AboutBlank;
        public string Url
        {
            get => _Url;
            set => this.RaiseAndSetIfChanged(ref _Url, value);
        }
    }
}
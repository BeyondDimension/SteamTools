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

        string[]? _StreamResponseFilterUrls;
        public string[]? StreamResponseFilterUrls
        {
            get => _StreamResponseFilterUrls;
            set => this.RaiseAndSetIfChanged(ref _StreamResponseFilterUrls, value);
        }

        public Action<string, byte[]>? OnStreamResponseFilterResourceLoadComplete { get; set; }

        public bool FixedSinglePage { get; set; }

        bool _IsLoading = true;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }
    }
}
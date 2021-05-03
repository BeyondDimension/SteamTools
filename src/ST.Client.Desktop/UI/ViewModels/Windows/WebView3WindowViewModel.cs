using ReactiveUI;
using System.Net.Http;

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

        /// <summary>
        /// 设置网页加载超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; } = GeneralHttpClientFactory.DefaultTimeout;

        /// <summary>
        /// 网页加载超时时提示文本
        /// </summary>
        public string? TimeoutErrorMessage { get; set; }

        public bool IsSecurity { get; set; }

        public Action? Close { get; set; }
    }
}
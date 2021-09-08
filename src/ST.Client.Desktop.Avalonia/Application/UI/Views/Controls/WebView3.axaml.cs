using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CefNet;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Views.Controls
{
    public partial class WebView3 : UserControl, IDisposable
    {
        readonly ProgressRing webViewLoading;
        bool disposedValue;

        public WebViewBase Browser { get; }

        /// <summary>
        /// webview url <see cref="TextGap"/> property.
        /// </summary>
        public static readonly StyledProperty<string> UrlProperty =
            AvaloniaProperty.Register<ScrollingTextBlock, string>(nameof(Url), WebView3WindowViewModel.AboutBlank);

        public string Url
        {
            get { return GetValue(UrlProperty); }
            set { SetValue(UrlProperty, value); }
        }

        public WebView3()
        {
            InitializeComponent();

            webViewLoading = this.FindControl<ProgressRing>(nameof(webViewLoading));
            Browser = this.FindControl<WebViewBase>(nameof(Browser));
            Browser.Opacity = 0;
            Browser.LoadingStateChange += WebView_LoadingStateChange;

            this.GetObservable(UrlProperty)
                  .Subscribe(x =>
                  {
                      Navigate(Url);
                  });
        }

        public void Navigate(string url)
        {
            if (Browser.BrowserObject == null)
            {
                Browser.InitialUrl = url;
            }
            else
            {
                Browser.Navigate(url);
            }
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        private void WebView_LoadingStateChange(object? sender, LoadingStateChangeEventArgs e)
        {
            if (!e.Busy)
            {
                if (Browser.Opacity != 1) Browser.Opacity = 1;
                if (webViewLoading.IsVisible) webViewLoading.IsVisible = false;
            }
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (Browser != null)
                    {
                        Browser.LoadingStateChange -= WebView_LoadingStateChange;
                        ((IDisposable)Browser).Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}

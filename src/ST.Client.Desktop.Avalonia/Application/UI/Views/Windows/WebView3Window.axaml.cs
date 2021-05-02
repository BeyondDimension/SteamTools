using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.Views.Windows
{
    public class WebView3Window : FluentWindow, IDisposable
    {
        readonly WebView3 webView;
        bool disposedValue;

        public WebView3Window()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            webView = this.FindControl<WebView3>(nameof(webView));
            webView.InitialUrl = WebView3WindowViewModel.AboutBlank;
            webView.Opacity = 0;
            webView.DocumentTitleChanged += WebView_DocumentTitleChanged;
        }

        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is WebView3WindowViewModel vm)
            {
                if (!string.IsNullOrWhiteSpace(vm.Title)) webView.DocumentTitleChanged -= WebView_DocumentTitleChanged;
                vm.WhenAnyValue(x => x.Url).WhereNotNull().Subscribe(x =>
                {
                    if (x == WebView3WindowViewModel.AboutBlank)
                    {
                        webView.Opacity = 0;
                    }
                    else if (IsHttpUrl(x))
                    {
                        if (x.Contains("steampp.net"))
                            x = string.Format(
                                x + "?theme={0}&language={1}",
                                CefNetApp.GetTheme(),
                                R.Language);
                        if (webView.Opacity != 1) webView.Opacity = 1;
                        if (webView.BrowserObject == null)
                        {
                            webView.InitialUrl = x;
                        }
                        else
                        {
                            webView.Navigate(x);
                        }
                    }
                }).AddTo(vm);
                vm.WhenAnyValue(x => x.StreamResponseFilterUrls).Subscribe(x => webView.StreamResponseFilterUrls = x).AddTo(vm);
                vm.WhenAnyValue(x => x.FixedSinglePage).Subscribe(x => webView.FixedSinglePage = x).AddTo(vm);
                webView.OnStreamResponseFilterResourceLoadComplete += vm.OnStreamResponseFilterResourceLoadComplete;
            }
        }

        void WebView_DocumentTitleChanged(object? sender, DocumentTitleChangedEventArgs e)
        {
            if (DataContext is WindowViewModel vm)
            {
                vm.Title = e.Title;
            }
            else
            {
                Title = e.Title;
            }
        }

        void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (webView != null)
                    {
                        if (DataContext is WebView3WindowViewModel vm)
                        {
                            webView.OnStreamResponseFilterResourceLoadComplete -= vm.OnStreamResponseFilterResourceLoadComplete;
                        }
                        ((IDisposable)webView).Dispose();
                    }
                    if (DataContext is IDisposable d)
                    {
                        d.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~WebView3Window()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
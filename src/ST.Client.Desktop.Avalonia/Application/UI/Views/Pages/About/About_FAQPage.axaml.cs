using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CefNet;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class About_FAQPage : UserControl, IDisposable
    {
        readonly WebView3 webViewQA;
        readonly ProgressRing webViewQALoading;
        bool disposedValue;

        public About_FAQPage()
        {
            InitializeComponent();

            webViewQALoading = this.FindControl<ProgressRing>(nameof(webViewQALoading));
            webViewQA = this.FindControl<WebView3>(nameof(webViewQA));
            webViewQA.Opacity = 0;
            webViewQA.LoadingStateChange += WebView_LoadingStateChange;
            webViewQA.InitialUrl = string.Format(
                "https://steampp.net/faqbox?theme={0}&language={1}",
                CefNetApp.GetTheme(),
                R.Language);
        }

        private void WebView_LoadingStateChange(object? sender, LoadingStateChangeEventArgs e)
        {
            if (!e.Busy)
            {
                if (webViewQA.Opacity != 1) webViewQA.Opacity = 1;
                if (webViewQALoading.IsVisible) webViewQALoading.IsVisible = false;
            }
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
                    if (webViewQA != null)
                    {
                        webViewQA.LoadingStateChange -= WebView_LoadingStateChange;
                        ((IDisposable)webViewQA).Dispose();
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
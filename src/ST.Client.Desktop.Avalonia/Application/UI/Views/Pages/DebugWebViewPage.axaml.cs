using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using CefNet.Avalonia;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class DebugWebViewPage : ReactiveUserControl<DebugWebViewPage>, IDisposable
    {
        readonly WebView3 webView;
        readonly TextBox urlTextBox;

        public DebugWebViewPage()
        {
            InitializeComponent();

            webView = this.FindControl<WebView3>("webView");
            webView.Url = "chrome://version";
            //webView.BrowserCreated += WebView_BrowserCreated;

            urlTextBox = this.FindControl<TextBox>("urlTextBox");
            urlTextBox.KeyUp += UrlTextBox_KeyUp;
        }

        private void UrlTextBox_KeyUp(object? sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter)
            {
                webView.Navigate(urlTextBox.Text);
            }
        }

        //private async void WebView_BrowserCreated(object sender, EventArgs e)
        //{
        //    var value = await webView.GetUserAgentAsync();
        //}

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromLogicalTree(e);
            Dispose();
        }

        protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        {
            base.OnDetachedFromVisualTree(e);
            Dispose();
        }

        bool disposedValue;

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (webView != null)
                    {
                        ((IDisposable)webView).Dispose();
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
            //GC.SuppressFinalize(this);
        }
    }
}
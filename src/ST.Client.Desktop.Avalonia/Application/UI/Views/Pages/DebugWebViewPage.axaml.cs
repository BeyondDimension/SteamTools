using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using Avalonia.ReactiveUI;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using static System.Common.Constants;

namespace System.Application.UI.Views.Pages
{
    public class DebugWebViewPage : ReactiveUserControl<DebugWebViewPageViewModel>/*, IDisposable*/
    {
        readonly WebView2 webView;
        readonly TextBox urlTextBox;

        public DebugWebViewPage()
        {
            InitializeComponent();

            webView = this.FindControl<WebView2>("webView");
            webView.Source = new(UrlConstants.GitHub_Repository);

            urlTextBox = this.FindControl<TextBox>("urlTextBox");
            urlTextBox.KeyUp += UrlTextBox_KeyUp;
        }

        private void UrlTextBox_KeyUp(object? sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                var url = urlTextBox.Text;
                if (!Browser2.IsHttpUrl(url)) url = $"{Prefix_HTTPS}{url}";
                var array = url.Split('/');
                if (array.Length < 3) return;
                if (!array[2].Contains('.')) return;
                webView.CoreWebView2?.Navigate(url);
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

        //protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
        //{
        //    base.OnDetachedFromLogicalTree(e);
        //    Dispose();
        //}

        //protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
        //{
        //    base.OnDetachedFromVisualTree(e);
        //    Dispose();
        //}

        //bool disposedValue;

        //protected virtual void Dispose(bool disposing)
        //{
        //    if (!disposedValue)
        //    {
        //        if (disposing)
        //        {
        //            // TODO: 释放托管状态(托管对象)
        //            if (webView != null)
        //            {
        //                ((IDisposable)webView).Dispose();
        //            }
        //        }
        //        // TODO: 释放未托管的资源(未托管的对象)并替代终结器
        //        // TODO: 将大型字段设置为 null
        //        disposedValue = true;
        //    }
        //}

        //public void Dispose()
        //{
        //    // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //    Dispose(disposing: true);
        //    //GC.SuppressFinalize(this);
        //}
    }
}
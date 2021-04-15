using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet.Avalonia;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class About_FAQPage : UserControl, IDisposable
    {
        readonly WebView webViewQA;
        bool disposedValue;

        public About_FAQPage()
        {
            InitializeComponent();

            webViewQA = this.FindControl<WebView3>(nameof(webViewQA));
            webViewQA.InitialUrl = string.Format(
                "https://steampp.net/faqBox?theme={0}&language={1}",
                CefNetApp.GetTheme(),
                R.Language);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
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
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet.Avalonia;

namespace System.Application.UI.Views.Pages
{
    public class DebugWebViewPage : UserControl
    {
        readonly WebView webView;

        public DebugWebViewPage()
        {
            InitializeComponent();

            webView = this.FindControl<WebView3>("webView");
            webView.InitialUrl = "chrome://version";
            webView.BrowserCreated += WebView_BrowserCreated;
        }

        private async void WebView_BrowserCreated(object sender, EventArgs e)
        {
            var value = await webView.GetUserAgentAsync();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
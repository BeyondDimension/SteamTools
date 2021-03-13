using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet.Avalonia;
using System.Threading.Tasks;

namespace System.Application.UI.Views.Pages
{
    public class DebugWebViewPage : UserControl
    {
        readonly WebView webView;

        public DebugWebViewPage()
        {
            InitializeComponent();

            webView = this.FindControl<WebView>("webView");
            webView.InitialUrl = "http://pan.mossimo.net:9710/#/welcome"; /*"https://cn.bing.com";*/
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

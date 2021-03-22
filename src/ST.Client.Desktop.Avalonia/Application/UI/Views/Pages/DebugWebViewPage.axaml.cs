using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet.Avalonia;

namespace System.Application.UI.Views.Pages
{
    public class DebugWebViewPage : UserControl
    {
        readonly WebView webView;
        readonly TextBox urlTextBox;

        public DebugWebViewPage()
        {
            InitializeComponent();

            webView = this.FindControl<WebView3>("webView");
            webView.InitialUrl = "http://pan.mossimo.net:9710/c/";
                //"chrome://version";
            webView.BrowserCreated += WebView_BrowserCreated;

            urlTextBox = this.FindControl<TextBox>("urlTextBox");
            urlTextBox.KeyUp += UrlTextBox_KeyUp;
        }

        private void UrlTextBox_KeyUp(object sender, Avalonia.Input.KeyEventArgs e)
        {
            if (e.Key == Avalonia.Input.Key.Enter) 
            {
                webView.Navigate(urlTextBox.Text);
            }
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
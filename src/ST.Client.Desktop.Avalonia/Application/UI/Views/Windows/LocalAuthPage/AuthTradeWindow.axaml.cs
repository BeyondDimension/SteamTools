using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using WinAuth;

namespace System.Application.UI.Views.Windows
{
    public class AuthTradeWindow : FluentWindow<AuthTradeWindowViewModel>
    {
        readonly ContentDialog htmlDialog;
        readonly WebView2Compat webview;

        public AuthTradeWindow() : base()
        {
            InitializeComponent();

            htmlDialog = this.FindControl<ContentDialog>("HtmlDialog");
            webview = this.FindControl<WebView2Compat>("webview");
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public async void ShowHtmlDialog_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            if (htmlDialog != null && ViewModel != null)
            {
                var auth = (sender as Control)?.Tag as WinAuthSteamClient.Confirmation;
                webview.HtmlSource = ViewModel.GetConfirmationDetailHtml(auth!);
                await htmlDialog.ShowAsync();
            }
        }
    }
}

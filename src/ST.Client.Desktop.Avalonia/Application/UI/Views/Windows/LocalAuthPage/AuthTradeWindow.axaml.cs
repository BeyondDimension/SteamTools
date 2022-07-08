using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using FluentAvalonia.UI.Controls;
using System.Application.UI.ViewModels;
using WinAuth;

namespace System.Application.UI.Views.Windows
{
    public class AuthTradeWindow : FluentWindow<AuthTradeWindowViewModel>
    {
        readonly ContentDialog htmlDialog;

        public AuthTradeWindow() : base()
        {
            InitializeComponent();

            htmlDialog = this.FindControl<ContentDialog>("HtmlDialog");
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
                //htmlControl.Text = ViewModel.GetConfirmationDetailHtml(auth!);
                await htmlDialog.ShowAsync();
            }
        }
    }
}

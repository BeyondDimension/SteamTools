using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CefNet;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class About_ChangeLog : UserControl
    {
        readonly WebView3 webView;

        public About_ChangeLog()
        {
            InitializeComponent();

            webView = this.FindControl<WebView3>(nameof(webView));
            webView.Url = string.Format(
                UrlConstants.OfficialWebsite_Box_Changelog_,
                CefNetApp.GetTheme(),
                R.Language);
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

    }
}
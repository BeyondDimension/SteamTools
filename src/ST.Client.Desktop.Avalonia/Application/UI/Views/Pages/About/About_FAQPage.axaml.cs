using Avalonia;
using Avalonia.Controls;
using Avalonia.LogicalTree;
using Avalonia.Markup.Xaml;
using CefNet;
using System.Application.UI.Resx;
using System.Application.UI.Views.Controls;

namespace System.Application.UI.Views.Pages
{
    public class About_FAQPage : UserControl
    {
        WebView3 webViewQA;

        public About_FAQPage()
        {
            InitializeComponent();
            webViewQA = this.FindControl<WebView3>("webViewQA");
            webViewQA.Url = string.Format(
                "https://steampp.net/faqbox?theme={0}&language={1}",
                CefNetApp.GetTheme(),
                R.Language);
        }


        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
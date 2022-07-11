using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.Application.UI.Resx;
#if WINDOWS
using Microsoft.Web.WebView2.Core;
#endif

namespace System.Application.UI.Views.Controls;

public partial class WebView2Compat : UserControl
{
    public readonly TextBlock TbContent;
    public readonly Button BtnDownloadAndInstall;
    public readonly WebView2 WebView2;

    public WebView2Compat()
    {
        InitializeComponent();

        TbContent = this.FindControl<TextBlock>("TbContent");
        BtnDownloadAndInstall = this.FindControl<Button>("BtnDownloadAndInstall");
        WebView2 = this.FindControl<WebView2>("WebView2");
        WebView2.IsVisible = false;
#if WINDOWS
        WebView2.DOMContentLoaded += WebView2_DOMContentLoaded;
#endif

        var webView2IsSupported = WebView2.IsSupported;
        TbContent.Text = webView2IsSupported ? AppResources.Loading : AppResources.YouNeedInstallWebView2Runtime;
        if (BtnDownloadAndInstall.IsVisible = !webView2IsSupported)
        {
            BtnDownloadAndInstall.Click += BtnDownloadAndInstall_Click;
        }

        this.GetPropertyChangedObservable(SourceProperty).Subscribe(OnSourceChanged);
        this.GetPropertyChangedObservable(HtmlSourceProperty).Subscribe(OnHtmlSourceChanged);
    }

    bool isNotFirstLoad;

#if WINDOWS
    void WebView2_DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
    {
        if (!isNotFirstLoad) isNotFirstLoad = true;
        WebView2.IsVisible = true;
    }
#endif

    void BtnDownloadAndInstall_Click(object? sender, RoutedEventArgs e)
    {
        // WebView2AutoInstaller
        // https://github.com/ProKn1fe/WebView2.Runtime/blob/62011b09436944143996fdb0039cd2c5dbb5c300/WebView2.Runtime.AutoInstaller/WebView2.Runtime.AutoInstaller/WebView2AutoInstaller.cs
    }

    void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static readonly DirectProperty<WebView2Compat, Uri?> SourceProperty =
           AvaloniaProperty.RegisterDirect<WebView2Compat, Uri?>(
               nameof(Source),
               x => x._Source,
               (x, y) => x.Source = y);

    Uri? _Source;

    public Uri? Source
    {
        get => _Source;
        set => SetAndRaise(SourceProperty, ref _Source, value);
    }

    void OnSourceChanged(EventArgs e)
    {
        if (!isNotFirstLoad) WebView2.IsVisible = true;
        WebView2.Source = Source;
        if (!isNotFirstLoad) WebView2.IsVisible = false;
    }

    public static readonly DirectProperty<WebView2Compat, string?> HtmlSourceProperty =
           AvaloniaProperty.RegisterDirect<WebView2Compat, string?>(
               nameof(HtmlSource),
               x => x._HtmlSource,
               (x, y) => x.HtmlSource = y);

    string? _HtmlSource;

    public string? HtmlSource
    {
        get => _HtmlSource;
        set => SetAndRaise(HtmlSourceProperty, ref _HtmlSource, value);
    }

    void OnHtmlSourceChanged(EventArgs e)
    {
#if WINDOWS
        var htmlString = _HtmlSource;
        if (!string.IsNullOrWhiteSpace(htmlString))
        {
            if (!isNotFirstLoad) WebView2.IsVisible = true;
            WebView2.NavigateToString(htmlString);
            if (!isNotFirstLoad) WebView2.IsVisible = false;
        }
#endif
    }
}

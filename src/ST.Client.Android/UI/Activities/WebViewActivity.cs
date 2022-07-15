using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using Google.Android.Material.AppBar;

namespace System.Application.UI.Activities;

[Register(JavaPackageConstants.Activities + nameof(WebViewActivity))]
[Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
     LaunchMode = LaunchMode.SingleTask,
     ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
public sealed class WebViewActivity : BaseActivity<WebViewActivity>
{
    protected override int? LayoutResource => Resource.Layout.activity_toolbar_webview;

    MaterialToolbar toolbar = null!;
    WebView? webView;

    public static string? HtmlString { get; set; }

    protected override void OnCreate2(Bundle? savedInstanceState)
    {
        base.OnCreate2(savedInstanceState);

        var htmlString = HtmlString;
        if (htmlString == null)
        {
            Finish();
            return;
        }
        HtmlString = null;

        toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar)!;
        webView = FindViewById<WebView>(Resource.Id.webView)!;
        this.SetSupportActionBarWithNavigationClick(toolbar, true);
        webView.InitOptimize();
        webView.LoadHtmlString(htmlString);
    }

    protected override void OnDestroy()
    {
        webView.DestroyAndRemove();
        base.OnDestroy();
    }
}

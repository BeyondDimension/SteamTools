using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Android.Webkit;
using AndroidX.AppCompat.App;
using Google.Android.Material.AppBar;

namespace System.Application.UI.Activities;

[Register(JavaPackageConstants.Activities + nameof(WebViewActivity))]
[Activity(Theme = ManifestConstants.MainTheme2_NoActionBar,
     LaunchMode = LaunchMode.SingleTask,
     ConfigurationChanges = ManifestConstants.ConfigurationChanges)]
public sealed class WebViewActivity : AppCompatActivity
{
    MaterialToolbar toolbar = null!;
    WebView? webView;

    public static string? HtmlString { get; set; }

    public static new string? Title { get; set; }

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);

        var htmlString = HtmlString;
        if (htmlString == null)
        {
            Finish();
            return;
        }
        HtmlString = null;

        SetContentView(Resource.Layout.activity_toolbar_webview);

        toolbar = FindViewById<MaterialToolbar>(Resource.Id.toolbar)!;
        webView = FindViewById<WebView>(Resource.Id.webView)!;
        this.SetSupportActionBarWithNavigationClick(toolbar, true);
        base.Title = Title;
        Title = null;
        webView.InitOptimize();
        webView.LoadHtmlString(htmlString);
    }

    protected override void OnDestroy()
    {
        webView.DestroyAndRemove();
        base.OnDestroy();
    }
}

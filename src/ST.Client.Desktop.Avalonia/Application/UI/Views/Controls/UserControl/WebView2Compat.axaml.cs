using Avalonia;
using Avalonia.Interactivity;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using System.IO;
using System.Threading.Tasks;
using System.Application.UI.Resx;
using System.Threading;
using ReactiveUI;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.Generic;
#if WINDOWS
using Microsoft.Web.WebView2.Core;
#endif

namespace System.Application.UI.Views.Controls;

public partial class WebView2Compat : UserControl
{
    const string TAG = "WebView2Compat";
    const string DownloadLink = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";

    readonly TextBlock TbContent;
    readonly Button BtnDownloadAndInstall;
    readonly ProgressBar LoadingProgress;
    public readonly WebView2 WebView2;

    static bool isInstalling;
    static bool webView2IsSupported;
    static readonly HashSet<WebView2Compat> compats = new();

    public WebView2Compat()
    {
        InitializeComponent();

        LoadingProgress = this.FindControl<ProgressBar>("LoadingProgress");
        TbContent = this.FindControl<TextBlock>("TbContent");
        BtnDownloadAndInstall = this.FindControl<Button>("BtnDownloadAndInstall");
        WebView2 = this.FindControl<WebView2>("WebView2");
        WebView2.IsVisible = false;
#if WINDOWS
        WebView2.DOMContentLoaded += WebView2_DOMContentLoaded;
#endif

        webView2IsSupported = WebView2.IsSupported;
        TbContent.Text = webView2IsSupported ? AppResources.Loading : AppResources.YouNeedInstallWebView2Runtime;
        LoadingProgress.IsVisible = webView2IsSupported;
        if (BtnDownloadAndInstall.IsVisible = !webView2IsSupported)
        {
            BtnDownloadAndInstall.Click += BtnDownloadAndInstall_Click;
            compats.Add(this);
        }

        this.GetPropertyChangedObservable(SourceProperty).Subscribe(OnSourceChanged);
        this.GetPropertyChangedObservable(HtmlSourceProperty).Subscribe(OnHtmlSourceChanged);
        this.GetPropertyChangedObservable(IsVisibleProperty).Subscribe(IsVisibleChanged);
    }

    protected Window? Window { get; set; }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        if (!webView2IsSupported)
        {
            if (e.Root is Window window)
            {
                var prevWindow = Window;
                var isSameWindow = prevWindow == window;
                if (prevWindow != null)
                {
                    if (!isSameWindow)
                    {
                        prevWindow.Closed -= Window_Closed;
                    }
                }
                if (!isSameWindow)
                {
                    // Different windows cannot be reinitialized successfully
                    Window = window;
                    Window.Closed += Window_Closed;
                }
            }
        }
        base.OnAttachedToVisualTree(e);
    }

    void Window_Closed(object? sender, EventArgs e)
    {
        if (!webView2IsSupported)
        {
            compats.Remove(this);
        }
    }

    bool isNotFirstLoad;

#if WINDOWS
    void WebView2_DOMContentLoaded(object? sender, CoreWebView2DOMContentLoadedEventArgs e)
    {
        if (!isNotFirstLoad) isNotFirstLoad = true;
        WebView2.IsVisible = true;
    }
#endif

    /// <summary>
    /// WebView2AutoInstaller
    /// <para>https://github.com/ProKn1fe/WebView2.Runtime/blob/62011b09436944143996fdb0039cd2c5dbb5c300/WebView2.Runtime.AutoInstaller/WebView2.Runtime.AutoInstaller/WebView2AutoInstaller.cs</para>
    /// <para>https://developer.microsoft.com/zh-cn/microsoft-edge/webview2/#download-section</para>
    /// </summary>
    async void BtnDownloadAndInstall_Click(object? s, RoutedEventArgs e)
    {
        if (webView2IsSupported)
        {
#if WINDOWS
            EnsureCoreWebView2();
#endif
            return;
        }
        if (isInstalling) return;
        try
        {
            isInstalling = true;
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                foreach (var item in compats)
                {
                    item.BtnDownloadAndInstall.IsEnabled = false;
                }
            });
            var installerPath = Path.Combine(IOPath.CacheDirectory, "MicrosoftEdgeWebview2Setup.exe");
            var isdownloading = false;
            var isInstalled = false;
            try
            {
                if (!File.Exists(installerPath))
                {
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        TbContent.Text = AppResources.Downloading.Format("0");
                    });
                    try
                    {
                        // 下载常青版引导程序(在线安装包)
                        using var client = new HttpClient();
                        var downloadReponse = await client.GetAsync(DownloadLink);
                        isdownloading = true;
                        using var installerStream = new FileStream(installerPath, FileMode.Create, FileAccess.Write);
                        await downloadReponse.Content.CopyToAsync(installerStream);
                        installerStream.Flush();
                    }
                    catch (Exception ex)
                    {
                        MainThread2.BeginInvokeOnMainThread(() =>
                        {
                            TbContent.Text = AppResources.DownloadFailedPleaseTryAgainLater;
                        });
                        Toast.Show(AppResources.DownloadFailedPleaseTryAgainLater);
                        Log.Error(TAG, ex, "Failed to download WebView2 Installer.");
                        return;
                    }

                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        TbContent.Text = AppResources.InstallingPleaseWait;
                    });

                    isdownloading = false;
                }

                try
                {
                    var install_process = Process.Start(new ProcessStartInfo()
                    {
                        FileName = installerPath,
                        Arguments = "/install",
                        Verb = "runas",
                        UseShellExecute = true,
                    });

                    await install_process!.WaitForExitAsync();
                }
                catch (Exception ex)
                {
                    var install_process_start_failed = AppResources.FailedToRunSetupWithPath_PleaseRunItManually.Format(installerPath);
                    MainThread2.BeginInvokeOnMainThread(() =>
                    {
                        TbContent.Text = install_process_start_failed;
                    });
                    Toast.Show(install_process_start_failed);
                    Log.Error(TAG, ex, install_process_start_failed);
                    return;
                }

                isInstalled = true;

                MainThread2.BeginInvokeOnMainThread(() =>
                {
                    TbContent.Text = AppResources.Loading;
                    LoadingProgress.IsVisible = true;
                    foreach (var item in compats)
                    {
                        item.BtnDownloadAndInstall.IsVisible = false;
                    }
                });

                EnsureAllCoreWebView2();
            }
            finally
            {
                if ((isInstalled || isdownloading) && File.Exists(installerPath)) IOPath.FileTryDelete(installerPath);
            }
        }
        finally
        {
            isInstalling = false;
            MainThread2.BeginInvokeOnMainThread(() =>
            {
                foreach (var item in compats)
                {
                    item.BtnDownloadAndInstall.IsEnabled = true;
                }
            });
        }
    }

#if WINDOWS
    async void EnsureCoreWebView2() => await WebView2.EnsureCoreWebView2Async();
#endif

    static void EnsureAllCoreWebView2()
    {
#if WINDOWS
        WebView2.RefreshIsSupported();
        webView2IsSupported = WebView2.IsSupported;
        MainThread2.BeginInvokeOnMainThread(() =>
        {
            foreach (var item in compats)
            {
                item.EnsureCoreWebView2();
            }
        });
#endif
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
        if (!isNotFirstLoad) WebView2.IsVisible = true;
        WebView2.HtmlSource = HtmlSource;
        if (!isNotFirstLoad) WebView2.IsVisible = false;
#endif
    }

    void IsVisibleChanged(EventArgs e)
    {
#if !WINDOWS
        WebView2.IsVisible = IsVisible;
#endif
    }
}

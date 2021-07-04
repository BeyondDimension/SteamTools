using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using CefNet;
using CefSharp;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Controls;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using static System.Application.Services.CloudService.Constants;
using static System.Application.Services.ISteamService;

namespace System.Application.UI.Views.Windows
{
    public class WebView3Window : FluentWindow<WebView3WindowViewModel>, IDisposable
    {
        readonly WebView3 webView;
        bool disposedValue;

        public WebView3Window() : base()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
            webView = this.FindControl<WebView3>(nameof(webView));
            webView.Browser.InitialUrl = WebView3WindowViewModel.AboutBlank;
            webView.Browser.DocumentTitleChanged += WebView_DocumentTitleChanged;
            webView.Browser.LoadingStateChange += WebView_LoadingStateChange;
            webView.Browser.BrowserCreated += WebView_BrowserCreated;
        }

        CancellationTokenSource? cts;

        /// <summary>
        /// 第一次 WebView 加载超时检查
        /// </summary>
        /// <param name="vm"></param>
        async void FirstWebViewLoadingTimeoutInspect(WebView3WindowViewModel vm)
        {
            if (cts == null)
            {
                cts = new();
                var isDelayed = false;
                try
                {
                    await Task.Delay(vm.Timeout, cts.Token);
                    isDelayed = true;
                }
                catch (OperationCanceledException)
                {
                }
                if (isDelayed && vm.IsLoading)
                {
                    webView.Browser.Stop();
                    await WebViewLoadingTimeoutAsync(vm);
                }
            }
        }

        bool isShowTimeoutErrorMessageed;
        async Task WebViewLoadingTimeoutAsync(WebView3WindowViewModel? vm = null)
        {
            if (disposedValue) return;
            if (vm == null && DataContext is WebView3WindowViewModel _vm) vm = _vm;
            if (vm != null && !isShowTimeoutErrorMessageed && vm.TimeoutErrorMessage != null)
            {
                isShowTimeoutErrorMessageed = true;
                Hide();
                await MessageBoxCompat.ShowAsync(
                    vm.TimeoutErrorMessage!,
                    vm.Title,
                    MessageBoxButtonCompat.OKCancel);
            }
            Close();
        }

        bool isFirstWebViewLoading;
        //#if DEBUG
        //        bool isShowDevTools;
        //#endif
        private async void WebView_LoadingStateChange(object? sender, LoadingStateChangeEventArgs e)
        {
            //#if DEBUG
            //            if (!isShowDevTools)
            //            {
            //                webView.ShowDevTools();
            //                isShowDevTools = true;
            //            }
            //#endif
            if (DataContext is WebView3WindowViewModel vm)
            {
                if (!isFirstWebViewLoading)
                {
                    isFirstWebViewLoading = true;
                    if (!string.IsNullOrWhiteSpace(vm.TimeoutErrorMessage))
                    {
                        FirstWebViewLoadingTimeoutInspect(vm);
                    }
                }
                if (!e.Busy)
                {
                    var mainFrame = webView.Browser.GetMainFrame();
                    if (mainFrame != null)
                    {
                        var mainFrameSource = await mainFrame.GetSourceAsync(default);
                        if (mainFrameSource == "<html><head></head><body></body></html>")
                        {
                            await WebViewLoadingTimeoutAsync(vm);
                            return;
                        }
                        if (vm.IsLoading)
                        {
                            vm.IsLoading = false;
                        }
                    }
                }
            }
        }

        string? initialUrl;
        private void WebView_BrowserCreated(object? sender, EventArgs e)
        {
            if (initialUrl != null && webView.Browser.BrowserObject != null)
            {
                webView.Navigate(initialUrl);
            }
        }

        void Navigate(string url)
        {
            if (webView.Browser.BrowserObject == null)
            {
                initialUrl = url;
            }
            else
            {
                webView.Navigate(url);
            }
        }

        [Obsolete(LoginUsingSteamClientCookieObsolete)]
        async Task LoginUsingSteamClientCookiesAsync()
        {
            var (resultCode, cookies) = await Instance.GetLoginUsingSteamClientCookieCollectionAsync(runasInvoker: DI.Platform == Platform.Windows);
            if (resultCode == LoginUsingSteamClientResultCode.Success && cookies != null)
            {
                var manager = CefRequestContext.GetGlobalContext().GetCookieManager(null);
                foreach (Cookie item in cookies)
                {
                    if (item.Domain.Equals(url_store_steampowered_, StringComparison.OrdinalIgnoreCase))
                    {
                        item.Domain = url_steamcommunity_;
                    }
                    var cookie = item.GetCefNetCookie();
                    var setCookieResult = await manager.SetCookieAsync(url_steamcommunity_checkclientautologin, cookie);
                    if (item.Name == "steamLoginSecure" && setCookieResult)
                    {
                        loginUsingSteamClientState = LoginUsingSteamClientState.Success;
                    }
                }
                foreach (Cookie item in cookies)
                {
                    var cookie = item.GetCefNetCookie();
                    await manager.SetCookieAsync(url_store_steampowered_checkclientautologin, cookie);
                }
            }
            else
            {
                resultCode = resultCode ==
                    LoginUsingSteamClientResultCode.Success ?
                    LoginUsingSteamClientResultCode.EmptyOrNull :
                    resultCode;
            }
            if (loginUsingSteamClientState == LoginUsingSteamClientState.Loading)
            {
                loginUsingSteamClientState = LoginUsingSteamClientState.None;
                resultCode = resultCode ==
                    LoginUsingSteamClientResultCode.Success ?
                    LoginUsingSteamClientResultCode.MissingCookieSteamLoginSecure :
                    resultCode;
            }
            if (resultCode != LoginUsingSteamClientResultCode.Success)
            {
                if (resultCode == LoginUsingSteamClientResultCode.CantConnSteamCommunity)
                {
                    await WebViewLoadingTimeoutAsync();
                }
                else
                {
                    Toast.Show(AppResources.GetLoginUsingSteamClientCookiesFail_.Format((int)resultCode));
                }
            }
        }

        [Obsolete(LoginUsingSteamClientCookieObsolete)]
        async void GetLoginUsingSteamClientCookies()
        {
            await LoginUsingSteamClientCookiesAsync();
            if (loginUsingSteamClientState == LoginUsingSteamClientState.Success
                && webView.Browser.BrowserObject != null)
            {
                webView.Browser.Reload();
            }
        }

        static LoginUsingSteamClientState loginUsingSteamClientState;
        protected override void OnDataContextChanged(EventArgs e)
        {
            base.OnDataContextChanged(e);
            if (DataContext is WebView3WindowViewModel vm)
            {
                vm.Close += Close;
                if (!string.IsNullOrWhiteSpace(vm.Title))
                {
                    webView.Browser.DocumentTitleChanged -= WebView_DocumentTitleChanged;
                }
                if (vm.UseLoginUsingSteamClient)
                {
                    if (loginUsingSteamClientState == LoginUsingSteamClientState.None)
                    {
                        var steamUser = SteamConnectService.Current.CurrentSteamUser;
                        if (steamUser != null)
                        {
                            loginUsingSteamClientState = LoginUsingSteamClientState.Loading;
                            //GetLoginUsingSteamClientCookies();
                        }
                    }
                }
                vm.WhenAnyValue(x => x.Url).WhereNotNull().Subscribe(x =>
                {
                    if (x == WebView3WindowViewModel.AboutBlank)
                    {
                        webView.Opacity = 0;
                    }
                    else if (IsHttpUrl(x))
                    {
                        if (x.StartsWith(UrlConstants.OfficialWebsite, StringComparison.OrdinalIgnoreCase))
                            x = string.Format(
                                x + "?theme={0}&language={1}",
                                CefNetApp.GetTheme(),
                                R.Language);
                        //if (webView.Opacity != 1) webView.Opacity = 1;
                        Navigate(x);
                    }
                }).AddTo(vm);
                vm.WhenAnyValue(x => x.StreamResponseFilterUrls).Subscribe(x => webView.Browser.StreamResponseFilterUrls = x).AddTo(vm);
                vm.WhenAnyValue(x => x.FixedSinglePage).Subscribe(x => webView.Browser.FixedSinglePage = x).AddTo(vm);
                vm.WhenAnyValue(x => x.IsSecurity).Subscribe(x => webView.Browser.IsSecurity = x).AddTo(vm);
                webView.Browser.OnStreamResponseFilterResourceLoadComplete += vm.OnStreamResponseFilterResourceLoadComplete;
            }
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            if (loginUsingSteamClientState == LoginUsingSteamClientState.Loading)
            {
                Toast.Show(AppResources.GetLoginUsingSteamClientCookies);
            }
        }

        void WebView_DocumentTitleChanged(object? sender, DocumentTitleChangedEventArgs e)
        {
            if (DataContext is WindowViewModel vm)
            {
                vm.Title = e.Title;
            }
            else
            {
                Title = e.Title;
            }
        }

        void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            Dispose();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    if (loginUsingSteamClientState == LoginUsingSteamClientState.Loading)
                    {
                        loginUsingSteamClientState = LoginUsingSteamClientState.None;
                    }
                    if (webView != null)
                    {
                        cts?.Cancel();
                        webView.Browser.DocumentTitleChanged -= WebView_DocumentTitleChanged;
                        webView.Browser.LoadingStateChange -= WebView_LoadingStateChange;
                        if (DataContext is WebView3WindowViewModel vm)
                        {
                            webView.Browser.OnStreamResponseFilterResourceLoadComplete -= vm.OnStreamResponseFilterResourceLoadComplete;
                        }
                        ((IDisposable)webView).Dispose();
                    }
                    if (DataContext is IDisposable d)
                    {
                        d.Dispose();
                    }
                }

                // TODO: 释放未托管的资源(未托管的对象)并替代终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~WebView3Window()
        // {
        //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        enum LoginUsingSteamClientState
        {
            None,
            Loading,
            Success,
        }
    }
}
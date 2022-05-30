namespace System.Application.Services.Implementation;

sealed class BrowserPlatformServiceImpl : IBrowserPlatformService
{
    Task IBrowserPlatformService.OpenAsync(string uri)
        => Browser.OpenAsync(uri);

    Task IBrowserPlatformService.OpenAsync(Uri uri)
        => Browser.OpenAsync(uri);

    Task IBrowserPlatformService.OpenAsync(string uri, BrowserLaunchMode launchMode)
        => Browser.OpenAsync(uri, launchMode.Convert());

    Task IBrowserPlatformService.OpenAsync(Uri uri, BrowserLaunchMode launchMode)
        => Browser.OpenAsync(uri, launchMode.Convert());

    Task IBrowserPlatformService.OpenAsync(string uri, BrowserLaunchOptions options)
        => Browser.OpenAsync(uri, options.Convert());

    Task<bool> IBrowserPlatformService.OpenAsync(Uri uri, BrowserLaunchOptions options)
        => Browser.OpenAsync(uri, options.Convert());
}


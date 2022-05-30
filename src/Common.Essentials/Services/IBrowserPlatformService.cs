namespace System.Application.Services;

public interface IBrowserPlatformService
{
    static IBrowserPlatformService Instance => DI.Get<IBrowserPlatformService>();

    Task OpenAsync(string uri);

    Task OpenAsync(Uri uri);

    Task OpenAsync(string uri, BrowserLaunchMode launchMode);

    Task OpenAsync(Uri uri, BrowserLaunchMode launchMode);

    Task OpenAsync(string uri, BrowserLaunchOptions options);

    Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options);
}

using System.Threading.Tasks;
using Xamarin.Essentials;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    /// <inheritdoc cref="Browser"/>
    public static partial class Browser2
    {
        public static event Action<Exception>? OnError;

        public static bool HttpsOnly { get; set; }

        /// <summary>
        /// 兼容 Linux/macOS/.Net Core/Android/iOS 的打开链接方法
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static Task<bool> OpenAsync(string? url) => OpenAnalysis(url) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(url),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static bool Open(string? url) => OpenAnalysis(url) switch
        {
            OpenResultCode.HttpUrl => OpenCore(url),
            OpenResultCode.StartedByProcess2 => true,
            _ => false,
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(string? url, BrowserLaunchMode launchMode) => OpenAnalysis(url) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(url, launchMode),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(string? url, BrowserLaunchOptions options) => OpenAnalysis(url) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(url, options),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(Uri uri) => OpenAnalysis(uri.ToString()) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(uri),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(Uri uri, BrowserLaunchMode launchMode) => OpenAnalysis(uri.ToString()) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(uri, launchMode),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };

        /// <inheritdoc cref="OpenAsync(string?)"/>
        public static Task<bool> OpenAsync(Uri uri, BrowserLaunchOptions options) => OpenAnalysis(uri.ToString()) switch
        {
            OpenResultCode.HttpUrl => OpenCoreAsync(uri, options),
            OpenResultCode.StartedByProcess2 => Task.FromResult(true),
            _ => Task.FromResult(false),
        };
    }
}
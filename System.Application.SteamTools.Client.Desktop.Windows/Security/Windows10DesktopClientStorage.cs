// https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/SecureStorage/SecureStorage.shared.cs
using System.Application;
using System.Threading.Tasks;

namespace System.Security
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider?view=winrt-10240
    /// </summary>
    internal sealed partial class Windows10DesktopClientStorage : IStorage
    {
        // Special Alias that is only used for Secure Storage. All others should use: Preferences.GetPrivatePreferencesSharedName
        internal static readonly string Alias = $"{BuildConfig.APPLICATION_ID}.xamarinessentials";

        public string? Get(string key)
        {
            Func<Task<string?>> func = () => GetAsync(key);
            var result = func.RunSync();
            return result;
        }

        public Task<string?> GetAsync(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));
            return PlatformGetAsync(key);
        }

        public void Set(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Remove(key);
                return;
            }
            Func<Task> func = () => SetAsync(key, value);
            func.RunSync();
        }

        public Task SetAsync(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                Remove(key);
                return Task.CompletedTask;
            }
            return SetAsync_(key, value);
        }

        static Task SetAsync_(string key, string? value)
        {
            if (string.IsNullOrWhiteSpace(key))
                throw new ArgumentNullException(nameof(key));

            if (value == null)
                throw new ArgumentNullException(nameof(value));

            return PlatformSetAsync(key, value);
        }

        public bool Remove(string key) => PlatformRemove(key);

        public Task<bool> RemoveAsync(string key) => Task.FromResult(Remove(key));

        public static void RemoveAll() => PlatformRemoveAll();
    }
}
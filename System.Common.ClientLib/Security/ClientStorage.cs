using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Security
{
    /// <summary>
    /// https://docs.microsoft.com/zh-cn/xamarin/essentials/secure-storage
    /// </summary>
    internal sealed class ClientStorage : IStorage
    {
        string? IStorage.Get(string key)
        {
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.android.cs#L74
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.ios.tvos.watchos.cs#L26
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.uwp.cs#L23
            Func<Task<string?>> func = () => SecureStorage.GetAsync(key);
            var result = func.RunSync();
            return result;
        }

        Task<string?> IStorage.GetAsync(string key)
        {
            return SecureStorage.GetAsync(key);
        }

        void IStorage.Set(string key, string? value)
        {
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.android.cs#L93
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.ios.tvos.watchos.cs#L34
            // https://github.com/xamarin/Essentials/blob/1.5.3.2/Xamarin.Essentials/SecureStorage/SecureStorage.uwp.cs#L37
            if (string.IsNullOrEmpty(value))
            {
                SecureStorage.Remove(key);
                return;
            }
            Func<Task> func = () => SecureStorage.SetAsync(key, value);
            func.RunSync();
        }

        Task IStorage.SetAsync(string key, string? value)
        {
            if (string.IsNullOrEmpty(value))
            {
                SecureStorage.Remove(key);
                return Task.CompletedTask;
            }
            return SecureStorage.SetAsync(key, value);
        }

        bool IStorage.Remove(string key)
        {
            return SecureStorage.Remove(key);
        }

        Task<bool> IStorage.RemoveAsync(string key)
        {
            var result = SecureStorage.Remove(key);
            return Task.FromResult(result);
        }
    }
}
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace System.Security
{
    /// <summary>
    /// https://docs.microsoft.com/zh-cn/xamarin/essentials/secure-storage
    /// </summary>
    internal sealed class ClientStorage : IStorage
    {
        bool IStorage.IsNativeSupportedBytes => false;

        Task<string?> IStorage.GetAsync(string key)
        {
            return SecureStorage.GetAsync(key);
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

        Task<bool> IStorage.RemoveAsync(string key)
        {
            var result = SecureStorage.Remove(key);
            return Task.FromResult(result);
        }

        async Task<bool> IStorage.ContainsKeyAsync(string key)
        {
            var result = await SecureStorage.GetAsync(key);
            return result != null;
        }
    }
}
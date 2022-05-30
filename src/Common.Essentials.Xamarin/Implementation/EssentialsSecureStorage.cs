// ReSharper disable once CheckNamespace
namespace System.Security;

/// <summary>
/// 由 Xamarin.Essentials 实现的 <see cref="ISecureStorage"/>
/// <para>https://docs.microsoft.com/zh-cn/xamarin/essentials/secure-storage</para>
/// </summary>
internal sealed class EssentialsSecureStorage : ISecureStorage
{
    bool ISecureStorage.IsNativeSupportedBytes => false;

    async Task<string?> ISecureStorage.GetAsync(string key)
    {
        return await SecureStorage.GetAsync(key);
    }

    Task ISecureStorage.SetAsync(string key, string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            SecureStorage.Remove(key);
            return Task.CompletedTask;
        }
        return SecureStorage.SetAsync(key, value);
    }

    Task<bool> ISecureStorage.RemoveAsync(string key)
    {
        var result = SecureStorage.Remove(key);
        return Task.FromResult(result);
    }

    async Task<bool> ISecureStorage.ContainsKeyAsync(string key)
    {
        var result = await SecureStorage.GetAsync(key);
        return result != null;
    }
}
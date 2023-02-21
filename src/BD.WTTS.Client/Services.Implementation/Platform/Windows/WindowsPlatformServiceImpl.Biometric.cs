#if WINDOWS
using Windows.Security.Credentials;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

partial class WindowsPlatformServiceImpl
{
    public async ValueTask<bool> IsSupportedBiometricAsync()
    {
        // https://docs.microsoft.com/zh-cn/windows/uwp/security/microsoft-passport#3-implementing-windows-hello
        var keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();
        return keyCredentialAvailable;
    }
}

#endif
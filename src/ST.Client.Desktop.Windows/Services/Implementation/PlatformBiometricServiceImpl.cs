using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Security.Credentials;

namespace System.Application.Services.Implementation
{
    [SupportedOSPlatform("Windows10.0.10240.0")]
    internal sealed class PlatformBiometricServiceImpl : IBiometricService
    {
        public async ValueTask<bool> IsSupportedAsync()
        {
            // https://docs.microsoft.com/zh-cn/windows/uwp/security/microsoft-passport#3-implementing-windows-hello
            var keyCredentialAvailable = await KeyCredentialManager.IsSupportedAsync();
            return keyCredentialAvailable;
        }
    }
}
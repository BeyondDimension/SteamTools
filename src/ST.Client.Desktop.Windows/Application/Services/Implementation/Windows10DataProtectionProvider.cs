// https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/SecureStorage/SecureStorage.uwp.cs
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Security.Cryptography.DataProtection;
using static System.Application.Services.ILocalDataProtectionProvider;
namespace System.Application.Services.Implementation
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider?view=winrt-10240
    /// </summary>
    [SupportedOSPlatform("Windows10.0.10240.0")]
    internal sealed class Windows10DataProtectionProvider : IDataProtectionProvider
    {
        public async Task<byte[]> ProtectAsync(byte[] data)
        {
            // LOCAL=user and LOCAL=machine do not require enterprise auth capability
            var provider = new DataProtectionProvider("LOCAL=user");

            var buffer = await provider.ProtectAsync(data.AsBuffer());

            var encBytes = buffer.ToArray();

            return encBytes;
        }

        public async Task<byte[]> UnprotectAsync(byte[] data)
        {
            var provider = new DataProtectionProvider();

            var buffer = await provider.UnprotectAsync(data.AsBuffer());

            return buffer.ToArray();
        }
    }
}
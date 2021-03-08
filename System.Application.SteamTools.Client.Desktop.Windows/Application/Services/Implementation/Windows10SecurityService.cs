// https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/SecureStorage/SecureStorage.uwp.cs
using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Windows.Security.Cryptography.DataProtection;

namespace System.Application.Services.Implementation
{
    /// <summary>
    /// https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider?view=winrt-10240
    /// </summary>
    [SupportedOSPlatform("Windows10.0.10240.0")]
    internal sealed class Windows10SecurityService : SecurityService
    {
        public Windows10SecurityService(IOptions<AppSettings> options) : base(options)
        {
        }

        static async Task<byte[]?> E__(byte[]? bytes)
        {
            if (bytes == null) return null;

            // LOCAL=user and LOCAL=machine do not require enterprise auth capability
            var provider = new DataProtectionProvider("LOCAL=user");

            var buffer = await provider.ProtectAsync(bytes.AsBuffer());

            var encBytes = buffer.ToArray();

            return encBytes;
        }

        static async Task<byte[]?> D__(byte[]? encBytes)
        {
            if (encBytes == null) return null;

            var provider = new DataProtectionProvider();

            var buffer = await provider.UnprotectAsync(encBytes.AsBuffer());

            return buffer.ToArray();
        }

        public override Task<byte[]?> E(string? value)
        {
            var value2 = E_(value);
            return E__(value2);
        }

        public override Task<byte[]?> EB(byte[]? value)
        {
            var value2 = EB_(value);
            return E__(value2);
        }

        public override async Task<string?> D(byte[]? value)
        {
            try
            {
                var value2 = await D__(value);
                return D_(value2);
            }
            catch
            {
                return null;
            }
        }

        public override async Task<byte[]?> DB(byte[]? value)
        {
            try
            {
                var value2 = await D__(value);
                return DB_(value2);
            }
            catch
            {
                return null;
            }
        }
    }
}
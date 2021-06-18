using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static System.Application.Services.ILocalDataProtectionProvider;
using IProtectedData = System.Application.Services.ILocalDataProtectionProvider.IProtectedData;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ILocalDataProtectionProvider"/>
    public abstract class LocalDataProtectionProviderBase : ILocalDataProtectionProvider
    {
        readonly Lazy<Aes> _aes;
#pragma warning disable IDE1006 // 命名样式
        protected Aes aes => _aes.Value;
#pragma warning restore IDE1006 // 命名样式
        protected readonly LocalDataProtectionType defaultELocalDataProtectionType;
        protected readonly IProtectedData protectedData;
        protected readonly IDataProtectionProvider dataProtectionProvider;

        public LocalDataProtectionProviderBase(
            IProtectedData protectedData,
            IDataProtectionProvider dataProtectionProvider)
        {
            this.protectedData = protectedData;
            this.dataProtectionProvider = dataProtectionProvider;
            defaultELocalDataProtectionType = DI.Platform switch
            {
                Platform.Windows => DI.IsWindows10OrLater
                    ? LocalDataProtectionType.Win10WithAesCFB
                    : LocalDataProtectionType.ProtectedDataWithAesCFB,
                _ => LocalDataProtectionType.AesCFB,
            };
            _aes = new Lazy<Aes>(() =>
            {
                (var key, var iv) = MachineSecretKey;
                // https://github.com/dotnet/runtime/issues/42214#issuecomment-698495584
                // AES CFB in Windows 7 catch Internal.Cryptography.CryptoThrowHelper+WindowsCryptographicException: Unknown error (0xc10000bb)
                var r = AESUtils.Create(key, iv, CipherMode.CFB, PaddingMode.PKCS7);
                return r;
            });
        }

        protected enum LocalDataProtectionType
        {
            None,

            AesCFB,

            ProtectedDataWithAesCFB,

            Win10WithAesCFB,
        }

        protected abstract (byte[] key, byte[] iv) MachineSecretKey { get; }

        byte[] Concat(byte[] value)
        {
            var r = BitConverter.GetBytes((int)defaultELocalDataProtectionType).Concat(value).ToArray();
            return r;
        }

        byte[] E___(byte[] value)
        {
            var r = AESUtils.Encrypt(aes, value);
            return r;
        }

        public async ValueTask<byte[]?> EB(byte[]? value)
        {
            if (value == null) return value;
            if (value.Length == 0) return value;
            switch (defaultELocalDataProtectionType)
            {
                case LocalDataProtectionType.None:
                    return value;
                case LocalDataProtectionType.AesCFB:
                    var value_1 = E___(value);
                    var value_1_r = Concat(value_1);
                    return value_1_r;
                case LocalDataProtectionType.ProtectedDataWithAesCFB:
                    var value_2 = E___(value);
                    var value_2_pd = protectedData.Protect(value_2);
                    var value_2_r = Concat(value_2_pd);
                    return value_2_r;
                case LocalDataProtectionType.Win10WithAesCFB:
                    var value_3 = E___(value);
                    var value_3_dpp = await dataProtectionProvider.ProtectAsync(value_3);
                    var value_3_r = Concat(value_3_dpp);
                    return value_3_r;
                default:
                    throw new ArgumentOutOfRangeException(nameof(defaultELocalDataProtectionType), defaultELocalDataProtectionType, null);
            }
        }

        byte[]? D___(byte[] value)
        {
            using var transform = aes.CreateDecryptor();
            var data_r = transform.TransformFinalBlock(value, sizeof(int), value.Length - sizeof(int));
            return data_r;
        }

        static byte[] UnConcat(byte[] value) => value.Skip(sizeof(int)).ToArray();

        public async ValueTask<byte[]?> DB(byte[]? value)
        {
            if (value == null) return value;
            if (value.Length == 0) return value;
            if (value.Length <= sizeof(int)) return null;

            var d_type = (LocalDataProtectionType)BitConverter.ToInt32(value, 0);
            try
            {
                switch (d_type)
                {
                    case LocalDataProtectionType.None:
                        return value;
                    case LocalDataProtectionType.AesCFB:
                        return D___(value);
                    case LocalDataProtectionType.ProtectedDataWithAesCFB:
                        var value_2 = UnConcat(value);
                        var value_2_pd = protectedData.Unprotect(value_2);
                        var value_2_r = AESUtils.Decrypt(aes, value_2_pd);
                        return value_2_r;
                    case LocalDataProtectionType.Win10WithAesCFB:
                        var value_3 = UnConcat(value);
                        var value_3_dpp = await dataProtectionProvider.UnprotectAsync(value_3);
                        var value_3_r = AESUtils.Decrypt(aes, value_3_dpp);
                        return value_3_r;
                    default:
                        return null;
                }
            }
            catch
            {
                return null;
            }
        }

        public sealed class EmptyProtectedData : IProtectedData
        {
            public byte[] Protect(byte[] userData) => userData;

            public byte[] Unprotect(byte[] encryptedData) => encryptedData;
        }

        public sealed class EmptyDataProtectionProvider : IDataProtectionProvider
        {
            public Task<byte[]> ProtectAsync(byte[] data) => Task.FromResult(data);

            public Task<byte[]> UnprotectAsync(byte[] data) => Task.FromResult(data);
        }
    }
}
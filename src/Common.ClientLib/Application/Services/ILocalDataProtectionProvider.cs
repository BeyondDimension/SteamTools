using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 本机加密模式数据保护提供者
    /// </summary>
    public interface ILocalDataProtectionProvider
    {
        /// <inheritdoc cref="IEmbeddedAesDataProtectionProvider.E(string?)"/>
        ValueTask<byte[]?> EB(byte[]? value);

        /// <inheritdoc cref="IEmbeddedAesDataProtectionProvider.D(byte[]?)"/>
        ValueTask<byte[]?> DB(byte[]? value);

        public interface IProtectedData
        {
            byte[] Protect(byte[] userData);

            byte[] Unprotect(byte[] encryptedData);
        }

        public interface IDataProtectionProvider
        {
            Task<byte[]> ProtectAsync(byte[] data);

            Task<byte[]> UnprotectAsync(byte[] data);
        }
    }
}
using System.Application.Security;
using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 安全服务
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// 此值不可更改
        /// </summary>
        const EncryptionMode DefaultEncryptionMode = EncryptionMode.EmbeddedAesWithLocal;

        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="encryptionMode"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        ValueTask<byte[]?> E(string? value, EncryptionMode encryptionMode = DefaultEncryptionMode, string? secondaryPassword = null);

        /// <inheritdoc cref="E(string?, EncryptionMode, string?)"/>
        ValueTask<byte[]?> EB(byte[]? value, EncryptionMode encryptionMode = DefaultEncryptionMode, string? secondaryPassword = null);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        ValueTask<string?> D(byte[]? value, string? secondaryPassword = null);

        /// <inheritdoc cref="D(byte[]?, string?)"/>
        ValueTask<byte[]?> DB(byte[]? value, string? secondaryPassword = null);
    }
}
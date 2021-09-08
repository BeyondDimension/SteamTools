using System.Security;
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
        ValueTask<(string? content, DResultCode resultCode)> D2(byte[]? value, string? secondaryPassword = null);

        /// <inheritdoc cref="D(byte[]?, string?)"/>
        ValueTask<byte[]?> DB(byte[]? value, string? secondaryPassword = null);

        /// <inheritdoc cref="D(byte[]?, string?)"/>
        ValueTask<(byte[]? content, DResultCode resultCode)> DB2(byte[]? value, string? secondaryPassword = null);

        public enum DResultCode
        {
            /// <summary>
            /// 成功
            /// </summary>
            Success = 200,

            /// <summary>
            /// 嵌入 Aes 失败
            /// </summary>
            EmbeddedAesFail = 901,

            /// <summary>
            /// 仅本机失败
            /// </summary>
            LocalFail,

            /// <summary>
            /// 二级密码失败
            /// </summary>
            SecondaryPasswordFail,

            /// <summary>
            /// 密文值不正确
            /// </summary>
            IncorrectValueFail,

            /// <summary>
            /// 转换字符串失败
            /// </summary>
            UTF8GetStringFail,
        }
    }
}
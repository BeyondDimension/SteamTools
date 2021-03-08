using System.Threading.Tasks;

namespace System.Application.Services
{
    /// <summary>
    /// 安全服务
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<byte[]?> E(string? value);

        /// <inheritdoc cref="E(string?)"/>
        Task<byte[]?> EB(byte[]? value);

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        Task<string?> D(byte[]? value);

        /// <inheritdoc cref="D(byte[]?)"/>
        Task<byte[]?> DB(byte[]? value);
    }
}
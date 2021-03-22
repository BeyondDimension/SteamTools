namespace System.Application.Services
{
    /// <summary>
    /// 二级密码加密模式数据保护提供者
    /// </summary>
    public interface ISecondaryPasswordDataProtectionProvider
    {
        /// <inheritdoc cref="IEmbeddedAesDataProtectionProvider.E(string?)"/>
        byte[]? EB(byte[]? value, string secondaryPassword);

        /// <inheritdoc cref="IEmbeddedAesDataProtectionProvider.D(byte[]?)"/>
        byte[]? DB(byte[]? value, string secondaryPassword);
    }
}
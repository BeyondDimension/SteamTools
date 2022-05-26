namespace System.Application.Services;

/// <summary>
/// 嵌入 AES 加密模式数据保护提供者
/// </summary>
public interface IEmbeddedAesDataProtectionProvider
{
    /// <summary>
    /// 加密
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    byte[]? E(string? value);

    /// <inheritdoc cref="E(string?)"/>
    byte[]? EB(byte[]? value);

    /// <summary>
    /// 解密
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    string? D(byte[]? value);

    /// <inheritdoc cref="D(byte[]?)"/>
    byte[]? DB(byte[]? value);
}
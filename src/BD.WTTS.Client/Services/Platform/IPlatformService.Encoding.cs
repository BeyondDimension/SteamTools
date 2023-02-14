// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    /// <summary>
    /// 在 Windows 上时使用 .NET Framework 中 <see cref="Encoding.Default"/> 行为。
    /// <para></para>
    /// 非 Windows 上等同于 <see cref="Encoding.Default"/>(UTF8)
    /// </summary>
    Encoding Default => Encoding.Default;
}